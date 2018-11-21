using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

using schoolRPG.Quests;

[CustomEditor(typeof(QuestMarker))]
public class QuestMarkerEditor : Editor
{
    bool setValues = false;
    
    string[] choices;
    Quest[] quests;
    string[] questObjs;
    int _choiceIndex;
    int _objIndex;
    string MarkType;

    QuestMarker qm;
    Quest q;

    //GameObject marker;
    int size;
    bool fold;
    //List<bool> fold1;
    //List<Marker> markers;
    //List<int> objSize;
    //List<List<string>> qObjectives;
    //List<string> markType;

    public void Awake()
    {
        OnEnable();
    }

    public void OnEnable()
    {
        qm = (QuestMarker)target;

        // Make sure the Quests are set
        if (QuestController.Quests == null && GameObject.Find("Quests") != null)
        {
            QuestController.SetStatic(GameObject.Find("Quests").GetComponent<QuestController>());
        }

        if (choices == null)
        {
            fold = false;
            //markers = new List<Marker>();
            //objSize = new List<int>();
            //markType = new List<string>(); 
            //qObjectives = new List<List<string>>();
            //markType = new List<string>();
            choices = new string[QuestController.Quests.Count];
            quests = new Quest[choices.Length];
        }

        // Add each Quest ID and Quest Name to the list
        for (int i = 0; i < QuestController.Quests.Count; ++i)
        {
            choices[i] = QuestController.Quests[i].QuestID + "_" + QuestController.Quests[i].Name;
            quests[i] = QuestController.Quests[i];
        }
        
        _choiceIndex = qm.questPos;
        _objIndex = qm.objPos;
        qm.questID = choices[_choiceIndex].Split('_')[0];
        qm.questPos = _choiceIndex;
        q = quests[_choiceIndex];
        qm.quest = q;

        qm.obj = q.Objectives[_objIndex];
        qm.objPos = _objIndex;
        qm.type = qm.obj.Type;
        fold = qm.fold;

        MarkType = GetMarkType(qm.obj.Type, qm.obj);
    }

    bool fold2;

    public override void OnInspectorGUI()
    {
        OnEnable();
        // Draw the default inspector
        //DrawDefaultInspector();

        EditorGUI.indentLevel = 0;

        //EditorGUILayout.BeginHorizontal();
        //marker = (GameObject)EditorGUILayout.ObjectField("Marker Object: ", marker, typeof(GameObject), true, null);
        //EditorGUILayout.EndHorizontal();


        ActiveMarker();

        if (GUILayout.Button("Save Settings"))
        {
            qm.questID = choices[_choiceIndex].Split('_')[0];
            qm.questPos = _choiceIndex;
            q = quests[_choiceIndex];
            qm.quest = q;

            qm.obj = q.Objectives[_objIndex];
            qm.objPos = _objIndex;
            qm.type = qm.obj.Type;
            qm.fold = fold;

            // Save the changes back to the object
            EditorUtility.SetDirty(target);
        }


        //GUIStyle style = new GUIStyle();
        //style.padding = new RectOffset(35, 0, 0, 0);
        //fold = EditorGUILayout.Foldout(fold, "All Quest Markers", true);
        //if (fold)
        //{
        //    //if (qm.markers != null)
        //    //    qm.markers.Clear();
        //    //if (markers != null)
        //    //    markers.Clear();
        //    if (qObjectives != null)
        //        qObjectives.Clear();
        //    //if (objSize != null)
        //    //    objSize.Clear();

        //    EditorGUI.indentLevel = 1;

        //    size = EditorGUILayout.IntField("Size: ", size);

        //    while (fold1.Count < size)
        //        fold1.Add(false);
        //    while (fold1.Count > size)
        //        fold1.RemoveAt(fold1.Count - 1);

        //    while (markers.Count < size)
        //        markers.Add(new Marker());
        //    while (markers.Count > size)
        //        markers.RemoveAt(markers.Count - 1);

        //    while (qm.markers.Count < size)
        //        qm.markers.Add(new Marker());
        //    while (qm.markers.Count > size)
        //        qm.markers.RemoveAt(qm.markers.Count - 1);

        //    while (objSize.Count < size)
        //        objSize.Add(0);
        //    while (objSize.Count > size)
        //        objSize.RemoveAt(objSize.Count - 1);

        //    for (int i = 0; i < size; ++i)
        //    {
        //        if (qm.markers[i].quest != null)
        //        {
        //            markers[i] = qm.markers[i];
        //            markers[i].quest = quests[markers[i].qPos];
        //            objSize[i] = quests[markers[i].qPos].Objectives.Count;
        //            List<string> s = new List<string>();
        //            for (int j = 0; j < objSize[i]; ++j)
        //            {
        //                // Add each Objective to the list
        //                s.Add("Stage: " + qm.markers[i].quest.Objectives[j].Stage + ", Display:" + qm.markers[i].quest.Objectives[j].ObjectiveDisplay);
        //            }
        //            // Add a new list for the current Quests Objectives
        //            if (qObjectives == null)
        //                qObjectives = new List<List<string>>();
        //            qObjectives.Add(s);
        //        }

        //    }

        //    // Add each Quest Marker for every Marker we wanted to add
        //    for (int i = 0; i < size; ++i)
        //    {
        //        EditorGUILayout.BeginHorizontal();
        //        EditorGUI.indentLevel = 1;
        //        fold1[i] = EditorGUILayout.Foldout(fold1[i], "Marker " + (i + 1), true);
        //        EditorGUILayout.EndHorizontal();

        //        // Write each element for the Marker
        //        if (fold1[i])
        //        {
        //            EditorGUI.indentLevel = 2;

        //            if (markers.Count >= 1)
        //            {
        //                markers[i].qPos = qm.markers[i].qPos;

        //                markers[i].qPos = EditorGUILayout.Popup("Quest ID: ", markers[i].qPos, choices);

        //                // Update the selected choice in the underlying object
        //                if (!setValues)
        //                {
        //                    qm.markers[i] = markers[i];
        //                    //objSize[i] = quests[markers[i].qPos].Objectives.Count;
        //                }

        //                //List<string> s = new List<string>();
        //                //for (int j = 0; j < objSize[i]; ++j)
        //                //{
        //                //    // Add each Objective to the list
        //                //    s.Add("Stage: " + qm.markers[i].quest.Objectives[j].Stage + ", Display:" + qm.markers[i].quest.Objectives[j].ObjectiveDisplay);
        //                //}
        //                //// Add a new list for the current Quests Objectives
        //                //if (qObjectives == null)    
        //                //    qObjectives = new List<List<string>>();
        //                //qObjectives.Add(s);

        //                if (qm.markers.Count > i && qm.markers[i] != null)
        //                    markers[i].oPos = qm.markers[i].oPos;

        //                markers[i].oPos = EditorGUILayout.Popup("Objective ID: ", markers[i].oPos, qObjectives[i].ToArray());

        //                // set the Objective and Objective Position for this Quest Marker
        //                if (!setValues)
        //                {
        //                    qm.markers[i].oPos = markers[i].oPos;
        //                    qm.markers[i].obj = qm.markers[i].quest.Objectives[markers[i].oPos];
        //                    qm.markers[i].type = qm.markers[i].obj.Type;
        //                    qm.markers[i].mType = GetMarkType(qm.markers[i].type, qm.markers[i].obj);
        //                }

        //                EditorGUILayout.LabelField("Marker Type: ", MarkType);
        //            }
        //        }

        //    }
        //}


        //EditorGUI.indentLevel = 0;

        //EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.LabelField("Marker Type: ", MarkType);
        //EditorGUILayout.EndHorizontal();

        //EditorGUILayout.BeginHorizontal();
        //// If we attempt to save the Quest Marker
        //if (GUILayout.Button("Save Marker"))
        //{
        //    setValues = true;
        //    // Save Quest
        //    qm.questID = choices[_choiceIndex];
        //    q = quests[_choiceIndex];
        //    qm.quest = q;
        //    questObjs = new string[q.Objectives.Count];
        //    for (int i = 0; i < questObjs.Length; ++i)
        //    {
        //        questObjs[i] = "Stage: " + q.Objectives[i].Stage + " Display:" + q.Objectives[i].ObjectiveDisplay;
        //    }

        //    // Save Objective
        //    qm.obj = q.Objectives[_objIndex];
        //    qm.objPos = _objIndex;
        //    qm.type = qm.obj.Type;
        //    qm.questMarkerSprite = marker;
        //    MarkType = GetMarkType(qm.type, qm.obj);
        //    // Save the changes back to the object
        //    EditorUtility.SetDirty(target);
        //    Debug.Log("Saved Quest: " + qm.questID + ", Objective: " + qm.obj.ObjectiveDisplay);
        //}
        //EditorGUILayout.EndHorizontal();

        //// Save the changes back to the object
        //EditorUtility.SetDirty(target);
    }

    private void ActiveMarker()
    {
        fold2 = EditorGUILayout.Foldout(fold2, "Active Quest Marker", true);

        if (fold2)
        {
            EditorGUILayout.BeginHorizontal();
            _choiceIndex = qm.questPos;
            _choiceIndex = EditorGUILayout.Popup("Quest ID: ", _choiceIndex, choices);
            // Update the selected choice in the underlying object
            if (!setValues)
            {
                qm.questID = choices[_choiceIndex].Split('_')[0];
                qm.questPos = _choiceIndex;
                q = quests[_choiceIndex];
                qm.quest = q;
                questObjs = new string[q.Objectives.Count];
                for (int i = 0; i < questObjs.Length; ++i)
                {
                    questObjs[i] = "Stage: " + q.Objectives[i].Stage + " Display:" + q.Objectives[i].ObjectiveDisplay;
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _objIndex = EditorGUILayout.Popup("Objective ID: ", _objIndex, questObjs);

            // set the Objective and Objective Position for this Quest Marker
            if (!setValues)
            {
                qm.obj = q.Objectives[_objIndex];
                qm.objPos = _objIndex;
                qm.type = qm.obj.Type;
                MarkType = GetMarkType(qm.type, qm.obj);
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private string GetMarkType(ObjectiveType type, Objective o)
    {
        string ret = "";

        if (type == ObjectiveType.FETCHITEM_BYNAME)
        {
            ret = "Fetch Item (By Name)";
        }
        else if (type == ObjectiveType.FETCHITEM_UNIQUE)
        {
            ret = "Fetch Unique Item";
        }
        else if (type == ObjectiveType.KILLNPC_ANY)
        {
            ret = "Kill " + o.KillCountNPC + " Enemies";
        }
        else if (type == ObjectiveType.KILLNPC_UNIQUE)
        {
            ret = "Kill Current NPC: " + qm.gameObject.name;
        }
        else if (type == ObjectiveType.LOCATION)
        {
            ret = "Go To Location: " + qm.gameObject.transform.position;
        }
        else if (type == ObjectiveType.TALKWITH_ANY)
        {
            ret = "Talk With Any NPC";
        }
        else if (type == ObjectiveType.TALKWITH_UNIQUE)
        {
            ret = "Talk With Unique NPC";
        }

        return ret;
    }
}
