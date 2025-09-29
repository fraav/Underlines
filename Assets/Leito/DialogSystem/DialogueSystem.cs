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
    
    [Header("Objects to Disable During Dialogue")]
    public GameObject[] objectsToDisable;
    private List<GameObject> objectsToReenable = new List<GameObject>();
    
    private Queue<DialogueLine> currentDialogueLines;
    private DialogueSequence currentSequence;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private string currentText = "";
    private BubblePosition currentBubblePosition;
    
    public static DialogueSystem Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        dialogueCanvas.SetActive(false);
        currentDialogueLines = new Queue<DialogueLine>();
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
        if (isDialogueActive) return;
        
        currentSequence = sequence;
        currentDialogueLines.Clear();
        
        foreach (DialogueLine line in sequence.dialogueLines)
        {
            currentDialogueLines.Enqueue(line);
        }
        
        // ►►► DESACTIVAR OBJETOS ◄◄◄
        DisableObjects();
        
        dialogueCanvas.SetActive(true);
        isDialogueActive = true;
        sequence.onStartSequence?.Invoke();
        DisplayNextLine();
    }
    
    // ►►► MÉTODO SIMPLIFICADO PARA DESACTIVAR OBJETOS ◄◄◄
    private void DisableObjects()
    {
        objectsToReenable.Clear();
        
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
            {
                // Guardar todos los objetos para reactivarlos después
                objectsToReenable.Add(obj);
                
                // Desactivar el objeto (sin importar si ya estaba desactivado)
                obj.SetActive(false);
            }
        }
    }
    
    // ►►► MÉTODO SIMPLIFICADO PARA REACTIVAR OBJETOS ◄◄◄
    private void EnableObjects()
    {
        foreach (GameObject obj in objectsToReenable)
        {
            if (obj != null)
            {
                // ►►► ACTIVAR TODOS LOS OBJETOS ◄◄◄
                obj.SetActive(true);
            }
        }
        objectsToReenable.Clear();
    }
    
    public void DisplayNextLine()
    {
        if (isTyping)
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
        StartCoroutine(TypeLine(line));
    }
    
    private IEnumerator TypeLine(DialogueLine line)
    {
        isTyping = true;
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
                    AudioSource.PlayClipAtPoint(line.typeSound, Camera.main.transform.position);
                yield return new WaitForSeconds(line.textSpeed);
            }
        }
        
        isTyping = false;
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
        isTyping = false;
    }
    
    private void EndDialogue()
    {
        isDialogueActive = false;
        dialogueCanvas.SetActive(false);
        
        // ►►► REACTIVAR TODOS LOS OBJETOS ◄◄◄
        EnableObjects();
        
        currentSequence.onEndSequence?.Invoke();
    }
    
    private void Update()
    {
        if (isDialogueActive && Input.GetMouseButtonDown(0))
        {
            DisplayNextLine();
        }
    }
    
    public bool IsDialogueActive => isDialogueActive;
    public bool IsTyping => isTyping;
    
    // ►►► MÉTODO PÚBLICO PARA AÑADIR OBJETOS A DESACTIVAR DINÁMICAMENTE ◄◄◄
    public void AddObjectToDisable(GameObject obj)
    {
        if (obj != null && !objectsToDisable.Contains(obj))
        {
            var list = new List<GameObject>(objectsToDisable);
            list.Add(obj);
            objectsToDisable = list.ToArray();
        }
    }
    
    // ►►► MÉTODO PÚBLICO PARA REMOVER OBJETOS DE LA LISTA ◄◄◄
    public void RemoveObjectToDisable(GameObject obj)
    {
        if (obj != null && objectsToDisable.Contains(obj))
        {
            var list = new List<GameObject>(objectsToDisable);
            list.Remove(obj);
            objectsToDisable = list.ToArray();
        }
    }
}