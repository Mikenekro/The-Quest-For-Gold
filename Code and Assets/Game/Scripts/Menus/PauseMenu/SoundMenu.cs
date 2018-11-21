using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace schoolRPG
{
    /// <summary>
    /// Class used to store the Volumes for the Audio Source
    /// </summary>
    public class SoundVolumes
    {
        public float master;
        public float environment;
        public float music;
    }

    public class SoundMenu : MonoBehaviour
    {
        // Delegates for changing Audio Volume
        public delegate void OnVolumeChange();
        public static event OnVolumeChange ChangeVolume;

        public static SoundVolumes sound;

        public Slider master;
        public Slider music;
        public Slider environment;

        private bool loaded = false;
        

        public void OnEnable()
        {
            sound = new SoundVolumes();
            master.maxValue = 1;
            music.maxValue = 1;
            environment.maxValue = 1;

            if (PlayerPrefs.HasKey("MasterVolume"))
            {
                master.value = PlayerPrefs.GetFloat("MasterVolume");
                music.value = PlayerPrefs.GetFloat("MusicVolume");
                environment.value = PlayerPrefs.GetFloat("EnvironmentVolume");
            }

            loaded = true;
        }
        public void OnDisable()
        {
            loaded = false;
        }

        public void SetVals()
        {
            if (loaded)
            {
                sound.master = master.value;
                sound.music = music.value;
                sound.environment = environment.value;

                if (ChangeVolume != null)
                    ChangeVolume();

                Save();
            }
        }

        /// <summary>
        /// Saves the Sound Volumes
        /// </summary>
        public void Save()
        {
            PlayerPrefs.SetFloat("MasterVolume", sound.master);
            PlayerPrefs.SetFloat("MusicVolume", sound.music);
            PlayerPrefs.SetFloat("EnvironmentVolume", sound.environment);
            PlayerPrefs.Save();
        }


    }
}

