using schoolRPG.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace schoolRPG.SaveLoad
{
    /// <summary>
    /// Script that will control how the Save Game Menu is displayed
    /// </summary>
    public class SaveMenu : MonoBehaviour
    {
        [SerializeField]
        private PlayerController pc;

        // Save Panel Properties
        [SerializeField]
        private GameObject saveContent;
        [SerializeField]
        private Button baseSave;
        [SerializeField]
        private Scrollbar scrollBar;
        [SerializeField]
        private Button saveBtn;

        private bool doClick;
        private int maxItem;
        private int minItem;
        private int lastItem;
        private int currentSaves;
        private int selectedSave;
        
        private List<Button> btns;
        private Image lastImg;

        // Use this for initialization
        private void Awake()
        {
            btns = new List<Button>();
            lastItem = -1;
        }
        private void OnEnable()
        {
            if (saveBtn != null)
                StartCoroutine(HoverSelect.SelectFirstButton(saveBtn));

            doClick = true;
            LoadList(baseSave.gameObject, saveContent);
        }
        private void OnDisable()
        {
            int i;

            for (i = 0; i < btns.Count; ++i)
            {
                if (Application.isEditor)
                    DestroyImmediate(btns[i].gameObject);
                else
                    Destroy(btns[i].gameObject);
            }

            saveContent.GetComponent<RectTransform>().sizeDelta = new Vector2(saveContent.GetComponent<RectTransform>().sizeDelta.x, 400);
            currentSaves = 0;
            selectedSave = 0;
            lastItem = -1;
            btns.Clear();
        }

        // Update is called once per frame
        private void Update()
        {
            ClampContentArea(saveContent);
        }

        /// <summary>
        /// Deletes the selected saved game
        /// </summary>
        public void DeleteBtn()
        {
            WorldController.Save.DeleteFile(btns[selectedSave].GetComponent<HoverSelect>().savePath);

            LoadList(baseSave.gameObject, saveContent);
        }
        /// <summary>
        /// Writes over the selected saved game
        /// </summary>
        public void SaveBtn()
        {
            string oldName = btns[selectedSave].GetComponent<HoverSelect>().savePath;
            oldName = oldName.Split('/')[oldName.Split('/').Length - 1];

            WorldController.AllData(true);

            if (WorldController.Save.SaveGame(oldName, WorldController.Data))
            {
                Debug.Log("Save Successful!");
                PlayerController.DisplayTextToScreen("Save Successful!", false);
            }
            else
            {
                PlayerController.DisplayTextToScreen("<color=red>Save Failed!</color>", false);
            }
            
            gameObject.SetActive(false);
        }
        /// <summary>
        /// Creates a new Save
        /// </summary>
        public void NewSaveBtn()
        {
            WorldController.AllData(true);

            if (WorldController.Save.SaveGame(WorldController.Data))
            {
                Debug.Log("Save Successful!");
                PlayerController.DisplayTextToScreen("Save Successful!", false);
            }
            else
            {
                PlayerController.DisplayTextToScreen("<color=red>Save Failed!</color>", false);
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Loads the list with each Saved Game in the SavedGames folder
        /// </summary>
        public void LoadList(GameObject baseSlot, GameObject content)
        {
            GameObject obj;
            string[] saves = SaveScript.AllSaves();

            // Disable any current items
            OnDisable();
            // Enable the base slot when instantiating new slots
            baseSlot.SetActive(true);

            if (saves.Length < 1)
                doClick = false;

            // Loop through each item sorted
            for (int i = 0; i < saves.Length; ++i)
            {
                int num = i;
                GameObject c = content;

                // Instantiate the object
                obj = Instantiate(baseSlot);
                obj.transform.SetParent(content.transform);
                obj.transform.localScale = new Vector3(1, 1, 1);
                obj.GetComponent<RectTransform>().sizeDelta = baseSlot.GetComponent<RectTransform>().sizeDelta;
                obj.transform.localPosition = new Vector3(baseSlot.transform.localPosition.x,
                    (baseSlot.transform.localPosition.y - (75.0f * (i))), baseSlot.transform.localPosition.z);
                obj.name = "Save" + currentSaves;

                // Set the text for the save name
                obj.GetComponentInChildren<TextMeshProUGUI>().text = 
                    saves[i].Split('/')[saves[i].Split('/').Length - 1].Replace(".sav", "") + 
                    " " + File.GetLastWriteTime(saves[i]);
                
                obj.GetComponent<HoverSelect>().Pos = num;
                obj.GetComponent<HoverSelect>().savePath = saves[i];
                // Set the function that will run if we click on the item
                obj.GetComponent<Button>().onClick.AddListener(() => { ClampContentArea(c); });
                btns.Add(obj.GetComponent<Button>());

                // Resize the content area if there are more items that can fit in the screen
                if (obj.transform.localPosition.y < -400)
                {
                    content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().sizeDelta.x,
                        content.GetComponent<RectTransform>().sizeDelta.y + 75);
                }

                currentSaves++;
            }

            // Disable the base slot
            baseSlot.SetActive(false);

            // Make sure we select an item if we have made any changes to the list size
            if (doClick)
            {
                doClick = false;
                StartCoroutine(HoverSelect.SelectFirstButton(btns[selectedSave]));
            }
        } // End LoadList()

        /// <summary>
        /// Clamps the scroll rect to the content area of the scroll view, AKA. Don't let the user select an item outside of their viewable area
        /// </summary>
        /// <param name="contArea"></param>
        public void ClampContentArea(GameObject contArea)
        {
            Vector3 vec = contArea.transform.localPosition;
            // If we select an item that is further down the list than the last item
            if (selectedSave > lastItem)
            {
                lastItem = selectedSave;
                // If we select an item that is further down than the maxItem (out of view at bottom)
                if ((selectedSave - 4) * 75 > vec.y)
                {
                    //if (sItem < 5)
                    // sItem = 0;
                    contArea.transform.localPosition = new Vector3(vec.x, ((selectedSave - 4) * 75));
                    maxItem = selectedSave;
                    minItem = maxItem - 6;
                    if (contArea.transform.localPosition.y < 0)
                        contArea.transform.localPosition = new Vector3(vec.x, 0);
                }
            }
            // If we select an item that is further up the list than the last item
            else if (selectedSave < lastItem)
            {
                lastItem = selectedSave;
                // If we select an item that is further up than the minItem (out of view at top)
                if (vec.y > selectedSave * 75)
                {
                    contArea.transform.localPosition = new Vector3(vec.x, (selectedSave * 75));
                    minItem = selectedSave;
                    maxItem = minItem + 5;
                }
            }

        }

        /// <summary>
        /// Display the Selection Border to let the user know they have this item selected
        /// </summary>
        /// <param name="img"></param>
        public void EnableSelect(Image img)
        {
            if (lastImg != null)
                lastImg.enabled = false;

            img.enabled = true;

            lastImg = img;
        }

        /// <summary>
        /// Called when a Saved Game is selected in the list
        /// </summary>
        /// <param name="num"></param>
        public void SelectSave(int num)
        {
            selectedSave = num;
        }

    }
}

