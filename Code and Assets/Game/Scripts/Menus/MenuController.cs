using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using schoolRPG.Items;
using schoolRPG.Stats;

// Input Manager
using TeamUtility.IO;

namespace schoolRPG
{
    public enum WindowOpen
    {
        NONE, INVENTORY, EQUIPPED, ATTRIBUTES, SKILLS, QUESTLOG, STATS
    }

    public class MenuController : MonoBehaviour
    {
        public static bool inAttrib { get; set; }
        public static GameObject bottomUI { get; set; }

        /// <summary>
        /// Menu button names
        /// </summary>
        private string[] options;
        /// <summary>
        /// A List of each Window GameObject that we will activate when it is open
        /// </summary>
        private Dictionary<WindowOpen, GameObject> windows;

        /// <summary>
        /// Previously opened window (For Controllers)
        /// </summary>
        private WindowOpen prev;
        /// <summary>
        /// Which window is open
        /// </summary>
        public WindowOpen open;
        
        public GameObject menuBackground;
        public GameObject inventory;
        public GameObject equipped;
        public GameObject attributes;
        public GameObject skills;
        public GameObject questlog;
        public GameObject stats;

        // Use this for initialization
        void Start()
        {
            open = WindowOpen.NONE;
            prev = open;
            windows = new Dictionary<WindowOpen, GameObject>();
            options = new string[7];

            bottomUI = GameObject.Find("BottomUI");

            options[0] = "Pause";
            options[1] = "OpenInv";
            options[2] = "OpenEquip";
            options[3] = "OpenAttrib";
            options[4] = "OpenSkill";
            options[5] = "OpenQuest";
            options[6] = "OpenStats";

            // List of each menu object
            windows.Add(WindowOpen.NONE, menuBackground);
            windows.Add(WindowOpen.INVENTORY, inventory);
            windows.Add(WindowOpen.EQUIPPED, equipped);
            windows.Add(WindowOpen.ATTRIBUTES, attributes);
            windows.Add(WindowOpen.SKILLS, skills);
            windows.Add(WindowOpen.QUESTLOG, questlog);
            windows.Add(WindowOpen.STATS, stats);
            
            inventory.SetActive(false);
            equipped.SetActive(false);
            attributes.SetActive(false);
            skills.SetActive(false);
            questlog.SetActive(false);
            stats.SetActive(false);
            menuBackground.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            // If there is currently a window open
            if (open != WindowOpen.NONE && WorldController.InMenu && !CreationMenu.Creating)
            {
                // We can navigate with the Next and Previous buttons
                if (InputManager.GetButtonDown("NextMenu"))
                {
                    // Disable the currently open window
                    windows[open].SetActive(false);

                    if ((int)(open + 1) > options.Length - 1)
                        open = (WindowOpen)1;
                    else
                        open = open + 1;

                    // Enable the new window
                    windows[open].SetActive(true);
                }
                else if (InputManager.GetButtonDown("PrevMenu"))
                {
                    // Disable the currently open window
                    windows[open].SetActive(false);

                    if ((int)(open - 1) < 1)
                        open = (WindowOpen)6;
                    else
                        open = open - 1;

                    // Enable the new window
                    windows[open].SetActive(true);
                }
                // If we want to exit the menu
                else if (InputManager.GetButtonDown("Pause"))
                {
                    // Close the Menu windows
                    windows[open].SetActive(false);
                    windows[WindowOpen.NONE].SetActive(false);
                    open = WindowOpen.NONE;
                    WorldController.TimeScale(true);
                    WorldController.InMenu = false;
                    bottomUI.SetActive(true);
                }
                else
                {
                    if (InputManager.PlayerOneConfiguration == InputManager.GetInputConfiguration("KeyboardAndMouse"))
                    {
                        // Loop through each Window
                        for (int i = 0; i < options.Length; ++i)
                        {
                            if (InputManager.GetButtonDown(options[i]) && (WindowOpen)i != WindowOpen.NONE)
                            {
                                // Activate the new window
                                windows[open].SetActive(false);
                                open = (WindowOpen)i;
                                if (open == WindowOpen.INVENTORY)
                                    InventoryMenu.IsTrade = false;

                                windows[open].SetActive(true);
                                WorldController.InMenu = true;
                                bottomUI.SetActive(false);
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (InputManager.GetButtonDown("OpenMenu"))
                        {
                            // Close the Menu windows
                            windows[open].SetActive(false);
                            windows[WindowOpen.NONE].SetActive(false);
                            open = WindowOpen.NONE;
                            WorldController.TimeScale(true);
                            WorldController.InMenu = false;
                            bottomUI.SetActive(true);
                        }
                    }
                    
                }
                
            }
            // Otherwise, wait for a specific window button to be pressed if the game is not paused
            // and the game is currently not in another menu
            else if (!WorldController.Paused && !WorldController.InMenu && !CreationMenu.Creating)
            {
                if (InputManager.PlayerOneConfiguration == InputManager.GetInputConfiguration("KeyboardAndMouse"))
                {
                    // Loop through each besides the Non-Open window (since no windows are open yet)
                    for (int i = 1; i < options.Length; ++i)
                    {
                        if (InputManager.GetButtonDown(options[i]))
                        {
                            WorldController.InMenu = true;
                            WorldController.TimeScale(false);

                            // Open the background menu
                            windows[open].SetActive(true);

                            open = (WindowOpen)i;

                            if (open == WindowOpen.INVENTORY)
                                InventoryMenu.IsTrade = false;

                            // Open the sub-window of the menu
                            windows[open].SetActive(true);
                            break;
                        }
                    }
                }
                else
                {
                    if (InputManager.GetButtonDown("OpenMenu"))
                    {
                        if (open == WindowOpen.NONE)
                        {
                            WorldController.InMenu = true;
                            WorldController.TimeScale(false);
                            
                            // Activate the background
                            windows[open].SetActive(true);

                            if (prev == WindowOpen.NONE)
                                open = WindowOpen.INVENTORY;
                            else
                                open = prev;

                            if (open == WindowOpen.INVENTORY)
                                InventoryMenu.IsTrade = false;

                            windows[open].SetActive(true);
                            WorldController.InMenu = true;
                            bottomUI.SetActive(false);
                        }
                    }
                }

            } // End if menu open

            // Make sure the attributes screen uses the correct options
            if (!inAttrib && open == WindowOpen.ATTRIBUTES)
            {
                inAttrib = true;
                AttribMenu.setOpenMenu(false);
            }
        } // End Update()
    }
}

