using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueSystem dialogueSystem;
    public string dialogueSequenceName;
    public bool triggerOnStart = false;
    public bool triggerOnCollision = false;
    public bool triggerOnClick = false;
    
    private void Start()
    {
        if (triggerOnStart)
        {
            TriggerDialogue();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnCollision && other != null && other.CompareTag("Player"))
        {
            TriggerDialogue();
        }
    }
    
    private void OnMouseDown()
    {
        if (triggerOnClick)
        {
            TriggerDialogue();
        }
    }
    
    public void TriggerDialogue()
    {
        if (string.IsNullOrEmpty(dialogueSequenceName))
        {
            Debug.LogError("[DialogueTrigger] dialogueSequenceName is null or empty!");
            return;
        }
        
        // Usar el DialogueSystem asignado por Inspector; si está vacío, intentar buscar uno en la escena
        DialogueSystem targetSystem = dialogueSystem != null ? dialogueSystem : FindObjectOfType<DialogueSystem>();
        
        if (targetSystem == null)
        {
            Debug.LogError($"[DialogueTrigger] No DialogueSystem found for sequence '{dialogueSequenceName}'!");
            return;
        }
        
        targetSystem.StartDialogue(dialogueSequenceName);
    }
}