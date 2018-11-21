using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace schoolRPG.Quests
{
    public enum QuestSort
    {
        ALL, ACTIVE, COMPLETED, FAILED
    }

    /// <summary>
    /// Used to populate the Quests Menu
    /// </summary>
    public class QuestsMenu : MonoBehaviour
    {
        private QuestSort curSort;

        private Vector3 vec;
        private int selectedQuest;
        private int selectedObj;
        private int lastQuest;
        private int lastObj;
        private int track;

        private bool inQuest;
        private bool inObjective;

        private List<Quest> quests;
        private List<Quest> lq;


        public TextMeshProUGUI sortTitle;
        public TextMeshProUGUI objTitle;

        public GameObject contentArea;
        public GameObject contentArea1;

        public Button baseQuestBtn;
        public Button baseObjBtn;

        public TextMeshProUGUI questName;
        public TextMeshProUGUI questDesc;
       
        private List<Button> questBtns;
        private List<Button> objBtns;

        // Use this for initialization
        void OnEnable()
        {
            inQuest = false;
            inObjective = false;

            ClearBtns(questBtns);
            ClearBtns(objBtns);
            questName.text = "[None]";
            questDesc.text = "";

            SelectSort(curSort);
        }

        // Update is called once per frame
        void Update()
        {
            // If we want to track an Active quest
            if (InputManager.GetButtonDown("Use") && quests[selectedQuest].IsActive && !quests[selectedQuest].Completed)
            {
                if (quests != null && (selectedQuest >= 0 && selectedQuest < quests.Count))
                {
                    // Untrack the previously tracked quest
                    if (track > -1)
                    {
                        QuestController.SetTrackQuest();
                        questBtns[track].GetComponentsInChildren<Image>()[1].enabled = false;
                    }

                    track = selectedQuest;
                    // Call the function to track this quest
                    QuestController.SetTrackQuest(quests[track]);
                    questBtns[track].GetComponentsInChildren<Image>()[1].enabled = true;
                    Debug.Log("Tracking: " + quests[track].Name);
                }
            }
            else if (InputManager.GetButtonDown("Drop") && track > -1)
            {
                if (quests != null && QuestController.Tracking.QuestID == quests[track].QuestID)
                {
                    QuestController.SetTrackQuest();
                    questBtns[track].GetComponentsInChildren<Image>()[1].enabled = false;
                    track = -1;
                }
            }
        }

        public void SortQuests(string sort)
        {
            if (sort == "all")
                SelectSort(QuestSort.ALL);
            else if (sort == "active")
                SelectSort(QuestSort.ACTIVE);
            else if (sort == "complete")
                SelectSort(QuestSort.COMPLETED);
            else if (sort == "failed")
                SelectSort(QuestSort.FAILED);
        }

        /// <summary>
        /// Runs when a Quest Sorting method is selected
        /// </summary>
        /// <param name="sort"></param>
        public void SelectSort(QuestSort sort)
        {
            int i;
            List<Quest> q = new List<Quest>();
            curSort = sort;

            sortTitle.text = sort.ToString().Substring(0, 1) + sort.ToString().Substring(1).ToLower() + " Quests";

            ClearBtns(objBtns);
            questName.text = "[None]";
            questDesc.text = "";

            // Get the correct Quests for the sorting method used
            if (sort == QuestSort.ALL)
            {
                for (i = 0; i < QuestController.Quests.Count; ++i)
                {
                    if (QuestController.Quests[i].Objectives.Count > 0)
                        if (QuestController.Quests[i].Objectives[0].IsActive || QuestController.Quests[i].Objectives[0].IsCompleted)
                        q.Add(QuestController.Quests[i]);
                }

                quests = q;
            }
            else if (sort == QuestSort.ACTIVE)
                quests = QuestController.GetActiveQuests();
            else if (sort == QuestSort.COMPLETED)
                quests = QuestController.GetCompletedQuests();
            else if (sort == QuestSort.FAILED)
                quests = QuestController.GetFailedQuests();

            LoadQuests();
        }

        public void ClearBtns(List<Button> btns)
        {
            int i;
            if (btns != null)
            {
                for (i = 0; i < btns.Count; ++i)
                {
                    if (Application.isEditor)
                        DestroyImmediate(btns[i].gameObject);
                    else
                        Destroy(btns[i].gameObject);
                }
            }
            else
                btns = new List<Button>();

            btns.Clear();
        }

        /// <summary>
        /// Loads all Quests for the sorting mode selected
        /// </summary>
        public void LoadQuests()
        {
            int i = 0;
            Quest firstQuest = new Quest();
            RectTransform rect = contentArea.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, 475);

            if (questBtns == null)
                questBtns = new List<Button>();

            ClearBtns(questBtns);

            baseQuestBtn.gameObject.SetActive(true);

            for (i = 0; i < quests.Count; ++i)
            {
                int pos = i;
                Quest q = quests[i];
                GameObject obj = Instantiate(baseQuestBtn.gameObject, baseQuestBtn.transform.parent, true);
                obj.transform.localPosition = new Vector2(baseQuestBtn.transform.localPosition.x, baseQuestBtn.transform.localPosition.y - (75 * i));

                // Add a listener for this Object to load the Objectives when clicked on
                obj.GetComponent<Button>().onClick.AddListener(() => { LoadObjectives(q); });
                if (q.Failed)
                    obj.GetComponentInChildren<TextMeshProUGUI>().text = "<color=red>(Failed) " + q.Name + "</color>";
                else if (q.Completed)
                    obj.GetComponentInChildren<TextMeshProUGUI>().text = "<color=green>(Completed) " + q.Name + "</color>";
                else
                    obj.GetComponentInChildren<TextMeshProUGUI>().text = q.Name;

                obj.GetComponent<HoverSelect>().Pos = pos;
                obj.GetComponent<HoverSelect>().IsObj = false;

                if (QuestController.Tracking != null && QuestController.Tracking.QuestID == quests[i].QuestID)
                {
                    track = i;
                    obj.GetComponentsInChildren<Image>()[1].enabled = true;
                    Debug.Log("Tracking: " + quests[track].Name);
                }
                else
                    obj.GetComponentsInChildren<Image>()[1].enabled = false;

                questBtns.Add(obj.GetComponent<Button>());

                if (i == 0)
                    firstQuest = q;

                if (i > 5)
                {
                    rect.sizeDelta = new Vector2(rect.sizeDelta.x, 475 + (75 * (i - 5)));
                }
            }


            if (questBtns.Count > 0)
            {
                LoadObjectives(firstQuest);
                StartCoroutine(HoverSelect.SelectFirstButton(questBtns[0]));
                selectedQuest = 0;
            }

            baseQuestBtn.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Loads all objectives for the selected Quest
        /// </summary>
        /// <param name="q"></param>
        public void LoadObjectives(Quest q)
        {
            int i;
            RectTransform rect = contentArea1.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, 475);

            if (objBtns == null)
                objBtns = new List<Button>();

            ClearBtns(objBtns);

            questName.text = q.Name;
            questDesc.text = q.Description;
            
            baseObjBtn.gameObject.SetActive(true);

            for (i = 0; i < q.Objectives.Count; ++i)
            {
                // If this objective has at least been displayed to the Player
                if (q.Objectives[i].IsActive || q.Objectives[i].IsCompleted)
                {
                    int pos = i;
                    GameObject obj = Instantiate(baseObjBtn.gameObject, baseObjBtn.transform.parent, true);  
                    obj.transform.localPosition = new Vector2(baseObjBtn.transform.localPosition.x, baseObjBtn.transform.localPosition.y - (75 * i));

                    obj.GetComponentsInChildren<TextMeshProUGUI>()[0].text = q.Objectives[i].ObjectiveDescription;

                    if (q.Objectives[i].IsPassed)
                    {
                        obj.GetComponentsInChildren<TextMeshProUGUI>()[1].enabled = false;
                        obj.GetComponentsInChildren<TextMeshProUGUI>()[2].enabled = true;
                        obj.GetComponentsInChildren<TextMeshProUGUI>()[3].enabled = false;
                    }
                    else if (q.Objectives[i].IsFailed)
                    {
                        obj.GetComponentsInChildren<TextMeshProUGUI>()[1].enabled = false;
                        obj.GetComponentsInChildren<TextMeshProUGUI>()[2].enabled = false;
                        obj.GetComponentsInChildren<TextMeshProUGUI>()[3].enabled = true;
                    }
                    else
                    {
                        obj.GetComponentsInChildren<TextMeshProUGUI>()[1].enabled = true;
                        obj.GetComponentsInChildren<TextMeshProUGUI>()[2].enabled = false;
                        obj.GetComponentsInChildren<TextMeshProUGUI>()[3].enabled = false;
                    }

                    obj.GetComponent<HoverSelect>().Pos = pos;
                    obj.GetComponent<HoverSelect>().IsObj = true;

                    objBtns.Add(obj.GetComponent<Button>());

                    if (i > 1)
                    {
                        rect.sizeDelta = new Vector2(rect.sizeDelta.x, 475 + (75 * (i - 1)));
                    }
                }
                
            }

            baseObjBtn.gameObject.SetActive(false);
        }


        public void SelectQuest(bool isObj, int val)
        {
            if (!isObj)
                selectedQuest = val;
            else
                selectedObj = val;
        }


        /// <summary>
        /// Clamps the scroll rect to the content area of the scroll view, AKA. Don't let the user select an item outside of their viewable area
        /// </summary>
        public void clampContentArea(GameObject content, ref int selected, ref int last, ref int max, ref int min, float height)
        {
            // If we select an item that is further down the list than the last item
            if (selected > last)
            {
                vec = content.transform.localPosition;

                last = selected;
                // If we select an item that is further down than the maxItem (out of view at bottom)
                if ((selected - 5) * height > vec.y)
                {
                    //if (sItem < 5)
                    // sItem = 0;
                    vec.Set(0, ((selected - 5) * height), vec.z);
                    max = selected;
                    min = max - 6;
                    if (vec.y < 0)
                        vec.Set(0, 0, vec.y);

                    content.transform.localPosition = vec;
                }
            }
            // If we select an item that is further up the list than the last item
            else if (selected < last)
            {
                vec = content.transform.localPosition;

                last = selected;
                // If we select an item that is further up than the minItem (out of view at top)
                if (vec.y > selected * height)
                {
                    vec.Set(0, (selected * height), vec.z);
                    min = selected;
                    max = min + 5;
                    content.transform.localPosition = vec;
                }
            }

        }
    }
}


