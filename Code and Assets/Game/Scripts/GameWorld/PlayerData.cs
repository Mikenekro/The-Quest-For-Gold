using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using schoolRPG.Items;
using schoolRPG.Stats;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using schoolRPG.Dialogue;
using schoolRPG.Quests;

namespace schoolRPG.SaveLoad
{
    /// <summary>
    /// The PlayerData class will hold all the data about the GameWorld that we will be saving 
    /// when the Player goes to Save. This way we only need to Serialize 1 script and we can save or load
    /// the state of a game quite simply.
    /// </summary>
    [System.Serializable]
    public class PlayerData
    {
        private bool _isLoaded;
        private DateTime _date;
        private int _days;
        private float _secLeftDay;
        private float _secInDay;
        private float _nightcolor;
        private float _aInterval;
        
        /// <summary>
        /// Is the current PlayerData loaded yet?
        /// </summary>
        public bool IsLoaded { get { return _isLoaded; } }
        /// <summary>
        /// The current In-Game DateTime
        /// </summary>
        public DateTime Date { get { return _date; } set { _date = value; } }
        /// <summary>
        /// How many days have passed in game
        /// </summary>
        public int DaysPassed { get { return _days; } set { _days = value; } }
        /// <summary>
        /// The number of seconds left in the current day
        /// </summary>
        public float SecondsLeftInDay { get { return _secLeftDay; } set { _secLeftDay = value; } }
        /// <summary>
        /// The number of seconds in one in-game-day
        /// </summary>
        public float SecondsInDay { get { return _secInDay; } set { _secInDay = value; } }
        /// <summary>
        /// The Alpha of the NightMask when we saved
        /// </summary>
        public float NightMaskAlpha { get { return _nightcolor; } set { _nightcolor = value; } }
        /// <summary>
        /// How long between autosaves
        /// </summary>
        public float AutosaveInterval { get { return _aInterval; } set { _aInterval = value; } }
        /// <summary>
        /// When this is false, there will be no autosaves
        /// </summary>
        public bool CanAutosave { get; set; }

        /// <summary>
        /// The Players current Location in the world
        /// </summary>
        public Locations Loc { get; set; }

        /// <summary>
        /// Total days that have passed since the Merchant Inventories have reset
        /// </summary>
        public int MerchantResetDays { get; set; }


        // Below is the Data from the Player, NPC's, Quests, etc...
        
        /// <summary>
        /// A List of each Quest in the game
        /// </summary>
        public List<BaseQuest> AllQuests;
        /// <summary>
        /// The number of current Active Quests 
        /// </summary>
        public int ActiveQuests;
        /// <summary>
        /// The current Quest that we were tracking when we saved the game
        /// </summary>
        public string TrackingQuest;

        /// <summary>
        /// The Players data
        /// </summary>
        public Character Player { get; set; }
        /// <summary>
        /// The Players Character Looks Data
        /// </summary>
        public CStats PlayerStats { get; set; }

        /// <summary>
        /// A List of any NPC's that have Altered Data
        /// </summary>
        public List<Character> NPC { get; set; }
        /// <summary>
        /// A List of any NPC's Character Data
        /// </summary>
        public List<CStats> NPCStats { get; set; }
        /// <summary>
        /// A Unique number used to identify the in-game NPC who we have saved
        /// </summary>
        public List<int> NPCNum { get; set; }
        /// <summary>
        /// A List of each Greeting for each NPC
        /// </summary>
        public List<Greeting> NPCGreeting { get; set; }
        /// <summary>
        /// A List of each Dialogue List for each NPC
        /// </summary>
        public List<List<Dialogue.Dialogue>> NPCDialogue { get; set; }

        /// <summary>
        /// A List of each Disabled Dialogue Triggers in the game
        /// </summary>
        public List<string> DisabledTriggers { get; set; }

        /// <summary>
        /// A List of each Changed Types on an NPC
        /// </summary>
        public List<string> ChangedTypes { get; set; }

        /// <summary>
        /// Each of the Players Stats (Such as Total Damage, Total Gold, Etc.) will be stored in the Stats Object
        /// </summary>
        public StatsValues StatValue { get; set; }

        /// <summary>
        /// A List of any Containers that have Altered Data
        /// </summary>
       // UNCOMMENT WHEN CONTAINERS ARE MADE public List<Container> Cont { get; set; }

        public PlayerData()
        {
            AllQuests = new List<BaseQuest>();
            NPC = new List<Character>();
            NPCStats = new List<CStats>();
            NPCNum = new List<int>();
            NPCGreeting = new List<Greeting>();
            NPCDialogue = new List<List<Dialogue.Dialogue>>();
            DisabledTriggers = new List<string>();
            ChangedTypes = new List<string>();
            StatValue = new StatsValues();
            _aInterval = 300;
            // UNCOMMENT WHEN CONTAINERS ARE MADE Cont = new List<Container>();
        }

        public void SetLoaded(bool loaded)
        {
            _isLoaded = loaded;
        }
        

    } // End PlayerData Class
} // End Namespace
