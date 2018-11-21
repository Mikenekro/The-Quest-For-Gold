using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using schoolRPG;
using TMPro;
using System.IO;

namespace schoolRPG.SaveLoad
{
    public class LoadMenu : MonoBehaviour
    {
        // Save Panel Properties
        [SerializeField]
        private GameObject loadContent;
        [SerializeField]
        private Button baseLoad;
        [SerializeField]
        private Scrollbar scrollBar;
        [SerializeField]
        private Button loadBtn;

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
            if (loadBtn != null)
                StartCoroutine(HoverSelect.SelectFirstButton(loadBtn));

            doClick = true;
            LoadList(baseLoad.gameObject, loadContent);
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

            loadContent.GetComponent<RectTransform>().sizeDelta = new Vector2(loadContent.GetComponent<RectTransform>().sizeDelta.x, 400);
            currentSaves = 0;
            selectedSave = 0;
            lastItem = -1;
            btns.Clear();
        }

        // Update is called once per frame
        private void Update()
        {
            ClampContentArea(loadContent);
        }

        /// <summary>
        /// Deletes the selected saved game
        /// </summary>
        public void DeleteBtn()
        {
            WorldController.Save.DeleteFile(btns[selectedSave].GetComponent<HoverSelect>().savePath);

            LoadList(baseLoad.gameObject, loadContent);
        }

        /// <summary>
        /// Loads the selected Saved Game
        /// </summary>
        public void LoadBtn()
        {
            string oldName = btns[selectedSave].GetComponent<HoverSelect>().savePath;
            oldName = oldName.Split('/')[oldName.Split('/').Length - 1];
            oldName = oldName.Replace(SaveScript.SaveExtension, "");

            // Set the name of the game we are loading
            LoadPersistant.NewGame = false;
            LoadPersistant.LoadName = oldName;
            WorldController.TimeScale(true);
            WorldController.InMenu = false;
            // Load the Loading screen to re-load the game
            SceneManager.LoadScene(2, LoadSceneMode.Single);
        }

        /// <summary>
        /// Loads the list with each Saved Game in the SavedGames folder
        /// </summary>
        public void LoadList(GameObject baseSlot, GameObject content)
        {
            GameObject obj;
            string[] loads = SaveScript.AllSaves();

            // Disable any current items
            OnDisable();
            // Enable the base slot when instantiating new slots
            baseSlot.SetActive(true);

            if (loads.Length < 1)
                doClick = false;

            // Loop through each item sorted
            for (int i = 0; i < loads.Length; ++i)
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
                obj.name = "Load" + currentSaves;

                // Set the text for the save name
                obj.GetComponentInChildren<TextMeshProUGUI>().text =
                    loads[i].Split('/')[loads[i].Split('/').Length - 1].Replace(".sav", "") +
                    " " + File.GetLastWriteTime(loads[i]);

                obj.GetComponent<HoverSelect>().Pos = num;
                obj.GetComponent<HoverSelect>().savePath = loads[i];
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
