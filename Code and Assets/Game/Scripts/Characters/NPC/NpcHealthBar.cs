using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace schoolRPG
{
    /// <summary>
    /// Attach this to the NPC's HealthBar Slider to hide it when not in use
    /// </summary>
    public class NpcHealthBar : MonoBehaviour
    {
        [SerializeField, Tooltip("Enter the amount of Time that the health bar will be displayed.")]
        private float displayTime;
        private float lastValue;
        private bool first;
        private Slider slide;

        /// <summary>
        /// Time the displayHealth() Coroutine takes for 1 iteration
        /// </summary>
        private WaitForSeconds wfs;
        /// <summary>
        /// Coroutine Iterates every x seconds
        /// </summary>
        private float displayIteration = 0.1f;
        /// <summary>
        /// Time that we have been in the displayHealth() Coroutine
        /// </summary>
        private float timeElapsed;
        /// <summary>
        /// Are we in the coroutine?
        /// </summary>
        private bool inCoroutine;
        /// <summary>
        /// Fill and Background of the Slider
        /// </summary>
        private Image[] img;

        private bool true1 = true;
        private bool false1 = false;

        // Use this for initialization
        void Start()
        {
            slide = GetComponent<Slider>();
            first = true;
            inCoroutine = false;
            timeElapsed = 0.0f;
            wfs = new WaitForSeconds(displayIteration);
            img = gameObject.GetComponentsInChildren<Image>();
            for (int i = 0; i < img.Length; ++i)
                img[i].enabled = false;
        }

        /// <summary>
        /// Call this function when you want to change the NPC's health bar value without displaying
        /// </summary>
        public IEnumerator NoShow()
        {
            // Waits a frame to run any code before resetting
            inCoroutine = true1;
            yield return null;
            inCoroutine = false1;
        }
        
        /// <summary>
        /// Called when the Health Bar changes value
        /// </summary>
        public void OnHealthChange()
        {
            // If we have not started the coroutine yet
            if ((!inCoroutine && lastValue > slide.value) || first)
            {
                inCoroutine = true;
                StartCoroutine(DisplayHealth());
            }
            else // If the health bar changes and we are already in the corouting
            {
                // Reset the timeElapsed so the health bar always displays for the 
                // displayTime after being changed
                timeElapsed = 0;
            }

            if (slide != null)
                lastValue = slide.value;
            if (first)
                first = false;

            if (slide != null && slide.value <= 0)
            {
                inCoroutine = false;
                for (int i = 0; i < img.Length; ++i)
                {
                    img[i].enabled = false;
                }
            }
            
        }

        public IEnumerator DisplayHealth()
        {
            // Enable the images if we can
            if (slide.value > 0)
                for (int i = 0; i < img.Length; ++i)
                    img[i].enabled = true;

            // Loop while we have not elapsed the set time
            while (timeElapsed < displayTime && inCoroutine)
            {
                // Wait for iteration and add to the time elapsed
                yield return wfs;
                timeElapsed += displayIteration;
            }

            // Disable the images
            for (int i = 0; i < img.Length; ++i)
                img[i].enabled = false;
            // Reset the Time Elapsed
            timeElapsed = 0;
            // Exit the coroutine
            inCoroutine = false;
            yield return null;
        }
    }
}

