using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace schoolRPG.Quests
{
    /// <summary>
    /// Quest Controller will hold every quest in the game
    /// </summary>
    public class QuestController : MonoBehaviour
    {
        private static QuestController qc;

        private static List<Quest> active;
        private static int lastActive;
        /// <summary>
        /// A list of every Active quest in the game
        /// </summary>
        //public static List<Quest> Active { get { if (active == null || lastActive != active.Count) SetActiveQuests(); return active; } }
        /// <summary>
        /// A list of every quest in the game
        /// </summary>
        public static List<Quest> Quests;

        /// <summary>
        /// The current Quest we are tracking
        /// </summary>
        public static Quest Tracking { get; set; }

        /// <summary>
        /// Total Active Quests
        /// </summary>
        //public static int ActiveQuests {
        //    get
        //    {
        //        int c = Active.Count;
        //        lastActive = c;
        //        return c;
        //    }
        //}
        
        [SerializeField]
        private List<Quest> quests;
        

        /// <summary>
        /// ONLY FOR USE WITH THE QUEST CONTROLLER EDITOR SCRIPT
        /// </summary>
        public List<Quest> forEditor { get { return quests; } }


        // Use this for initialization
        void Awake()
        {
            if (FindObjectsOfType<QuestController>().Length > 1)
            {
                if (Application.isEditor)
                    DestroyImmediate(gameObject);
                else
                    Destroy(gameObject);
            }

            Quests = quests;
            active = new List<Quest>();
            StartCoroutine(DisplayInitialQuest());
        }

        /// <summary>
        /// Sets the Quest that we should track
        /// </summary>
        /// <param name="q"></param>
        public static void SetTrackQuest(Quest q = null)
        {
            // Call the TrackQuest delegate to display any quest markers if we want to track a quest
            if (q != null)
            {
                SetActiveQuests();

                for (int i = 0; i < active.Count; ++i)
                {
                    // Track the correct quest
                    if (active[i].QuestID == q.QuestID)
                        active[i].TrackQuest();
                }
            }
            // Otherwise, if we do not want to track a quest and we are currently tracking a quest
            else if (Tracking != null)
            {
                SetActiveQuests();

                for (int i = 0; i < active.Count; ++i)
                {
                    // Untrack the correct quest
                    if (active[i].QuestID == Tracking.QuestID)
                        active[i].UntrackQuest();
                }
            }

            Tracking = q;
        }

        /// <summary>
        /// Used to set the Static Quests in the Editor
        /// </summary>
        /// <param name="controller"></param>
        public static void SetStatic(QuestController controller)
        {
            Quests = controller.quests;
            active = new List<Quest>();
            Debug.Log("Set Static");
        }

        /// <summary>
        /// Displays the On Screen Text for the Initial Quests after a 3.0 second waiting period
        /// </summary>
        /// <returns></returns>
        public static IEnumerator DisplayInitialQuest()
        {
            int i;

            do
            {
                yield return null;
            } while (CreationMenu.Creating || WorldController.InMenu);

            for (i = 0; i < Quests.Count; ++i)
            {
                if (Quests[i].IsActive)
                {
                    Quests[i].SetInitialObjective();
                }
            }

            yield return new WaitForSeconds(3.0f);
            
            if (GetActiveQuests().Count != 0)
            {
                Debug.Log("Active Quests Count: " + GetActiveQuests().Count);
            }

            yield return null;
        }

        /// <summary>
        /// Gets all the relevant Quest Data before Saving the game
        /// </summary>
        /// <returns></returns>
        public static List<BaseQuest> GetQuestData()
        {
            int i;
            List<BaseQuest> bQuest = new List<BaseQuest>();

            for (i = 0; i < Quests.Count; ++i)
            {
                // Save the Quest Objective Data
                Quests[i].SaveData();
                // Add the current Quest to the Base Quest data
                bQuest.Add(Quests[i].GetBase());
            }

            return bQuest;
        }

        /// <summary>
        /// Sets all the relevant Quest Data while Loading the game
        /// </summary>
        /// <param name="bQuest"></param>
        public static void SetQuestData(List<BaseQuest> bQuest)
        {
            int i;

            for (i = 0; i < Quests.Count; ++i)
            {
                // Load the BaseQuest Data into the Quest object
                Quests[i].LoadData(bQuest[i]);
            }
        }

        /// <summary>
        /// Sets the Static Quests if they are not already set
        /// </summary>
        public static void SetStatic()
        {
            if (Quests == null)
            {
                if (qc == null)
                    qc = GameObject.Find("AllQuests").GetComponent<QuestController>();

                Quests = qc.quests;
            }
        }

        /// <summary>
        /// Sets the next active Quest in the Questline if there is any
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static string SetNextQuestline(Quest q)
        {
            int i;
            int qNum;
            int atNum;
            bool gotNum;
            string id;
            string nextQuest = "";
            
            // Get the base name for the Quest Series
            id = q.QuestID.Split('0')[0];
            gotNum = int.TryParse(q.QuestID.Split('0')[1], out qNum);

            for (i = 0; i < Quests.Count; ++i)
            {
                // If we found a quest with the same starting ID
                if (Quests[i].QuestID.Split('0')[0] == id)
                {
                    // If we got numbers from both quest IDs
                    if (gotNum && int.TryParse(Quests[i].QuestID.Split('0')[1], out atNum))
                    {
                        // If the current quest was finished before the next quest
                        if (qNum < atNum && Quests[i].Stage < 10)
                        {
                            // Start the quest
                            Quests[i].SetStage(true, false);
                            nextQuest = Quests[i].Name;
                            break;
                        }
                    }
                }
            }

            return nextQuest;
        }

        public static void SetQuestObjective(Quest q, Objective o)
        {
        }
        

        /// <summary>
        /// Returns the Quest related to the entered questID
        /// </summary>
        public static Quest GetQuestForDialogue(string questID)
        {
            Quest foundQuest = null;
            
            for (int i = 0; i < Quests.Count; ++i)
            {
                if (Quests[i].QuestID == questID)
                {
                    foundQuest = Quests[i];
                    break;
                }
            }


            return foundQuest;
        }

        /// <summary>
        /// Sets the Active Quest List
        /// </summary>
        private static void SetActiveQuests()
        {
            int i;

            if (active == null)
                active = new List<Quest>();

            active.Clear();
            
            for (i = 0; i < Quests.Count; ++i)
            {
                if (Quests[i].IsActive)
                    active.Add(Quests[i]);
            }
            
        }

        public static List<Quest> GetActiveQuests()
        {
            SetActiveQuests();

            return active;
        }

        /// <summary>
        /// Returns a list of every completed quest
        /// </summary>
        /// <returns></returns>
        public static List<Quest> GetCompletedQuests()
        {
            int i;
            List<Quest> completed = new List<Quest>();

            for (i = 0; i < Quests.Count; ++i)
            {
                if (Quests[i].Completed)
                    completed.Add(Quests[i]);
            }

            return completed;
        }

        /// <summary>
        /// Returns a list of every failed quest
        /// </summary>
        /// <returns></returns>
        public static List<Quest> GetFailedQuests()
        {
            int i;
            List<Quest> failed = new List<Quest>();

            for (i = 0; i < Quests.Count; ++i)
            {
                if (Quests[i].Failed)
                    failed.Add(Quests[i]);
            }

            return failed;
        }
    }
}

