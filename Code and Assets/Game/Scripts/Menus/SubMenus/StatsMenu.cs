using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace schoolRPG.Stats
{
    /// <summary>
    /// Menu to display all stats about the player
    /// </summary>
    public class StatsMenu : MonoBehaviour
    {
        [SerializeField]
        private PlayerController pc;

        [SerializeField]
        private List<Button> btns;

        [SerializeField]
        private GameObject contentArea;
        [SerializeField]
        private Scrollbar scroll;

        private int lastItem;

        public int maxItem;
        public int minItem = 0;
        public int selectedItem;

        private int multi;


        // Use this for initialization
        void Start()
        {
            scroll.value = 1;
            for (int i = 0; i < btns.Count; ++i)
                btns[i].GetComponent<HoverSelect>().Pos = i;
            OnEnable();
        }

        public void OnEnable()
        {
            maxItem = btns.Count;
            selectedItem = 0;
            lastItem = -1;
            LoadStats();
            StartCoroutine(HoverSelect.SelectFirstButton(btns[selectedItem]));
        }

        // Update is called once per frame
        void Update()
        {
            // Only run when the selected item is not the same as the last iteration
            if (selectedItem != lastItem)
                clampContentArea();
        }

        public void LoadStats()
        {
            StatsValues temp = WorldController.Data.StatValue;

            btns[1].GetComponentInChildren<TextMeshProUGUI>().text = "Times Hit Enemy: " + temp.HitEnemy.ToString("###,###,##0");
            btns[2].GetComponentInChildren<TextMeshProUGUI>().text = "Times Hit Friendly: " + temp.HitFriendly.ToString("###,###,##0");
            btns[3].GetComponentInChildren<TextMeshProUGUI>().text = "Damage Dealt: " + temp.DamageDealt.ToString("###,###,##0");
            btns[4].GetComponentInChildren<TextMeshProUGUI>().text = "Damage Taken: " + temp.DamageTaken.ToString("###,###,##0");
            btns[5].GetComponentInChildren<TextMeshProUGUI>().text = "Damage Resisted: " + temp.DamageBlocked.ToString("###,###,##0");
            btns[6].GetComponentInChildren<TextMeshProUGUI>().text = "Damage Resisted (Enemy): " + temp.DamageMissed.ToString("###,###,##0");
            btns[7].GetComponentInChildren<TextMeshProUGUI>().text = "Highest Damage Dealt: " + temp.HighestDamageDealt.ToString("###,###,##0");
            btns[8].GetComponentInChildren<TextMeshProUGUI>().text = "Highest Damage Taken: " + temp.HighestDamageTaken.ToString("###,###,##0");

            btns[10].GetComponentInChildren<TextMeshProUGUI>().text = "Jump Points Used: " + temp.JumpPointsUsed.ToString("###,###,##0");

            btns[12].GetComponentInChildren<TextMeshProUGUI>().text = "Current Gold: " + temp.CurrentGold.ToString("###,###,##0") + " Gold";
            btns[13].GetComponentInChildren<TextMeshProUGUI>().text = "Most Gold Carried: " + temp.MostGold.ToString("###,###,##0") + " Gold";
            btns[14].GetComponentInChildren<TextMeshProUGUI>().text = "Total Gold Spent: " + temp.GoldSpent.ToString("###,###,##0") + " Gold";
            btns[15].GetComponentInChildren<TextMeshProUGUI>().text = "Total Steps Taken: " + temp.StepsTaken.ToString("###,###,##0") + " Steps";
            btns[16].GetComponentInChildren<TextMeshProUGUI>().text = "Total Distance Traveled: " + (temp.DistanceTraveled / 5280.0f).ToString("###,###,##0.##") + " Miles";
            btns[17].GetComponentInChildren<TextMeshProUGUI>().text = "Total Conversations: " + temp.TotalConversations.ToString("###,###,##0") + " Times";
            btns[18].GetComponentInChildren<TextMeshProUGUI>().text = "Total Speech Checks: " + temp.SpeechChecks.ToString("###,###,##0") + " Times";
            btns[19].GetComponentInChildren<TextMeshProUGUI>().text = "Speech Check Percentage (Passed): " + (temp.SpeechPercent * 100.0f).ToString("##0.##") + "%";
            btns[20].GetComponentInChildren<TextMeshProUGUI>().text = "Total Items Bought: " + temp.ItemsBought.ToString("###,###,##0") + " Items";
            btns[21].GetComponentInChildren<TextMeshProUGUI>().text = "Total Items Sold: " + temp.ItemsSold.ToString("###,###,##0") + " Items";
            btns[22].GetComponentInChildren<TextMeshProUGUI>().text = "Times Bartered: " + temp.TimesBartered.ToString("###,###,##0") + " Times";
            btns[23].GetComponentInChildren<TextMeshProUGUI>().text = "Total Gold Saved From Bartering: " + temp.GoldSavedBarter.ToString("###,###,##0") + " Saved";
        }

        /// <summary>
        /// Clamps the scroll rect to the content area of the scroll view, AKA. Don't let the user select an item outside of their viewable area
        /// </summary>
        /// <param name="contArea"></param>
        public void clampContentArea()
        {
            Vector3 vec = contentArea.transform.localPosition;
            Vector3 selected = btns[selectedItem].transform.localPosition;
            
            // Moving Down 
            if (selectedItem > lastItem)
            {
                lastItem = selectedItem;

                // Set the current Position of the Content Area if we are not within range

                // Moving Down, and a Stats Item, Clamp must be at least the selected items Y axis position - 425
                if (btns[selectedItem].name.Contains("Stats") && Mathf.Abs(vec.y) < (Mathf.Abs(selected.y) - 425.0f))
                    contentArea.transform.localPosition = new Vector3(vec.x, Mathf.Abs(selected.y) - 425.0f, vec.z);
                // Moving Down, and a Header Item, Clamp must be at least the selected items Y axis position - 437.5
                else if (!btns[selectedItem].name.Contains("Stats") && Mathf.Abs(vec.y) < (Mathf.Abs(selected.y) - 437.5f))
                    contentArea.transform.localPosition = new Vector3(vec.x, Mathf.Abs(selected.y) - 437.5f, vec.z);
            }
            // Moving Up
            else if (selectedItem < lastItem)
            {
                lastItem = selectedItem;

                // Set the current Position of the Content Area if we are not within range

                // Moving Up, and a Stats Item, Clamp must be less than or equal to selected items Y axis position - 25
                if (btns[selectedItem].name.Contains("Stats") && Mathf.Abs(vec.y) > (Mathf.Abs(selected.y) - 25.0f))
                    contentArea.transform.localPosition = new Vector3(vec.x, Mathf.Abs(selected.y) - 25.0f, vec.z);
                // Moving Up, and a Header Item, Clamp must be less than or equal to selected items Y axis position - 37.5
                else if (!btns[selectedItem].name.Contains("Stats") && Mathf.Abs(vec.y) > (Mathf.Abs(selected.y) - 37.5f))
                    contentArea.transform.localPosition = new Vector3(vec.x, Mathf.Abs(selected.y) - 37.5f, vec.z);
            }

        }

    }
}


