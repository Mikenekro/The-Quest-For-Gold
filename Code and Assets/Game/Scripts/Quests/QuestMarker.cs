using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using schoolRPG.Items;
using System;

namespace schoolRPG.Quests
{

    /// <summary>
    /// The Marker class holds each Quest Marker for the gameObject that ths Quest Marker is attached to
    /// </summary>
    [System.Serializable]
    public class Marker
    {
        [SerializeField]
        public Quest quest;
        [SerializeField]
        public Objective obj;

        [SerializeField]
        public int qPos;
        [SerializeField]
        public int oPos;

        [SerializeField]
        public string questID;
        [SerializeField]
        public ObjectiveType type;
        [SerializeField]
        public string mType;
    }

    /// <summary>
    /// Place this on an Item that will be used in a Quest
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D)), System.Serializable]
    public class QuestMarker : MonoBehaviour
    {
        private static List<GameObject> trackers;

        private static GameObject marker;

        private bool displaying;
        private Guid uniqueID;

        public bool fold;

        public QuestMarker qm;
        /// <summary>
        /// List of each Quest Marker for this GameObject
        /// </summary>
        public List<Marker> markers;
        /// <summary>
        /// List of each currently Active Marker Position in the markers list
        /// </summary>
        public List<int> activeMarkers;

        
        public Quest quest;
        public Objective obj;

        public GameObject questMarkerSprite;
        public GameObject qmWorld;
        
        public int questPos;
        public int objPos;
        /// <summary>
        /// The ID of the Quest this Marker is for
        /// </summary>
        public string questID;

        /// <summary>
        /// The type of Objective this will be
        /// </summary>
        public ObjectiveType type;

        public void Awake()
        {
            // Register for the OnObjectiveStart Delegate
            Quest.DisplayMarkers += Display;
            Quest.HideMarkers += Hide;

            if (marker == null)
                marker = GameObject.Find("BaseQuestArrow");

            if (questMarkerSprite == null)
                questMarkerSprite = marker;


            if (questMarkerSprite.activeInHierarchy)
                questMarkerSprite.SetActive(false);

            if (trackers == null)
                trackers = new List<GameObject>();

            uniqueID = Guid.NewGuid();

            CheckMarker();

        }

        public void OnDestroy()
        {
            // Make sure we remove the Quest Marker when we destroy the gameobject
            if (displaying)
                Hide(QuestController.Quests[questPos]);

            // Un-Register for the OnObjectiveStart Delegate
            Quest.DisplayMarkers -= Display;
            Quest.HideMarkers -= Hide;
        }


        public void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.tag == "Player")
            {
                // If this is a location type and the quest is at the objectives stage
                if (type == ObjectiveType.LOCATION && QuestController.Quests[questPos].Stage == obj.Stage)
                {
                    Debug.Log("Arrived At Location");

                    QuestController.Quests[questPos].SetStage(true, false);
                }
            }
        }

        /// <summary>
        /// Display the Quest Marker
        /// </summary>
        /// <param name="q"></param>
        public void Display(Quest q)
        {
            int i;
            GameObject obj;
            LookToObjective lto;

            for (i = 0; i < markers.Count; ++i)
            {
                if (markers[i].questID == q.QuestID)
                {
                    quest = markers[i].quest;
                    objPos = markers[i].oPos;
                    questPos = markers[i].qPos;
                }
            }

            if (q.IsActive && q.QuestID == quest.QuestID && q.ObjectiveAt == objPos)
            {
                if (qmWorld == null)
                {
                    qmWorld = Instantiate(questMarkerSprite, gameObject.transform.parent, true);
                    qmWorld.transform.SetParent(gameObject.transform);
                    qmWorld.transform.localPosition = new Vector3(0.0f, 0.35f, 0.0f);
                    qmWorld.GetComponent<RectTransform>().localScale = new Vector3(0.16f, 0.16f, 1.0f);
                    qmWorld.name = "QuestMarker";
                    qmWorld.SetActive(true);
                }
                qmWorld.SetActive(true);
                
                PlayerController.Pc.QuestTracker.SetActive(true);
                obj = Instantiate(PlayerController.Pc.QuestTracker, PlayerController.Pc.QuestTracker.transform.parent, true);
                obj.name = "ObjectiveTracker";
                obj.SetActive(true);
                lto = obj.GetComponent<LookToObjective>();
                lto.SetLookAt(transform);
                lto.QuestID = q.QuestID;
                lto.UniqueID = uniqueID;
                trackers.Add(obj);
                PlayerController.Pc.QuestTracker.SetActive(false);
                displaying = true;
            }
        }

        /// <summary>
        /// Hide the Quest Marker
        /// </summary>
        /// <param name="q"></param>
        public void Hide(Quest q)
        {
            int i;

            if (q != null && q.IsActive && q.QuestID == quest.QuestID)
            {
                if (qmWorld != null)
                    qmWorld.SetActive(false);

                for (i = 0; i < trackers.Count; ++i)
                {
                    if (trackers[i].GetComponent<LookToObjective>().QuestID == q.QuestID && trackers[i].GetComponent<LookToObjective>().UniqueID == uniqueID)
                    {
                        Destroy(trackers[i]);

                        // Make sure to remove the tracker from the list
                        trackers.RemoveAt(i);
                        displaying = false;
                    }
                }
            }
        }


        public void CheckMarker()
        {
            if (type == ObjectiveType.LOCATION)
            {

            }
            else if (type == ObjectiveType.FETCHITEM_UNIQUE && GetComponent<Item>() != null)
            {
                // Make sure the Fetch Unique Items will always be Quest Items
                GetComponent<Item>().SetQuestItem(true);
            }
        }
        
    }
}

