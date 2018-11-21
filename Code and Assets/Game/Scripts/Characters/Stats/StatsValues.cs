using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace schoolRPG.Stats
{
    /// <summary>
    /// Holds the General, Combat, and Magic Stats for the Player
    /// </summary>
    [System.Serializable]
    public class StatsValues
    {
        // Combat
        [SerializeField]
        private int _timesHitEnemy;
        [SerializeField]
        private int _timesHitFriendly;
        [SerializeField]
        private float _damageDealt;
        [SerializeField]
        private float _damageTaken;
        [SerializeField]
        private float _damageBlocked;
        [SerializeField]
        private float _damageMissed;
        [SerializeField]
        private float _highDamageDealt;
        [SerializeField]
        private float _highDamageTaken;
        [SerializeField]
        private float _highDamageResist;

        // Magic
        [SerializeField]
        private int _jumpPointsUsed;

        // General
        [SerializeField]
        private double _currentGold;
        [SerializeField]
        private double _mostGold;
        [SerializeField]
        private double _goldSpent;
        [SerializeField]
        private int _stepsTaken;
        [SerializeField]
        private float _distanceTraveled;
        [SerializeField]
        private int _totalConversations;
        [SerializeField]
        private int _totalSpeechChecks;
        [SerializeField]
        private int _speechPassed;
        [SerializeField]
        private int _speechFailed;
        [SerializeField]
        private int _totalItemsBought;
        [SerializeField]
        private int _totalItemsSold;
        [SerializeField]
        private int _timesBartered;
        [SerializeField]
        private double _goldSavedBarter;


        // Properties

        public int HitEnemy { get { return _timesHitEnemy; } set { _timesHitEnemy = value; } }
        public int HitFriendly { get { return _timesHitFriendly; } set { _timesHitFriendly = value; } }
        /// <summary>
        /// Total Damage Done
        /// </summary>
        public float DamageDealt { get { return _damageDealt; } set { _damageDealt = value; } }
        /// <summary>
        /// Total Damage Taken
        /// </summary>
        public float DamageTaken { get { return _damageTaken; } set { _damageTaken = value; } }
        /// <summary>
        /// Damage Resisted
        /// </summary>
        public float DamageBlocked { get { return _damageBlocked; } set { _damageBlocked = value; } }
        /// <summary>
        /// Enemy Damage Resisted
        /// </summary>
        public float DamageMissed { get { return _damageMissed; } set { _damageMissed = value; } }
        public float HighestDamageDealt { get { return _highDamageDealt; } set { _highDamageDealt = value; } }
        public float HighestDamageTaken { get { return _highDamageTaken; } set { _highDamageTaken = value; } }
        public float HighestDamageResisted { get { return _highDamageResist; } set { _highDamageResist = value; } }

        public int JumpPointsUsed { get { return _jumpPointsUsed; } set { _jumpPointsUsed = value; } }

        public double CurrentGold { get { return _currentGold; } set { _currentGold = value; } }
        public double MostGold { get { return _mostGold; } set { if (value > _mostGold) _mostGold = value; } }
        public double GoldSpent { get { return _goldSpent; } set { _goldSpent = value; } }
        public int StepsTaken { get { return _stepsTaken; } set { _stepsTaken = value; } }
        /// <summary>
        /// 1 Mile = 5280 Feet
        /// </summary>
        public float DistanceTraveled { get { return _distanceTraveled; } set { _distanceTraveled = value; } }
        public int TotalConversations { get { return _totalConversations; } set { _totalConversations = value; } }
        public int SpeechChecks { get { return _totalSpeechChecks; } set { _totalSpeechChecks = value; } }
        public float SpeechPercent { get { if (_totalSpeechChecks > 0 && _speechPassed > 0) return (float)((float)_speechPassed / (float)_totalSpeechChecks); else return 0.0f; } }
        public int SpeechPassed { get { return _speechPassed; } set { _speechPassed = value; } }
        public int SpeechFailed { get { return _speechFailed; } set { _speechFailed = value; } }
        public int ItemsBought { get { return _totalItemsBought; } set { _totalItemsBought = value; } }
        public int ItemsSold { get { return _totalItemsSold; } set { _totalItemsSold = value; } }
        public int TimesBartered { get { return _timesBartered; } set { _timesBartered = value; } }
        public double GoldSavedBarter { get { return _goldSavedBarter; } set { _goldSavedBarter = value; } }
    }
}
