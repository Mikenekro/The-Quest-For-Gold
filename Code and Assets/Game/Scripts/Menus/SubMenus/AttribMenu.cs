using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using schoolRPG.Items;
using UnityEngine.UI;
using TMPro;

namespace schoolRPG.Stats
{
    public class AttribMenu : MonoBehaviour
    {
        private bool _createMenu;
        /// <summary>
        /// Are we using the Creation Menu or the General Attribute Screen
        /// </summary>
        private static bool createMenu; 

        private int spentPoints;
        private int spentStr;
        private int spentEnd;
        private int spentInt;
        private int spentSpd;

        [SerializeField]
        private PlayerController pc;

        [SerializeField]
        private TextMeshProUGUI strengthT;
        [SerializeField]
        private TextMeshProUGUI enduranceT;
        [SerializeField]
        private TextMeshProUGUI intelligenceT;
        [SerializeField]
        private TextMeshProUGUI speedT;
        [SerializeField]
        private TextMeshProUGUI points;
        [SerializeField]
        private TextMeshProUGUI attribName;
        [SerializeField]
        private TextMeshProUGUI attribDesc;

        public Button confirm;
        public Button reset;

        /// <summary>
        /// The button this screen will select when it is opened
        /// </summary>
        public Button startButton;

        // Use this for initialization
        void Start()
        {
            
        }

        void OnEnable()
        {
            if (createMenu)
            {
                pc.Player.Init(true, true);
                if (pc.Player.Strength < 10)
                {
                    pc.Player.AddAttribute(AttributeEnum.ENDURANCE, 9);
                    pc.Player.AddAttribute(AttributeEnum.INTELLIGENCE, 9);
                    pc.Player.AddAttribute(AttributeEnum.SPEED, 9);
                    pc.Player.AddAttribute(AttributeEnum.STRENGTH, 9);
                }
                
            }

            resetPoints(false);
            attribName.text = "Strength";
            attribDesc.text = pc.Player.GetAttribDesc(AttributeEnum.STRENGTH);
            // Select the first button when the inventory opens
            StartCoroutine(HoverSelect.SelectFirstButton(startButton));
        }
        void OnDisable()
        {
            resetPoints(false);
        }

        /// <summary>
        /// Sets the script to use the Creation Screen or the General Attribute Menu
        /// </summary>
        /// <param name="creationScreen"></param>
        public static void setOpenMenu(bool creationScreen)
        {
            createMenu = creationScreen;
        }

        /// <summary>
        /// Resets all spent points to zero
        /// </summary>
        public void resetPoints(bool select)
        {
            spentPoints = 0;
            spentStr = 0;
            spentInt = 0;
            spentEnd = 0;
            spentSpd = 0;

            // Should we select the button above Reset?
            if (select)
                reset.FindSelectableOnUp().Select();
            setText();
        }
        /// <summary>
        /// Confirms and Approves any points that we have spent. Called by the Confirm Button
        /// </summary>
        public void confirmPoints()
        {
            if ((!createMenu && spentPoints <= pc.Player.AttributePoints) || 
                (createMenu && spentPoints == pc.Player.AttributePoints))
            {
                if (spentStr > 0 && pc.Player.Strength + spentStr < 255)
                    pc.Player.AddAttribute(AttributeEnum.STRENGTH, (sbyte)spentStr);
                else
                    spentStr = 0;
                if (spentEnd > 0 && pc.Player.Endurance + spentEnd < 255)
                    pc.Player.AddAttribute(AttributeEnum.ENDURANCE, (sbyte)spentEnd);
                else
                    spentEnd = 0;
                if (spentInt > 0 && pc.Player.Intelligence + spentInt < 255)
                    pc.Player.AddAttribute(AttributeEnum.INTELLIGENCE, (sbyte)spentInt);
                else
                    spentInt = 0;
                if (spentSpd > 0 && pc.Player.Speed < 255)
                    pc.Player.AddAttribute(AttributeEnum.SPEED, (sbyte)spentSpd);
                else
                    spentSpd = 0;

                spentPoints = spentStr + spentEnd + spentInt + spentSpd;

                pc.Player.AttributePoints -= spentPoints;

                confirm.FindSelectableOnUp().Select();
                resetPoints(false);
                setText();

                if (createMenu)
                {
                    gameObject.GetComponentInParent<CreationMenu>().NextMenu(true);
                }
            }
            else if (createMenu)
            {
                StartCoroutine(
                    gameObject.GetComponentInParent<CreationMenu>().DisplayMSG(
                        "<color=red>You must use all of your Attribute Points to continue!</color>"));
            }
        }

        /// <summary>
        /// Sets the Text when a change is made
        /// </summary>
        public void setText()
        {
            if (createMenu)
            {
                strengthT.text = "Strength: " + (pc.Player.Strength + spentStr).ToString();
                enduranceT.text = "Endurance: " + (pc.Player.Endurance + spentEnd).ToString();
                intelligenceT.text = "Intelligence: " + (pc.Player.Intelligence + spentInt).ToString();
                speedT.text = "Speed: " + (pc.Player.Speed + spentSpd).ToString();
                points.text = "Attribute Points: " + (pc.Player.AttributePoints - spentPoints).ToString();
            }
            else
            {
                strengthT.text = "Strength: " + pc.Player.Strength.ToString("0") + ((spentStr != 0)?("<color=green>" +
                    "(+" + spentStr + ")" + "</color>"):(""));
                enduranceT.text = "Endurance: " + pc.Player.Endurance.ToString("0") + ((spentEnd != 0)?("<color=green>" +
                    "(+" + spentEnd + ")" + "</color>"):(""));
                intelligenceT.text = "Intelligence: " + pc.Player.Intelligence.ToString("0") + ((spentInt != 0)?("<color=green>" +
                    "(+" + spentInt + ")" + "</color>"):(""));
                speedT.text = "Speed: " + pc.Player.Speed.ToString("0") + ((spentSpd != 0)?("<color=green>" +
                    "(+" + spentSpd + ")" + "</color>"):(""));
                points.text = "Attribute Points: " + pc.Player.AttributePoints.ToString("0") + ((spentPoints != 0)?("<color=red>" +
                    "(-" + spentPoints + ")" + "</color>"):(""));
            }
            
            setButtons();
        }
        /// <summary>
        /// Controls if we can select the "Confirm" or "Reset" Buttons 
        /// </summary>
        public void setButtons()
        {
            if (spentPoints > 0)
            {
                confirm.gameObject.SetActive(true);
                reset.gameObject.SetActive(true);
            }
            else
            {
                confirm.gameObject.SetActive(false);
                reset.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Called when one of the Strength buttons are pressed. Called by the "Str Up" or "Str Down" buttons
        /// </summary>
        /// <param name="up"></param>
        public void OnStrength(bool up)
        {
            attribName.text = "Strength";
            attribDesc.text = pc.Player.GetAttribDesc(AttributeEnum.STRENGTH);

            if (up && pc.Player.AttributePoints > spentPoints)
            {
                spentPoints++;
                spentStr++;
            }
            else if (!up && spentStr > 0)
            {
                spentPoints--;
                spentStr--;
            }

            setText();
        }
        /// <summary>
        /// Called when one of the Endurance buttons are pressed. Called by the "End Up" or "End Down" buttons
        /// </summary>
        /// <param name="up"></param>
        public void OnEndurance(bool up)
        {
            attribName.text = "Endurance";
            attribDesc.text = pc.Player.GetAttribDesc(AttributeEnum.ENDURANCE);
            
            if (up && pc.Player.AttributePoints > spentPoints)
            {
                spentPoints++;
                spentEnd++;
            }
            else if (!up && spentEnd > 0)
            {
                spentPoints--;
                spentEnd--;
            }

            setText();
        }
        /// <summary>
        /// Called when one of the Intelligence buttons are pressed. Called by the "Int Up" or "Int Down" buttons
        /// </summary>
        /// <param name="up"></param>
        public void OnIntelligence(bool up)
        {
            attribName.text = "Intelligence";
            attribDesc.text = pc.Player.GetAttribDesc(AttributeEnum.INTELLIGENCE);

            if (up && pc.Player.AttributePoints > spentPoints)
            {
                spentPoints++;
                spentInt++;
            }
            else if (!up && spentInt > 0)
            {
                spentPoints--;
                spentInt--;
            }

            setText();
        }

        int val;
        public void Change(TextMeshProUGUI texting)
        {
            val += 1;
            texting.text = "New: " + val;
        }

        /// <summary>
        /// Called when one of the Speed buttons are pressed. Called by the "Speed Up" or "Speed Down" buttons
        /// </summary>
        /// <param name="up"></param>
        public void OnSpeed(bool up)
        {
            attribName.text = "Speed";
            attribDesc.text = pc.Player.GetAttribDesc(AttributeEnum.SPEED);

            if (up && pc.Player.AttributePoints > spentPoints)
            {
                spentPoints++;
                spentSpd++;
            }
            else if (!up && spentSpd > 0)
            {
                spentPoints--;
                spentSpd--;
            }

            setText();
        }
    }
}

