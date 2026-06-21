using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class DialogueSignalReceiver : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public string dialogueSequenceName;
    public PlayableDirector timelineDirector;
    [Tooltip("Referencia al DialogueSystem de esta escena")]
    public DialogueSystem dialogueSystem;

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
        if (dialogueSystem != null && !string.IsNullOrEmpty(dialogueSequenceName))
        {
            dialogueSystem.StartDialogue(dialogueSequenceName);
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
        yield return new WaitWhile(() => dialogueSystem != null && dialogueSystem.IsDialogueActive);

        Debug.Log("diálogo completado - Reanudando timeline");
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
