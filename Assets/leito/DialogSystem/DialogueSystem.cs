using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Linq;

[System.Serializable]
public class DialogueLine
{
    [TextArea(3, 10)]
    public string text;
    public string characterName;
    public BubblePosition bubblePosition = BubblePosition.Left;
    public AudioClip typeSound;
    public float textSpeed = 0.05f;
    public UnityEvent onStartLine;
    public UnityEvent onEndLine;
}

[System.Serializable]
public class DialogueSequence
{
    public string sequenceName;
    public List<DialogueLine> dialogueLines;
    public UnityEvent onStartSequence;
    public UnityEvent onEndSequence;
}

public enum BubblePosition
{
    Left,
    Right
}

public enum DialogueState
{
    Inactive,
    Active,
    Typing,
    Paused
}

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogueCanvas;
    public GameObject leftSpeechBubble;
    public TextMeshProUGUI leftCharacterText;
    public TextMeshProUGUI leftCharacterName;
    public GameObject rightSpeechBubble;
    public TextMeshProUGUI rightCharacterText;
    public TextMeshProUGUI rightCharacterName;

    [Header("Dialogue Data")]
    public List<DialogueSequence> dialogueSequences;

    [Header("Input Settings")]
    public bool allowMouseInput = true;
    public bool allowKeyboardInput = false;
    public KeyCode advanceKey = KeyCode.Space;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public float typeSoundVolume = 0.5f;

    [Header("Objects to Disable During Dialogue")]
    public List<GameObject> objectsToDisable = new List<GameObject>();
    private List<GameObject> objectsToReenable = new List<GameObject>();

    [Header("Exit Animation Settings")]
    [Tooltip("Nombre del trigger en el Animator para la animación de salida (por ejemplo, 'Hide'). Si está vacío, se desactiva inmediatamente.")]
    public string exitAnimationTrigger = "";
    [Tooltip("Tiempo a esperar después de disparar la animación de salida antes de desactivar el objeto.")]
    public float exitAnimationDelay = 0.3f;

    [Header("Re-enable Options")]
    [Tooltip("Si es true, los objetos desactivados por el sistema de diálogo se reactivan automáticamente al terminar el diálogo.")]
    public bool reenableObjectsOnDialogueEnd = true;

    private DialogueState currentState = DialogueState.Inactive;
    private Queue<DialogueLine> currentDialogueLines;
    private DialogueSequence currentSequence;
    private bool isTyping = false;
    private string currentText = "";
    private BubblePosition currentBubblePosition;
    private Coroutine currentDialogueCoroutine = null;

    private void Awake()
    {
        // Cada escena tendrá su propio DialogueSystem; no usamos singleton global.

        dialogueCanvas.SetActive(false);
        currentDialogueLines = new Queue<DialogueLine>();

        // Configurar AudioSource si no existe
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = typeSoundVolume;
        }
    }

    public void StartDialogue(string sequenceName)
    {
        DialogueSequence sequence = dialogueSequences.Find(s => s.sequenceName == sequenceName);
        if (sequence != null)
        {
            StartDialogue(sequence);
        }
        else
        {
            Debug.LogError($"Dialogue sequence '{sequenceName}' not found!");
        }
    }

    public void StartDialogue(DialogueSequence sequence)
    {
        if (currentState != DialogueState.Inactive) return;

        currentSequence = sequence;
        currentDialogueLines.Clear();

        foreach (DialogueLine line in sequence.dialogueLines)
        {
            currentDialogueLines.Enqueue(line);
        }

        // --- DESACTIVAR OBJETOS ---
        DisableObjects();

        dialogueCanvas.SetActive(true);
        currentState = DialogueState.Active;
        sequence.onStartSequence?.Invoke();
        DisplayNextLine();
    }

    // --- MÉTODO SIMPLIFICADO PARA DESACTIVAR OBJETOS ---
    private void DisableObjects()
    {
        objectsToReenable.Clear();

        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null && obj.activeInHierarchy)
            {
                // IMPORTANTE: No desactivar el objeto que TIENE directamente el ActionButtonsController.
                // Así evitamos matar las corrutinas de combate, pero sí podemos desactivar paneles
                // padres o hermanos que solo contienen UI de cartas/botones.
                if (obj.GetComponent<ActionButtonsController>() != null)
                {
                    continue;
                }

                objectsToReenable.Add(obj);

                // Si hay Animator y trigger configurado, reproducir animación de salida
                Animator animator = obj.GetComponent<Animator>();
                if (animator != null && !string.IsNullOrEmpty(exitAnimationTrigger))
                {
                    animator.SetTrigger(exitAnimationTrigger);
                }

                if (exitAnimationDelay > 0f && animator != null && !string.IsNullOrEmpty(exitAnimationTrigger))
                {
                    // Desactivar con retraso para dejar reproducir la animación
                    StartCoroutine(DeactivateAfterDelay(obj, exitAnimationDelay));
                }
                else
                {
                    // Sin animación de salida, desactivar inmediatamente
                    obj.SetActive(false);
                }
            }
        }
    }

    private IEnumerator DeactivateAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
        {
            obj.SetActive(false);
        }
    }

    // --- MÉTODO SIMPLIFICADO PARA REACTIVAR OBJETOS ---
    private void EnableObjects()
    {
        foreach (GameObject obj in objectsToReenable)
        {
            if (obj != null)
            {
                // --- ACTIVAR TODOS LOS OBJETOS ---
                obj.SetActive(true);
            }
        }
        objectsToReenable.Clear();
    }

    public void DisplayNextLine()
    {
        if (currentState == DialogueState.Typing)
        {
            CompleteCurrentLine();
            return;
        }

        if (currentDialogueLines.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = currentDialogueLines.Dequeue();
        // Asegurarnos de que no haya una corrutina anterior de tipeo aún activa
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;
        }

        currentDialogueCoroutine = StartCoroutine(TypeLine(line));
    }

    private IEnumerator TypeLine(DialogueLine line)
    {
        currentState = DialogueState.Typing;
        currentText = line.text;
        currentBubblePosition = line.bubblePosition;

        SetupSpeechBubbles(line.bubblePosition);

        TextMeshProUGUI currentTextComponent = GetTextComponentForBubble(line.bubblePosition);
        TextMeshProUGUI currentNameComponent = GetNameComponentForBubble(line.bubblePosition);

        if (currentNameComponent != null) currentNameComponent.text = line.characterName;
        if (currentTextComponent != null) currentTextComponent.text = "";

        line.onStartLine?.Invoke();

        if (currentTextComponent != null)
        {
            for (int i = 0; i < line.text.Length; i++)
            {
                currentTextComponent.text += line.text[i];
                if (line.typeSound != null && line.text[i] != ' ')
                    PlayTypeSound(line.typeSound);
                yield return new WaitForSeconds(line.textSpeed);
            }
        }

        currentState = DialogueState.Active;
        line.onEndLine?.Invoke();
    }

    private void SetupSpeechBubbles(BubblePosition position)
    {
        if (leftSpeechBubble != null) leftSpeechBubble.SetActive(position == BubblePosition.Left);
        if (rightSpeechBubble != null) rightSpeechBubble.SetActive(position == BubblePosition.Right);
    }

    private TextMeshProUGUI GetTextComponentForBubble(BubblePosition position)
    {
        return position == BubblePosition.Left ? leftCharacterText : rightCharacterText;
    }

    private TextMeshProUGUI GetNameComponentForBubble(BubblePosition position)
    {
        return position == BubblePosition.Left ? leftCharacterName : rightCharacterName;
    }

    private void CompleteCurrentLine()
    {
        TextMeshProUGUI currentTextComponent = GetTextComponentForBubble(currentBubblePosition);
        if (currentTextComponent != null) currentTextComponent.text = currentText;
        // Detener la corrutina de tipeo actual para evitar que siga escribiendo caracteres
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;
        }
        currentState = DialogueState.Active;
    }

    private void EndDialogue()
    {
        currentState = DialogueState.Inactive;
        dialogueCanvas.SetActive(false);

        // --- REACTIVAR TODOS LOS OBJETOS (si está permitido) ---
        if (reenableObjectsOnDialogueEnd)
        {
            EnableObjects();
        }

        currentSequence.onEndSequence?.Invoke();

        // Limpiar corutina actual
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;
        }
    }

    private void Update()
    {
        if (!HandleInput()) return;
        DisplayNextLine();
    }

    private bool HandleInput()
    {
        if (currentState != DialogueState.Active && currentState != DialogueState.Typing)
            return false;

        // Mouse input
        if (allowMouseInput && Input.GetMouseButtonDown(0))
            return true;

        // Keyboard input
        if (allowKeyboardInput && Input.GetKeyDown(advanceKey))
            return true;

        return false;
    }

    public bool IsDialogueActive => currentState == DialogueState.Active || currentState == DialogueState.Typing;
    public bool IsTyping => currentState == DialogueState.Typing;
    public DialogueState CurrentState => currentState;

    // --- MÉTODO PÚBLICO PARA OCULTAR LA UI DE COMBATE DESDE OTROS SISTEMAS ---
    public void HideCombatUI()
    {
        DisableObjects();
    }

    // --- MÉTODO PÚBLICO PARA REACTIVAR OBJETOS DESACTIVADOS POR EL DIÁLOGO ---
    public void ReactivateDialogueObjects()
    {
        EnableObjects();
    }

    // --- MÉTODO PÚBLICO PARA AÑADIR OBJETOS A DESACTIVAR DINÁMICAMENTE ---
    public void AddObjectToDisable(GameObject obj)
    {
        if (obj != null && !objectsToDisable.Contains(obj))
        {
            objectsToDisable.Add(obj);
        }
    }

    // --- MÉTODO PÚBLICO PARA REMOVER OBJETOS DE LA LISTA ---
    public void RemoveObjectToDisable(GameObject obj)
    {
        if (obj != null && objectsToDisable.Contains(obj))
        {
            objectsToDisable.Remove(obj);
        }
    }

    // --- MÉTODO PARA REPRODUCIR SONIDOS DE TIPING ---
    private void PlayTypeSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // --- MÉTODO PARA DETENER DIÁLOGO FORZADAMENTE ---
    public void ForceStopDialogue()
    {
        if (currentState != DialogueState.Inactive)
        {
            EndDialogue();
        }
    }

    // --- MÉTODO PARA PAUSAR/REANUDAR DIÁLOGO ---
    public void PauseDialogue()
    {
        if (currentState == DialogueState.Typing)
        {
            currentState = DialogueState.Paused;
        }
    }

    public void ResumeDialogue()
    {
        if (currentState == DialogueState.Paused)
        {
            currentState = DialogueState.Typing;
        }
    }
}