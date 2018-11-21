using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using TeamUtility.IO;
using TMPro;

using schoolRPG.Items;
using schoolRPG.SaveLoad;
using schoolRPG.Stats;

namespace schoolRPG
{
    public class MainMenuController : MonoBehaviour
    {
        private bool noSaves;
        private bool loading;
        private bool options;
        private bool cancel;
        private int currentGames;

        /// <summary>
        /// Menu that we will use to load the game
        /// </summary>
        public GameObject loadGameMenu;
        public GameObject noGameMenu;
        public GameObject optionsMenu;

        public GameObject contentArea;
        public GameObject baseLoadBtn;

        public Button newGameBtn;
        public Button loadGameBtn;
        public Button optionsBtn;
        public Button quitBtn;
        public GraphicsMenu menu;

        private List<Button> btns;

        // Use this for initialization
        void Start()
        {
            NpcController.nNum = 0;

            noSaves = false;
            currentGames = 0;
            btns = new List<Button>();
            loadGameMenu.SetActive(false);
            noGameMenu.SetActive(false);
            optionsMenu.SetActive(false);
            StartCoroutine(HoverSelect.SelectFirstButton(gameObject.GetComponentInChildren<Button>()));

            GraphicsMenu.menu = menu;
            // try to load any Graphic Settings
            GraphicsMenu.LoadSettings();
        }

        void Update()
        {
            // Make sure we show the no games popup if necessary
            if (cancel)
                StartCoroutine(NoGames());

            if (InputManager.AnyInput("Xbox_One_Input"))
            {
                InputManager.SetInputConfiguration("Xbox_One_Input", PlayerID.One);
            }
            else if (InputManager.AnyInput("KeyboardAndMouse"))
            {
                InputManager.SetInputConfiguration("KeyboardAndMouse", PlayerID.One);
            }

            if (((InputManager.GetButtonUp("Cancel") || InputManager.GetButtonUp("Pause")) && loading))
            {
                CloseLoad();
            }
        }

        /// <summary>
        /// Let the user know that there are no saved games
        /// </summary>
        /// <returns></returns>
        public IEnumerator NoGames()
        {
            WaitForSeconds wfs = new WaitForSeconds(3.0f);

            noGameMenu.SetActive(true);
            loadGameBtn.interactable = false;
            noSaves = true;
            yield return wfs;
            noGameMenu.SetActive(false);

            yield return null;
        }

        public void NewGame()
        {
            LoadPersistant.LoadName = "";
            LoadPersistant.NewGame = true;
            SceneManager.LoadScene(2, LoadSceneMode.Single);
        }
        public void LoadGame()
        {
            loadGameMenu.SetActive(true);
            loading = true;
            newGameBtn.interactable = false;
            loadGameBtn.interactable = false;
            optionsBtn.interactable = false;
            quitBtn.interactable = false;
            LoadGames();
        }
        public void CloseLoad()
        {
            currentGames = 0;
            
            for (int i = 0; i < btns.Count; ++i)
            {
                if (Application.isPlaying)
                    DestroyImmediate(btns[i].gameObject);
                else
                    Destroy(btns[i].gameObject);
            }

            btns.Clear();

            loadGameMenu.SetActive(false);
            loading = false;
            newGameBtn.interactable = true;
            if (!noSaves)
            {
                loadGameBtn.interactable = true;
                StartCoroutine(HoverSelect.SelectFirstButton(loadGameBtn));
            }
            else
                StartCoroutine(HoverSelect.SelectFirstButton(newGameBtn));
            optionsBtn.interactable = true;
            quitBtn.interactable = true;
        }
        public void Options()
        {
            optionsMenu.SetActive(true);
            options = true;
            newGameBtn.interactable = false;
            loadGameBtn.interactable = false;
            optionsBtn.interactable = false;
            quitBtn.interactable = false;
        }
        public void CloseOptions()
        {
            optionsMenu.SetActive(false);
            options = false;
            newGameBtn.interactable = true;
            loadGameBtn.interactable = true;
            optionsBtn.interactable = true;
            quitBtn.interactable = true;
            StartCoroutine(HoverSelect.SelectFirstButton(optionsBtn));
        }
        public void QuitGame()
        {
            Application.Quit();
        }

        /// <summary>
        /// Loads each Saved Game into the list
        /// </summary>
        public void LoadGames()
        {
            int i;
            bool none = true;
            string appPath;

            if (Application.isEditor)
                appPath = Application.dataPath;
            else
                appPath = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/QuestForGold";

            string savePath = "/SavedGames/";
            string[] games;
            string ext;
            GameObject obj = null;

            savePath = appPath.Replace("/Assets", savePath);
            
            // If we find the specified Saved Games directory
            if (Directory.Exists(savePath))
            {
                // Load each Saved Game from the directory
                games = SaveScript.AllSaves();
                baseLoadBtn.SetActive(true);

                // Loop through each file in the directory
                for (i = 0; i < games.Length; ++i)
                {
                    // Make sure the file has the correct extension
                    ext = Path.GetExtension(games[i]);
                    if (ext == ".sav")
                    {
                        // Local text object to prevent using the same instance when adding listeners to the buttons
                        TextMeshProUGUI txt;
                        none = false;

                        obj = Instantiate(baseLoadBtn);
                        obj.transform.SetParent(contentArea.transform);
                        obj.transform.localScale = new Vector3(1, 1, 1);
                        obj.GetComponent<RectTransform>().sizeDelta = baseLoadBtn.GetComponent<RectTransform>().sizeDelta;
                        obj.transform.localPosition = new Vector3(baseLoadBtn.transform.localPosition.x,
                            (baseLoadBtn.transform.localPosition.y - (50.0f * (i))), baseLoadBtn.transform.localPosition.z);
                        obj.name = "Game" + currentGames;

                        txt = obj.GetComponentInChildren<TextMeshProUGUI>();
                        // Button Text Format - [Game Name]: [Date] [Time]
                        txt.text = games[i].Split('/')[games[i].Split('/').Length-1].Split('.')[0] + 
                            " : " + File.GetCreationTime(games[i]).ToShortDateString() + ", " + 
                            File.GetCreationTime(games[i]).ToShortTimeString();

                        // Set the function that will run if we click on the item
                        obj.GetComponent<Button>().onClick.AddListener(() => { ConfirmLoad(txt); });

                        // Resize the content area if there are more items that can fit in the screen
                        if (obj.transform.localPosition.y < -500)
                        {
                            contentArea.GetComponent<RectTransform>().sizeDelta = new Vector2(contentArea.GetComponent<RectTransform>().sizeDelta.x,
                                contentArea.GetComponent<RectTransform>().sizeDelta.y + 50);
                        }

                        currentGames++;
                        btns.Add(obj.GetComponent<Button>());
                    }
                }
                
                baseLoadBtn.SetActive(false);

                // Display the "No Games" popup if no saved games were found
                if (none)
                    cancel = true;
                else
                    StartCoroutine(HoverSelect.SelectFirstButton(btns[0]));
            }
            else // Otherwise, we need to create the directory and there will be no saved games
            {
                Directory.CreateDirectory(savePath);
                cancel = true;
            }
        }

        /// <summary>
        /// Confirms loading the selected game from the LoadGame Menu
        /// </summary>
        public void ConfirmLoad(TextMeshProUGUI text)
        {
            loading = false;
            LoadPersistant.NewGame = false;
            // Button Text Format - [Game Name]: [Date] [Time]
            LoadPersistant.LoadName = text.text.Split(':')[0].Substring(0, text.text.Split(':')[0].Length-1);
            SceneManager.LoadScene(2, LoadSceneMode.Single);
        }
    }
}

