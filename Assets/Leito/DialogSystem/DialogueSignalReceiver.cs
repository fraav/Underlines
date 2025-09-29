using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class DialogueSignalReceiver : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public string dialogueSequenceName;
    public PlayableDirector timelineDirector;
    
    private bool dialogueCompleted = false;
    
    public void OnDialogueSignal()
    {
        Debug.Log("Signal recibido - Iniciando diálogo y pausando timeline");
        
        // Pausar la timeline
        if (timelineDirector != null)
        {
            timelineDirector.Pause();
        }
        
        // Iniciar el diálogo
        if (DialogueSystem.Instance != null && !string.IsNullOrEmpty(dialogueSequenceName))
        {
            DialogueSystem.Instance.StartDialogue(dialogueSequenceName);
            StartCoroutine(WaitForDialogueCompletion());
        }
        else
        {
            // Si no hay diálogo, continuar la timeline
            ResumeTimeline();
        }
    }
    
    private System.Collections.IEnumerator WaitForDialogueCompletion()
    {
        // Esperar a que el diálogo termine
        yield return new WaitWhile(() => DialogueSystem.Instance.IsDialogueActive);
        
        Debug.Log("Diálogo completado - Reanudando timeline");
        ResumeTimeline();
    }
    
    private void ResumeTimeline()
    {
        if (timelineDirector != null)
        {
            timelineDirector.Resume();
        }
    }
}