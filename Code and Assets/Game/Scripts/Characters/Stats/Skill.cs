using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace schoolRPG.Stats
{
    /// <summary>
    /// The Skill class is used to create Skills for the Character class
    /// </summary>
    [System.Serializable]
    public class Skill
    {
        public delegate void OnSkillLevelup(string displayText, bool playSound);
        /// <summary>
        /// Delegate that will trigger Text to display when a Skill levels up
        /// </summary>
        public static event OnSkillLevelup SkillLevelupDelegate;

        private bool _isPlayer;
        private string _skillName;
        private string _skillDescription;
        [SerializeField, Range(1,255)]
        private byte _baseLevel;
        [SerializeField, Range(1, 255)]
        private byte _skillLevel;

        private double _currentExp;
        private double _totalExp;
        private double _nextExpLvl;

        /// <summary>
        /// The Name of this Skill
        /// </summary>
        public string Name { get { return _skillName; } set { _skillName = value; } }
        /// <summary>
        /// Description of the current Skill
        /// </summary>
        public string Description { get { return _skillDescription; } set { _skillDescription = value; } }
        /// <summary>
        /// The Base Level of this skill
        /// <para></para>
        /// NOTE: Base Level does not effect the EXP needed to level up
        /// </summary>
        public byte BaseLevel { get { return _baseLevel; } set { _baseLevel = value; } }
        /// <summary>
        /// (Read Only) The current Level of this Skill
        /// </summary>
        public int Level { get { return _skillLevel + _baseLevel; } }
        /// <summary>
        /// (Read Only) The current Experience for this Skill Level
        /// </summary>
        public double CurrentEXP { get { return _currentExp; } }
        /// <summary>
        /// (Read Only) The total Experience gained for this Skill Level
        /// </summary>
        public double TotalEXP { get { return _totalExp; } }
        /// <summary>
        /// (Read Only) The Current Experience we need to level up this Skill
        /// </summary>
        public double NextLevelEXP { get { setNextExp(); return _nextExpLvl; } }

        /// <summary>
        /// Main Skill Constructor
        /// </summary>
        public Skill(string name, string desc) : this(name, desc, 1, true) { }
        /// <summary>
        /// Override Skill Constructor
        /// </summary>
        public Skill(string name, string desc, byte level, bool player)
        {
            _isPlayer = player;
            _skillName = name;
            _skillDescription = desc;
            // Force the Skill to the input level
            ForceLevelup(level);
            // Make sure the EXP levels are correct
            _currentExp = 0;
            _totalExp = 0;
            _nextExpLvl = 30;
        }

        public Skill()
        {
            _currentExp = 0;
            _totalExp = 0;
            // Default to 30
            _nextExpLvl = 30;
        }

        /// <summary>
        /// Sets the Next EXP Level in the case that it starts out at zero (From assigning values in the inspector)
        /// </summary>
        private void setNextExp()
        {
            if (_nextExpLvl <= 30)
            {
                // Default to 30
                _nextExpLvl = 30;

                for (int i = 0; i < Level; ++i)
                {
                    // Set Previous values
                    _currentExp = 0;
                    _totalExp += _nextExpLvl;

                    // Set next EXP level
                    _nextExpLvl = 30 + Mathf.Pow(_skillLevel, 2);
                    //_nextExpLvl += _nextExpLvl * 0.5f;
                }
            }
        }

        /// <summary>
        /// Adds Experience Points to the current Skill
        /// <para></para>
        /// Note: Using AddEXP Will display any Levelups to the screen
        /// </summary>
        /// <param name="exp">The number of Experience Points we want to add to the Skill</param>
        public void AddEXP(double exp)
        {
            // Hold the current EXP after adding the input Exp
            double holdExp = _currentExp + exp;
            // Add amount to total EXP
            if (_totalExp + exp > 0)
                _totalExp += exp;

            setNextExp();

            // Make sure we add a level when the Current EXP is more than the next levels Levelup EXP
            while (holdExp >= _nextExpLvl)
            {
                // Levelup the current Skill
                holdExp -= _nextExpLvl;
                LevelupSkill(true);
            }
            
            // Make sure we remove each level when the current EXP is less than the previous levels levelup EXP (AKA we lost a level)
            while (holdExp < 0 && _skillLevel > 0)
            {
                //holdExp += _nextExpLvl - (_nextExpLvl * (1 / 3));
                //_nextExpLvl -= _nextExpLvl * (1 / 3);
                if (_skillLevel > 0)
                    _skillLevel -= 1;
                
                _nextExpLvl = 30 + Mathf.Pow(_skillLevel, 2);
                holdExp += _nextExpLvl;
            }

            // Set the current EXP to any value left over in the HoldExp
            _currentExp = holdExp;
        }

        /// <summary>
        /// Forces this Skill to levelup by the specified amount
        /// <para></para>
        /// Note: Using ForceLevelup will not display any Levelups to the screen
        /// </summary>
        /// <param name="amt"></param>
        /// <param name="display">Display these levels to the screen?</param>
        public void ForceLevelup(int amt)
        {
            setNextExp();

            while (amt > 0 && _skillLevel < 255)
            {
                amt--;
                _totalExp += _nextExpLvl - _currentExp;
                LevelupSkill(false);
            }
        }

        /// <summary>
        /// Sets the next EXP Level
        /// </summary>
        private void LevelupSkill(bool display)
        {
            // Increase Level
            _skillLevel += 1;
            // Reset the Current EXP
            _currentExp = 0;
            // Set the next EXP Level
            _nextExpLvl = 30 + Mathf.Pow(_skillLevel, 2);
            //_nextExpLvl = _nextExpLvl + (_nextExpLvl * 0.5f);

            // Call the Delegate to initiate displaying the Levelup Text if we want to display it on the screen
            if (display && _isPlayer)
                SkillLevelupDelegate("Leveled Up " + _skillName + " to " + (_skillLevel + _baseLevel), true);
        }

    }
}
