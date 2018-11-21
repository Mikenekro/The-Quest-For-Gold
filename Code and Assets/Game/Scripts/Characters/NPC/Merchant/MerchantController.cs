using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace schoolRPG.Items
{
    /// <summary>
    /// The MerchantController defines the NPC that it is attached to as a Merchant
    /// </summary>
    public class MerchantController : MonoBehaviour
    {
        private Inventory inv;

        [SerializeField, Tooltip("Will we allow this Merchants Inventory to reset?")]
        private bool canReset;
        [SerializeField, Tooltip("The amount of Gold the Merchant will have to spend every few days")]
        private double startingGold;
        [SerializeField, Tooltip("A List of default Items that needs to be placed in the Merchants inventory")]
        private List<Item> baseMerchantItems;


        public Inventory Inv { get { return inv; } }

        // Use this for initialization
        public void Start()
        {
            ResetInv();
            // Register this Merchant to allow resetting
            if (canReset)
                WorldController.ResetMerchant += ResetInv;
        }
        
        public void OnDestroy()
        {
            // Un-Register this Merchant from resetting
            if (canReset)
                WorldController.ResetMerchant -= ResetInv;
        }


        /// <summary>
        /// Resets the Inventory once a week
        /// </summary>
        public void ResetInv()
        {
            BaseItem item = new BaseItem();
            BaseItem main;

            inv = new Inventory();
            inv.RemoveGold(inv.Gold);
            inv.GiveGold(startingGold);

            for (int i = 0; i < baseMerchantItems.Count; ++i)
            {
                if (baseMerchantItems[i] != null)
                {
                    if (baseMerchantItems[i].item == null)
                        baseMerchantItems[i].Start();

                    main = baseMerchantItems[i].item;
                    item = new BaseItem();
                    // Create a new item for the Inventory with the selected quantity
                    item.CreateItem(main.IsStackable, main.ArrayPos, main.ID, main.Name, main.Description,
                        main.Value, main.Weight, main.Quantity, main.ItemColor, main.UniqueID, main.Condition,
                        main.ArmorType, main.Armor, main.WeaponType, main.Damage, main.Effect, main.EffectValue,
                        main.Duration, main.Type);
                    baseMerchantItems[i].LoadingItem = false;

                    inv.AddItem(item);
                }
            }
        }
    }
}

