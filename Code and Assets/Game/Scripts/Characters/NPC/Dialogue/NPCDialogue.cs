using schoolRPG.Items;
using schoolRPG.Quests;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace schoolRPG.Dialogue
{
    /// <summary>
    /// Each type of Skill that can alter the outcome of a Skill Check Dialogue
    /// </summary>
    public enum SkillCheck
    {
        NONE, SPEECH, BARTER, SWORDS, SPELLS, ARMOR, UNARMORED, ATHLETICS
    }

    /// <summary>
    /// A Class that will be used when initiating Dialogue with an NPC
    /// </summary>
    [System.Serializable]
    public class Greeting
    {
        [SerializeField, Tooltip("The text that will display when initially talking with an NPC")]
        private string text;
        [SerializeField, Tooltip("A List of options to display when Greeting the Player")]
        private List<DialogueOption> options;

        public string Greet { get { return text; } }
        public List<DialogueOption> Options { get { return options; } }
    }

    /// <summary>
    /// A Class that will be used to store Dialogue and Dialogue Trees for the game
    /// </summary>
    [System.Serializable]
    public class Dialogue
    {
        [SerializeField, Tooltip("A Unique Identifier used to identify this Dialogue Option")]
        private string id;
        [SerializeField, Tooltip("The text that will display on screen")]
        private string dText;
        [SerializeField, Tooltip("Options that the player can say in response to the NPC")]
        private List<DialogueOption> options;
        [SerializeField, Tooltip("When the current Dialogue plays, the \"Add To Greet\" options will be added to the Greeting Dialogue Options")]
        private List<DialogueOption> addToGreet;
        

        // Quest Related Dialogue
        [SerializeField, Tooltip("Select this if this Dialogue will lead to completing a Quest Objective")]
        private bool isQuestInfo;
        [SerializeField, Tooltip("The Quest ID of the Quest we will update")]
        private string questID;

        public string ID { get { return id; } }
        public string Response { get { return dText; } }
        public List<DialogueOption> Options { get { return options; } }
        /// <summary>
        /// Options that will be Added to the Greeting Dialogue Options
        /// </summary>
        public List<DialogueOption> AddToGreet { get { return addToGreet; } }
        public bool IsQuestInfo { get { return isQuestInfo; } }
        public string QuestID { get { return questID; } }
    }
    /// <summary>
    /// A Class that will hold a set of Options the player can choose between when responding to an NPC
    /// </summary>
    [System.Serializable]
    public class DialogueOption
    {
        [SerializeField, Tooltip("A Text Option the player has")]
        private string option;
        [SerializeField, Tooltip("A Dialogue ID that will play after this option has been selected")]
        private string nextID;

        // One-Time-Use Options
        [SerializeField, Tooltip("Set this to True if this Option should only appear one time")]
        private bool isOneTime;
        private bool usedOneTime;

        // Quest Related Options
        [SerializeField, Tooltip("Does this option only display at a certain point during a Quest?")]
        private bool isQuestOption;
        [SerializeField, Tooltip("Does this option Advance the quest to the next Stage? Or does it just unlock during the quest?")]
        private bool advancesQuest;
        [SerializeField, Tooltip("Does this option Fail the current Objective?")]
        private bool failsObjective;
        [SerializeField, Tooltip("The ID of the Quest that this option is for")]
        private string questID;
        [SerializeField, Tooltip("The Quest will only advance if the Quest is at this Stage")]
        private int stageFor;
        [SerializeField, Tooltip("The Stage that the Quest needs to be at before this option will appear")]
        private int questStage;
        [SerializeField, Tooltip("The Stage that this Option will stop being available after. \n\n (Leave at 0 to always have this option open after the quest)")]
        private int questStageClose;
        [SerializeField, Tooltip("This option will add some text to the Quest Description once it has been triggered")]
        private string addQuestDescription;

        // Skill Check related Options
        [SerializeField, Tooltip("Does this Dialogue Option require the player know a Skill?")]
        private SkillCheck skillRequire;
        [SerializeField, Tooltip("How difficult is the skill check? (Relates to Skill Level)")]
        private int skillDiff;
        [SerializeField, Tooltip("Enter the Dialogue ID that we should play if we fail the speech check")]
        private string failID;

        // Merchant Related Options
        [SerializeField, Tooltip("Does this option initiate the Buy/Sell menu for a Merchant?")]
        private bool startsMerchant;
        [SerializeField, Tooltip("Will this be a Free Trade? Or a Buy/Sell Trade?")]
        private bool freeMerchant;

        // End Dialogue
        [SerializeField, Tooltip("Does this option end the Dialogue?")]
        private bool endsDialogue;
        [SerializeField, Tooltip("Does this EndDialogue option go at the end of the list?")]
        private bool lastOption;
        //[SerializeField, Tooltip("Does this Dialogue Option take an item from the Players Inventory?")]
        //private bool takeItem;
        //[SerializeField, Tooltip("The ID of the Item that we will remove")]
        //private string itemID;
        //[SerializeField, Tooltip("The Quantity of the Item that we will remove")]
        //private int itemQTY;

        [SerializeField, Tooltip("Should we use the NPCType below?")]
        private bool useType;
        [SerializeField, Tooltip("Changes the NPC's Opinion of others")]
        private NPCType type;


        public string OptionText { get { return option; } }
        public string NextID { get { return nextID; } }
        public string FailID { get { return failID; } }

        // One-Time-Use
        public bool IsOneTime { get { return isOneTime; } }
        public bool UsedOneTime { get { return usedOneTime; } set { usedOneTime = value; } }

        // Quest Related 
        public bool IsQuestOption { get { return isQuestOption; } }
        public bool AdvancesQuest { get { return advancesQuest; } }
        public bool FailObjective { get { return failsObjective; } }
        public string QuestID { get { return questID; } }
        public int QuestStage { get { return stageFor; } }
        public int StageUnlock { get { return questStage; } }
        public int QuestStageClose { get { return questStageClose; } }
        public string AddQuestDesc { get { return addQuestDescription; } }

        // Skill Check Related
        public SkillCheck SkillRequire { get { return skillRequire; } }
        public int SkillDifficulty { get { return skillDiff; } }

        // Merchant Related
        public bool StartsMerchant { get { return startsMerchant; } }
        public bool FreeTrade { get { return freeMerchant; } }

        // End Dialogue
        public bool EndsDialogue { get { return endsDialogue; } }
        public bool LastOption { get { return lastOption; } }

        // Take Item
        //public bool TakeItem { get { return takeItem; } }
        //public string ItemID { get { return itemID; } }
        //public int ItemQTY { get { return itemQTY; } }

        // Change Opinion
        public bool ChangeOpinion { get { return useType; } }
        public NPCType NewOpinion { get { return type; } }
    }

    /// <summary>
    /// The NPCDialogue script will hold all of the Dialogue options for the NPC it is attached to
    /// </summary>
    public class NPCDialogue : MonoBehaviour
    {
        private int pos;

        [SerializeField, Tooltip("Add the Greeting to the Player")]
        private Greeting npcGreet;

        [SerializeField, Tooltip("Add any NPC Dialogue for the Player")]
        private List<Dialogue> npcSpeak;

        private List<Dialogue> activeDialogue;

        /// <summary>
        /// The Greeting this NPC has for the player
        /// </summary>
        public Greeting Greet { get { return npcGreet; } }
        /// <summary>
        /// A List that stores each Dialogue Option for this NPC
        /// </summary>
        public List<Dialogue> DialogueList { get { return npcSpeak; } }


        // Use this for initialization
        void Start()
        {

        }

        /// <summary>
        /// Call this when you want to Load dialogue while Loading the game
        /// </summary>
        /// <param name="g"></param>
        /// <param name="d"></param>
        public void LoadDialogue(Greeting g, List<Dialogue> d)
        {
            npcGreet = g;
            npcSpeak = d;
        }
        
        /// <summary>
        /// Returns a list of Dialogue Options for the Greeting
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public List<DialogueOption> GetDialogueOptions(Greeting g)
        {
            return GetDialogueOptions(g.Options);
        }

        /// <summary>
        /// Returns a list of Dialogue Options for the Dialogue
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public List<DialogueOption> GetDialogueOptions(Dialogue d)
        {
            return GetDialogueOptions(d.Options);
        }

        /// <summary>
        /// Returns a list of Dialogue Options that are currently available
        /// </summary>
        /// <returns></returns>
        private List<DialogueOption> GetDialogueOptions(List<DialogueOption> Options)
        {
            List<DialogueOption> endDialogue = new List<DialogueOption>();
            List<DialogueOption> active = new List<DialogueOption>();
            Quest q;

            for (int i = 0; i < Options.Count; ++i)
            {
                // If this is a Use-One-Time option
                if (Options[i].IsOneTime)
                {
                    // Only add if is hasn't been used yet
                    if (!Options[i].UsedOneTime)
                        active.Add(Options[i]);
                }
                // If this is a quest option
                else if (Options[i].IsQuestOption)
                {
                    // Get the Quest this dialogue option is related to
                    q = QuestController.GetQuestForDialogue(Options[i].QuestID);

                    // If we returned a quest
                    if (q != null)
                    {
                        // If we have a Stage to close 
                        if (Options[i].QuestStageClose > 0)
                        {
                            // Add if the quest is between the two quest stages
                            if (q.Stage >= Options[i].QuestStage && q.Stage < Options[i].QuestStageClose)
                                active.Add(Options[i]);
                        }
                        else // If this options is never closed after the related Quest Stage
                        {
                            // Add if the quest is at least at the quest stage or the quest has been failed/completed
                            if (q.Stage >= Options[i].QuestStage || q.Completed || q.Failed)
                                active.Add(Options[i]);
                        }
                        
                    }
                }
                // If this is a Merchant option
                else if (Options[i].StartsMerchant)
                {
                    // Only add if the NPC has a Merchant Controller attached
                    if (gameObject.GetComponent<MerchantController>() != null)
                        active.Add(Options[i]);
                }
                // If this option ends the conversation and it is the last option
                else if (Options[i].EndsDialogue && Options[i].LastOption)
                {
                    endDialogue.Add(Options[i]);
                }
                else
                {
                    // Add the rest of the Options to the list
                    active.Add(Options[i]);
                }
            }

            // Move all of the End Conversation positions to the end of the List
            for (int i = 0; i < endDialogue.Count; ++i)
            {
                active.Add(endDialogue[i]);
            }

            return active;
        }

        
        /// <summary>
        /// Gets the next Dialogue based on the Option we selected
        /// </summary>
        /// <param name="nextID">The ID of the next Dialogue Text and Options</param>
        /// <returns></returns>
        public int GetNextDialogue(DialogueOption option, bool failID)
        {
            int next = 0;

            for (int i = 0; i < npcSpeak.Count; ++i)
            {
                // If this dialogue has the same ID as the speak item
                if (!failID && npcSpeak[i].ID == option.NextID)
                {
                    next = i;
                    break;
                }
                // If we failed a Speech Check and the speach has the failed ID
                else if (failID && npcSpeak[i].ID == option.FailID)
                {
                    next = i;
                    break;
                }
            }

            return next;
        }

    }
}

