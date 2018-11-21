using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using schoolRPG.Items;

/// <summary>
/// Main Namespace for RPG
/// </summary>
namespace schoolRPG.Stats
{
    /// <summary>
    /// Enum with each Skill in the game
    /// </summary>
    public enum SkillsEnum
    {
        ATHLETICS, SPEECH, BARTER, SWORDS, SPELLS, ARMOR, UNARMORED
    }

    /// <summary>
    /// Enum with each Attribute in the game
    /// </summary>
    public enum AttributeEnum
    {
        STRENGTH, ENDURANCE, INTELLIGENCE, SPEED
    }

    /// <summary>
    /// The Char Delegates class allows us to call the delegates within this class
    /// from the Character Class while still allowing Serialization (Saving/Loading) of the Character Class
    /// </summary>
    public class CharDelegates
    {
        public delegate void OnLevelup(string displayText, bool playSound);
        public static event OnLevelup LevelupDelegate;

        public delegate void OnDeath(Guid uniqueID);
        public static event OnDeath DeathDelegate;

        public delegate void OnNPCHealth(float val, Guid uniqueID);
        public static event OnNPCHealth NpcHealthDelegate;

        /// <summary>
        /// Send the following message when the Character levels up
        /// </summary>
        /// <param name="msg"></param>
        public static void CallLevelup(string msg)
        {
            LevelupDelegate(msg, true);
        }
        /// <summary>
        /// Send the following delegate when the character is dead
        /// </summary>
        public static void CallDeath(Guid uniqueID)
        {
            // Play any death related events
            DeathDelegate(uniqueID);
        }

        /// <summary>
        /// Send the following value to the NPC's Health Bar
        /// </summary>
        /// <param name="val"></param>
        public static void CallNPCHealthChange(float val, Guid uID)
        {
            NpcHealthDelegate(val, uID);
        }
    }
    
    /// <summary>
    /// The Character class defines the Skills and Stats for each NPC and Player in the game
    /// </summary>
    [System.Serializable]
    public class Character
    {
        // Was the player the last one to kill this Character?
        private bool _playerLastHit;

        // Base Variables
        [SerializeField]
        private bool _isPlayer;
        private Guid _uniqueId;
        [SerializeField]
        private string _name;
        [SerializeField]
        private bool _isMale;
        private string _race;
        private int _age;
        private float _baseCarryWeight;
        private float _maxCarryWeight;
        [SerializeField, Range(1,1000)]
        private int _level;
        private int _totalSkillsGained;
        private int _skillsGainedThisLevel;
        private int _skillToLevel;
        [SerializeField]
        private Inventory _charInv;

        // World Interaction Variables
        private float _baseDamage;
        private float _modDamage;
        private float _baseResistance;
        private float _modResistance;
        private float _walkSpeed;
        private float _runSpeed;
        private SaveVector3 _worldPos;

        // Stats
        private float _health;
        private float _maxHealth;
        private float _stamina;
        private float _maxStamina;
        private float _magica;
        private float _maxMagica;

        // Attributes
        private int _attributePoints;
        [SerializeField, Range(1, 255)]
        private byte _strength;
        [SerializeField, Range(1, 255)]
        private byte _endurance;
        [SerializeField, Range(1, 255)]
        private byte _speed;
        [SerializeField, Range(1, 255)]
        private byte _intelligence;
        private string[] _attribDesc;

        // Skills
        [SerializeField]
        private Skill _athletics;
        [SerializeField]
        private Skill _speech;
        [SerializeField]
        private Skill _barter;
        [SerializeField]
        private Skill _swords;
        [SerializeField]
        private Skill _spells;
        [SerializeField]
        private Skill _armor;
        [SerializeField]
        private Skill _unarmored;

        // Booleans to determine Character States
        private bool _isArmored;
        private bool _isUnarmored;
        private bool _isRunning;
        private bool _hitEnemy;
        private bool _isAttack;
        private bool _isPowerAttack;
        private bool _isCasting;
        private bool _isTalking;
        private bool _isTrading;

        private BaseItem _equipHelm;
        private BaseItem _equipBody;
        private BaseItem _equipPants;
        private BaseItem _equipBoots;
        private BaseItem _equipGloves;
        private BaseItem _equipWeapon;

        private List<Spell> _spellList;
        private Spell _activeSpell;

        private Character ch;

        /// <summary>
        /// Lets the teacher know that the player hit me
        /// </summary>
        public bool PlayerHitMe { get { return _playerLastHit; } }
        /// <summary>
        /// Did this character hit any enemy?
        /// </summary>
        public bool HitEnemy { get { return _hitEnemy; } set { _hitEnemy = value; } }

        public bool IsArmored { get { return _isArmored; } set { _isArmored = value; } }
        public bool IsUnarmored { get { return _isUnarmored; } set { _isUnarmored = value; } }
        public bool IsTalking { get { return _isTalking; } set { _isTalking = value; } }
        public bool IsTrading { get { return _isTrading; } set { _isTrading = value; } }
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                // Cannot be running and any other of these states
                if (value == true)
                {
                    _isAttack = false;
                    _isPowerAttack = false;
                    _isCasting = false;
                }

                _isRunning = value;
            }
        }
        public bool IsAttacking
        {
            get { return _isAttack; }
            set
            {
                // Cannot be attacking and any other of these states
                if (value == true)
                {
                    _isPowerAttack = false;
                    _isCasting = false;
                    _isRunning = false;
                }

                _isAttack = value;
            }
        }
        public bool IsPowerAttacking
        {
            get { return _isPowerAttack; }
            set
            {
                // Cannot be Power Attacking and any other of these states
                if (value == true)
                {
                    _isAttack = false;
                    _isCasting = false;
                    _isRunning = false;
                }

                _isPowerAttack = value;
            }
        }
        public bool IsCasting
        {
            get { return _isCasting; }
            set
            {
                // Cannot be Casting and any other of these states
                if (value == true)
                {
                    _isAttack = false;
                    _isPowerAttack = false;
                    _isRunning = false;
                }

                _isCasting = value;
            }
        }

        public SaveVector3 WorldPos { get { return _worldPos; } set { _worldPos = value; } }
        /// <summary>
        /// A Unique Identifier for this Character
        /// </summary>
        public Guid UniqueID { get { return _uniqueId; } }
        /// <summary>
        /// (Read Only) The Name of this Character
        /// </summary>
        public string Name { get { return _name; } set { _name = value; } }
        public string Gender {
            get { if (_isMale) return "Male"; else return "Female"; }
            set { if (value.ToLower().Trim() == "male") _isMale = true; else if (value.ToLower().Trim() == "female") _isMale = false; }
        }
        public bool IsFemale { get { return ((Gender.ToLower() == "female") ? (true) : (false)); } }
        public string Race { get { return _race; } set { _race = value; } }
        public int Age { get { return _age; } set { _age = value; } }
        /// <summary>
        /// Modifies the Carry Weight for an amount of time
        /// </summary>
        public float ModCarryWeight { get; set; }
        /// <summary>
        /// (Read Only) The base carry weight for this character
        /// </summary>
        public float BaseCarryWeight { get { return _baseCarryWeight; } }
        /// <summary>
        /// (Read Only) How many pounds of items this character can carry
        /// </summary>
        public float MaxCarryWeight { get { return _maxCarryWeight + _baseCarryWeight + ModCarryWeight; } }
        /// <summary>
        /// (Read Only) How much this character is currently carrying
        /// </summary>
        public float CarryWeight { get { return Inventory.Weight; } }
        /// <summary>
        /// (Read Only) Characters current Level
        /// </summary>
        public int Level { get { return _level; } }
        /// <summary>
        /// (Read Only) Total Skills this Character has Leveled up
        /// </summary>
        public int TotalSkillsGained { get { return _totalSkillsGained; } }
        /// <summary>
        /// (Read Only) Number of Skills this Character has leveled up this Level
        /// </summary>
        public int SkillsGainedThisLevel { get { return _skillsGainedThisLevel; } }
        /// <summary>
        /// (Read Only) Number of Skill Levelups needed before this Character increases a Level
        /// </summary>
        public int SkillsToLevelup { get { return _skillToLevel; } }
        /// <summary>
        /// (Read Only) This Characters Inventory
        /// </summary>
        public Inventory Inventory { get { return _charInv; } }

        public float BaseDamage { get { return _baseDamage; } }
        /// <summary>
        /// The damage this character can deal against another character
        /// </summary>
        public float Damage { get { return (((_baseDamage + _modDamage) > WorldController.MaxDamage)?(WorldController.MaxDamage):((_baseDamage + _modDamage))); } }
        /// <summary>
        /// Conditional Damage: The damage this character can deal based on the Armors current Condition
        /// </summary>
        public float CDamage { get { SetDamageAndResist(); return Damage; } }

        public float BaseResistance { get { return _baseResistance; } }
        /// <summary>
        /// The resistance this character has against any damage
        /// </summary>
        public float Resistance { get { return (((_baseResistance + _modResistance) > WorldController.MaxArmor)?(WorldController.MaxArmor):(_baseResistance + _modResistance)); } }
        /// <summary>
        /// Conditional Resistance: The resistance this character has based on the Armors current Condition
        /// </summary>
        public float CResistance { get { SetDamageAndResist(); return Resistance; } }
        public float WalkSpeed { get { return (1.0f + (((float)Speed / 100.0f) * 7.0f)); } }
        public float RunSpeed { get { return (3.0f + (((float)Speed / 100.0f) * 15.0f)); } }

        /// <summary>
        /// (Read Only) Hitpoints before this Character dies
        /// </summary>
        public float Health { get { return _health; } }
        public float ModHealth { get; set; }
        public float MaxHealth { get { return _maxHealth + ModHealth; } }
        /// <summary>
        /// (Read Only) Points used to run or power attack
        /// </summary>
        public float Stamina { get { return _stamina; } }
        public float ModStamina { get; set; }
        public float MaxStamina { get { return _maxStamina + ModStamina; } }
        /// <summary>
        /// (Read Only) Points used to cast a spell
        /// </summary>
        public float Magicka { get { return _magica; } }
        public float ModMagicka { get; set; }
        public float MaxMagicka { get { return _maxMagica + ModMagicka; } }

        /// <summary>
        /// The number of Attribute Points we can assign to one of the Attributes
        /// </summary>
        public int AttributePoints { get { return _attributePoints; } set { _attributePoints = value; } }
        /// <summary>
        /// (Read Only) (Effects Hitpoints) Calculates how much this Character can carry and how much damage swords do
        /// </summary>
        public byte Strength { get { return _strength; } }
        /// <summary>
        /// (Read Only) (Effects Stamina) Calculates how long the Character can run and power attack before getting tired
        /// </summary>
        public byte Endurance { get { return _endurance; } }
        /// <summary>
        /// (Read Only) (Effects Run Speed) How fast this Character can move between 2 points
        /// </summary>
        public byte Speed { get { return _speed; } }
        /// <summary>
        /// (Read Only) (Effects Magicka) How many spells this character can cast before being drained
        /// </summary>
        public byte Intelligence { get { return _intelligence; } }
        
        public Skill Athletics { get { return _athletics; } }
        public Skill Speech { get { return _speech; } }
        public Skill Barter { get { return _barter; } }
        public Skill Swords { get { return _swords; } }
        public Skill SpellSkill { get { return _spells; } }
        public Skill Armor { get { return _armor; } }
        public Skill Unarmored { get { return _unarmored; } }

        public BaseItem EquipHelm { get { return _equipHelm; } set { _equipHelm = value; SetDamageAndResist(); } }
        public BaseItem EquipBody { get { return _equipBody; } set { _equipBody = value; SetDamageAndResist(); } }
        public BaseItem EquipPants { get { return _equipPants; } set { _equipPants = value; SetDamageAndResist(); } }
        public BaseItem EquipBoots { get { return _equipBoots; } set { _equipBoots = value; SetDamageAndResist(); } }
        public BaseItem EquipGloves { get { return _equipGloves; } set { _equipGloves = value; SetDamageAndResist(); } }
        public BaseItem EquipWeapon { get { return _equipWeapon; } set { _equipWeapon = value; SetDamageAndResist(); } }

        /// <summary>
        /// (Read Only) A List of every Spell this Character can use
        /// </summary>
        public List<Spell> SpellList { get { return _spellList; } }
        /// <summary>
        /// The current Active Spell for this Character
        /// </summary>
        public Spell ActiveSpell { get { return _activeSpell; } set { _activeSpell = value; } }

        /// <summary>
        /// Run this in place of the constructor
        /// </summary>
        public void Init(bool player, bool reset)
        {
            _isPlayer = player;
            // Create a Unique ID for this instance
            _uniqueId = Guid.NewGuid();
            if (_name == null)
                _name = "Null Name";
            if (_charInv == null)
                _charInv = new Inventory();
            

            _age = 18;
            
            _baseCarryWeight = 50;
            _maxCarryWeight = _baseCarryWeight;
            if (_level < 1)
                _level = 1;
            _totalSkillsGained = 0;
            _skillsGainedThisLevel = 0;
            _skillToLevel = 4;

            _health = 100;
            _maxHealth = 100;
            _stamina = 100;
            _maxStamina = 100;
            _magica = 100;
            _maxMagica = 100;

            _attributePoints = 20;
            if (_strength < 1 || reset)
                _strength = 1;
            if (_endurance < 1 || reset)
                _endurance = 1;
            if (_speed < 1 || reset)
                _speed = 1;
            if (_intelligence < 1 || reset)
                _intelligence = 1;
            if (player)
            {
                _attribDesc = new string[4];
                _attribDesc[0] = "Strength is the measure of your Characters ability to carry more items and deal more damage with swords. " +
                    "It effects your Maximum Health and also helps increase your Armor and Sword Skills. Your Health increase is calculated " +
                    "as so: Increase = (Strength * 1.75). Armor and Swords will be increased by 1 for every 5 Strength you have.";
                _attribDesc[1] = "Endurance measures your Characters ability to run for longer amounts of time and to fight for longer. " +
                    "It effects your Maximum Stamina and also helps increase your Unarmored and Athletics Skills. Your Stamina increase is " +
                    "calculated as so: Increase = (Endurance * 1.75). Unarmored will be increased by 1 for every 5 Endurance you have and Athletics " +
                    "will be increased by 1 for every 10 Endurance levels (Speed also effects Athletics Skill).";
                _attribDesc[2] = "Intelligence measures your Characters magical abilities, critical thinking, and allows you to use more magic spells. It effects " +
                    "your Maximum Magicka and also helps increase your Spell, Barter, and Speech skills. Your Magicka is calculated as so: " +
                    "Increase = (Intelligence * 1.75). Speech and Barter will be increased by 1 for every 5 Intelligence levels while the Spell " +
                    "Skill will be increased by 1 for every 10 Intelligence levels (Speed also effects Spell Skill).";
                _attribDesc[3] = "Speed measures how fast your Character moves through the world and how fast you cast spells. It effects each " +
                    "of your characters base stats (Health, Magicka, and Stamina) and increases each of them as so: Increase = (Speed * 0.25). " +
                    "Speed also effects your Characters Athletics and Spell skills by 1 for every 10 Speed levels.";

                _athletics = new Skill("Athletics", "The characters ability to run fast and for longer amounts of time.", 1, player);
                _speech = new Skill("Speech", "The characters ability to persuade and get people to like them.", 1, player);
                _barter = new Skill("Barter", "How easily this character can bargain for a better deal during a trade.", 1, player);
                _swords = new Skill("Swords", "How good this character is at swordfights.", 1, player);
                _spells = new Skill("Spells", "How good this character is at spellfights.", 1, player);
                _armor = new Skill("Armor", "Allows the character to resist damage in battle.", 1, player);
                _unarmored = new Skill("Unarmored", "Resistance to damage without armor.", 1, player);
            }

            _spellList = new List<Spell>();
            if (Spells.spellList == null)
            {
                WorldController.SetSpells();
            }
            _spellList.Add(Spells.spellList[1]);
            _spellList.Add(Spells.spellList[2]);
            _spellList.Add(Spells.spellList[3]);
            _spellList.Add(Spells.spellList[4]);

            _activeSpell = _spellList[0];
            // Default world position
            WorldPos = new SaveVector3(0, 0, 0);
            SetBaseStats(true);
        }

        /// <summary>
        /// Adds EXP to a specified skill
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="exp"></param>
        public void AddEXP(SkillsEnum skill, double exp)
        {
            if (skill == SkillsEnum.ARMOR)
                _armor.AddEXP(exp);
            else if (skill == SkillsEnum.ATHLETICS)
                _athletics.AddEXP(exp);
            else if (skill == SkillsEnum.BARTER)
                _barter.AddEXP(exp);
            else if (skill == SkillsEnum.SPEECH)
                _speech.AddEXP(exp);
            else if (skill == SkillsEnum.SPELLS)
                _spells.AddEXP(exp);
            else if (skill == SkillsEnum.SWORDS)
                _swords.AddEXP(exp);
            else if (skill == SkillsEnum.UNARMORED)
                _unarmored.AddEXP(exp);
        }

        /// <summary>
        /// Get the description for the selected attribute
        /// </summary>
        /// <param name="attrib"></param>
        /// <returns></returns>
        public string GetAttribDesc(AttributeEnum attrib)
        {
            string ret = "";
            if (_attribDesc != null)
                ret = _attribDesc[(int)attrib];
            return ret;
        }

        /// <summary>
        /// Forces this Character to Levelup. Also adds Attribute Points to Distribute
        /// </summary>
        /// <param name="amt"></param>
        public void ForceCharLevel(int amt)
        {
            while (amt > 0)
            {
                amt--;
                LevelupChar(false);
            }
        }
        
        /// <summary>
        /// Add a specified amount to the specified Attribute of this Character
        /// </summary>
        /// <param name="att"></param>
        /// <param name="val"></param>
        public void AddAttribute(AttributeEnum att, SByte val)
        {
            if (att == AttributeEnum.ENDURANCE && _endurance < byte.MaxValue)
                _endurance += (byte)val;
            else if (att == AttributeEnum.INTELLIGENCE && _intelligence < byte.MaxValue)
                _intelligence += (byte)val;
            else if (att == AttributeEnum.SPEED && _speed < byte.MaxValue)
                _speed += (byte)val;
            else if (att == AttributeEnum.STRENGTH && _strength < byte.MaxValue)
                _strength += (byte)val;

            SetBaseStats(false);
        }

        /// <summary>
        /// Checks to see if the Player should levelup
        /// </summary>
        private void CheckGainedSkills()
        {
            if (_skillsGainedThisLevel < _skillToLevel)
                SetBaseStats(false);

            while (_skillsGainedThisLevel >= _skillToLevel)
            {
                _skillsGainedThisLevel -= _skillToLevel;
                LevelupChar(true);
            }
            
        }

        /// <summary>
        /// Levels up the Character and displays the screen if bool is true
        /// </summary>
        private void LevelupChar(bool display)
        {
            _level += 1;

            // Increase the Skills needed to levelup by 1 every 5 levels
            _skillToLevel = 4 + (int)Math.Floor(_level * 0.2f);
            _attributePoints += 5;

            // Set the Base Stats for this character
            SetBaseStats(true);

            // Call the Delegate to initiate displaying the Levelup Text if we want to display it on the screen
            if (display && _isPlayer)
                CharDelegates.CallLevelup("Player Leveled Up to Level " + _level + "!");
        }

        /// <summary>
        /// Sets the Health, Stamina, and Magicka of this Character during Levelups
        /// </summary>
        public void SetBaseStats(bool reset)
        {
            float healthPercent = _health / _maxHealth;
            float staminaPercent = _stamina / _maxStamina;
            float magickaPercent = _magica / _maxMagica;

            SetBaseSkills();
            
            // set the base stat values
            _maxHealth = 100 + ((_level - 1) * 20) + (_strength * 1.75f) + (_speed * 0.25f);
            _maxStamina = 100 + ((_level - 1) * 20) + (_endurance * 1.75f) + (_speed * 0.25f) + (_athletics.Level * 0.25f);
            _maxMagica = 100 + ((_level - 1) * 20) + (_intelligence * 1.75f) + (_speed * 0.25f);

            // Set the maximum carry weight
            _maxCarryWeight = (_strength * 5) + (int)(_athletics.Level * 0.5f) + _baseCarryWeight;

            // Update Damage
            SetDamageAndResist();

            // Are we going to reset the characters current values or just keep the current percent?
            if (reset)
            {
                _health = _maxHealth;
                _stamina = _maxStamina;
                _magica = _maxMagica;
            }
            else
            {
                _health = _maxHealth * healthPercent;
                _stamina = _maxStamina * staminaPercent;
                _magica = _maxMagica * magickaPercent;
            }
        }

        /// <summary>
        /// Set the Damage and Resistance Modifiers
        /// </summary>
        private void SetDamageAndResist()
        {
            // Set the Damage modifiers for this character
            _baseDamage = 1.0f + _strength / 5.0f + _swords.Level / 5.0f;
            // Set the damage for the Characters equipped weapon
            if (EquipWeapon != null)
                _modDamage = EquipWeapon.CDamage;
            else // There is no modified level
                _modDamage = 0;

            // Check if we are armored or unarmored
            CheckArmor();

            // Set the Base Resistance modifier for this character
            _baseResistance = (_endurance / 5.0f);
            // Add extra armor for the armor level if the character is armored
            if (IsArmored)
                _baseResistance += _armor.Level / 5.0f;
            // Add extra armor for the unarmored level if the character is unarmored
            if (IsUnarmored)
                _baseResistance += _unarmored.Level / 5.0f;

            // mod resistance starts at 0
            _modResistance = 0;

            // Set the resistance for the characters equipped armor
            if (_equipBody != null)
                _modResistance = _equipBody.CArmor;
            if (_equipBoots != null)
                _modResistance += _equipBoots.CArmor;
            if (_equipGloves != null)
                _modResistance += _equipGloves.CArmor;
            if (_equipHelm != null)
                _modResistance += _equipHelm.CArmor;
            if (_equipPants != null)
                _modResistance += _equipPants.CArmor;
        }

        /// <summary>
        /// Check if we have any armor on
        /// </summary>
        private void CheckArmor()
        {
            if (_equipBody != null || _equipBoots != null || _equipGloves != null || _equipHelm != null || _equipPants != null)
                IsArmored = true;
            else
                IsArmored = false;

            if (_equipBody == null || _equipBoots == null || _equipGloves == null || _equipHelm == null || _equipPants == null)
                IsUnarmored = true;
            else
                IsUnarmored = false;
        }

        /// <summary>
        /// Sets each Skills BaseLevel
        /// </summary>
        private void SetBaseSkills()
        {
            Armor.BaseLevel = (byte)Mathf.Floor(Strength / 5);
            Swords.BaseLevel = (byte)Mathf.Floor(Strength / 5);
            Unarmored.BaseLevel = (byte)Mathf.Floor(Endurance / 5);
            Speech.BaseLevel = (byte)Mathf.Floor(Intelligence / 5);
            Barter.BaseLevel = (byte)Mathf.Floor(Intelligence / 5);
            Athletics.BaseLevel = (byte)(Mathf.Floor(Endurance / 10) + Mathf.Floor(Speed / 10));
            SpellSkill.BaseLevel = (byte)(Mathf.Floor(Speed / 10) + Mathf.Floor(Intelligence / 10));
        }

        public void ResetHitByPlayer()
        {
            _playerLastHit = false;
        }

        /// <summary>
        /// Adds or Subtracts health from this Character
        /// </summary>
        /// <param name="hp">the Health we are Adding or Subtracting</param>
        /// <param name="fixLoss">If true, any damage taken will be fixed</param>
        /// <param name="byPlayer">Did the player cause this damage?</param>
        /// <returns>The health taken after modifications have been made</returns>
        public float AddHealth(float hp, bool fixLoss, bool byPlayer)
        {
            if (_health > 0 && hp < 0)
                _playerLastHit = byPlayer;

            if (hp < 0)
            {
                // Find the Resistance based on the Armor's Condition
                SetDamageAndResist();

                // If we don't want a fixed loss in health, get a random range
                // between 25% of loss and 100% of loss
                if (!fixLoss)
                    hp = UnityEngine.Random.Range(hp * 0.25f, hp);

                // Calculate any resistance this Character has to absorb some of the blow
                hp *= (1 - Mathf.Clamp((Resistance / WorldController.MaxArmor), 0, 0.95f));
            }
            
            
            _health += hp;

            // Clamp Health within Range
            if (_health < 0)
                _health = 0;
            else if (_health > MaxHealth)
                _health = MaxHealth;

            // Calculate any skills we gained from losing health
            if (hp < 0)
                HealthSkills(hp);

            // If this Character is an NPC
            if (!_isPlayer)
            {
                // Set the NPC's Health Bar
                CharDelegates.CallNPCHealthChange(_health, _uniqueId);
            }

            // If the Character died
            if (_health <= 0)
            {
                // Call the Death Delegate
                CharDelegates.CallDeath(_uniqueId);
            }

            return hp;
        }

        /// <summary>
        /// Adds or Subtracts Stamina from this Character
        /// </summary>
        /// <param name="sta"></param>
        /// <returns>The Stamina taken after modifications have been made</returns>
        public float AddStamina(float sta)
        {
            if (sta != WorldController.TechN9ne)
                _stamina += sta;
            else
                _stamina = 0;

            // Stamina should not check for stamina skills because an attack or power attack
            // can miss and we don't want to add EXP if they miss an attack

            // Clamp Stamina within Range
            if (_stamina < 0)
                _stamina = 0;
            else if (_stamina > MaxStamina)
                _stamina = MaxStamina;

            return sta;
        }

        /// <summary>
        /// Adds or Subtracts Magicka for this Character
        /// </summary>
        /// <param name="mag">Magicka gained or lost</param>
        /// <param name="casting">Are we casting a spell?</param>
        /// <returns>The Magicka taken after modifications have been made</returns>
        public float AddMagicka(float mag, bool casting)
        {
            int levelBefore = 0;
            int gained = 0;
            float spellExp = 0.0f;

            _magica += mag;

            // Clamp Magicka within range
            if (_magica < 0)
                _magica = 0;
            else if (_magica > MaxMagicka)
                _magica = MaxMagicka;

            // Magicka can be checked within this function because
            // Magic will give XP based on the spells cast instead of hits taken/given
            if (mag < 0)
            {
                levelBefore = _spells.Level;

                if (_isCasting)
                {
                    // 250% of Magicka Removed from a Spell goes towards Spells Skill
                    spellExp = (Mathf.Abs(mag) * 2.5f);
                    _spells.AddEXP(spellExp);
                }

                // Record any skills we gained
                if (levelBefore != _spells.Level)
                {
                    gained = _spells.Level - levelBefore;
                    _skillsGainedThisLevel += gained;
                    _totalSkillsGained += gained;
                }

                CheckGainedSkills();
            }

            return mag;
        }

        /// <summary>
        /// Call this function when the player is hit by an enemy
        /// </summary>
        /// <param name="healthLost"></param>
        public void HealthSkills(float healthLost)
        {
            int[] levelBefore = new int[2];
            int gained = 0;
            float armExp = 0.0f;
            float unarmExp = 0.0f;

            // If the Character is being Damaged
            if (healthLost < 0 && _health > 0)
            {
                levelBefore[0] = _armor.Level;
                levelBefore[1] = _unarmored.Level;

                // Adding XP to both will give half of the XP to each
                if (_isArmored && _isUnarmored)
                {
                    // 100% of the damage goes to Experience
                    armExp = (Mathf.Abs(healthLost) * 1f);
                    unarmExp = (Mathf.Abs(healthLost) * 1f);
                    _armor.AddEXP(armExp);
                    _unarmored.AddEXP(unarmExp);
                }
                else if (_isArmored) // Only Armored EXP
                {
                    // 200% of the damage goes to Experience
                    armExp = (Mathf.Abs(healthLost) * 2f);
                    _armor.AddEXP(armExp);
                }
                else if (_isUnarmored) // Only Unarmored EXP
                {
                    // 200% of the damage goes to Experience
                    unarmExp = (Mathf.Abs(healthLost) * 2f);
                    _unarmored.AddEXP(unarmExp);
                }

                // Record any skills we gained
                if (levelBefore[0] != _armor.Level)
                {
                    gained = _armor.Level - levelBefore[0];
                    _skillsGainedThisLevel += gained;
                    _totalSkillsGained += gained;
                }
                if (levelBefore[1] != _unarmored.Level)
                {
                    gained = _unarmored.Level - levelBefore[1];
                    _skillsGainedThisLevel += gained;
                    _totalSkillsGained += gained;
                }

                CheckGainedSkills();
            }
        }

        /// <summary>
        /// Call this function when the player loses some Stamina from running 
        /// OR if the player attacks an enemy and deals some damage
        /// </summary>
        /// <param name="runningSta">If a character is running, assign the stamina lost here</param>
        /// <param name="healthTaken">If a character has taken health from an enemy, assign the health taken here</param>
        public void StaminaSkills(float runningSta, float healthTaken)
        {
            int[] levelBefore = new int[2];
            int gained = 0;
            float athExp = 0.0f;
            float fightExp = 0.0f;
            float val = 0.0f;

            if (runningSta != 0)
                val = runningSta;
            if (healthTaken != 0)
                val = healthTaken;

            // If the Character is running or using a Power Attack
            if (val < 0)
            {
                levelBefore[0] = _athletics.Level;
                levelBefore[1] = _swords.Level;

                if (runningSta != 0 && _isRunning && Stamina > -val)
                {
                    // 125% of Stamina Removed goes towards Athletics
                    athExp = (Mathf.Abs(val) * 1.25f);
                    _athletics.AddEXP(athExp);
                }
                else if (healthTaken != 0 && _isPowerAttack && Stamina > -val)
                {
                    // 15% of Stamina Removed from a Power Attack goes towards Athletics
                    athExp = (Mathf.Abs(val) * 0.15f);
                    // 100% of Stamina Removed from a Power Attack goes towards Swordfighting
                    fightExp = (Mathf.Abs(val) * 1.00f);
                    _athletics.AddEXP(athExp);
                    _swords.AddEXP(fightExp);
                }
                else if (healthTaken != 0 && _isAttack && Stamina > -val)
                {
                    // 125% of Stamina Removed from a Regular Attack goes towards Swordfighting
                    fightExp = (Mathf.Abs(val) * 1.25f);
                    _swords.AddEXP(fightExp);
                }

                // Record any skills we gained
                if (levelBefore[0] != _athletics.Level)
                {
                    gained = _athletics.Level - levelBefore[0];
                    _skillsGainedThisLevel += gained;
                    _totalSkillsGained += gained;
                }
                if (levelBefore[1] != _swords.Level)
                {
                    gained = _swords.Level - levelBefore[1];
                    _skillsGainedThisLevel += gained;
                    _totalSkillsGained += gained;
                }

                CheckGainedSkills();
            }
        }

        /// <summary>
        /// Insert what the seller wanted, what the buyer paid, and if this Character was buying
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <param name="paidValue"></param>
        /// <param name="isBuying"></param>
        public void BarterSuccess(double defaultValue, double paidValue, bool isBuying, bool isTrading)
        {
            int levelBefore = 0;
            int gained = 0;
            float tradeExp = 0.0f;
            double difference = 0.0f;

            if (isTrading)
            {
                levelBefore = _barter.Level;

                // If this character is receiving or spending gold
                if (defaultValue > paidValue)
                {
                    // It is better to buy for less
                    difference = defaultValue - paidValue;
                }
                else // If this character is selling the item/s
                {
                    // It is better to sell for more
                    difference = paidValue - defaultValue;
                }

                // 25% of the Trade Value (Positive or Negative) goes towards Experience
                tradeExp = (float)(difference * 0.25f);

                // It is possible to lose Barter EXP when bartering in the wrong direction
                if (tradeExp != 0)
                    _barter.AddEXP(tradeExp);

                Debug.Log("Player Barter");

                // Record any skills we gained
                if (levelBefore != _barter.Level)
                {
                    gained = _barter.Level - levelBefore;
                    _skillsGainedThisLevel += gained;
                    _totalSkillsGained += gained;
                }

                CheckGainedSkills();
            }

        }

        /// <summary>
        /// Insert the Chances that the speech check was a success (where 1.0 = 100%)
        /// </summary>
        /// <param name="successChance">The chance that a success was inevitable</param>
        /// <param name="success">Was the Speech action a success</param>
        public void SpeechSuccess(float successChance, bool success)
        {
            int levelBefore = 0;
            int gained = 0;
            float speechExp = 0.0f;
            float baseExp;

            if (_isTalking)
            {
                levelBefore = _speech.Level;

                if (success)
                {
                    // Exp can range from 50-500 when speech was a success
                    baseExp = 50;
                    speechExp = (baseExp / Mathf.Abs(successChance));
                }
                else
                {
                    // When we fail a speech check, 
                    // A Tiny amount of experience will be gained 
                    // (0.5 Exp when chance was close to 100%, 5 Exp when chance was close to 1%)
                    baseExp = 0.5f;
                    speechExp = (baseExp / Mathf.Abs(successChance));
                }
                

                if (speechExp != 0)
                    _speech.AddEXP(speechExp);

                Debug.Log("Player Speech");

                // Record any skills we gained
                if (levelBefore != _speech.Level)
                {
                    gained = _speech.Level - levelBefore;
                    _skillsGainedThisLevel += gained;
                    _totalSkillsGained += gained;
                }

                CheckGainedSkills();
            }
        }


    } // End Character
    
} // End Namespace

