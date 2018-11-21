using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace schoolRPG
{
    public class OptionsMenu : MonoBehaviour
    {
        public MainMenuController mmc;
        public Button firstSelect;

        public GameObject optionsTemplate;
        public GameObject control;
        public GameObject graphic;
        public GameObject game;
        public GameObject sound;
        public TextMeshProUGUI title;

        private Button[] btns;

        public void Start()
        {
            OnEnable();
        }

        public void OnEnable()
        {
            if (btns == null)
                btns = gameObject.GetComponentsInChildren<Button>();

            control.SetActive(false);
            graphic.SetActive(false);
            game.SetActive(false);
            sound.SetActive(false);
            optionsTemplate.SetActive(false);
            if (firstSelect != null)
                StartCoroutine(HoverSelect.SelectFirstButton(firstSelect));
        }
        public void OnDisable()
        {
            optionsTemplate.SetActive(false);
        }

        private void SetButtons(bool interactable)
        {
            for (int i = 0; i < btns.Length; ++i)
                btns[i].interactable = interactable;
        }

        /// <summary>
        /// Closes the Options Template
        /// </summary>
        public void CloseTemplate()
        {
            control.SetActive(false);
            graphic.SetActive(false);
            game.SetActive(false);
            sound.SetActive(false);
            optionsTemplate.SetActive(false);
            SetButtons(true);
            if (firstSelect != null)
                StartCoroutine(HoverSelect.SelectFirstButton(firstSelect));
        }

        /// <summary>
        /// Start the menu where the player can choose their own controls
        /// </summary>
        public void Controls()
        {
            optionsTemplate.SetActive(true);
            control.SetActive(true);
            SetButtons(false);

            title.text = "Control Settings";
        }
        /// <summary>
        /// Start the menu where the player can choose different game options (such as time in 1 day and difficulty)
        /// </summary>
        public void GameOptions()
        {
            optionsTemplate.SetActive(true);
            game.SetActive(true);
            SetButtons(false);

            title.text = "Game Settings";
        }
        /// <summary>
        /// Start the menu where the player can choose different Graphic options
        /// </summary>
        public void GraphicOptions()
        {
            optionsTemplate.SetActive(true);
            graphic.SetActive(true);
            SetButtons(false);

            title.text = "Graphic Settings";
        }
        /// <summary>
        /// Start the menu where the player can choose different sound options
        /// </summary>
        public void SoundOptions()
        {
            optionsTemplate.SetActive(true);
            sound.SetActive(true);
            SetButtons(false);

            title.text = "Sound Settings";
        }
        /// <summary>
        /// Close the options menu
        /// </summary>
        public void CloseMenu()
        {
            if (mmc != null)
                mmc.CloseOptions();
        }
    }
}


