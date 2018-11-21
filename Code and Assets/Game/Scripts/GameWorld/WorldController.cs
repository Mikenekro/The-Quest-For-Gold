using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Input manager
using TeamUtility.IO;

using schoolRPG.Stats;
using schoolRPG.Items;
using schoolRPG.SaveLoad;
using UnityEngine.UI;
using TMPro;
using System;
using schoolRPG.Sound;

/// <summary>
/// Main Namespace for RPG
/// </summary>
namespace schoolRPG
{
    public class WorldController : MonoBehaviour
    {
        public delegate void OnNpcData(bool setData);
        /// <summary>
        /// Retrieves all NPC's data before saving
        /// </summary>
        public static event OnNpcData NpcGetData;
        public delegate void OnPlayerData(bool setData);
        /// <summary>
        /// Retrieves the Players data before saving
        /// </summary>
        public static event OnPlayerData PlayerGetData;

        public delegate void OnMerchantReset();
        public static event OnMerchantReset ResetMerchant;

        private static Spells spells;
        private static GameObject spp;
        private static GameObject _npcHoverUi;
        private static float n9ne;
        private static float _maxDamage = 1000.0f;
        private static float _maxArmor = 500.0f;
        private static TextMeshProUGUI _useMsg;
        
        /// <summary>
        /// The maximum value any Character can deal to another Character
        /// </summary>
        public static float MaxDamage { get { return _maxDamage; } }
        /// <summary>
        /// The maximum value any armor in the game can take
        /// </summary>
        public static float MaxArmor { get { return _maxArmor; } }

        /// <summary>
        /// Value that is used to determine that what is being passed should not be used as a real value
        /// </summary>
        public static float TechN9ne { get { return n9ne; } }
        /// <summary>
        /// The name of the game we are loading
        /// </summary>
        public static string LoadName { get; set; }
        /// <summary>
        /// Is the PlayerData loaded?
        /// </summary>
        public static bool IsLoaded { get; set; }

        /// <summary>
        /// Is the game paused?
        /// </summary>
        public static bool Paused { get; set; }

        public static bool InMenu { get; set; }

        /// <summary>
        /// Holds all data about the current Game
        /// </summary>
        public static PlayerData Data { get; set; }
        /// <summary>
        /// Script used to save and load games
        /// </summary>
        public static SaveScript Save { get; set; }

        /// <summary>
        /// Pool of every item in the game
        /// </summary>
        public static List<Item> ItemPool { get; set; }

        public static IconList list;

        public static bool playerAlive { get; set; }

        /// <summary>
        /// Player Move Direction
        /// </summary>
        public static Vector3 MoveDir { get; set; }
        public static float RespawnRate { get; set; }
        public static GameObject ItemParent { get; set; }
        public static GameObject NpcHoverUI { get { return _npcHoverUi; } }

        public static TextMeshProUGUI UseMsg { get { return _useMsg; } }

        public static bool InWait { get; set; }

        public static Locations loc;



        private float maxNightAlpha;
        private bool simulatingDay;
        private WaitForSeconds wfs;
        /// <summary>
        /// Total days to wait before the Merchant Inventories reset
        /// </summary>
        private int merchantResetTotal = 7;
        private float _curInterval = 0.0f;
        private bool inAutosave;
        private WaitForSeconds saveWait;
        
        [SerializeField, Tooltip("Place the NPC Hover UI here to display NPC Stats when hovering over that NPC")]
        private GameObject npcHoverObj;
        /// <summary>
        /// The time (In seconds) that it takes for 1 in-game minute
        /// </summary>
        [SerializeField, Tooltip("The time (In seconds) that it takes for 1 in-game minute")]
        private float min;
        /// <summary>
        /// The length of one day (In Seconds) for the game world
        /// </summary>
        [SerializeField, Tooltip("The length of one day (In Seconds) for the game world")]
        private float dayLength;
        [SerializeField]
        private float secLeftInDay;
        [SerializeField, Tooltip("The Night Mask will gradually get darker as the Game-Time moves from day to night. This simulates a day-night cycle")]
        private Image nightMask;
        [SerializeField]
        private GameObject pauseMenu;
        [SerializeField]
        private GameObject poolParent;
        [SerializeField]
        private GameObject ip;

        [SerializeField]
        private GameObject waitMenu;

        [SerializeField]
        private TextMeshProUGUI date;
        [SerializeField]
        private TextMeshProUGUI time;
        
        /// <summary>
        /// The length of 1 In-Game Day in Real World Seconds
        /// </summary>
        public float DayLength
        {
            get { return dayLength; }
            set
            {
                if (dayLength != Data.SecondsInDay)
                    Data.SecondsInDay = dayLength;

                SetSeconds(value);
            }
        }

        public void SetSeconds(float value)
        {
            // Make sure we update the number of seconds left in the current day
            float per = Data.SecondsLeftInDay / dayLength;
            dayLength = value;
            Data.SecondsInDay = dayLength;
            //Data.SecondsLeftInDay = value * per;
            Data.SecondsLeftInDay = ((Data.SecondsInDay / 24.0f) * (24.0f - Data.Date.Hour)) - (Data.Date.Minute * min);
            // The seconds per hour value will be off, so we need to reset it
            SetHour();
            Debug.Log("Set Seconds");
        }

        public static void SetSpells()
        {
            if (spells == null)
                spells = new Spells();
        }
        /// <summary>
        /// Stores data in the PlayerData when setData is true
        /// <para></para>
        /// Gets data from the PlayerData when setData is false
        /// </summary>
        public static void AllData(bool setData)
        {
            // Get or Set any Quest data
            if (setData)
            {
                // Set the data from the QuestController into the PlayerData list
                Data.AllQuests = Quests.QuestController.GetQuestData();
                Data.ActiveQuests = Quests.QuestController.GetActiveQuests().Count;
                // Save Location
                Data.Loc = loc;
            }
            else
            {
                // Get the data from the PlayerData list into the QuestController
                Quests.QuestController.SetQuestData(Data.AllQuests);
                // Load Location
                loc = Data.Loc;
            }

            // Get or Set any NPC's data
            NpcGetData(setData);
            // Get or Set the Players data
            PlayerGetData(setData);
        }

        public void Awake()
        {
            n9ne = 6688846993;
            maxNightAlpha = 0.5882353f;
            playerAlive = true;
            simulatingDay = false;
            inAutosave = false;
            spp = poolParent;
            ItemParent = ip;
            if (PauseController.pause == null)
            {
                PauseController.pause = pauseMenu.GetComponent<PauseController>();
                Debug.Log("Got Pause?");
            }
            pauseMenu.SetActive(false);
            _npcHoverUi = npcHoverObj;
            NpcHoverUI.SetActive(false);
            _useMsg = GameObject.Find("UseMessage1").GetComponent<TextMeshProUGUI>();
            if (_useMsg != null)
                _useMsg.text = "";

            Save = new SaveScript();

            if (LoadPersistant.LoadName != null)
                LoadName = LoadPersistant.LoadName;
            // TESTING
            else
                LoadName = SaveScript.GetAutoSaveName();
            //else
            //    LoadName = "AutoSave_10-13-2017";
            // END TESTING

            GameOptionsMenu.SaveOptions += SetGameOptions;
            WaitMenu.Wait += Wait;
        }
        
        // Use this for initialization
        public void Start()
        {
            Paused = false;
            list = GameObject.Find("BodyParts").GetComponent<IconList>();
            SetSpells();

            loc = Locations.PINEFOREST;

            // Load the specified game if we set a name from main menu/loading screen
            if (LoadName != "" && LoadName != null)
            {
                Data = Save.LoadData(LoadName);
                if (Data.Player != null && Data.PlayerStats != null)
                    IsLoaded = true;
                nightMask.color = new Color(nightMask.color.r, nightMask.color.g, nightMask.color.b, Data.NightMaskAlpha);
                AllData(false);
                
                if (RespawnRate == 0)
                    RespawnRate = 60.0f;

                SetGameOptions();
                dayLength = Data.SecondsInDay;
            }
            else // If this is a new game 
            {
                if (Data == null)
                    Data = new PlayerData();
                Data.Date = new DateTime(7510, 6, 6, 12, 0, 0);
                date.text = "Date: " + Data.Date.Month + "/" + Data.Date.Day + "/" + Data.Date.Year;
                time.text = "Time: " + ((Data.Date.Hour < 13 && Data.Date.Hour > 0) ? (Data.Date.TimeOfDay.Hours.ToString("00")) : (Mathf.Abs(Data.Date.Hour - 12).ToString("00"))) + ":" + Data.Date.Minute.ToString("00") + " " + ((Data.Date.Hour < 12) ? ("A.M.") : ("P.M."));
                Debug.Log("Date" + Data.Date);
                if (RespawnRate == 0)
                    RespawnRate = 60.0f;

                SetGameOptions();
                Data.SecondsLeftInDay = DayLength * 0.5f;
                Data.MerchantResetDays = 0;

                // Sunset Between 7PM and 12AM Or Sunrise Between 5AM and 10AM
                if (Data.Date.Hour >= 19 && Data.Date.Hour <= 24 || Data.Date.Hour >= 5 && Data.Date.Hour <= 10)
                {
                    if (nightMask.color.a < 0.5882353f)
                        nightMask.color = new Color(nightMask.color.r, nightMask.color.g, nightMask.color.b,
                            ((maxNightAlpha / 5.0f) * (Data.Date.Hour - 19)) + (((maxNightAlpha / 5.0f) / 60.0f) * (Data.Date.Minute)));

                    Data.NightMaskAlpha = nightMask.color.a;
                }
                else if (Data.Date.Hour > 10)
                {
                    Data.NightMaskAlpha = 0;
                    nightMask.color = new Color(nightMask.color.r, nightMask.color.g, nightMask.color.b, 0.0f);
                }
                else if (Data.Date.Hour < 5)
                {
                    Data.NightMaskAlpha = maxNightAlpha;
                    nightMask.color = new Color(nightMask.color.r, nightMask.color.g, nightMask.color.b, maxNightAlpha);
                }

                // NOTE: This is now done when we exit the Creating Menu
                //// Save the game for the first time
                //StartCoroutine(FirstSave());
            }

            DayLength = DayLength;
            

            if (IsLoaded)
                Debug.Log("Loaded A Game!");
            else
                Debug.Log("No Game to Load!");

            LoadItemPool();
            
        }

        /// <summary>
        /// Called when the GameOptions have been changed
        /// </summary>
        private void SetGameOptions()
        {
            if (PlayerPrefs.HasKey("AutosaveInterval"))
            {
                RespawnRate = PlayerPrefs.GetFloat("RespawnRate");
                dayLength = PlayerPrefs.GetFloat("SecondsPerDay");
                DayLength = DayLength;
                Data.AutosaveInterval = PlayerPrefs.GetFloat("AutosaveInterval");
                SetSeconds(dayLength);

                if (PlayerPrefs.GetInt("CanAutosave") == 1)
                    Data.CanAutosave = true;
                else
                    Data.CanAutosave = false;

                Debug.Log("Respawn Rate: " + RespawnRate);
                Debug.Log("Day Length: " + dayLength);
                Debug.Log("Autosave Interval: " + Data.AutosaveInterval);
                Debug.Log("Can Autosave: " + Data.CanAutosave);
            }

            if (PlayerPrefs.HasKey("MasterVolume"))
            {
                SoundController.master = PlayerPrefs.GetFloat("MasterVolume");
                SoundController.enviro = PlayerPrefs.GetFloat("EnvironmentVolume");
                SoundController.music = PlayerPrefs.GetFloat("MusicVolume");

                if (SoundController.Audio != null)
                {
                    SoundController.Audio.BackgroundSource.volume = SoundController.music * SoundController.master;
                    SoundController.Audio.Levelup.volume = SoundController.enviro * SoundController.master;
                }
            }
        }
        
        void OnDestroy()
        {
            // Make sure we release the timescale if it was paused when we are loading a game
            TimeScale(true);
            InMenu = false;

            GameOptionsMenu.SaveOptions -= SetGameOptions;
            WaitMenu.Wait -= Wait;
        }
        
        // Update is called once per frame
        void Update()
        {
            // Simulate the current day
            if (!Paused && !InMenu)
            {
                secLeftInDay = Data.SecondsLeftInDay;
                if (!simulatingDay)
                {
                    Debug.Log("Date Start: " + Data.Date);
                    simulatingDay = true;
                    StartCoroutine(SimulateDay());
                }
                MoveDir.Set(Data.PlayerStats.MoveDir.X, Data.PlayerStats.MoveDir.Y, Data.PlayerStats.MoveDir.Z);

                if (playerAlive && Data.CanAutosave && !inAutosave)
                {
                    inAutosave = true;
                    // Start to Autosave
                    StartCoroutine(AutoSave());
                }

                if (InputManager.GetButtonUp("Wait") || InputManager.GetAxis("Wait") > 0)
                {
                    // Bring up the Wait Menu
                    TimeScale(false);
                    waitMenu.SetActive(true);
                }
            }

            if (InputManager.AnyInput("Xbox_One_Input") && InputManager.PlayerOneConfiguration != InputManager.GetInputConfiguration("Xbox_One_Input"))
            {
                InputManager.SetInputConfiguration("Xbox_One_Input", PlayerID.One);
                ControllerBox.SetBox(true);
            }
            else if (InputManager.AnyInput("KeyboardAndMouse") && InputManager.PlayerOneConfiguration != InputManager.GetInputConfiguration("KeyboardAndMouse"))
            {
                InputManager.SetInputConfiguration("KeyboardAndMouse", PlayerID.One);
                ControllerBox.SetBox(false);
            }

            if (InputManager.GetButtonDown("Pause") && playerAlive)
            {
                if (!InMenu && !CreationMenu.Creating)
                {
                    if (Paused)
                    {
                        TimeScale(true);
                        pauseMenu.SetActive(false);
                    }
                    else
                    {
                        TimeScale(false);
                        pauseMenu.SetActive(true);
                    }
                }
            }

        } // End Update()

        /// <summary>
        /// Brings up the Load Game menu when the Player dies
        /// </summary>
        public static void DeathLoad()
        {
            PauseController.IsDeath = true;
            PauseController.pause.gameObject.SetActive(true);
        }

        public IEnumerator FirstSave()
        {
            inAutosave = true;

            if (saveWait == null)
                saveWait = new WaitForSeconds(5.0f);

            while (_curInterval < 5.0f)
            {
                yield return saveWait;
                _curInterval += 5.0f;
            }

            AllData(true);

            if (Save.AutoSave(Data))
            {
                Debug.Log("Initial Save @: " + DateTime.Now);
                PlayerController.DisplayTextToScreen("Autosaved Game!", false);
            }
            else
            {
                Debug.Log("Initial Save Failed! @: " + DateTime.Now);
            }

            if (_curInterval >= Data.AutosaveInterval)
                _curInterval = 0;

            inAutosave = false;
            yield return null;
        }
        public IEnumerator AutoSave()
        {
            // Minimum time to wait is 5 seconds to save on resources
            if (saveWait == null)
                saveWait = new WaitForSeconds(5.0f);

            while (_curInterval < Data.AutosaveInterval)
            {
                yield return saveWait;
                _curInterval += 5.0f;

                if (!Data.CanAutosave)
                {
                    break;
                }
            }

            // Make sure we don't autosave if we exit early when CanAutosave is switched off
            if (Data.CanAutosave && _curInterval >= Data.AutosaveInterval)
            {
                // Store all data in the PlayerData
                AllData(true);

                // Save the game
                if (Save.AutoSave(Data))
                {
                    Debug.Log("Auto Saved @: " + DateTime.Now);
                    PlayerController.DisplayTextToScreen("Autosaved Game!", false);
                }
                else
                {
                    Debug.Log("Auto Save Failed! @: " + DateTime.Now);
                }
                
                _curInterval = 0;
            }

            inAutosave = false;
            yield return null;
        }

        /// <summary>
        /// Sets hour many real world seconds that 1 hour in-game will take
        /// </summary>
        public void SetHour()
        {
            float hour = DayLength / 24.0f;
            min = hour / 60.0f;
            wfs = new WaitForSeconds(min);
        }

        public IEnumerator Wait(int hours, Slider slide, TextMeshProUGUI txt)
        {
            WaitForSecondsRealtime sec = new WaitForSecondsRealtime(1.0f);
            // Get the total seconds we will add per hour
            float addSec = Data.SecondsInDay / 24.0f;
            float secLeft = 0.0f;

            Debug.Log("Starting Wait");

            while (hours > 0)
            {
                sec = new WaitForSecondsRealtime(1.0f);
                yield return sec;
                // Add 1 Hours
                Data.Date = Data.Date.AddMinutes(60);
                Data.SecondsLeftInDay -= addSec;

                // clean up the current day
                if (Data.SecondsLeftInDay <= 0)
                {
                    Debug.Log("Its a new day.");
                    Data.DaysPassed += 1;
                    Data.MerchantResetDays += 1;

                    if (Data.SecondsLeftInDay < 0)
                        secLeft = Data.SecondsLeftInDay;

                    Data.SecondsLeftInDay = Data.SecondsInDay;
                    Data.SecondsLeftInDay += secLeft;
                    secLeft = 0.0f;

                    // Call each Merchant Inventory to reset
                    if (Data.MerchantResetDays >= merchantResetTotal)
                    {
                        ResetMerchant();
                        Data.MerchantResetDays = 0;
                    }
                }
                // Set the Date and Time
                date.text = "Date: " + Data.Date.Month + "/" + Data.Date.Day + "/" + Data.Date.Year;
                time.text = "Time: " + ((Data.Date.Hour < 13 && Data.Date.Hour > 0) ? (Data.Date.TimeOfDay.Hours.ToString("00")) : (Mathf.Abs(Data.Date.Hour - 12).ToString("00"))) + ":" + Data.Date.Minute.ToString("00") + " " + ((Data.Date.Hour < 12) ? ("A.M.") : ("P.M."));
                // Calculate the Alpha value of the Night Mask
                SetNightMask();

                // Update the Wait Menu
                hours--;
                slide.value = hours;
                txt.text = hours.ToString() + " Hours";
                
            }

            InWait = false;
            waitMenu.SetActive(false);
            TimeScale(true);
            yield return null;
        }

        /// <summary>
        /// Simulates the day in the game
        /// </summary>
        /// <returns></returns>
        public IEnumerator SimulateDay()
        {
            // Make sure we do not reset a day that needed to exit the coroutine before it was finished
            if (Data.SecondsLeftInDay <= 0)
                Data.SecondsLeftInDay = dayLength;

            SetSeconds(DayLength);
            
            // Loop while there are seconds left in the day and while the game is not paused
            while (!Paused && !InMenu && Data.SecondsLeftInDay > 0)
            {
                yield return wfs;

                Data.SecondsLeftInDay -= min;
                Data.Date = Data.Date.AddMinutes(1);
                secLeftInDay = Data.SecondsLeftInDay;

                // Set the Date and Time 
                date.text = "Date: " + Data.Date.Month + "/" + Data.Date.Day + "/" + Data.Date.Year;
                time.text = "Time: " + ((Data.Date.Hour < 13 && Data.Date.Hour > 0)?(Data.Date.TimeOfDay.Hours.ToString("00")):(Mathf.Abs(Data.Date.Hour-12).ToString("00"))) + ":" + Data.Date.Minute.ToString("00") + " " + ((Data.Date.Hour < 12) ? ("A.M.") : ("P.M."));
                // Calculate the Alpha value of the Night Mask
                SetNightMask();
                
                yield return null;
                
            }

            // clean up the current day
            if (Data.SecondsLeftInDay <= 0)
            {
                Debug.Log("Its a new day.");
                Data.DaysPassed += 1;
                Data.MerchantResetDays += 1;

                // Call each Merchant Inventory to reset
                if (Data.MerchantResetDays >= merchantResetTotal)
                {
                    ResetMerchant();
                    Data.MerchantResetDays = 0;
                }
            }
            simulatingDay = false;
            yield return null;
        }

        /// <summary>
        /// Mask the night with the Alpha field of the night mask object
        /// </summary>
        public void SetNightMask()
        {
            // Sunset Between 7PM and 12AM
            if (Data.Date.Hour >= 19 && Data.Date.Hour <= 24)
            {
                if (nightMask.color.a < maxNightAlpha)
                    nightMask.color = new Color(nightMask.color.r, nightMask.color.g, nightMask.color.b, 
                        nightMask.color.a + ((maxNightAlpha / 5.0f) / 60.0f));
                
                Data.NightMaskAlpha = nightMask.color.a;
                Debug.Log("Sunset At: " + Data.Date.Hour);
            }
            // Sunrise Between 5AM and 10AM
            else if (Data.Date.Hour >= 5 && Data.Date.Hour <= 10)
            {
                if (nightMask.color.a > 0)
                    nightMask.color = new Color(nightMask.color.r, nightMask.color.g, nightMask.color.b, 
                        nightMask.color.a - ((maxNightAlpha / 5.0f) / 60.0f));

                Data.NightMaskAlpha = nightMask.color.a;
                Debug.Log("Sunrise At: " + Data.Date.Hour);
            }
            else if (Data.Date.Hour > 10 && Data.NightMaskAlpha != 0)
            {
                Data.NightMaskAlpha = 0;
                nightMask.color = new Color(nightMask.color.r, nightMask.color.g, nightMask.color.b, 0.0f);
            }
            else if (Data.Date.Hour < 5 && Data.NightMaskAlpha != maxNightAlpha)
            {
                Data.NightMaskAlpha = maxNightAlpha;
                nightMask.color = new Color(nightMask.color.r, nightMask.color.g, nightMask.color.b, maxNightAlpha);
            }
        }

        /// <summary>
        /// Loads the item pool when a change is made
        /// </summary>
        public static void LoadItemPool()
        {
            Item[] objs = spp.GetComponentsInChildren<Item>();
            ItemPool = new List<Item>();
            // Add each item in the item pool to the itemPool list
            for (int i = 0; i < objs.Length; ++i)
                ItemPool.Add(objs[i]);
        }

        /// <summary>
        /// Adds and Spawns a new item to the Item Pool. This does NOT Spawn an item in the game and is only to store
        /// a reference to the new item. Use "AddItemObj" to Spawn an item in-game
        /// </summary>
        /// <param name="item"></param>
        public static void AddItemPool(BaseItem item)
        {
            Item itemObj;

            itemObj = Instantiate(ItemPool[0]);
            itemObj.transform.SetParent(spp.transform);
            itemObj.transform.localScale = new Vector3(1, 1, 1);
            itemObj.transform.localPosition = new Vector3(ItemPool[0].transform.localPosition.x,
                ItemPool[0].transform.localPosition.y, ItemPool[0].transform.localPosition.z);
            itemObj.gameObject.name = item.ID;
            itemObj.LoadItem(item, true);
        }

        /// <summary>
        /// Spawns the input Item into the gameworld at the specified position.
        /// <para></para>
        /// NOTE: The item must be in the itemPool to be spawned this way
        /// </summary>
        /// <param name="item"></param>
        /// <param name="position"></param>
        public static void AddItemObj(BaseItem item, Vector3 position, Vector3 lookDir)
        {
            int i;
            bool good = false;
            Item itemObj;

            for (i = 0; i < ItemPool.Count; ++i)
            {
                if (ItemPool[i].item.ID == item.ID)
                {
                    good = true;
                    break;
                }
            }

            if (good)
            {
                itemObj = Instantiate(ItemPool[i]);
                itemObj.transform.SetParent(ItemParent.transform);
                itemObj.transform.localScale = new Vector3(1, 1, 1);
                // Make sure we calculate the direction we will drop the item in so it will be instantly visible
                itemObj.transform.position = new Vector3(
                    ((lookDir.x != 0)?(position.x + (1.18f * lookDir.x)):(position.x)),
                    ((lookDir.y != 0) ? (position.y + (1.18f * lookDir.y)) : (position.y)), 0);
                itemObj.gameObject.name = item.ID;
                
                // Load any changed values
                itemObj.LoadItem(item, true);
                itemObj.SetBaseItem(false);
            }
        }
        

        /// <summary>
        /// Turns the timescale on or off
        /// </summary>
        /// <param name="on"></param>
        public static void TimeScale(bool on)
        {
            if (on)
            {
                Paused = false;
                Time.timeScale = 1.0f;
            }
            else
            {
                Paused = true;
                Time.timeScale = 0.0f;
            }
        }
    }
}

