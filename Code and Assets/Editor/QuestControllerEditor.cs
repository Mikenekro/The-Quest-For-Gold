using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using schoolRPG.Quests;

[CustomEditor(typeof(QuestController))]
public class QuestControllerEditor : Editor {

    //SerializedProperty staticQuests;
    //SerializedProperty Quests;
    
    private QuestController qc;

    // Use this for initialization
    private void OnEnable()
    {
        qc = (QuestController)target;
        // Setup the SerializedProperties.
        //staticQuests = serializedObject.FindProperty("Quests");
        //Quests = serializedObject.FindProperty("quests");
        
    }

    public override void OnInspectorGUI()
    {
        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
        serializedObject.Update();
        DrawDefaultInspector();

        QuestController.Quests = qc.forEditor;
        

        // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties();
        // Save the changes back to the object
        EditorUtility.SetDirty(target);
    }
}
