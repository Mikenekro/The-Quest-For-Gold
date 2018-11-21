using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace schoolRPG
{
    /// <summary>
    /// Used to store all GameOptions that we want to use
    /// </summary>
    public class GameOptions
    {
        public float AutosaveInterval { get; set; }
        public float SecondsPerDay { get; set; }
        public float RespawnRate { get; set; }
    }

    public class GameOptionsMenu : MonoBehaviour
    {
        public delegate void OnSaveOptions();
        public static event OnSaveOptions SaveOptions;

        public GameOptions options;

        public List<Slider> sliders;
        public List<TextMeshProUGUI> textVals;

        public Toggle tog;

        private bool canAutosave;


        // Use this for initialization
        public void OnEnable()
        {
            options = new GameOptions();

            if (PlayerPrefs.HasKey("AutosaveInterval"))
            {
                sliders[0].value = PlayerPrefs.GetFloat("AutosaveInterval") / 60;
                sliders[1].value = PlayerPrefs.GetFloat("SecondsPerDay") / 60;
                sliders[2].value = PlayerPrefs.GetFloat("RespawnRate");

                if (PlayerPrefs.GetInt("CanAutosave") == 1)
                    canAutosave = true;
                else
                    canAutosave = false;

                tog.isOn = canAutosave;
            }
            else
                tog.isOn = true;

            for (int i = 0; i < sliders.Count; ++i)
            {
                if (textVals.Count >= i)
                {
                    string add = "";
                    if (textVals[i].text.Split(' ')[1] != null)
                        add = textVals[i].text.Split(' ')[1];
                    textVals[i].text = sliders[i].value.ToString("###,##0.##") + " " + add;
                }
            }
        }

        public void Save()
        {
            PlayerPrefs.SetFloat("AutosaveInterval", sliders[0].value * 60);
            PlayerPrefs.SetFloat("SecondsPerDay", sliders[1].value * 60);
            PlayerPrefs.SetFloat("RespawnRate", sliders[2].value);
            PlayerPrefs.SetInt("CanAutosave", ((canAutosave)?(1):(0)));
            PlayerPrefs.Save();

            if (SaveOptions != null)
                SaveOptions();
        }

        public void ToggleAutosave()
        {
            canAutosave = tog.isOn;
            Debug.Log("Can Autosave?: " + canAutosave);
        }

        public void SetValue(int pos)
        {
            if (textVals.Count >= pos)
            {
                string add = "";
                if (textVals[pos].text.Split(' ')[1] != null)
                    add = textVals[pos].text.Split(' ')[1];
                textVals[pos].text = sliders[pos].value.ToString("###,##0.##") + " " + add;
            }
                
        }
    }
}

