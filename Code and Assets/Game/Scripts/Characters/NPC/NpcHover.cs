using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using TeamUtility.IO;
using schoolRPG.Dialogue;
using schoolRPG.Quests;
using schoolRPG.Items;

namespace schoolRPG
{
    /// <summary>
    /// Displays the NPC's stats when the player overs the mouse over the containing NPC
    /// </summary>
    public class NpcHover : MonoBehaviour
    {
        private static DialogueMenuController dmc;

        private bool isOver;
        private string[] nUIText;
        private GameObject hoverUI;
        private Transform hoverTran;
        private TextMeshProUGUI[] hoverTxt;
        private Vector2 mousePos;
        private NpcController nParent;

        private NPCType oldType;

        private int i;

        // Use this for initialization
        void Start()
        {
            isOver = false;
            hoverUI = WorldController.NpcHoverUI;
            if (hoverUI != null)
            {
                hoverTran = hoverUI.transform;
                hoverTxt = hoverUI.GetComponentsInChildren<TextMeshProUGUI>();
                nUIText = new string[hoverTxt.Length];
                nParent = gameObject.GetComponent<NpcController>();
                oldType = nParent.Type;

                // Name
                nUIText[0] = nParent.NPC.Name;
                // Type
                nUIText[1] = "Type: " + oldType.ToString();
                // Level
                nUIText[2] = "Level: " + nParent.NPC.Level.ToString();
                // Interact Key
                nUIText[3] = "Press " + 'E' + " To Interact";

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
            }
        }

        private void OnDisable()
        {
            isOver = false;
            if (hoverUI != null)    
                hoverUI.SetActive(false);
        }
        private void OnDestroy()
        {
            isOver = false;
            if (hoverUI != null)
                hoverUI.SetActive(false);
        }

        private void OnMouseOver()
        {
            OnOver();
        }

        private void OnMouseExit()
        {
            OnExit();
        }

        private void OnTriggerStay2D(Collider2D col)
        {
            if (col.tag == "Interact")
                OnOver();
        }
        private void OnTriggerExit2D(Collider2D col)
        {
            if (col.tag == "Interact")
                OnExit();
        }

        private void Update()
        {
            if (WorldController.InMenu || WorldController.Paused)
            {
                OnExit(true);
            }
        }


        private void OnOver()
        {
            int pos = -1;

            if (!isOver && hoverUI != null && !WorldController.Paused && !WorldController.InMenu)
            {
                if (oldType != nParent.Type)
                {
                    oldType = nParent.Type;
                    nUIText[1] = "Type: " + oldType.ToString();
                }

                // Level
                nUIText[2] = "Level: " + nParent.NPC.Level.ToString();

                // Don't re-calculate until we leave the object
                isOver = true;

                if (hoverUI != null)
                {
                    hoverUI.SetActive(true);
                    hoverTran.position = Camera.main.ScreenToWorldPoint(InputManager.mousePosition);
                    hoverTran.position = new Vector3(hoverTran.position.x + 2.0f, hoverTran.position.y - 1.25f, 0);

                    for (i = 0; i < hoverTxt.Length; ++i)
                        hoverTxt[i].text = nUIText[i];
                }
                else
                {
                    // Attempt to cache the Hover UI GameObject if it failed in Start()
                    hoverUI = WorldController.NpcHoverUI;
                    hoverTran = hoverUI.transform;
                    hoverTxt = hoverUI.GetComponentsInChildren<TextMeshProUGUI>();
                }
            }
            else if (isOver)
            {
                // If we want to talk to this NPC, the DMC is not active, and this is not an enemy
                if (InputManager.GetButton("Use") && !dmc.gameObject.activeInHierarchy && nParent.Type != NPCType.ENEMY)
                {
                    List<Quest> active = QuestController.GetActiveQuests();
                    // Activate the Dialogue Menu Controller and set the NPC that we will be talking to
                    nParent.NPC.IsTalking = true;
                    WorldController.InMenu = true;
                    DialogueMenuController.npcC = nParent;
                    DialogueMenuController.IsOpen = true;
                    dmc.gameObject.SetActive(true);
                    Debug.Log(nParent.NPC.Name + " is talking to Player!");

                    for (i = 0; i < active.Count; ++i)
                    {
                        pos = active[i].CheckForAnyItem(nParent.NPC.Name);
                        // If we find a quest item in the active quests list
                        if (pos > -1)
                        {
                            // Take item from player
                            BaseItem itm = PlayerController.Pc.Player.Inventory.GiveItem(nParent.NPC.Inventory, pos, 1, false);
                            PlayerController.DisplayTextToScreen("Removed " + itm.Name + " from Inventory", true);
                        }
                    }

                    // De-Activate the HoverUI
                    isOver = false;
                    hoverUI.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Do we want to exit the hover UI? Set Over to true to override the normal parameters needed to exit
        /// </summary>
        /// <param name="over"></param>
        private void OnExit(bool over = false)
        {
            if (over || isOver)
            {
                isOver = false;
                hoverUI.SetActive(false);
            }
        }
    }
}

