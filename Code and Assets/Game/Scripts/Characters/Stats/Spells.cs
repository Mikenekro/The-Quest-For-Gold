using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace schoolRPG.Stats
{
    /// <summary>
    /// A List of every Spell in the game
    /// </summary>
    public enum SpellEffect
    {
        FIRE, ICE, ELECTRIC, TURN_ENEMY, CARRY_WEIGHT, TELEPORT, RAISE_DEAD, HEALING
    }

    public enum SpellType
    {
        TARGET, TOUCH, SELF, RANGE, RANGE_SELF
    }

    [System.Serializable]
    public class Spell
    {
        private string spellName;
        private string spellDescription;
        private double spellValue;
        private int spellLoc;
        private SpellEffect effect;
        private SpellType type;
        private float spellPower;
        private float spellRange;
        private float spellCost;
        private float spellTime;

        public string Name { get { return spellName; } }
        public string Description { get { return spellDescription; } }
        public double Value { get { return spellValue; } }
        public SpellEffect Effect { get { return effect; } }
        public SpellType Type { get { return type; } }
        /// <summary>
        /// Damage/Strength of effect per second
        /// </summary>
        public float Power { get { return spellPower; } }
        /// <summary>
        /// Range of effect
        /// </summary>
        public float Range { get { return spellRange; } }
        /// <summary>
        /// Cost of effect
        /// </summary>
        public float Cost { get { return spellCost; } }
        /// <summary>
        /// How long does the effect last?
        /// </summary>
        public float Time { get { return spellTime; } }
        
        /// <summary>
        /// Constructor for creating a new Spell
        /// </summary>
        public Spell(string n, string d, double v, SpellEffect eff, SpellType t, float p, float r, float c, float ti)
        {
            spellName = n;
            spellDescription = d;
            spellValue = v;
            effect = eff;
            type = t;
            spellPower = p;
            spellRange = r;
            spellCost = c;
            spellTime = ti;
        }
    }

    /// <summary>
    /// The Spells class holds each Spell in the game
    /// </summary>
    [System.Serializable]
    public class Spells
    {
        public static List<Spell> spellList;

        public Spells()
        {
            // Creates every default spell in the game
            spellList = new List<Spell>
            {
                new Spell("Carry Weight", "Increases the casters Carry Weight for 60 seconds", 1225, SpellEffect.CARRY_WEIGHT, SpellType.SELF, 50, 0, 100, 60),
                new Spell("Electricity", "Electrifies the target with a stream of energy", 120, SpellEffect.ELECTRIC, SpellType.TARGET, 10, 15, 20, 1),
                new Spell("Lava Flow", "Burns the target with a flaming ball of fire", 110, SpellEffect.FIRE, SpellType.TARGET, 10, 15, 20, 1),
                new Spell("Heal Self", "Heals the caster by 25 points", 75, SpellEffect.HEALING, SpellType.SELF, 25, 0, 50, 1),
                new Spell("Ice Beam", "Freezes the target with a burst of freezing ice", 115, SpellEffect.ICE, SpellType.TARGET, 10, 15, 20, 1),
                new Spell("Lesser Raise Dead", "Raises a dead body for 60 seconds", 250, SpellEffect.RAISE_DEAD, SpellType.TOUCH, 1, 0, 100, 60),
                new Spell("Teleport Self", "Teleports the caster to another location", 500, SpellEffect.TELEPORT, SpellType.SELF, 1, 0, 50, 1),
                new Spell("Teleport Area", "Teleports everyone within a certain range from the caster, including the caster themselves", 2500, SpellEffect.TELEPORT, SpellType.RANGE_SELF, 1, 10, 500, 1),
                new Spell("Lesser Turn Enemy", "Forces an enemy to fight for you for 60 seconds", 600, SpellEffect.TURN_ENEMY, SpellType.TARGET, 1, 20, 50, 60)
            };

        }

        /// <summary>
        /// Adds a Spell to the games Spell List and returns that spell
        /// </summary>
        public static Spell CreateSpell(string name, string desc, double val, SpellEffect eff, SpellType type, float power, float range, float cost, float time)
        {
            int i;
            bool alreadyCreated = false;
            Spell spell;

            for (i = 0; i < spellList.Count; ++i)
            {
                if (name == spellList[i].Name)
                    alreadyCreated = true;
            }

            // Don't add the spell we are trying to create if it has already been created
            if (!alreadyCreated)
            {
                spell = new Spell(name, desc, val, eff, type, power, range, cost, time);
                spellList.Add(spell);
            }
            else
                spell = null;
            
            return spell;
        }

        /// <summary>
        /// Gets the location of the input spell from the Spell List
        /// </summary>
        /// <param name="spell"></param>
        public static int GetSpellLoc(Spell spell)
        {
            int i;
            int loc = -1;
            for (i = 0; i < spellList.Count; ++i)
            {
                // The spell must match in either name or effect
                if (spell.Name == spellList[i].Name ||
                    spell.Effect == spellList[i].Effect)
                {
                    loc = i;
                    break;
                }
            }

            return loc;
        }
    }
}

