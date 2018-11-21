using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TeamUtility.IO;

namespace schoolRPG
{
    public class ColorSelect : MonoBehaviour
    {
        private static bool colorKnobOn;


        [SerializeField]
        private float speed = 0.1f;
        [SerializeField]
        private Vector3 move;
        [SerializeField]
        private ColorSelector selector;
        private SpriteRenderer img;

        // Use this for initialization
        void Start()
        {
            colorKnobOn = false;
            move = new Vector3();
            img = gameObject.GetComponent<SpriteRenderer>();
            if (InputManager.PlayerOneConfiguration.name == "KeyboardAndMouse")
            {
                img.enabled = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            //if (InputManager.GetAxis("Horizontal") != 0)
            //    Debug.Log("Horizontal: " + InputManager.GetAxis("Horizontal"));
            //if (InputManager.GetAxis("Vertical") != 0)
            //    Debug.Log("Vertical: " + InputManager.GetAxis("Vertical"));
            //if (InputManager.PlayerOneConfiguration.name != "KeyboardAndMouse")
            //    Debug.Log("Input Name: " + InputManager.PlayerOneConfiguration.name);

            if ((!img.enabled && colorKnobOn) || (img.enabled && !colorKnobOn))
                img.enabled = colorKnobOn;

            // Cancel the Color Selector when the user backs out of the menu with a controller
            if (InputManager.GetButtonUp("Cancel") || InputManager.GetButtonUp("Pause"))
            {
                SetColorKnob(false);
                CreationMenu.PickingColor = false;
            }

            if (img.enabled)
            {
                // Use the movement axis' to change the color of the selected item
                if (InputManager.PlayerOneConfiguration.name != "KeyboardAndMouse")
                {
                    if (InputManager.GetAxis("Horizontal") != 0)
                    {
                        move.x = InputManager.GetAxis("Horizontal") * speed * (Time.deltaTime);
                        if (move.x > 1)
                            move.x = 1;
                    }
                    else
                        move.x = 0;
                    if (InputManager.GetAxis("Vertical") != 0)
                    {
                        move.y = InputManager.GetAxis("Vertical") * speed * (Time.deltaTime);
                        if (move.y > 1)
                            move.y = 1;
                    }
                    else
                        move.y = 0;

                    if (move != Vector3.zero)
                    {
                        transform.Translate(move);
                        selector.origin = transform.position;
                        selector.SelectColor(true);
                    }
                }
                
            }
            
        }
        

        /// <summary>
        /// Calls the Static version of this function
        /// </summary>
        /// <param name="isOn"></param>
        public void SetColorKnob(bool isOn)
        {
            StaticSetColorKnob(isOn);
        }
        /// <summary>
        /// Turns the knob for choosing colors to on
        /// </summary>
        /// <param name="isOn"></param>
        public static void StaticSetColorKnob(bool isOn)
        {
            // Make sure the ColorKnob is not turned on when we are using the keyboard
            if (InputManager.PlayerOneConfiguration.name != "KeyboardAndMouse")
                colorKnobOn = isOn;
        }




    }
}

