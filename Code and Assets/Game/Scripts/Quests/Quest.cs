using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using schoolRPG.Items;
using System.Linq;

namespace schoolRPG.Quests
{

    /// <summary>
    /// The BaseQuest class is used to Save and Load data for each Quest
    /// </summary>
    [System.Serializable]
    public class BaseQuest
    {
        protected List<BaseObjective> bObjectives;
        
        protected int objectiveAt;
        protected Guid uniqueID;

        protected bool questCompleted;
        protected bool questFailed;

        [SerializeField, Tooltip("Add a Unique ID for this quest")]
        protected string questID;
        [SerializeField]
        protected string name;
        [SerializeField]
        protected string desc;
        [SerializeField, Tooltip("What Stage is the Quest at?")]
        protected int currentStage;
        [SerializeField, Tooltip("What Stage is the Quest completed at?")]
        protected int completeStage;
        [SerializeField, Tooltip("What Stage is the Quest failed at?")]
        protected int failStage;
        [SerializeField, Tooltip("Is this Quest currently Active?")]
        protected bool questActive;
        [SerializeField, Tooltip("Is this the Quest that we are currently tracking?")]
        protected bool questSelected;
        [SerializeField, Tooltip("Should we show the popup dialogue when this quest starts, fails, or changes stages?")]
        protected bool showQuestText;

        protected void LoadObjectives(BaseQuest q)
        {
            objectiveAt = q.objectiveAt;
            uniqueID = q.uniqueID;
            questCompleted = q.questCompleted;
            questFailed = q.questFailed;
            questID = q.questID;
            name = q.name;
            desc = q.desc;
            currentStage = q.currentStage;
            completeStage = q.completeStage;
            failStage = q.failStage;
            questActive = q.questActive;
            showQuestText = q.showQuestText;
            
            // Save the Base Objectives into the Base Objectives list
            for (int i = 0; i < q.bObjectives.Count; ++i)
            {
                bObjectives[i] = q.bObjectives[i];
            }
        }

        /// <summary>
        /// Returns the BaseQuest Data ONLY!
        /// </summary>
        /// <returns></returns>
        public BaseQuest GetBase()
        {
            BaseQuest newBase = new BaseQuest();
            
            // Create a new BaseQuest so we are not passing GameObject or Monobehavior data to be Serialized
            newBase.objectiveAt = objectiveAt;
            newBase.uniqueID = uniqueID;
            newBase.questCompleted = questCompleted;
            newBase.questFailed = questFailed;
            newBase.questID = questID;
            newBase.name = name;
            newBase.desc = desc;
            newBase.currentStage = currentStage;
            newBase.completeStage = completeStage;
            newBase.failStage = failStage;
            newBase.questActive = questActive;
            newBase.showQuestText = showQuestText;

            newBase.bObjectives = bObjectives;

            return newBase;
        }
    }

    /// <summary>
    /// The script used to create a new Quest in the game
    /// </summary>
    [System.Serializable]
    public class Quest : BaseQuest
    {
        // Displays any markers for the current quest objective
        public delegate void OnObjectiveStart(Quest q);
        public static event OnObjectiveStart DisplayMarkers;

        // Hides any markers for the previous quest objective
        public delegate void OnObjectiveEnd(Quest q);
        public static event OnObjectiveEnd HideMarkers;

        private int i;
        [SerializeField, Tooltip("A List of each Objective needed to complete this Quest")]
        private List<Objective> objectives;

        [SerializeField, Tooltip("How much gold is given to the player when they complete the Quest?")]
        private double goldReward;
        [SerializeField, Tooltip("How many Attribute Points are given to the player when they complete the Quest?")]
        private int attribPointReward;
        [SerializeField, Tooltip("What Items are given to the player when they complete the Quest?")]
        private List<Item> itemReward;

        // public properties

        public int ObjectiveAt { get { return objectiveAt; } }
        public bool Completed { get { return questCompleted; } }
        public bool Failed { get { return questFailed; } }

        public string QuestID { get { return questID; } }
        public string Name { get { return name; } }
        public string Description { get { return desc; } }
        /// <summary>
        /// Adds text to the end of the Description
        /// </summary>
        public string AddDescription { set { desc = desc + " \n\n " + value; } }
        public int Stage { get { return currentStage; } }
        public int StageDone { get { return completeStage; } }
        public int StageFail { get { return failStage; } }
        public List<Objective> Objectives { get { return objectives; } }

        public bool IsActive { get { return questActive; } }
        /// <summary>
        /// If we are currently tracking this quest, Directions for the quest appear on the screen
        /// </summary>
        public bool IsTracking { get { return questSelected; } set { questSelected = value; } }
        public bool ShowQuest { get { return showQuestText; } }

        public double GoldReward { get { return goldReward; } }
        public int AttributeReward { get { return attribPointReward; } }
        public List<Item> ItemReward { get { return itemReward; } }

        public Quest()
        {
            uniqueID = Guid.NewGuid();
            objectiveAt = -1;

            // Make sure the Base Objectives are instantiated
            bObjectives = new List<BaseObjective>();
        }

        public void TrackQuest()
        {
            DisplayMarkers(this);
        }

        public void UntrackQuest()
        {
            HideMarkers(this);
        }

        /// <summary>
        /// Call this before Saving the game to Save relevant Quest Objective Data
        /// </summary>
        public void SaveData()
        {
            // Save the Quest ID if we are tracking this Quest
            if (QuestController.Tracking != null && QuestController.Tracking.QuestID == QuestID)
                WorldController.Data.TrackingQuest = QuestID;

            // Make sure the Base Objectives have the correct Size
            if (bObjectives.Count < objectives.Count)
                bObjectives.AddRange(Enumerable.Repeat(new BaseObjective(), objectives.Count));

            // Save the Base Objectives into the Base Objectives list
            for (i = 0; i < objectives.Count; ++i)
            {
                bObjectives[i] = objectives[i].GetBase();
            }
        }

        /// <summary>
        /// Call this after Loading the game data 
        /// </summary>
        /// <param name="q"></param>
        public void LoadData(BaseQuest q)
        {
            // Make sure the Base Objectives have the correct Size
            if (bObjectives.Count < objectives.Count)
                bObjectives.AddRange(Enumerable.Repeat(new BaseObjective(), objectives.Count));

            LoadObjectives(q);

            // Load the data from the Base Objective to the Main Objective
            for (i = 0; i < objectives.Count; ++i)
            {
                objectives[i].LoadData(bObjectives[i]);
            }


            // If we are loading the game where the tracking quest was saved with this quests ID, track this Quest
            if (WorldController.Data.TrackingQuest == QuestID)
                QuestController.SetTrackQuest(this);
        }

        public void SetInitialObjective()
        {
            if (objectives.Count > 0)
            {
                // Make sure we do not try to change the objectiveAt value if we have passed the first stage
                if (objectiveAt <= 0)
                    objectiveAt = 0;

                // Start the objective but only display text to screen if we are tracking this quest
                objectives[objectiveAt].StartObj(false, IsTracking);
            }
        }

        /// <summary>
        /// (FOR KILLNPC_ANY ONLY!) Checks if we should move to the next objective when we finish the kill count 
        /// </summary>
        public void CheckFinishKillCount()
        {
            if (objectives[objectiveAt].Type == ObjectiveType.KILLNPC_ANY)
            {
                if (objectives[objectiveAt].KillCountNPC <= objectives[objectiveAt].KillCountCurrent)
                    SetStage(true, false);
            }
            
        }

        /// <summary>
        /// (FOR FETCHITEM_BYNAME ONLY) Checks if the Players Inventory has any items by the name that we are looking for
        /// </summary>
        public int CheckForAnyItem(string talkName)
        {
            int i;
            int qty;
            bool foundItem = false;
            int pos = -1;
            Inventory inv;

            // If the current objective is of type Fetch Item by Name
            if (objectives[objectiveAt].Type == ObjectiveType.TALKWITH_UNIQUE)
            {
                if (PlayerController.Pc != null)
                {
                    inv = PlayerController.Pc.Player.Inventory;

                    // Loop through the players inventory
                    for (i = 0; i < inv.Count; ++i)
                    {
                        // If we found an item by this name
                        if (inv.Items[i].Name == objectives[objectiveAt].FetchItemByName && talkName == objectives[objectiveAt].TalkWithNPC.GetComponent<NpcController>().NPC.Name)
                        {
                            foundItem = true;
                            // Mark this as a Quest Item
                            //inv.Items[i].IsQuest = true;
                            pos = i;

                            // Remove the item
                            //qty = inv.Items[i].Quantity;
                            //inv.RemoveItem(inv.Items[i], ref qty);
                            // Move to the next Stage
                            SetStage(true, false);
                            break;
                        }
                    }
                }
            }

            return pos;
        }

        /// <summary>
        /// Move to this Quests next Stage
        /// </summary>
        /// <param name="next"></param>
        /// <param name="failed"></param>
        public void SetStage(bool next, bool failed)
        {
            string nextQuest = "";
            int stage = currentStage;

            // Make sure the quest is active
            if (!questActive)
            {
                PlayerController.DisplayTextToScreen("Started Quest \"" + Name + "\"!", false);
                questActive = true;
            }

            // Complete the previous objective if there was any
            if (objectiveAt > -1)
                objectives[objectiveAt].Complete(failed);

            // Fail the quest
            if (failed)
            {
                stage = failStage;
                objectiveAt = objectives.Count - 1;
                objectives[objectiveAt].StartObj(true, IsTracking);
            }
            // Move to the next Objective if possible
            else if (objectiveAt < objectives.Count - 1)
            {
                // Hide the previous markers if there are any
                if (QuestController.Tracking != null && QuestController.Tracking.questID == questID)
                    UntrackQuest();

                objectiveAt++;
                // Start the next objective and set the stage
                objectives[objectiveAt].StartObj(true, IsTracking);
                stage = objectives[objectiveAt].Stage;

                // While the next objective is already completed, advance the Quest again
                while (objectives[objectiveAt].CompleteBeforeActive && objectiveAt < objectives.Count - 1)
                {
                    objectives[objectiveAt].Complete(false);
                    objectiveAt++;
                    // Start the next objective and set the stage
                    objectives[objectiveAt].StartObj(true, IsTracking);
                    stage = objectives[objectiveAt].Stage;
                }

                // Display all quest markers for the quest at the current Stage
                if (QuestController.Tracking != null && QuestController.Tracking.questID == questID)
                    TrackQuest();
                // Display the Quest Markers if there is no other tracked quest
                else if (QuestController.Tracking == null)
                {
                    QuestController.SetTrackQuest(this);
                }
            }
            // Start the Quest
            else if (objectiveAt == -1)
            {
                objectiveAt = 0;
                objectives[0].StartObj(true, IsTracking);
                stage = objectives[objectiveAt].Stage;
            }
            else if (objectiveAt == objectives.Count - 1 && !failed)
            {
                stage = completeStage;
            }
            else if (objectiveAt == objectives.Count - 1 && failed)
            {
                stage = failStage;
            }
            

            if (stage == completeStage)
            {
                questCompleted = true;
                PlayerController.DisplayTextToScreen("Completed Quest \"" + name + "\"!", true);

                // Give the player any Attribute Point Rewards
                if (attribPointReward > 0)
                {
                    PlayerController.Pc.Player.AttributePoints += attribPointReward;
                }
                // Give the player any Gold Rewards
                if (goldReward > 0)
                {
                    PlayerController.Pc.Player.Inventory.GiveGold(GoldReward);
                }
                // Give the player any Item Rewards
                if (itemReward != null)
                {
                    for (int i = 0; i < itemReward.Count; ++i)
                    {
                        PlayerController.Pc.Player.Inventory.AddItem(itemReward[i].item);
                    }
                }

                // Stop tracking the Quest if we have completed the Quest
                if (QuestController.Tracking != null && QuestController.Tracking.questID == questID)
                    QuestController.SetTrackQuest();

                // Start the next quest in the questline if there is any
                nextQuest = QuestController.SetNextQuestline(this);

                if (nextQuest != "")
                    PlayerController.DisplayTextToScreen("Started Quest \"" + nextQuest + "\"!", false);
            }
            else if (stage == failStage)
            {
                questFailed = true;
                PlayerController.DisplayTextToScreen("<color=red>Failed Quest \"" + name + "\"...</color>", false);

                // Stop tracking the Quest if we have failed the Quest
                if (QuestController.Tracking != null && QuestController.Tracking.questID == questID)
                    QuestController.SetTrackQuest();
            }

            if (questCompleted || questFailed)
            {
                questActive = false;
            }

            currentStage = stage;
        }
        
    }
}


