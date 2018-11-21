using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using schoolRPG.Items;
using System;

namespace schoolRPG.Quests
{
    /// <summary>
    /// What are the Types of Objectives that the Player can complete?
    /// </summary>
    public enum ObjectiveType
    {
        FETCHITEM_UNIQUE, FETCHITEM_BYNAME, KILLNPC_UNIQUE, KILLNPC_ANY, TALKWITH_UNIQUE, TALKWITH_ANY, LOCATION
    }

    /// <summary>
    /// All the data that we can save will be stored in BaseObjective
    /// </summary>
    [System.Serializable]
    public class BaseObjective
    {
        protected Guid uniqueID;

        protected bool complete;
        protected bool passed;
        protected bool failed;

        [SerializeField, Tooltip("What text will be displayed on the screen when this objective is activated?")]
        protected string objDisp;
        [SerializeField, Tooltip("What will the text in the Quest Journal say?")]
        protected string objDesc;
        [SerializeField, Tooltip("Will reaching this objective automatically unlock a Quest?")]
        protected string startsQuest;
        [SerializeField, Tooltip("Is this Objective currently active?")]
        protected bool isActive;
        [SerializeField, Tooltip("What stage does this Objective become active at?")]
        protected int stageUnlock;
        [SerializeField, Tooltip("What type of objective is this?")]
        protected ObjectiveType type;

        protected int curKillCount;

        
        
        /// <summary>
        /// Was this Objective completed before being activated?
        /// </summary>
        public bool CompleteBeforeActive { get; set; }

        /// <summary>
        /// Loads the Data into this class when we are Loading a game
        /// </summary>
        /// <param name="obj"></param>
        public void LoadData(BaseObjective obj)
        {
            uniqueID = obj.uniqueID;
            complete = obj.complete;
            passed = obj.passed;
            failed = obj.failed;
            objDisp = obj.objDisp;
            objDesc = obj.objDesc;
            isActive = obj.isActive;
            stageUnlock = obj.stageUnlock;
            type = obj.type;
            curKillCount = obj.curKillCount;
            CompleteBeforeActive = obj.CompleteBeforeActive;
        }

        /// <summary>
        /// Returns the BaseObjective Data ONLY!
        /// </summary>
        /// <returns></returns>
        public BaseObjective GetBase()
        {
            BaseObjective newBase = new BaseObjective();

            // Create a new BaseObjective object so we are not passing GameObject or Monobehavior data to be serialized
            newBase.uniqueID = uniqueID;
            newBase.complete = complete;
            newBase.passed = passed;
            newBase.failed = failed;
            newBase.objDisp = objDisp;
            newBase.objDesc = objDesc;
            newBase.isActive = isActive;
            newBase.stageUnlock = stageUnlock;
            newBase.type = type;
            newBase.curKillCount = curKillCount;
            newBase.CompleteBeforeActive = CompleteBeforeActive;

            return newBase;
        }
    }

    /// <summary>
    /// What are we trying to accomplish in the current stage of the quest?
    /// </summary>
    [System.Serializable]
    public class Objective : BaseObjective
    {
        
        [SerializeField, Tooltip("(FETCHITEM_UNIQUE)What Item do we need to retrieve?")]
        private Item objItem;
        [SerializeField, Tooltip("(FETCHITEM_BYNAME)What is the name of the Item we need to retrieve?")]
        private string objItemName;
        [SerializeField, Tooltip("(KILLNPC_UNIQUE)What NPC do we need to kill?")]
        private GameObject objKillNpc;
        [SerializeField, Tooltip("(KILLNPC_ALL)How many random NPCs do we need to kill?")]
        private int objKillCount;
        [SerializeField, Tooltip("(TALKWITH_UNIQUE)Which NPC do we need to talk to for the Quest?")]
        private GameObject objNpcTalk;
        [SerializeField, Tooltip("(LOCATION)Which Location does the player need to be at for the Objective to complete")]
        private GameObject objLocation;
        [SerializeField, Tooltip("(LOCATION)The location the player needs to be at to continue the Quest")]
        private Locations objLoc;


        public Item FetchItem { get { return objItem; } }
        public string FetchItemByName { get { return objItemName; } }
        public GameObject KillUniqueNPC { get { return objKillNpc; } }
        public int KillCountNPC { get { return objKillCount; } }
        public int KillCountCurrent { get { return curKillCount; } }
        public GameObject TalkWithNPC { get { return objNpcTalk; } }
        public GameObject GoToLocation { get { return objLocation; } }
        public Locations ObjLocation { get { return objLoc; } }

        /// <summary>
        /// Unique Identifier for this Objective
        /// </summary>
        public Guid UniqueID { get { return uniqueID; } }
        /// <summary>
        /// Is this Objective active?
        /// </summary>
        public bool IsActive { get { return isActive; } }
        public bool IsCompleted { get { return complete; } }
        public bool IsPassed { get { return passed; } }
        public bool IsFailed { get { return failed; } }

        /// <summary>
        /// The Description of this Objective
        /// </summary>
        public string ObjectiveDescription { get { return objDesc; } }
        /// <summary>
        /// Text to display on the screen when the objective is activated
        /// </summary>
        public string ObjectiveDisplay { get { return objDisp; } }
        /// <summary>
        /// The Stage that this Objective becomes active. Must be between 1 and 1000
        /// </summary>
        public int Stage { get { return stageUnlock; } }
        /// <summary>
        /// What Type of Objective is this?
        /// </summary>
        public ObjectiveType Type { get { return type; } }

        /// <summary>
        /// What quest does this Objective start? If any
        /// </summary>
        public string StartQuest { get { return startsQuest; } }
        
        public Objective()
        {
            uniqueID = Guid.NewGuid();
            if (stageUnlock <= 0)
                stageUnlock = 1;
            if (stageUnlock > 1000)
                stageUnlock = 1000;
        }


        /// <summary>
        /// Starts the Objective
        /// <paramref name="first">Is this the first time this objective has been started?</paramref>
        /// <paramref name="tracking">Are we tracking the Objectives related Quest?</paramref>
        /// </summary>
        public void StartObj(bool first, bool tracking)
        {
            isActive = true;

            if (objDisp.Trim() != "" && (first || tracking))
                PlayerController.DisplayTextToScreen(objDisp, false);

            // If this objective will automatically start a quest, trigger is now
            if (StartQuest != null && StartQuest != "")
            {
                for (int i = 0; i < QuestController.Quests.Count; ++i)
                {
                    if (QuestController.Quests[i].QuestID == StartQuest)
                    {
                        QuestController.Quests[i].SetStage(true, false);
                    }
                }
            }
        }

        /// <summary>
        /// Complete this Objective
        /// </summary>
        /// <param name="failedObjective">Did we fail the Objective?</param>
        public void Complete(bool failedObjective)
        {
            isActive = false;
            complete = true;

            passed = !failedObjective;
            failed = failedObjective;

            if (failed)
                PlayerController.DisplayTextToScreen("<color=red>Failed Objective!</color>", false);
            else
                PlayerController.DisplayTextToScreen("Completed: " + objDisp, true);
        }

        /// <summary>
        /// Kills another NPC for KILLNPC_ALL
        /// </summary>
        /// <param name="killed"></param>
        public void KillAny(bool killed)
        {
            if (killed)
                curKillCount++;
        }
    }
}
