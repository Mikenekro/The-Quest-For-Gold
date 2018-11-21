using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace schoolRPG
{
    /// <summary>
    /// Individual Sound to register for the OnVolumeChange event
    /// </summary>
    public class IndiSound : MonoBehaviour
    {
        private AudioSource source;


        // Use this for initialization
        public void Start()
        {
            if (GetComponent<AudioSource>() != null)
                source = GetComponent<AudioSource>();

            SoundMenu.ChangeVolume += UpdateVolume;
        }

        public void OnDestroy()
        {
            SoundMenu.ChangeVolume -= UpdateVolume;
        }

        public void UpdateVolume()
        {
            source.volume = SoundMenu.sound.master * SoundMenu.sound.music;
        }
    }
}

