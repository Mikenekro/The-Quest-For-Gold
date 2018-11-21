using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using schoolRPG.Stats;
using System;

namespace schoolRPG.Items
{
    /// <summary>
    /// How we will sort the Inventory
    /// </summary>
    public enum SortType
    {
        WEAPON, ARMOR, POTION, INGREDIENTS, MISC, QUEST, ALL
    }

    /// <summary>
    /// The Inventory class is used to store items on each Character or Container
    /// </summary>
    [System.Serializable]
    public class Inventory
    {
        private bool firstSort;
        [SerializeField]
        private List<BaseItem> _items;
        private List<BaseItem> _sorted;
        [SerializeField]
        private double _gold;

        private float _weight;

        [SerializeField]
        private SortType _sortOption;

        /// <summary>
        /// (Read Only) A List of every item in this Inventory
        /// </summary>
        public List<BaseItem> Items { get { return _items; } }
        /// <summary>
        /// (Read Only) A List of the items Sorted by type
        /// </summary>
        public List<BaseItem> SortedItems { get { return _sorted; } }
        /// <summary>
        /// (Read Only) Number of items in the inventory
        /// </summary>
        public int Count { get { return _items.Count; } }
        /// <summary>
        /// (Read Only) Number of items in the sorted inventory
        /// </summary>
        public int CountSort { get { return _sorted.Count; } }
        /// <summary>
        /// (Read Only) The amount of Gold in this Inventory
        /// </summary>
        public double Gold { get { return _gold; } }
        /// <summary>
        /// (Read Only) The weight of items in this Inventory
        /// </summary>
        public float Weight { get { return _weight; } }
        /// <summary>
        /// (Read Only) Which option is the inventory sorted by?
        /// </summary>
        public SortType SortOption { get { return _sortOption; } }

        /// <summary>
        /// Is this the Players Inventory?
        /// </summary>
        public bool PlayerInv { get; set; }

        public Inventory()
        {
            firstSort = false;
            _items = new List<BaseItem>();
            _sorted = new List<BaseItem>();

            SortInv(_sortOption);
        }

        /// <summary>
        /// Adds an item to the main Inventory
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(BaseItem item)
        {
            _items.Add(item);
            _weight += (item.Weight * item.Quantity);

            firstSort = false;
        }

        /// <summary>
        /// Destroys the input item from the inventory and returns the success
        /// </summary>
        /// <param name="item"></param>
        public bool DestroyItem(BaseItem item)
        {
            bool good = false;
            System.Guid id = item.UniqueID[0];

            for (int i = 0; i < _items.Count; ++i)
            {
                if (_items[i].UniqueID[0] == id)
                {
                    _weight -= _items[i].Weight;
                    _items.RemoveAt(i);
                    good = true;
                    break;
                }
            }

            if (good)
            {
                firstSort = false;
                good = SortInv(SortOption);
            }

            return good;
        }

        /// <summary>
        /// Removes a number of stackable items from the inventory
        /// </summary>
        /// <param name="item"></param>
        /// <param name="qty"></param>
        /// <returns></returns>
        public bool RemoveItem(BaseItem item, ref int qty)
        {
            int i;
            bool good = false;
            System.Guid id = item.UniqueID[0];

            for (i = 0; i < _items.Count; ++i)
            {
                if (_items[i].UniqueID[0] == id)
                {
                    _weight -= (_items[i].Weight * qty);
                    _items[i].Quantity -= qty;
                    qty = _items[i].Quantity;
                    good = true;
                    firstSort = false;
                    break;
                }
            }

            if (_items[i].Quantity <= 0)
            {
                _items.RemoveAt(i);
                qty = 0;
            }

            return good;
        }

        /// <summary>
        /// Give an item to another inventory Based on the Item instead of Position
        /// </summary>
        /// <param name="inv"></param>
        /// <param name="itm"></param>
        /// <param name="sorted"></param>
        /// <returns></returns>
        public bool GiveItem(Inventory inv, BaseItem itm, bool sorted)
        {
            int i;
            int len = 0;
            bool good = false;
            List<BaseItem> main;
            Guid prevID = itm.GetPrevID();

            if (sorted)
            {
                len = _sorted.Count;
                main = _sorted;
            }
            else
            {
                len = _items.Count;
                main = _items;
            }

            for (i = 0; i < len; ++i)
            {
                // Find the item based on the previous ID it had
                if (prevID == main[i].GetPrevID())
                {
                    good = true;
                    itm.ResetPrevID();
                    break;
                }
            }

            if (good)
                GiveItem(inv, i, main[i].Quantity, sorted);
            else
                itm.SetPrevID(prevID);

            return good;
        }

        /// <summary>
        /// Gives the item at position to the specified Inventory and removes it from the current inventory
        /// </summary>
        /// <param name="inv"></param>
        /// <param name="pos"></param>
        /// <param name="qty"></param>
        /// <param name="sorted">Should we use the Sorted Inventory?</param>
        /// <returns></returns>
        public BaseItem GiveItem(Inventory inv, int pos, int qty, bool sorted)
        {
            BaseItem main;
            BaseItem itm = new BaseItem();

            // Should we get the item from the Sorted list or the Main list?
            if (sorted)
                main = _sorted[pos];
            else
                main = _items[pos];

            // Create a new item for the Inventory with the selected quantity
            itm.CreateItem(main.IsStackable, main.ArrayPos, main.ID, main.Name, main.Description, 
                main.Value, main.Weight, qty, main.ItemColor, main.UniqueID, main.Condition, 
                main.ArmorType, main.Armor, main.WeaponType, main.Damage, main.Effect, main.EffectValue, 
                main.Duration, main.Type);
            // Set the previous ID
            itm.SetPrevID(main.UniqueID[0]);
            firstSort = false;
            inv.AddItem(itm);
            RemoveItem(main, ref qty);

            return itm;
        }

        /// <summary>
        /// Gives some gold to this inventory
        /// </summary>
        /// <param name="value"></param>
        public void GiveGold(double value)
        {
            _gold += value;

            // Set the Current gold for Stats Menu
            if (PlayerInv)
            {
                WorldController.Data.StatValue.CurrentGold = _gold;

                if (_gold > WorldController.Data.StatValue.MostGold)
                    WorldController.Data.StatValue.MostGold = _gold;
            }
        }
        /// <summary>
        /// Transfers gold to this inventory from another inventory
        /// </summary>
        /// <param name="value"></param>
        /// <param name="inv"></param>
        /// <returns></returns>
        public bool GiveGold(double value, Inventory inv)
        {
            bool good = true;

            if (inv.RemoveGold(value))
                _gold += value;
            else
                good = false;

            // Set the Current gold for Stats Menu
            if (PlayerInv)
            {
                WorldController.Data.StatValue.CurrentGold = _gold;

                if (_gold > WorldController.Data.StatValue.MostGold)
                    WorldController.Data.StatValue.MostGold = _gold;
            }

            return good;
        }
        /// <summary>
        /// Removes some gold from this inventory if the inventory has enough gold
        /// </summary>
        /// <param name="value"></param>
        public bool RemoveGold(double value)
        {
            bool good = true;

            if (_gold >= value)
                _gold -= value;
            else
                good = false;

            // Set the Current gold for Stats Menu
            if (PlayerInv)
            {
                WorldController.Data.StatValue.CurrentGold = _gold;

                if (_gold > WorldController.Data.StatValue.MostGold)
                    WorldController.Data.StatValue.MostGold = _gold;
            }

            return good;
        }
        /// <summary>
        /// Transfers gold to another inventory from this inventory
        /// </summary>
        /// <param name="value"></param>
        /// <param name="inv"></param>
        /// <returns></returns>
        public bool RemoveGold(double value, Inventory inv)
        {
            bool good = false;

            if (inv.GiveGold(value, this))
                good = true;

            return good;
        }

        /// <summary>
        /// Sorts the Inventory by all items
        /// </summary>
        //public bool SortInv()
        //{
        //    // Try/Catch in case of a null value
        //    try
        //    {
        //        _sorted.Clear();
        //        _sorted = _items;
        //        // Sort by name
        //        _sorted.Sort((x, y) =>
        //            x.Name.CompareTo(y.Name));
        //        SortAll = true;
                
        //        return true;
        //    }
        //    catch
        //    {
        //        SortAll = false;
        //        return false;
        //    }
        //}

        /// <summary>
        /// Sorts the inventory by the specified ItemType
        /// </summary>
        /// <returns></returns>
        public bool SortInv(SortType type)
        {
            // Sort only if we are not sorting by this type already or
            // if we are currently sorting by all item types
            if (SortOption != type || !firstSort)
            {
                _sorted.Clear();

                // Add each item from the list that is the same ItemType
                for (int i = 0; i < _items.Count; ++i)
                {
                    // If the sorted type is by all
                    if (type == SortType.ALL)
                        _sorted.Add(_items[i]); // Add every item
                    // If the item type converts to the sorted type
                    else if ((SortType)_items[i].Type == type)
                        _sorted.Add(_items[i]); // Add the item
                }

                try
                {
                    // Sort by name
                    _sorted.Sort((x, y) =>
                        x.Name.CompareTo(y.Name));
                    // Set the sorted option
                   _sortOption = type;
                    firstSort = true;
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            // Return true since the sorted list is already created and we didn't fail the sort
            return true;
        }

        /// <summary>
        /// Returns the index of the input item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int ItemPosition(BaseItem item)
        {
            int pos = 0;

            pos = _items.IndexOf(item);

            return pos;
        }

        
    }
}
