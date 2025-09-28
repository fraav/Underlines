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
        if (triggerOnCollision && other.CompareTag("Player"))
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
        if (dialogueSystem != null)
        {
            dialogueSystem.StartDialogue(dialogueSequenceName);
        }
        else if (DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.StartDialogue(dialogueSequenceName);
        }
    }
}