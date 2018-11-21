using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace schoolRPG
{
    public class WaitMenu : MonoBehaviour
    {
        public delegate IEnumerator OnWait(int hours, Slider slide, TextMeshProUGUI txt);
        public static event OnWait Wait;

        [SerializeField]
        private Slider slider;
        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        private Button selectFirst;
        [SerializeField]
        private Button other;

        // Use this for initialization
        private void Start()
        {
            if (!WorldController.InWait)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            selectFirst.enabled = true;
            other.enabled = true;

            StartCoroutine(HoverSelect.SelectFirstButton(selectFirst));

            text.text = slider.value.ToString() + " Hours";
        }

        private void Update()
        {
            if (InputManager.GetButtonUp("NextMenu"))
            {
                slider.value += 1;
                text.text = slider.value.ToString() + " Hours";
            }
            else if (InputManager.GetButtonUp("PrevMenu"))
            {
                slider.value -= 1;
                text.text = slider.value.ToString() + " Hours";
            }
        }

        public void SetSlider()
        {
            text.text = slider.value.ToString() + " Hours";
        }

        public void Cancel()
        {
            WorldController.InWait = false;
            WorldController.TimeScale(true);
            gameObject.SetActive(false);
        }

        public void StartWait()
        {
            // IF we are not already waiting
            if (!WorldController.InWait)
            {
                selectFirst.enabled = false;
                other.enabled = false;
                WorldController.InWait = true;
                // Reset the players health, stamina, magicka
                PlayerController.Pc.Player.SetBaseStats(true);
                // Call the Wait routine in the WorldController
                StartCoroutine(Wait((int)slider.value, slider, text));
            }
        }

    }
}

