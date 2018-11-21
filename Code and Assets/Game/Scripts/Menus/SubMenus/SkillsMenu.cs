using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using schoolRPG.Items;

namespace schoolRPG.Stats
{
    public class SkillsMenu : MonoBehaviour
    {
        private static bool createMenu;

        // Reflection Properties
        private Type t;
        private PropertyInfo[] info;
        private List<Skill> skills;
        private Skill temp;
        private List<Button> btns;
        private Button selFirst;
        private Selectable sel;

        private int maxItem;
        private int minItem;
        private int lastItem;
        private int currentSkill;
        private int selectedSkill;

        public PlayerController pc;

        /// <summary>
        /// The Base Skill to create every other skill on the Menu
        /// </summary>
        public GameObject baseSkill;
        public GameObject contentArea;

        public TextMeshProUGUI skillName;
        public TextMeshProUGUI skillLevel;
        public TextMeshProUGUI skillDescription;
        public TextMeshProUGUI skillEXP;
        public Slider skillProgress; 


        // Use this for initialization
        void Awake()
        {
            skills = new List<Skill>();
            btns = new List<Button>();
            selFirst = null;
            minItem = 0;
            maxItem = 5;
        }

        void OnEnable()
        {
            GameObject obj;

            baseSkill.SetActive(true);

            // Make sure Player is loaded
            if (pc.Player != null)
            {
                t = typeof(Character);
                info = t.GetProperties();

                // Loop through each property in the Character class
                for (int i = 0; i < info.Length; ++i)
                {
                    // Look for any Skill properties
                    if (info[i].PropertyType == typeof(Skill))
                    {
                        // Get the value of the Player Characters Instance of this Property
                        temp = (Skill)info[i].GetValue(pc.Player, null);

                        // Make sure this skill is not already in the Skill list
                        if (!skills.Contains(temp) && temp != null && temp.Name != null)
                            // Add the new Skill to the list
                            skills.Add(temp);
                    }
                }

                btns.Clear();
                currentSkill = 1;
                for (int i = 0; i < skills.Count; ++i)
                {
                    // So we can reference the current position in the array without using the same instance 
                    // when adding the Listener to the buttons
                    int j = i;
                    Skill s = skills[j];

                    // Instantiate a Skill Menu Item for each known skill
                    obj = Instantiate(baseSkill);
                    obj.transform.SetParent(contentArea.transform);
                    obj.transform.localScale = new Vector3(1, 1, 1);
                    obj.GetComponent<RectTransform>().sizeDelta = baseSkill.GetComponent<RectTransform>().sizeDelta;
                    obj.transform.localPosition = new Vector3(baseSkill.transform.localPosition.x,
                        (baseSkill.transform.localPosition.y - (75.0f * (i))), baseSkill.transform.localPosition.z);
                    obj.name = "Skill" + currentSkill;

                    obj.GetComponent<HoverSelect>().Pos = j;
                    obj.GetComponentInChildren<TextMeshProUGUI>().text = skills[i].Name + " : Level " + skills[i].Level;
                    
                    // Set the function that will run if we click on the item
                    obj.GetComponent<Button>().onClick.AddListener(() => { selectSkill(s); });
                    btns.Add(obj.GetComponent<Button>());

                    // Resize the content area if there are more items that can fit in the screen
                    if ((obj.transform.localPosition.y < -450 && !createMenu) || 
                        (obj.transform.localPosition.y < -400 && createMenu))
                    {
                        contentArea.GetComponent<RectTransform>().sizeDelta = new Vector2(contentArea.GetComponent<RectTransform>().sizeDelta.x,
                            contentArea.GetComponent<RectTransform>().sizeDelta.y + 75);
                    }

                    currentSkill++;
                }

            }

            baseSkill.SetActive(false);
            selFirst = btns[0];
            if (selFirst == null)
                selFirst = baseSkill.GetComponent<Button>();
            StartCoroutine(HoverSelect.SelectFirstButton(selFirst));
            selFirst.onClick.AddListener(() => { selectSkill(skills[0]); });
            selectSkill(skills[0]);
        } // End OnEnable()
        

        void OnDisable()
        {
            int i;
            minItem = 0;
            maxItem = 5;
            
            for (i = 0; i < btns.Count; ++i)
            {
                // Destroy the active skill buttons
                if (btns[i].gameObject.activeSelf)
                {
                    if (Application.isEditor)
                        DestroyImmediate(btns[i].gameObject);
                    else
                        Destroy(btns[i].gameObject);
                }
            }

            btns.Clear();
            if (skillName != null)
                skillName.text = "";
            if (skillLevel != null)
                skillLevel.text = "";
            if (skillDescription != null)
                skillDescription.text = "";
            if (skillProgress != null)
            {
                skillProgress.maxValue = 1;
                skillProgress.value = 0;
            }
            if (skillEXP != null)
                skillEXP.text = "";
        }

        void Update()
        {
            if (WorldController.InMenu)
            {
                // Keep the selectable Skills within the viewable area
                clampContentArea(contentArea);
            }
        }

        public static void setCreateMenu(bool inCreate)
        {
            createMenu = inCreate;
        }

        /// <summary>
        /// Sets the current selected Skill
        /// </summary>
        /// <param name="num"></param>
        public void hoverSkill(int num)
        {
            selectedSkill = num;
        }
        
        public void selectSkill(Skill s)
        {
            if (skillName != null)
                skillName.text = s.Name;
            if (skillLevel != null)
                skillLevel.text = "Level: " + s.Level;
            if (skillDescription != null)
                skillDescription.text = s.Description;
            if (skillProgress != null)
            {
                skillProgress.maxValue = (float)s.NextLevelEXP;
                skillProgress.value = (float)s.CurrentEXP;
            }
            if (skillEXP != null)
                skillEXP.text = s.CurrentEXP.ToString("0") + " / " + s.NextLevelEXP.ToString("0") + " XP.";

            clampContentArea(contentArea);
        }

        /// <summary>
        /// Clamps the scroll rect to the content area of the scroll view, AKA. Don't let the user select an item outside of their viewable area
        /// </summary>
        /// <param name="contArea"></param>
        public void clampContentArea(GameObject contArea)
        {
            int skillSub = 5;
            Vector3 vec = contArea.transform.localPosition;

            // Create menu needs to subtract 4 from selected skill when changing position 
            // (Not needed when checking if ((selectedSkill - 4) * 75 > vec.y) )
            if (createMenu)
                skillSub = 4;

            if (selectedSkill > lastItem)
            {
                lastItem = selectedSkill;
                if ((selectedSkill - 4) * 75 > vec.y)
                {
                    contArea.transform.localPosition = new Vector3(vec.x, ((selectedSkill - skillSub) * 75));
                    maxItem = selectedSkill;
                    minItem = maxItem - 5;
                    if (contArea.transform.localPosition.y < 0)
                        contArea.transform.localPosition = new Vector3(vec.x, 0);
                }
            }
            else if (selectedSkill < lastItem)
            {
                lastItem = selectedSkill;
                if (vec.y > selectedSkill * 75)
                {
                    contArea.transform.localPosition = new Vector3(vec.x, (selectedSkill * 75));
                    minItem = selectedSkill;
                    maxItem = minItem + 4;
                    
                }
            }
        }

    } // End SkillMenu Class
}

