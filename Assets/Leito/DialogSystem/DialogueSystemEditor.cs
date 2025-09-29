#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

[CustomEditor(typeof(DialogueSystem))]
public class DialogueSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DialogueSystem dialogueSystem = (DialogueSystem)target;
        
        // Draw default properties
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("TMP Utilities", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Auto-Assign TMP Components"))
        {
            AutoAssignTMPComponents(dialogueSystem);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dialogue Management", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Add New Dialogue Sequence"))
        {
            dialogueSystem.dialogueSequences.Add(new DialogueSequence());
        }
        
        EditorGUILayout.Space();
        
        // Quick test buttons for each sequence
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Quick Test:", EditorStyles.boldLabel);
            foreach (var sequence in dialogueSystem.dialogueSequences)
            {
                if (GUILayout.Button($"Test: {sequence.sequenceName}"))
                {
                    dialogueSystem.StartDialogue(sequence.sequenceName);
                }
            }
        }
    }
    
    private void AutoAssignTMPComponents(DialogueSystem system)
    {
        if (system.dialogueCanvas != null)
        {
            // Buscar componentes TMP automáticamente
            TextMeshProUGUI[] tmpComponents = system.dialogueCanvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            
            foreach (TextMeshProUGUI tmp in tmpComponents)
            {
                if (tmp.gameObject.name.Contains("Left") && tmp.gameObject.name.Contains("Name"))
                    system.leftCharacterName = tmp;
                else if (tmp.gameObject.name.Contains("Left") && tmp.gameObject.name.Contains("Text"))
                    system.leftCharacterText = tmp;
                else if (tmp.gameObject.name.Contains("Right") && tmp.gameObject.name.Contains("Name"))
                    system.rightCharacterName = tmp;
                else if (tmp.gameObject.name.Contains("Right") && tmp.gameObject.name.Contains("Text"))
                    system.rightCharacterText = tmp;
            }
            
            // Buscar speech bubbles
            Transform leftBubble = system.dialogueCanvas.transform.Find("LeftSpeechBubble");
            Transform rightBubble = system.dialogueCanvas.transform.Find("RightSpeechBubble");
            
            if (leftBubble != null) system.leftSpeechBubble = leftBubble.gameObject;
            if (rightBubble != null) system.rightSpeechBubble = rightBubble.gameObject;
            
            EditorUtility.SetDirty(system);
        }
    }
}
#endif