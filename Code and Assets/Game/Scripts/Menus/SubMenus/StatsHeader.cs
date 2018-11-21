using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

namespace schoolRPG.Stats
{
    public class StatsHeader : MonoBehaviour
    {
        [SerializeField]
        private PlayerController pc;
        private TextMeshProUGUI[] menuUI;
        private Slider sli;
        

        // Use this for initialization
        void Start()
        {
            menuUI = gameObject.GetComponentsInChildren<TextMeshProUGUI>();
            sli = gameObject.GetComponentInChildren<Slider>();
        }

        void OnEnable()
        {
            if (menuUI == null)
                Start();

            if (menuUI != null)
            {
                menuUI[1].text = "Name: " + pc.Player.Name;
                menuUI[2].text = "Level: " + pc.Player.Level;
                menuUI[3].text = "Age: " + pc.Player.Age;
                menuUI[4].text = "Gender: " + pc.Player.Gender;
                menuUI[5].text = "Race: " + pc.Player.Race;
                menuUI[6].text = "Days Passed: " + WorldController.Data.DaysPassed;
                menuUI[8].text = pc.Player.SkillsGainedThisLevel + "/" + pc.Player.SkillsToLevelup;
                sli.maxValue = pc.Player.SkillsToLevelup;
                sli.minValue = 0;
                sli.value = pc.Player.SkillsGainedThisLevel;
            }
            
        }
    }
}

