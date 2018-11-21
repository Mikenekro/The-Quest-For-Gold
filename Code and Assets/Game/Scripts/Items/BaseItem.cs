using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using schoolRPG.Stats;

namespace schoolRPG.Items
{

    public enum ItemType
    {
        WEAPON, ARMOR, POTION, INGREDIENTS, MISC, QUEST
    }
    public enum WeaponType
    {
        LONGSWORD, RAPIER, MACE, SABER
    }
    public enum ArmorType
    {
        HELMET, BODY, PANTS, BOOTS, GLOVES
    }
    public enum PotionEffect
    {
        DAMAGE, HEAL, STAMINA, MAGICKA, INC_CARRYWEIGHT, INC_MAXHEALTH, INC_MAXSTAMINA, INC_MAXMAGICKA
    }
    
    /// <summary>
    /// BaseItem is the Parent class of every Interactable Item in the game
    /// <para></para>
    /// Note: Only uses 1 class to save time. Split into multiple child classes if we have time
    /// </summary>
    [System.Serializable]
    public class BaseItem
    {
        private List<Guid> _uID;
        /// <summary>
        /// For storing a Previous ID
        /// </summary>
        private Guid prevID;

        private bool _isStackable;
        [SerializeField, Tooltip("Is this an item built for a Female?")]
        private bool _isFemale;
        // Quest Item
        [SerializeField, Tooltip("Is this a Quest Item?")]
        private bool _isQuest;

        [SerializeField, Tooltip("Set the color of this item.")]
        private SaveColor _itemColor;

        [SerializeField, Range(0.0f, 100.0f), Tooltip("Set the Condition for each item in Percentage (Between 0% and 100%). " + 
            "If none are set, each item will start at 100%.")]
        private List<float> _condition;
        [SerializeField, Range(0,1000), Tooltip("Array Position decides what image this Item will get. " +
            "Takes position from 'CharList' based on the item type and Character Gender")]
        private int _arrayPos;
        [SerializeField]
        private string _id;
        [SerializeField]
        private string _name;
        [SerializeField]
        private string _desc;
        [SerializeField]
        private double _value;
        [SerializeField]
        private float _weight;
        [SerializeField, Range(1,1000), Tooltip("There should never be '0' of an item if there is an instance of that item. Items can only stack to 1000 Max")]
        private int _qty;
        [SerializeField]
        private ItemType _type;

        // Weapon
        [SerializeField]
        private WeaponType _wepType;
        [SerializeField]
        private float _damage;
        // Armor
        [SerializeField]
        private ArmorType _armType;
        [SerializeField]
        private float _resistance;
        // Potion
        [SerializeField]
        private float _duration;
        [SerializeField]
        private float _effectStrength;
        [SerializeField]
        private PotionEffect _effect;
        

        public bool IsStackable { get { return _isStackable; } }
        public bool IsFemale { get { return _isFemale; }
            set
            {
                // Modify the array position if the value is changing
                if (_isFemale != value)
                    _arrayPos += ((value) ? (1) : (-1));
                _isFemale = value;
            }
        }
        /// <summary>
        /// Each Instance of an Item is identified by its GUID
        /// </summary>
        public List<Guid> UniqueID { get { return _uID; } }
        public SaveColor ItemColor { get { return _itemColor; } }
        /// <summary>
        /// The Condition of each item in this Item Stack
        /// </summary>
        public List<float> Condition { get { return _condition; } }
        /// <summary>
        /// Position in the Items Array that this item gets its looks from
        /// </summary>
        public int ArrayPos { get { return _arrayPos; } }
        public string ID { get { return _id; } }
        public string Name { get { return _name; } }
        public string Description { get { return _desc; } }
        public double Value { get { return _value; } }
        public float Weight { get { return _weight; } }
        public int Quantity
        {
            set
            {
                if (value < _qty)
                {
                    // Remove any excess Unique IDs and Conditions
                    for (int i = (_qty - 1); i > value; --i)
                    {
                        _uID.RemoveAt(i);
                        _condition.RemoveAt(i);
                    }
                }
                else if (value > _qty)
                {
                    // Add for any new Qty 
                    for (int i = (_qty - 1); i < value; ++i)
                    {
                        _uID.Add(Guid.NewGuid());
                        _condition.Add(100.0f);
                    }
                }

                _qty = value;
            }
            get
            {
                if (!_isStackable && (_qty < 0 || _qty > 1))
                    _qty = 1;

                if (_qty > 1000)
                    _qty = 1000;
                else if (_qty < 0)
                    _qty = 0;
                return _qty;
            }
        }

        public ItemType Type { get { return _type; } }
        // Weapon
        public WeaponType WeaponType { get { return _wepType; } }
        public float Damage { get { return _damage; } }
        /// <summary>
        /// Conditional Damage: The Damage based on the Weapons Condition
        /// </summary>
        public float CDamage { get { return _damage * (_condition[0] / 100.0f); } }
        // Armor
        public ArmorType ArmorType { get { return _armType; } }
        public float Armor { get { return _resistance; } }
        /// <summary>
        /// Conditional Armor: The Armor based on the Armors Condition
        /// </summary>
        public float CArmor { get { return _resistance * (_condition[0] / 100.0f); } }
        // Potion
        public PotionEffect Effect { get { return _effect; } }
        public float EffectValue { get { return _effectStrength; } }
        public float Duration { get { return _duration; } }
        // Quest Item
        /// <summary>
        /// isQuest must be set manually so this can be ordered into the Quest item list
        /// </summary>
        public bool IsQuest
        {
            get { return _isQuest; }
            set
            {
                // Make sure we set the item as a Quest Item
                if (value)
                    _type = ItemType.QUEST;
                _isQuest = value;
            }
        }

        /// <summary>
        /// Constructor to use when we are Loading an item
        /// </summary>
        public void CreateItem(bool stack, int arrayPos, string id, string name, string desc, double val, float weight, int qty,
            SaveColor itmColor, List<Guid> guid, List<float> cond, ArmorType armType, float armor, WeaponType wepType, float damage,
            PotionEffect effect, float effectStrength, float duration, ItemType type = ItemType.MISC)
        {
            CreateItem(stack, arrayPos, id, name, desc, val, weight, qty, itmColor, type);
            _wepType = wepType;
            _damage = damage;
            _armType = armType;
            _resistance = armor;
            _effect = effect;
            _effectStrength = effectStrength;
            _duration = duration;

            //_uID = new List<Guid>();
            //_condition = new List<float>();
            //for (int i = 0; i < qty; ++i)
            //{
            //    _uID.Add(guid[i]);
            //    _condition.Add(cond[i]);
            //}
        }
        /// <summary>
        /// Base Constructor: Condition starts at 100% automatically. Override it before gameplay if neccessary
        /// </summary>
        public void CreateItem(bool stack, int arrayPos, string id, string name, string desc, double val, float weight, int qty, 
            SaveColor itmColor, ItemType type = ItemType.MISC)
        {
            _isStackable = stack;
            _type = type;
            _uID = new List<Guid>();
            _condition = new List<float>();
            _arrayPos = arrayPos;
            _id = id;
            _name = name;
            _desc = desc;
            _value = val;
            _weight = weight;
            if (!stack)
                _qty = 1;
            else
                _qty = qty;

            if (_type == ItemType.QUEST)
                _isQuest = true;
            
            _itemColor = itmColor;

            for (int i = 0; i < qty; ++i)
            {
                _uID.Add(Guid.NewGuid());
                _condition.Add(100.0f);
            }
        }
        
        /// <summary>
        /// Use this constructor to create weapons
        /// </summary>
        public void CreateItem(int arrayPos, string id, string name, string desc, double val, float weight, int qty,
                        SaveColor itmColor, WeaponType wepType, float damage) 
        {
            CreateItem(false, arrayPos, id, name, desc, val, weight, qty, itmColor, ItemType.WEAPON);
            _wepType = wepType;
            _damage = damage;
        }
        /// <summary>
        /// Use this constructor to create armor
        /// </summary>
        public void CreateItem(int arrayPos, string id, string name, string desc, double val, float weight, int qty,
                        SaveColor itmColor, ArmorType armType, float armor)
        {
            CreateItem(false, arrayPos, id, name, desc, val, weight, qty, itmColor, ItemType.ARMOR);
            _armType = armType;
            _resistance = armor;
        }
        /// <summary>
        /// Use this constructor to create potions
        /// </summary>
        public void CreateItem(int arrayPos, string id, string name, string desc, double val, float weight, int qty,
                        SaveColor itmColor, PotionEffect effect, float effectStrength, float duration)
        {
            CreateItem(true, arrayPos, id, name, desc, val, weight, qty, itmColor, ItemType.POTION);
            _effect = effect;
            _effectStrength = effectStrength;
            _duration = duration;
        }

        /// <summary>
        /// Returns the Potential Item Cost when purchasing or selling
        /// </summary>
        public double ItemCost(bool selling, Skill playerBarter, Skill npcBarter)
        {
            double calcVal = Value;
            double discount = 0.0f;

            // If the Player is purchasing this item from a vendor, the price will be increased
            if (selling)
            {
                // Seller will increase price by the BaseValue * 10% of the sellers barter level
                calcVal = (npcBarter.Level * 0.10f) * Value;
                // Player will get a price break by (Players barter level * PI) / 4
                discount = Mathf.Round(((playerBarter.Level * Mathf.PI) / 4) * 100);
                // Round to the nearest 2 decimal places
                calcVal -= (discount / 100);
            }
            else // If the Player is selling this item to a vendor, the price will be decreased
            {
                // 10% of the NPC's barter level is subtracted from 2/3rds of the Potential value
                calcVal = (Value / 1.5f) - (npcBarter.Level * 0.10f);
                // Player adds 10% of their barter level to the value
                discount = (playerBarter.Level * 0.10f);
                calcVal += discount;
            }

            // Keep the value from being lower than 0
            if (calcVal < 0)
                calcVal = 0;

            return calcVal;
        }

        public void SetPrevID(Guid gID)
        {
            prevID = gID;
        }
        public Guid GetPrevID()
        {
            return prevID;
        }
        public void ResetPrevID()
        {
            prevID = _uID[0];
        }
    }
}

