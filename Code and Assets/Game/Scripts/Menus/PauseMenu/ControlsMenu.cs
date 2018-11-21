using System;
using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace schoolRPG
{
    /// <summary>
    /// Each type of control that we can set will be listed here
    /// </summary>
    public enum InputConfigs
    {
        RUN, ATTACK, CAST, USE, DROP, PAUSE, SUBMIT, CANCEL, NEXTMENU, PREVMENU, OPENINV, OPENEQUIP, OPENATTRIB, OPENSKILL, OPENQUEST, OPENSTATS
    }

    /// <summary>
    /// Menu that allows the Player to change the default controls of the game
    /// </summary>
    public class ControlsMenu : MonoBehaviour
    {
        private int i, k;

        private InputConfiguration currentKeyboard;
        private InputConfiguration currentXbox;
        private string defaultKeyboard;
        private string saveKeyboard;

        [SerializeField, Tooltip("Set every Key the user can use in the game")]
        private List<KeyCode> keyboardKeys;
        [SerializeField, Tooltip("Set every Joystick Key the user can use in the game. (AKA. Xbox controllers, PS4 controllers, etc...")]
        private List<KeyCode> joyKeys;

        [SerializeField, Tooltip("The Dialogue box that will let the Player know they are attempting to change a key")]
        private GameObject dialogBox;

        private List<string> dialogQueue;

        [SerializeField]
        private GameObject contentArea;
        private Button[] btns;

        /// <summary>
        /// The text on the button we are pressing
        /// </summary>
        private TextMeshProUGUI selectedTxt;
        /// <summary>
        /// What key are we currently changing
        /// </summary>
        private string keyChange;
        /// <summary>
        /// Main key or alternate key?
        /// </summary>
        private bool mainKey;
        private bool canScan;
        private bool badKey;
        
        /// <summary>
        /// A List of every Key the user can set for controls
        /// </summary>
        public List<KeyCode> KeyboardKeys { get { return keyboardKeys; } }
        /// <summary>
        /// A List of every Key the user can set for controls with a Joystick/Controller
        /// </summary>
        public List<KeyCode> JoystickKeys { get { return joyKeys; } }
        


        // Use this for initialization
        void Start()
        {
            // Set the current input configs
            currentKeyboard = InputManager.Instance.inputConfigurations[0];
            currentXbox = InputManager.Instance.inputConfigurations[4];
            defaultKeyboard = InputManager.Instance.playerOneDefault;
            saveKeyboard = "SaveSetup1";

            // Save the default key layout
            InputManager.Instance.playerOneDefault = currentKeyboard.name;
            InputManager.Save("GameData/Config/" + currentKeyboard.name + ".xml");

            InputConfiguration ic = new InputConfiguration(saveKeyboard);
            ic.axes = currentKeyboard.axes;
            ic.isExpanded = currentKeyboard.isExpanded;

            InputManager.Instance.inputConfigurations.Add(ic);
            InputManager.Instance.playerOneDefault = ic.name;

            // Get each control button in the scene
            btns = contentArea.GetComponentsInChildren<Button>();

            dialogQueue = new List<string>();
            dialogBox.SetActive(false);
            canScan = false;
            keyChange = "";
        }

        // Update is called once per frame
        void Update()
        {
            // If the player wants to change a key 
            if (canScan && keyChange != "")
            {
                // Check if we want to cancel scanning
                if (InputManager.GetKeyDown(KeyCode.BackQuote))
                {
                    canScan = false;
                    keyChange = "";
                    dialogQueue.Clear();
                    dialogBox.SetActive(false);
                }

                // Scan for a control key to change
                ScanKeys();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadBtnTxt()
        {
            int i, k;
            bool isAlt = false;
            TextMeshProUGUI txt;

            for (i = 0; i < btns.Length; ++i)
            {
                if (btns[i].gameObject.activeInHierarchy && btns[i].gameObject.GetComponentInChildren<TextMeshProUGUI>() != null)
                {
                    txt = btns[i].gameObject.GetComponentInChildren<TextMeshProUGUI>();

                    for (k = 0; k < InputManager.PlayerOneConfiguration.axes.Count; ++k)
                    {
                        if (!isAlt && InputManager.PlayerOneConfiguration.axes[k].name == btns[i].gameObject.transform.parent.name)
                        {
                            txt.text = InputManager.PlayerOneConfiguration.axes[k].positive.ToString();
                            if (txt.text == "None")
                                txt.text = "_";
                            break;
                        }
                        else if (isAlt && InputManager.PlayerOneConfiguration.axes[k].name == btns[i].gameObject.transform.parent.name)
                        {
                            txt.text = InputManager.PlayerOneConfiguration.axes[k].altPositive.ToString();
                            if (txt.text == "None")
                                txt.text = "_";
                            break;
                        }
                    }

                    isAlt = !isAlt;
                }
            }
        }

        public void DefaultKeys()
        {
            string filename = System.IO.Path.Combine(
                Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/QuestForGold/GameData/Config/",
                currentKeyboard.name + ".xml");

            InputManager.Load(filename);
            InputManager.Instance.playerOneDefault = defaultKeyboard;

            LoadBtnTxt();
        }

        public void SaveKeys()
        {
            string filename = System.IO.Path.Combine(
                Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/QuestForGold/GameData/Config/", 
                saveKeyboard + ".xml");

            InputManager.Instance.playerOneDefault = saveKeyboard;
            
            InputManager.Save(filename);
        }

        public void LoadKeys()
        {
            string filename = System.IO.Path.Combine(
                Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/QuestForGold/GameData/Config/",
                saveKeyboard + ".xml");

            try
            {
                InputManager.Load(filename);

                InputManager.Instance.playerOneDefault = saveKeyboard;

                LoadBtnTxt();
            }
            catch (System.Exception e)
            {
                Debug.Log("Controls Load Error: " + e.Message);
            }

            
        }


        /// <summary>
        /// Scan the users input for changing one of the Control keys
        /// </summary>
        public void ScanKeys()
        {
            bool first = true;
            bool inUse = false;
            string key = "";
            string useName = "";

            // Loop through any potential keyboard keys
            for (i = 0; i < keyboardKeys.Count; ++i)
            {
                // If the player presses one of the keyboard keys
                if (InputManager.GetKeyDown(keyboardKeys[i]))
                {
                    first = true;

                    // Loop through each of the current configured keys
                    for (k = 0; k < currentKeyboard.axes.Count; ++k)
                    {
                        // If this is the first pass
                        if (first)
                        {
                            // Check if this key is already in use by the active configuration
                            if ((InputManager.PlayerOneConfiguration.axes[k].positive == keyboardKeys[i] || InputManager.PlayerOneConfiguration.axes[k].altPositive == keyboardKeys[i]) ||
                                (InputManager.PlayerOneConfiguration.axes[k].negative == keyboardKeys[i] || InputManager.PlayerOneConfiguration.axes[k].altNegative == keyboardKeys[i]))
                            {
                                // Leave the loop since we cannot change the key
                                inUse = true;
                                badKey = true;
                                key = keyboardKeys[i].ToString();
                                useName = InputManager.PlayerOneConfiguration.axes[k].name;
                                break;
                            }

                            // reset at the end of checking
                            if (k == currentKeyboard.axes.Count - 1)
                            {
                                first = false;
                                k = 0;
                            }
                        }
                        // If the keyboard axes name is the same as the key we are changing and we have already checked if this key is in use
                        else if (InputManager.PlayerOneConfiguration.axes[k].name.ToLower() == keyChange.ToLower())
                        {
                            // Change the Positive key, or the Alt Positive key
                            if (mainKey)
                                InputManager.PlayerOneConfiguration.axes[k].positive = keyboardKeys[i];
                            else
                                InputManager.PlayerOneConfiguration.axes[k].altPositive = keyboardKeys[i];

                            AddTxtQueue("Changed " + ((mainKey)?("Main Key "):("Alternate Key ")) + 
                                InputManager.PlayerOneConfiguration.axes[k].name + " to key " + keyboardKeys[i].ToString().ToUpper() + "!", true, 5.0f);
                            // Reset any values
                            selectedTxt.text = keyboardKeys[i].ToString().ToUpper();
                            selectedTxt = null;
                            canScan = false;
                            badKey = false;
                            keyChange = "";
                            mainKey = false;
                        }
                    }

                    // Leave the outer loop if the key is in use
                    if (inUse)
                        break;
                }
            }

            if (inUse)
            {
                canScan = false;
                keyChange = "";
                mainKey = false;
                AddTxtQueue("<color=red>Cannot change key. Key \"" + key + "\" already in use for \"" + useName + "\"!</color>", true, 5.0f);
            }

        } // End ScanKey()

        /// <summary>
        /// Adds text to the Dialogue Queue to be displayed
        /// </summary>
        /// <param name="txt"></param>
        private void AddTxtQueue(string txt, bool removeAll, float time)
        {
            if (removeAll)
                dialogQueue.Clear();

            dialogQueue.Add(txt);

            if (dialogQueue.Count <= 1)
                StartCoroutine(DspTxt(time));
        }

        /// <summary>
        /// Displays any text in the Dialogue Queue while we are scanning
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private IEnumerator DspTxt(float time)
        {
            float curTime = 0.0f;
            WaitForSeconds wfs = new WaitForSeconds(0.1f);
            TextMeshProUGUI textComp;

            if (!dialogBox.activeInHierarchy)
                dialogBox.SetActive(true);

            textComp = dialogBox.GetComponentInChildren<TextMeshProUGUI>();

            // While there is dialogue left and we can scan or there was a bad key pressed
            while (dialogQueue.Count > 0 && (canScan || badKey))
            {
                textComp.text = dialogQueue[0];
                dialogQueue.RemoveAt(0);
                curTime = 0.0f;

                while (curTime < time)
                {
                    if (!canScan && !badKey)
                        break;

                    curTime += 0.1f;
                    yield return wfs;
                }
                
            }

            // Reset scanning if timeout
            canScan = false;
            badKey = false;
            keyChange = "";
            textComp.text = "";
            dialogQueue.Clear();
            dialogBox.SetActive(false);

            yield return null;
        }

        /// <summary>
        /// Lets the script know that we will be changing this key
        /// </summary>
        /// <param name="keyName"></param>
        public void SetKey(string keyName)
        {
            mainKey = !keyName.ToLower().Contains("alt_");
            canScan = true;
            badKey = false;

            // Set the name of the key we will be changing
            if (mainKey)
            {
                keyChange = keyName;
            }
            else
            {
                keyChange = keyName.Replace("alt_", "");
            }

            // Let the user know to press a key
            AddTxtQueue("Press any Key to replace the input for " + keyChange + " \n\nPress ~ to cancel", true, 25.0f);

            // Maybe implement "InputManager.StartKeyboardButtonScan()"
        }
        /// <summary>
        /// Call this along with SetKey so we can change the text on the button when a key is changed
        /// </summary>
        /// <param name="txt"></param>
        public void SetBtnTxt(TextMeshProUGUI txt)
        {
            selectedTxt = txt;
        }


    }

}
