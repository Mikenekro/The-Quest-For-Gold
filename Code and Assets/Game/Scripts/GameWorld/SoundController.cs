using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace schoolRPG.Sound
{
    /// <summary>
    /// The situation the player is in determines which music should play
    /// </summary>
    public enum MusicType
    {
        DARK, LIGHT, EXPLORATION, FIGHT
    }

    /// <summary>
    /// Allows us to run the levelup, background music, and ambient sounds at the same time
    /// </summary>
    [System.Serializable]
    public class PlayerAudio
    {
        [SerializeField]
        private AudioSource levelup;
        [SerializeField]
        private AudioSource backgroundSource;
        [SerializeField]
        private AudioSource ambientSource;

        [SerializeField]
        private List<AudioClip> backgroundMusic;
        [SerializeField]
        private List<MusicType> type;
        [SerializeField]
        private List<AudioClip> ambient;
        [SerializeField]
        private AudioClip footstepLeft;
        [SerializeField]
        private AudioClip footstepRight;
        [SerializeField]
        private List<AudioClip> sword;

        public int BackgroundPos { get; set; }
        public int AmbientPos { get; set; }

        public AudioSource Levelup { get { return levelup; } }
        public AudioSource BackgroundSource { get { return backgroundSource; } }
        public AudioSource AmbientSource { get { return ambientSource; } }
        public AudioClip FootstepL { get { return footstepLeft; } }
        public AudioClip FootstepR { get { return footstepRight; } }
        public List<AudioClip> Sword { get { return sword; } }
        public List<AudioClip> Background { get { return backgroundMusic; } }
        public List<MusicType> BackgroundType { get { return type; } }
        public List<AudioClip> Ambient { get { return ambient; } }
    }

    public class SoundController : MonoBehaviour
    {
        public static PlayerAudio Audio;
        public static float master;
        public static float enviro;
        public static float music;

        private static MusicType curType;
        private static MusicType lastType;
        private static MusicType lastPlayed;
        // Bools to determine which music plays
        private static bool inBattle;
        private static bool inExplore;
        private static bool inDark;
        private static bool inLight;
        /// <summary>
        /// Are we in the Splash Screen?
        /// </summary>
        private static bool splash;

        [SerializeField]
        private PlayerAudio audio;
        
        private List<int> mPos;
        private float volume;
        private float lVolume;
        private int[] lastVal;
        private int lastPos;
        private int i;


        public void Awake()
        {
            Audio = audio;
            lastVal = new int[] { 0, 0, 0, 0 };
        }
        // Use this for initialization
        public void Start()
        {
            mPos = new List<int>();
            inExplore = true;

            lastPlayed = MusicType.FIGHT;
            curType = MusicType.EXPLORATION;
            lastType = MusicType.EXPLORATION;

            master = 1.0f;
            enviro = 1.0f;
            music = 1.0f;

            if (GameObject.Find("SplashTimer") != null)
                splash = true;
            else
                splash = false;

            if (PlayerPrefs.HasKey("MasterVolume") && !splash)
            {
                master = PlayerPrefs.GetFloat("MasterVolume");
                enviro = PlayerPrefs.GetFloat("EnvironmentVolume");
                music = PlayerPrefs.GetFloat("MusicVolume");
                
                volume = (music * master) * 0.2f;
                lVolume = enviro * master;
                audio.BackgroundSource.volume = (music * master) * 0.2f;
                audio.Levelup.volume = enviro * master;
            }
            else if (splash)
            {
                audio.BackgroundSource.volume = 0;
            }
        }

        public void OnEnable()
        {
            SoundMenu.ChangeVolume += Volume;

            if (PlayerPrefs.HasKey("MasterVolume") && !splash)
            {
                master = PlayerPrefs.GetFloat("MasterVolume");
                enviro = PlayerPrefs.GetFloat("EnvironmentVolume");
                music = PlayerPrefs.GetFloat("MusicVolume");

                volume = (music * master) * 0.2f;
                lVolume = enviro * master;
                audio.BackgroundSource.volume = (music * master) * 0.2f;
                audio.Levelup.volume = enviro * master;
            }
            else if (splash)
            {
                audio.BackgroundSource.volume = 0;
            }
        }

        public void OnDisable()
        {
            SoundMenu.ChangeVolume -= Volume;
        }

        // Update is called once per frame
        void Update()
        {
            if (!splash)
            {
                if (!Audio.BackgroundSource.isPlaying)
                {
                    // Only reset the list if we are changing types
                    if (lastPlayed != curType)
                    {
                        if (lastPlayed == MusicType.FIGHT)
                            lastVal[0] = i;
                        else if (lastPlayed == MusicType.EXPLORATION)
                            lastVal[1] = i;
                        else if (lastPlayed == MusicType.DARK)
                            lastVal[2] = i;
                        else if (lastPlayed == MusicType.LIGHT)
                            lastVal[3] = i;

                        lastPlayed = curType;
                        mPos.Clear();

                        for (i = 0; i < audio.Background.Count; ++i)
                        {
                            // If the track is not already set and this is the correct type of music
                            //if (inBattle && i != audio.BackgroundPos && audio.BackgroundType[i] == MusicType.FIGHT)
                            if (inBattle && audio.BackgroundType[i] == MusicType.FIGHT)
                                mPos.Add(i);
                            else if (inExplore && audio.BackgroundType[i] == MusicType.EXPLORATION)
                                mPos.Add(i);
                            else if (inDark && audio.BackgroundType[i] == MusicType.DARK)
                                mPos.Add(i);
                            else if (inLight && audio.BackgroundType[i] == MusicType.LIGHT)
                                mPos.Add(i);
                        }
                    }

                    // Set which position was the last song played for the specific type
                    if (curType == MusicType.FIGHT)
                        lastPos = 0;
                    else if (curType == MusicType.EXPLORATION)
                        lastPos = 1;
                    else if (curType == MusicType.DARK)
                        lastPos = 2;
                    else if (curType == MusicType.LIGHT)
                        lastPos = 3;

                    // Increment the track based on the last position that we played
                    if (lastVal[lastPos] + 1 >= mPos.Count - 1)
                        i = mPos[0];
                    else
                        i = mPos[lastVal[lastPos] + 1];

                    
                    // Tried Random Music, Plays same tracks too much
                    //// Get a random song from the list
                    //i = mPos[Random.Range(0, mPos.Count - 1)];
                    //// Try to not get the same song as last time
                    //if (i == audio.BackgroundPos && mPos.Count > 1)
                    //    i = mPos[Random.Range(0, mPos.Count - 1)];

                    audio.BackgroundPos = i;
                    audio.BackgroundSource.clip = audio.Background[audio.BackgroundPos];
                    Audio.BackgroundSource.Play();
                }
                else if (lastType != curType)
                {
                    // Fade the music out before stopping it to change types
                    if (audio.BackgroundSource.volume >= 0.003f)
                        audio.BackgroundSource.volume -= 0.003f;
                    else
                    {
                        // Set the last type and stop the music before playing the next track
                        lastType = curType;
                        audio.BackgroundSource.Stop();
                        audio.BackgroundSource.volume = volume;
                    }
                }
            }
        }

        public void Volume()
        {
            master = SoundMenu.sound.master;
            music = SoundMenu.sound.music;
            enviro = SoundMenu.sound.environment;

            volume = (music * master) * 0.2f;
            lVolume = enviro * master;
            audio.BackgroundSource.volume = (music * master) * 0.2f;
            audio.Levelup.volume = enviro * master;
        }

        /// <summary>
        /// Checks what type of music should be playing when an Enemy exits Battle with the Player
        /// </summary>
        public static void CheckMusicType()
        {
            int i;
            bool battle = false;

            // Check if any enemies are attacking the Player through the Battle Nodes
            if (!splash)
            {
                for (i = 0; i < PlayerController.NodeUse.Length; ++i)
                {
                    if (PlayerController.NodeUse[i])
                        battle = true;
                }
            }
            else
                battle = true;

            // Set the Battle state
            if (battle)
                SetMusicType(MusicType.FIGHT);
            else
                SetMusicType(MusicType.EXPLORATION);
        }

        /// <summary>
        /// Set the type of music to play
        /// </summary>
        /// <param name="type"></param>
        public static void SetMusicType(MusicType type)
        {
            inBattle = false;
            inExplore = false;
            inDark = false;
            inLight = false;

            if (type == MusicType.FIGHT)
                inBattle = true;
            else if (type == MusicType.DARK || WorldController.loc == Locations.PINEFOREST)
                inDark = true;
            else if (type == MusicType.LIGHT || WorldController.loc == Locations.DR_TOWNSQUARE || WorldController.loc == Locations.DS_TOWNSQUARE)
                inLight = true;
            else
                inExplore = true;

            if (inBattle)
                curType = MusicType.FIGHT;
            else if (inDark)
                curType = MusicType.DARK;
            else if (inLight)
                curType = MusicType.LIGHT;
            else
                curType = MusicType.EXPLORATION;

            Debug.Log("Current Music Type: " + curType.ToString());
        }

        public static void Footstep(AudioSource source, bool right)
        {
            if (right)
                source.clip = Audio.FootstepR;
            else
                source.clip = Audio.FootstepL;

            source.volume = 1.0f;
            source.Play();
        }

        public static int SwingSword(AudioSource source, int lastClip)
        {
            lastClip += 1;
            source.volume = 0.5f;

            if (lastClip >= Audio.Sword.Count)
            {
                lastClip = 0;
            }

            // Set the new clip and play
            source.volume = 0.5f;
            source.clip = Audio.Sword[lastClip];
            source.Play();

            return lastClip;
        }
    }
}


