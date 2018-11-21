using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using schoolRPG.Stats;
using TeamUtility.IO;
using schoolRPG.Quests;

namespace schoolRPG.Items
{
    /// <summary>
    /// The Item class is used to connect the BaseItem to a GameObject
    /// </summary>
    [System.Serializable]
    public class Item : MonoBehaviour
    {
        public delegate void OnHoverText(bool display, string dspTxt1, string dspTxt2, string dspTxt3, string dspTxt4, string dspTxt5);
        /// <summary>
        /// Delegate that will display up to 5 lines of Text when an item has the mouse hover over it
        /// </summary>
        public event OnHoverText HoverText;

        private static long _allItems;
        private string _saveID;
        private BaseItem _item;
        private List<Guid> _uID;
        private bool hovering;

        [SerializeField]
        private bool _isBaseItem;
        [SerializeField, Tooltip("Does this item stack with other items of the same type?")]
        private bool _isStackable;
        [SerializeField, Tooltip("Is this an item built for a Female?")]
        private bool _isFemale;
        [SerializeField, Tooltip("Is this a Quest Item?")]
        private bool _isQuest;
        [SerializeField, Tooltip("Set the color of this item.")]
        private Color _itemColor;
        [SerializeField, Range(0.0f, 100.0f), Tooltip("Set the Condition for each item in Percentage (Between 0% and 100%). " +
            "If none are set, each item will start at 100%.")]
        private List<float> _condition;
        [SerializeField, Range(0, 1000), Tooltip("Array Position decides what image this Item will get. " +
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
        [SerializeField, Tooltip("How much this item will encumber the player")]
        private float _weight;
        [SerializeField, Range(1, 1000), Tooltip("There should never be '0' of an item if there is an instance of that item. Items can only stack to 1000 Max")]
        private int _qty;
        [SerializeField]
        private ItemType _type;
        // Weapon
        [SerializeField, Tooltip("(WEAPON) Type of Weapon")]
        private WeaponType _wepType;
        [SerializeField, Tooltip("(WEAPON) How much Damage the Weapon will do")]
        private float _damage;
        // Armor
        [SerializeField, Tooltip("(ARMOR) Type of Armor")]
        private ArmorType _armType;
        [SerializeField, Tooltip("(ARMOR) How much Damage the Armor can block")]
        private float _resistance;
        // Potion
        [SerializeField, Tooltip("(POTION) Potion Effect")]
        private PotionEffect _effect;
        [SerializeField, Tooltip("(POTION) How strong the Potion is")]
        private float _effectStrength;
        [SerializeField, Tooltip("(POTION) How long the Potion Effect will last")]
        private float _duration;

        /// <summary>
        /// The Item connected to this GameObject
        /// </summary>
        public BaseItem item { get { return _item; } set { _item = value; } }
        /// <summary>
        /// Is this a Base Item from the object pool?
        /// </summary>
        public bool IsBaseItem { get { return _isBaseItem; } }
        /// <summary>
        /// Are we loading this item or should we use the values set in the inspector?
        /// </summary>
        public bool LoadingItem { get; set; }
        /// <summary>
        /// Is this item destroyed?
        /// </summary>
        public bool Destroyed { get; set; }
        /// <summary>
        /// The SaveID is how we will find this item when we are Loading a game so we can set its values 
        /// </summary>
        public string SaveID { get { return _saveID; } }
        /// <summary>
        /// This items last known position
        /// </summary>
        public Vector3 Position { get; set; }

        private SaveColor saveCol;

        private QuestMarker[] marker;
        private bool canUse;

        // TESTING
        [SerializeField]
        private string[] UniqueIDS;
        // END TESTING
     
        public void Start()
        {
            if (IsBaseItem)
                _saveID = "_base_" + _id;
            else
            {
                _allItems += 1;
                _saveID = _id + "_" + _allItems.ToString();
            }

            canUse = true;
            _item = new BaseItem();
            saveCol = new SaveColor(_itemColor.r, _itemColor.g, _itemColor.b, _itemColor.a);

            if (GetComponent<QuestMarker>() != null)
                marker = GetComponents<QuestMarker>();

            // If we are not loading the item
            if (!LoadingItem)
            {
                if (_type == ItemType.WEAPON)
                    _item.CreateItem(_arrayPos, _id, _name, _desc, _value, _weight, _qty, saveCol, _wepType, _damage);
                else if (_type == ItemType.ARMOR)
                    _item.CreateItem(_arrayPos, _id, _name, _desc, _value, _weight, _qty, saveCol, _armType, _resistance);
                else if (_type == ItemType.POTION)
                    _item.CreateItem(_arrayPos, _id, _name, _desc, _value, _weight, _qty, saveCol, _effect, _effectStrength, _duration);
                else // Basic Item
                    _item.CreateItem(_isStackable, _arrayPos, _id, _name, _desc, _value, _weight, _qty, saveCol, _type);
            }
            else // If we are loading the item from memory or Saved Game
            {
                LoadItem(item, IsBaseItem);
            }

            _uID = item.UniqueID;

            if (_uID.Count > 0)
            {
                UniqueIDS = new string[_uID.Count];
                for (int i = 0; i < _uID.Count; ++i)
                {
                    UniqueIDS[i] = _uID[i].ToString();
                }
            }
            
        }

        public void OnEnable()
        {
            // Register the OnHover event in the player controller
            PlayerController.Pc.RegisterDelegate(this);
        }
        public void OnDisable()
        {
            // Remove the OnHover event from the Player Controller
            PlayerController.Pc.UnregisterDelegate(this);
        }

        public void OnMouseOver()
        {
            // Since Base Items from the Object Pool are for referencing/instantiating only
            if (!IsBaseItem)
            {
                if (!WorldController.Paused && !WorldController.InMenu)
                {
                    // If we have a Quest Marker, make sure we are at the correct stage before allowing user to pick up
                    for (int i = 0; i < marker.Length; ++i)
                    {
                        // If we are not at the correct stage
                        if (QuestController.Quests[marker[i].questPos].Stage != QuestController.Quests[marker[i].questPos].Objectives[marker[i].objPos].Stage)
                            canUse = false;
                        else
                        {
                            canUse = true;
                            break;
                        }
                       
                    }


                    if (!hovering && canUse)
                    {
                        hovering = true;
                        // Start displaying the hover text
                        HoverText(true, "Press \"" + "E" + "\" to Pickup",
                                    "Item: " + item.Name,
                                    "Value: " + item.Value,
                                    "Quantity: " + item.Quantity,
                                    "Type: " + item.Type.ToString());
                    }
                    
                    // Check if the user wants to pick the item up
                    if (InputManager.GetButtonDown("Use"))
                    {
                        // Add this item to the players inventory
                        PlayerController.Pc.Player.Inventory.AddItem(item);

                        // Make sure to Advance the quest if this is a Quest item
                        if ((_type == ItemType.QUEST || _id == "goldBarDestruction01") && marker != null)
                        {
                            Quest q;
                            for (int i = 0; i < marker.Length; ++i)
                            {
                                q = QuestController.Quests[marker[i].questPos];

                                if (q.QuestID == marker[i].questID && q.Stage == marker[i].obj.Stage)
                                {
                                    q.SetStage(true, false);
                                }
                            }
                           
                        }
                        // Otherwise, if this item is not marked as a Quest Item, then there could be multiple items to pick up
                        else if (marker != null)
                        {
                            for (int i = 0; i < marker.Length; ++i)
                            {
                                Quest q = QuestController.Quests[marker[i].questPos];
                                string search;

                                // Make sure we are at the correct stage
                                if (q.Stage == marker[i].obj.Stage)
                                {
                                    // If we need to fetch item by name and this item contains the [first]_ tag
                                    if (q.Objectives[q.ObjectiveAt].Type == ObjectiveType.FETCHITEM_BYNAME && q.Objectives[q.ObjectiveAt].FetchItemByName.Contains("[first]_"))
                                    {
                                        // Get the name we will be searching for
                                        search = q.Objectives[q.ObjectiveAt].FetchItemByName.Split('_')[1].ToLower();

                                        // If the items name contains the search value, this is the item we want
                                        if (item.Name.ToLower().Contains(search))
                                        {
                                            // Set the number of items that we have grabbed
                                            q.Objectives[q.ObjectiveAt].KillAny(true);

                                            // If we have found at least the number of specified items
                                            if (q.Objectives[q.ObjectiveAt].KillCountCurrent >= q.Objectives[q.ObjectiveAt].KillCountNPC)
                                            {
                                                // Advance the Stage
                                                q.SetStage(true, false);
                                            }
                                        }
                                    }
                                }
                            }
                            

                           
                        }

                        // Disable the HoverItem text before Un-Registering
                        OnMouseExit();
                        // Make sure we remember to Un-Register the event associated with this item
                        OnDisable();
                        // destroy the item when we are finished
                        if (Application.isEditor)
                            DestroyImmediate(gameObject);
                        else
                            Destroy(gameObject);
                    }

                }
            }
        }

        public void OnMouseExit()
        {
            if (!IsBaseItem)
            {
                if (!WorldController.Paused && !WorldController.InMenu)
                {
                    if (hovering)
                    {
                        hovering = false;
                        // Stop displaying the hover text
                        HoverText(false, "", "", "", "", "");
                    }
                }
            }
        }
        
        /// <summary>
        /// Call this if this item will be a quest item
        /// </summary>
        /// <param name="isQuest"></param>
        public void SetQuestItem(bool isQuest)
        {
            //_isQuest = isQuest;
            //if (isQuest)
            //    _type = ItemType.QUEST;
        }

        /// <summary>
        /// Call this function when you want to load an item from memory or from a saved game
        /// </summary>
        /// <param name="_item"></param>
        public void LoadItem(BaseItem _item, bool baseItem)
        {
            item = _item;
            //item.CreateItem(item.isStackable, item.ArrayPos, item.ID, item.Name, item.Description, item.Value, 
            //    item.Weight, item.Quantity, item.ItemColor, item.UniqueID, item.Condition, item.ArmorType, item.Armor, item.WeaponType, 
            //    item.Damage, item.Effect, item.Duration, item.Type);

            _isBaseItem = baseItem;
            _isStackable = item.IsStackable;
            _arrayPos = item.ArrayPos;
            _id = item.ID;
            _name = item.Name;
            _desc = item.Description;
            _value = item.Value;
            _weight = item.Weight;
            _qty = item.Quantity;
            _uID = item.UniqueID;
            _condition = item.Condition;
            _armType = item.ArmorType;
            _resistance = item.Armor;
            _wepType = item.WeaponType;
            _damage = item.Damage;
            _effect = item.Effect;
            _effectStrength = item.EffectValue;
            _duration = item.Duration;
            _type = item.Type;
            _itemColor = new Color(item.ItemColor.R, item.ItemColor.G, item.ItemColor.B, item.ItemColor.A);

            if (_type == ItemType.QUEST)
                _isQuest = true;
        }

        public void SetBaseItem(bool isBase)
        {
            _isBaseItem = isBase;
        }

        /// <summary>
        /// Called when an object has been loaded and was destroyed during the last save
        /// </summary>
        private void DestroyItem()
        {
            Destroyed = true;
            gameObject.SetActive(false);
        }
    }
}
