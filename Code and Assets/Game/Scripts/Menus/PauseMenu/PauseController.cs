using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using schoolRPG.SaveLoad;

namespace schoolRPG
{
    public class PauseController : MonoBehaviour
    {
        public static PauseController pause;
        /// <summary>
        /// Has the player died yet?
        /// </summary>
        public static bool IsDeath = false;
        
        [SerializeField]
        private TextMeshProUGUI title;
        /// <summary>
        /// Object that is the parent to the pause menu
        /// </summary>
        [SerializeField]
        private GameObject buttonPanel;
        [SerializeField]
        private GameObject saveMenu;
        private bool saveClosed;
        [SerializeField]
        private GameObject loadMenu;
        private bool loadClosed;
        [SerializeField]
        private GameObject optionsMenu;
        private bool optionsClosed;
        [SerializeField]
        private Button firstSelect;

        // Load Panel Properties

        // Options Panel Properties


        // Misc Properties
        private Button[] btns;

        private void Awake()
        {
            if (pause == null)
                pause = gameObject.GetComponent<PauseController>();
            OnEnable();
        }

        private void OnEnable()
        {
            Screens(false);
            LoadButtons();

            if (firstSelect != null)
                StartCoroutine(HoverSelect.SelectFirstButton(firstSelect));

            if (IsDeath)
            {
                btns[0].interactable = false;
                title.text = "<color=red>Player Died!</color>";
            }
        }
        private void OnDisable()
        {
            Screens(false);
        }
        private void Screens(bool show)
        {
            if (saveMenu != null)
                saveMenu.SetActive(show);
            if (loadMenu != null)
                loadMenu.SetActive(show);
            if (optionsMenu != null)
                optionsMenu.SetActive(show);
        }
        private void LoadButtons()
        {
            if (buttonPanel != null)
                btns = buttonPanel.GetComponentsInChildren<Button>();
        }
        private void Buttons(bool interactable)
        {
            int i;
            // Disable or Enable the buttons
            for (i = 0; i < btns.Length; ++i)
            {
                // Make sure we don't activate the Resume button if the player has died
                if (IsDeath && interactable && i == 0 && btns.Length > 1)
                    i++;
                    
                btns[i].interactable = interactable;
            }
        }

        public void Update()
        {
            if ((!saveMenu.activeInHierarchy && !saveClosed) || 
                (!loadMenu.activeInHierarchy && !loadClosed) ||
                (!optionsMenu.activeInHierarchy && !optionsClosed))
            {
                CloseMenus();
            }
        }

        /// <summary>
        /// Resume the current Game
        /// </summary>
        public void BResume()
        {
            gameObject.SetActive(false);
            WorldController.TimeScale(true);
        }
        /// <summary>
        /// Closes the menus
        /// </summary>
        public void CloseMenus()
        {
            if (!saveClosed)
                StartCoroutine(HoverSelect.SelectFirstButton(btns[1]));
            else if (!loadClosed)
                StartCoroutine(HoverSelect.SelectFirstButton(btns[2]));
            else if (!optionsClosed)
                StartCoroutine(HoverSelect.SelectFirstButton(btns[3]));

            saveClosed = true;
            loadClosed = true;
            optionsClosed = true;
            Screens(false);
            Buttons(true);
        }
        /// <summary>
        /// Save the current Game
        /// </summary>
        public void BSave()
        {
            if (btns == null)
                LoadButtons();

            saveClosed = false;
            Buttons(false);
            saveMenu.SetActive(true);
        }
        /// <summary>
        /// Load a saved Game
        /// </summary>
        public void BLoad()
        {
            loadClosed = false;
            Buttons(false);
            loadMenu.SetActive(true);
        }
        /// <summary>
        /// Bring up the options menu to set things such as the Keybindings or audio volume
        /// </summary>
        public void BOptions()
        {
            optionsClosed = false;
            Buttons(false);
            optionsMenu.SetActive(true);
        }
        /// <summary>
        /// Brings the Player back to the Main Menu
        /// </summary>
        public void BMainMenu()
        {
            SceneManager.LoadScene(1, LoadSceneMode.Single);
        }
        /// <summary>
        /// Quits the Game entirely
        /// </summary>
        public void BQuit()
        {
            bool good = WorldController.Save.AutoSave(WorldController.Data);

            // Log the error if it fails.
            if (!good)
                Debug.Log("AutoSave Error, " + DateTime.UtcNow + ": AutoSave in bQuit() failed. Could not Save before quitting the game.");

            Application.Quit(); 
        }



    }
}

