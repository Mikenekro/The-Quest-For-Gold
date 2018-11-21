using schoolRPG.Dialogue;
using schoolRPG.Items;
using schoolRPG.Quests;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace schoolRPG
{
    /// <summary>
    /// When triggered, dialogue with the attached NPC will start automatically
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DialogueTrigger : MonoBehaviour
    {
        private static DialogueMenuController dmc;
        
        [SerializeField, Tooltip("The NPC that we will trigger Dialogue with")]
        private NpcController npc;

        [SerializeField, Tooltip("The ID of the Quest that this is associated with. (Leave Blank if no Quest is required)")]
        private string QuestID;

        [SerializeField, Tooltip("The Stage that this Dialogue Trigger becomes active at")]
        private int QuestStageOpen;
        [SerializeField, Tooltip("The Stage that this Dialogue Trigger will advance the quest at")]
        private int QuestStageFor;
        [SerializeField, Tooltip("The Stage that this Dialogue Trigger will De-Activate at")]
        private int QuestStageClose;

        [SerializeField, Tooltip("Do we want to add any Description to the Quest?")]
        private string AddQuestDescription;

        [SerializeField, Tooltip("Will this trigger advance the Quest?")]
        private bool advancesQuest;

        [SerializeField, Tooltip("Will we be taking an Item from the player?")]
        private bool isFetch;

        [SerializeField, Tooltip("Do we want to Force an Attack on the player once the Dialogue is finished?")]
        private bool isAttack;

        // Use this for initialization
        void Start()
        {
            // Find the Dialogue Menu and store it in a static variable to be accessed by all NPCs
            if (dmc == null)
            {
                // Make sure the Dialogue Menu is active before we try getting components
                GameObject obj = GameObject.Find("DynamicUI").GetComponentsInChildren<Transform>(true)[1].gameObject;
                Debug.Log("Loaded DialogueMenuController in NpcHover: " + obj.name);
                obj.SetActive(true);
                dmc = obj.GetComponent<DialogueMenuController>();
                //dmc = FindObjectOfType<DialogueMenuController>();
                dmc.gameObject.SetActive(false);
            }

            // Attempt to disable any saved trigger states
            for (int i = 0; i < WorldController.Data.DisabledTriggers.Count; ++i)
            {
                if (WorldController.Data.DisabledTriggers[i] == "Disable" + QuestID + QuestStageFor + QuestStageClose)
                {
                    GetComponent<BoxCollider2D>().enabled = false;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            int pos = -1;
            bool good = false;

            if (col.gameObject.transform.GetComponentInParent<PlayerController>() != null)
            {
                Quest q = QuestController.GetQuestForDialogue(QuestID);
                
                if (q.QuestID == QuestID && q.Stage >= QuestStageOpen && q.Stage < QuestStageClose)
                {
                    if (AddQuestDescription != null && AddQuestDescription != "")
                        q.AddDescription = AddQuestDescription;

                    if (isFetch)
                    {
                        pos = q.CheckForAnyItem(npc.name);
                        if (pos > -1)
                        {
                            // Take item from player
                            BaseItem itm = PlayerController.Pc.Player.Inventory.GiveItem(npc.NPC.Inventory, pos, 1, false);
                            PlayerController.DisplayTextToScreen("Removed " + itm.Name + " from Inventory", true);
                        }
                    }

                    if (advancesQuest)
                    {
                        GetComponent<BoxCollider2D>().enabled = false;
                        // Make sure we remember this trigger state
                        WorldController.Data.DisabledTriggers.Add("Disable" + QuestID + QuestStageFor + QuestStageClose);
                        q.SetStage(true, false);
                    }

                    good = true;
                }

                // Make sure we can start the Dialogue
                if (good || QuestID == null)
                {
                    // Activate the Dialogue Menu Controller and set the NPC that we will be talking to
                    npc.NPC.IsTalking = true;
                    WorldController.InMenu = true;
                    DialogueMenuController.npcC = npc;
                    DialogueMenuController.IsOpen = true;
                    dmc.gameObject.SetActive(true);
                    Debug.Log(npc.NPC.Name + " is talking to Player!");
                }
                
            }
            
        }

        private void Update()
        {
            if (transform.position != npc.transform.position)
                transform.position = npc.transform.position;
        }
    }
}

