using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Input manager
using TeamUtility.IO;
// TextMesh Pro
using TMPro;

using schoolRPG.Stats;
using schoolRPG.Dialogue;

namespace schoolRPG.Items
{
    
    public class InventoryMenu : MonoBehaviour
    {
        /// <summary>
        /// The Other Inventory for a Trade Menu
        /// </summary>
        private static Inventory otherInv;
        /// <summary>
        /// Will we be trading for Free? Or should we allow bartering?
        /// </summary>
        private static bool isFree;

        /// <summary>
        /// Is this the Trade Menu or the Inventory Menu?
        /// </summary>
        public static bool IsTrade { get; set; }
        /// <summary>
        /// Was the Inventory opened from Dialogue?
        /// </summary>
        public static GameObject dmc { get; set; }

        
        private BaseItem equipSlot;
        private BaseItem test;
        private List<Button> btns;
        private List<Button> otherBtns;
        private bool doClick;

        private Button lastButton;
        private Vector3 vec;
        private int maxItem;
        private int minItem;
        private int lastItem;
        private bool firstSort;

        /// <summary>
        /// What do we currently owe or how much gold will we get from the current trade?
        /// </summary>
        private double curCost;
        /// <summary>
        /// What we owe after Bartering
        /// </summary>
        private double barterCost;
        /// <summary>
        /// Will we be using the Bartering Price?
        /// </summary>
        private bool isBartering = false;

        
        [SerializeField, Tooltip("Put the PlayerController on this field so we can display the Players inventory")]
        private PlayerController player;
        private CharStats cs;
        private SortType sortingBy;
        private bool unconfirmed;
        private bool exiting;
        private bool exit;
        private List<BaseItem> tradeItemsP; // Player
        private List<BaseItem> tradeItemsO; // Other

        private WaitForSeconds wfs;
        private bool showingTxt;
        private float curTime;

        public bool InPlayerInv { get; set; }
        
        public int currentItems;
        public int selectedItem;

        public TextMeshProUGUI weight;
        public TextMeshProUGUI gold;
        public TextMeshProUGUI cost;
        public TextMeshProUGUI costTitle;
        public TextMeshProUGUI sellTxt;

        public GameObject baseItemSlot;
        public GameObject baseItemSlot1;
        public GameObject baseItemSlot2;
        public GameObject contentArea;
        public GameObject contentArea1;
        public GameObject contentArea2;
        public Scrollbar scrollBar;
        public Scrollbar scrollBar1;
        public Scrollbar scrollBar2;

        public TextMeshProUGUI itemSelection;
        public TextMeshProUGUI selectedName;
        public TextMeshProUGUI selectedValue;
        public TextMeshProUGUI selectedWeight;
        public TextMeshProUGUI selectedDamage;
        public TextMeshProUGUI selectedResistance;
        public TextMeshProUGUI selectedPotion;
        public TextMeshProUGUI selectedCondition;
        public TextMeshProUGUI selectedDuration;
        public TextMeshProUGUI selectedDescription;

        public Button allButton;
        public Button weaponButton;
        public Button armorButton;
        public Button potionButton;
        public Button ingredButton;
        public Button miscButton;
        public Button questButton;
        
        void Awake()
        {
            doClick = false;
            InPlayerInv = true;
            minItem = 0;
            maxItem = 5;

            btns = new List<Button>();
            otherBtns = new List<Button>();
            tradeItemsP = new List<BaseItem>();
            tradeItemsO = new List<BaseItem>();
            cs = player.gameObject.GetComponent<CharStats>();
            firstSort = false;
            exit = true;
            sortingBy = SortType.ALL;
            itemSelection.text = sortingBy.ToString()[0].ToString().ToUpper() + sortingBy.ToString().Substring(1).ToLower();
            allButton.GetComponentsInChildren<Image>()[1].enabled = true;
            lastButton = allButton;
            selectedName.text = "";
            selectedValue.text = "";
            selectedWeight.text = "";
            selectedDamage.text = "";
            selectedResistance.text = "";
            selectedPotion.text = "";
            selectedCondition.text = "";
            selectedDuration.text = "";
            selectedDescription.text = "";
            //Load("ALL");
        }
        // Starts when the script is enabled
        void OnEnable()
        {
            exiting = false;
            if (exit)
            {
                exit = false;
                allButton.Select();
                allButton.OnSubmit(new UnityEngine.EventSystems.BaseEventData(UnityEngine.EventSystems.EventSystem.current));
            }
            
            //Load(sortingBy.ToString());
            if (baseItemSlot != null)
            {
                baseItemSlot.SetActive(false);
            }
            else
            {
                if (isFree)
                    sellTxt.text = "  Give/Take Item";
                else
                    sellTxt.text = "  Buy/Sell Item";

                baseItemSlot1.SetActive(false);
                baseItemSlot2.SetActive(false);
                firstSort = false;
                Load(sortingBy.ToString());
            }

            
        }
        // Starts when the script is Disabled
        void OnDisable()
        {
            exit = true;
            WorldController.UseMsg.text = "";
            Disable();
        }

        /// <summary>
        /// Runs when we disable the menu internally only
        /// </summary>
        public void Disable()
        {
            int i;
            //Button[] items = contentArea.GetComponentsInChildren<Button>();

            // Cancel the trade it we are trying to leave an unconfirmed transaction
            if (exiting)
            {
                IsTrade = false;
                isFree = true;

                if (unconfirmed)
                    CancelTrade(false);
            }

            // Remove any item buttons we have made
            for (i = 0; i < btns.Count; ++i)
            {
                if (btns[i].name != "baseItem")
                {
                    if (Application.isEditor)
                        DestroyImmediate(btns[i].gameObject);
                    else
                        Destroy(btns[i].gameObject);
                }
            }
            btns.Clear();

            for (i = 0; i < otherBtns.Count; ++i)
            {
                if (otherBtns[i].name != "baseItem")
                {
                    if (Application.isEditor)
                        DestroyImmediate(otherBtns[i].gameObject);
                    else
                        Destroy(otherBtns[i].gameObject);
                }
            }
            otherBtns.Clear();

            if (contentArea != null)
            {
                contentArea.GetComponent<RectTransform>().sizeDelta = new Vector2(contentArea.GetComponent<RectTransform>().sizeDelta.x, 450);
                scrollBar.value = 1;
            }
            else
            {
                contentArea1.GetComponent<RectTransform>().sizeDelta = new Vector2(contentArea1.GetComponent<RectTransform>().sizeDelta.x, 450);
                contentArea2.GetComponent<RectTransform>().sizeDelta = new Vector2(contentArea2.GetComponent<RectTransform>().sizeDelta.x, 450);
                scrollBar1.value = 1;
                scrollBar2.value = 1;
            }
            minItem = 0;
            maxItem = 5;
            firstSort = false;
            currentItems = 0;
            lastItem = -1;
        }

        void Update()
        {
            if (WorldController.InMenu && !IsTrade)
            {
                if (InputManager.GetButtonUp("Use"))
                {
                    if (selectedItem > -1 && selectedItem < player.Player.Inventory.CountSort)
                    {
                        test = player.Player.Inventory.SortedItems[selectedItem];
                        UseItem(test);
                    }
                    
                }
                else if (InputManager.GetButtonUp("Drop"))
                {
                    if (selectedItem > -1 && selectedItem < player.Player.Inventory.CountSort)
                    {
                        test = player.Player.Inventory.SortedItems[selectedItem];
                        DropItem(test);
                    }
                }
                
                clampContentArea(contentArea);
            }
            else if (WorldController.InMenu && IsTrade)
            {
                if (InputManager.GetButtonUp("Use"))
                {
                    TradeItem();
                }
                else if (InputManager.GetButtonUp("Drop"))
                {
                    CancelTrade(true);
                }
                else if (InputManager.GetKeyUp(KeyCode.C))
                {
                    ConfirmTrade();
                }
                else if (InputManager.GetButtonUp("Pause"))
                { 
                    // If we have a Dialogue Menu Controller and we are not unconfirmed
                    if (dmc != null)
                    {
                        exiting = true;
                        Dialogue.DialogueMenuController.CanClose = true;
                        dmc.SetActive(true);
                        gameObject.transform.parent.gameObject.SetActive(false);
                    }
                }

                if (InPlayerInv)
                    clampContentArea(contentArea1);
                else
                    clampContentArea(contentArea2);
            }
        }

        /// <summary>
        /// Set the Other Inventory for an NPC
        /// </summary>
        public static void SetOtherInv(NpcController npc, bool freeItems)
        {
            IsTrade = true;
            isFree = freeItems;
            otherInv = npc.gameObject.GetComponent<MerchantController>().Inv;
        }
        /// <summary>
        /// Set the Other Inventory for a Container
        /// </summary>
        public static void SetOtherInv(Container cont)
        {
            IsTrade = true;
            isFree = true;
            otherInv = cont.Inv;
        }

        public void CallMsg(string txt)
        {
            if (!showingTxt)
                StartCoroutine(UseMsg(txt));
            else
            {
                // Reset the Message when it changes
                WorldController.UseMsg.text = txt;
                curTime = 0.0f;
            }
        }

        public IEnumerator UseMsg(string txt)
        {
            float totTime = 2.0f;

            if (wfs == null)
                wfs = new WaitForSeconds(0.1f);

            WorldController.UseMsg.text = txt;
            showingTxt = true;

            while (curTime < totTime)
            {
                curTime += 0.1f;
                yield return wfs;
            }

            curTime = 0.0f;
            showingTxt = false;
            WorldController.UseMsg.text = "";
            yield return null;
        }

        /// <summary>
        /// Toggle the Barter Price
        /// </summary>
        /// <param name="tog"></param>
        public void TogBarter(Toggle tog)
        {
            isBartering = tog.isOn;

            if (isBartering)
                SetCost(barterCost);
            else
                SetCost(curCost);
        }

        /// <summary>
        /// Set the Cost that we will display on screen
        /// </summary>
        /// <param name="val"></param>
        public void SetCost(double val)
        {
            val = Mathf.CeilToInt((float)val);

            if (val < 0)
            {
                costTitle.text = "Total Cost:";
                cost.text = "<color=red>" + val.ToString("#,###,##0") + "</color> Gold";
            }
            else
            {
                costTitle.text = "Total Sold:";
                cost.text = val.ToString("#,###,##0") + " Gold";
            }
        }

        public void CancelTrade(bool show)
        {
            int i;

            for (i = 0; i < tradeItemsP.Count; ++i)
            {
                otherInv.GiveItem(player.Player.Inventory, tradeItemsP[i], true);
            }
            for (i = 0; i < tradeItemsO.Count; ++i)
            {
                player.Player.Inventory.GiveItem(otherInv, tradeItemsO[i], true);
            }
            
            curCost = 0;
            barterCost = 0;

            if (isBartering)
                SetCost(barterCost);
            else
                SetCost(curCost);

            tradeItemsP.Clear();
            tradeItemsO.Clear();
            unconfirmed = false;
            Disable();
            Load(sortingBy.ToString());

            if (show)
                CallMsg("<color=red>Canceled Trade!</color>");
        }

        public void ConfirmTrade()
        {
            double val = ((isBartering)?(barterCost):(curCost));

            // If the Player has enough Gold
            if (player.Player.Inventory.Gold >= -val)
            {
                // Add to the Total Items Bought/Sold
                WorldController.Data.StatValue.ItemsBought += tradeItemsO.Count;
                WorldController.Data.StatValue.ItemsSold += tradeItemsP.Count;
                
                tradeItemsO.Clear(); // Clear Selling Items
                tradeItemsP.Clear(); // Clear Buying Items

                player.Player.Inventory.RemoveGold(-val);

                // Add Bartering Skill if we are bartering
                if (isBartering)
                {
                    isBartering = false;

                    // Add to number of times bartered
                    WorldController.Data.StatValue.TimesBartered += 1;

                    // Add to the amount of gold we have spent if we are spending gold
                    if (val < 0)
                        WorldController.Data.StatValue.GoldSpent += -val;

                    // Add to the amount of gold we saved from bartering
                    if (curCost > barterCost)
                        WorldController.Data.StatValue.GoldSavedBarter += (curCost - barterCost);
                    else if (curCost < barterCost)
                        WorldController.Data.StatValue.GoldSavedBarter += (barterCost - curCost);

                    if (val != 0)
                    {
                        player.Player.BarterSuccess(curCost, barterCost, ((val < 0) ? (true) : (false)), true);
                    }
                }

                curCost = 0;
                barterCost = 0;

                // Update the Current Cost Value if needed
                if (!isFree)
                {
                    SetCost(val);
                }

                gold.text = player.Player.Inventory.Gold.ToString("0") + " Gold";
                weight.text = player.Player.Inventory.Weight.ToString("#0.##") + "/" + player.Player.MaxCarryWeight.ToString("#0.##") + " lbs.";

                unconfirmed = false;
                Disable();
                Load(sortingBy.ToString());
                CallMsg("Confirmed Trade!");
            }
            else
            {
                CallMsg("<color=red>Not enough Gold to purchase!</color>");
            }
            
        }

        public void TradeItem()
        {
            int i;
            int sQty = 0;
            float max = 0, min = 0;
            double val = 0;
            bool canAdd = true;
            
            // which inventory are we trading From/To

            // If We are selling items
            if (InPlayerInv)
            {
                // Move the item to the other inventory
                test = player.Player.Inventory.SortedItems[selectedItem];
                test.SetPrevID(test.UniqueID[0]);
                // Store the Quantity to calculate any cost/profit
                sQty = test.Quantity;

                for (i = 0; i < tradeItemsO.Count; ++i)
                {
                    if (tradeItemsO[i].UniqueID[0] == test.GetPrevID())
                    {
                        canAdd = false;
                        player.Player.Inventory.GiveItem(otherInv, test, true);
                        tradeItemsO.RemoveAt(i);
                        break;
                    }
                }

                // If this item wasn't already player owned
                if (canAdd)
                {
                    // Give the other Inventory all of the Item
                    tradeItemsP.Add(player.Player.Inventory.GiveItem(otherInv, selectedItem, test.Quantity, true));
                }
                
                // If we are selling items for Profit
                if (!isFree)
                {
                    // Fixed Percent that we will add to the profit (Based on Players Barter Level)
                    float fixedPercent = (player.Player.Barter.Level / 100.0f);
                    // Start all sales at 40% of the Base Value
                    curCost += (test.Value * sQty) * 0.40f;
                    // Increase the Profit based on the Players Barter Level
                    curCost += (curCost * fixedPercent);
                }
                
            }
            else // If we are Buying Items
            {
                // Buy/Take item from the other inventory
                test = otherInv.SortedItems[selectedItem];
                test.SetPrevID(test.UniqueID[0]);
                sQty = test.Quantity;

                for (i = 0; i < tradeItemsP.Count; ++i)
                {
                    if (tradeItemsP[i].UniqueID[0] == test.GetPrevID()) 
                    {
                        canAdd = false;
                        otherInv.GiveItem(player.Player.Inventory, test, true);
                        tradeItemsP.RemoveAt(i);
                        break;
                    }
                }

                // If this item isn't already owned by the other inventory
                if (canAdd)
                {
                    // Give the Player all of the Item
                    tradeItemsO.Add(otherInv.GiveItem(player.Player.Inventory, selectedItem, test.Quantity, true));
                }

                // If we are purchasing an Item
                if (!isFree)
                {
                    // Fixed Percent that we will add to the value (Based on NPCS Barter Level)
                    float fixedPercent = (DialogueMenuController.npcC.NPC.Barter.Level / 100.0f);
                    // Start all purchases at 150% of the Base Value
                    curCost -= (test.Value * sQty) * 1.50f;
                    // Decrease the cost based on the Players Barter Level
                    curCost += (curCost * fixedPercent);
                }


            }

            // Update the Current Cost Value if needed
            if (!isFree)
            {

                val = GetVal(tradeItemsP, false);
                barterCost = val + GetVal(tradeItemsO, true);
                
                Debug.Log("Current Cost: " + curCost);
                Debug.Log("Barter Cost: " + barterCost);

                if (isBartering)
                    SetCost(barterCost);
                else
                    SetCost(curCost);

                unconfirmed = true;
            }
            
            // Allow the inventory to be re-sorted
            firstSort = false;
            // Reload the inventory 
            Recount();
            //Load(sortingBy.ToString());
        }

        /// <summary>
        /// Calculates the Barter Value of the Trade List entered
        /// </summary>
        /// <param name="list"></param>
        /// <param name="isBuying"></param>
        /// <returns></returns>
        public double GetVal(List<BaseItem> list, bool isBuying)
        {
            int i;
            double val = 0;
            double cost = 0;
            double barter = 0;

            for (i = 0; i < list.Count; ++i)
            {
                // Make sure we are bartering in the correct direction
                if (isBuying)
                {
                    // Fixed Percent that we will add to the value (Based on NPCS Barter Level)
                    float fixedPercent = (DialogueMenuController.npcC.NPC.Barter.Level / 100.0f);
                    // Start all purchases at 150% of the Base Value
                    cost -= (list[i].Value * list[i].Quantity) * 1.50f;
                    // Decrease the cost based on the Players Barter Level
                    cost += (cost * fixedPercent);

                    // Get the percentage that we should add to the Players Cost
                    val = 1.0f - (player.Player.Barter.Level / DialogueMenuController.npcC.NPC.Barter.Level);
                    if (val > -0.01f)
                        val = -0.01f;
                    else if (val < -0.90f)
                        val = -0.90f;
                }
                else
                {
                    // Fixed Percent that we will add to the profit (Based on Players Barter Level)
                    float fixedPercent = (player.Player.Barter.Level / 100.0f);
                    // Start all sales at 40% of the Base Value
                    cost += (list[i].Value * list[i].Quantity) * 0.40f;
                    // Increase the Profit based on the Players Barter Level
                    cost += (cost * fixedPercent);

                    // Get the percentage that we should take away from the Players Profit
                    val = 1.0f - (DialogueMenuController.npcC.NPC.Barter.Level / player.Player.Barter.Level);
                    if (val < 0.01f)
                        val = 0.01f;
                    else if (val > 0.90f)
                        val = 0.90f;
                }
            }

            // Remove the percent difference between the two barter levels
            barter = cost + (cost * val);

            return barter;
        }
        
        public void UseItem(BaseItem test)
        {
            int qty = 1;
            
            // Equip the Armor or Weapon
            if (test.Type == ItemType.ARMOR || test.Type == ItemType.WEAPON) 
            {
                if (PlayerController.Pc.Player.IsFemale)
                {
                    // Make sure the item we are using is for the correct gender
                    if (test.ArrayPos == 0 || (test.ArrayPos % 2) != 0)
                    {
                        // Setting IsFemale will also modify the Array Position to change the Sprites we are using to the Female Variant
                        test.IsFemale = true;
                    }
                }

                // Set the correct item and color for the equipped item

                // Check for Weapon first since it is checked with the Base Type
                if (test.Type == ItemType.WEAPON)
                {
                    cs.weaponsColor = new Color(test.ItemColor.R, test.ItemColor.G, test.ItemColor.B, test.ItemColor.A);
                    // Equip the BaseItem (test) to the equip slot (EquipWeapon)
                    equipSlot = player.Player.EquipWeapon;
                    EquipBase(test, BodyPart.WEAPONS);
                    player.Player.EquipWeapon = equipSlot;
                }
                // Check for Armor after Weapon
                else if (test.ArmorType == ArmorType.BODY)
                {
                    cs.shirtColor = new Color(test.ItemColor.R, test.ItemColor.G, test.ItemColor.B, test.ItemColor.A);
                    equipSlot = player.Player.EquipBody;
                    EquipBase(test, BodyPart.SHIRT);
                    player.Player.EquipBody = equipSlot; 
                }
                else if (test.ArmorType == ArmorType.BOOTS)
                {
                    cs.shoesColor = new Color(test.ItemColor.R, test.ItemColor.G, test.ItemColor.B, test.ItemColor.A);
                    equipSlot = player.Player.EquipBoots;
                    EquipBase(test, BodyPart.SHOES);
                    player.Player.EquipBoots = equipSlot;
                }
                else if (test.ArmorType == ArmorType.GLOVES)
                {
                    cs.glovesColor = new Color(test.ItemColor.R, test.ItemColor.G, test.ItemColor.B, test.ItemColor.A);
                    equipSlot = player.Player.EquipGloves;
                    EquipBase(test, BodyPart.GLOVES);
                    player.Player.EquipGloves = equipSlot;
                }
                else if (test.ArmorType == ArmorType.HELMET)
                {
                    cs.helmColor = new Color(test.ItemColor.R, test.ItemColor.G, test.ItemColor.B, test.ItemColor.A);
                    equipSlot = player.Player.EquipHelm;
                    EquipBase(test, BodyPart.HELMET);
                    player.Player.EquipHelm = equipSlot;
                }
                else if (test.ArmorType == ArmorType.PANTS)
                {
                    cs.pantsColor = new Color(test.ItemColor.R, test.ItemColor.G, test.ItemColor.B, test.ItemColor.A);
                    equipSlot = player.Player.EquipPants;
                    EquipBase(test, BodyPart.PANTS);
                    player.Player.EquipPants = equipSlot;
                }

                cs.UpdateColors();
            }            // Drink the Potion
            else if (test.Type == ItemType.POTION) 
            {
                if (test.Quantity >= 1)
                {
                    // If we removed at least 1 item from the Potion Item in the inventory
                    if (player.Player.Inventory.RemoveItem(test, ref qty))
                    {
                        player.DrinkPotion(test);
                    }
                }

                if (test.Quantity <= 0)
                    lastItem -= 1;

                doClick = true;
            }
            // Eat the Ingredients
            else if (test.Type == ItemType.INGREDIENTS) 
            {
                // EATETH' THY INGREDIENTS!
                lastItem -= 1;
                doClick = true;
            }
            else if (test.Type == ItemType.MISC)
            {
                // Do the Misc.
                lastItem -= 1;
                doClick = true;
            }

            // Allow the inventory to be re-sorted
            firstSort = false;
            equipSlot = null;
            // Reload the inventory when we equip an item
            Recount();
        }

        // Equips the selected item
        private void EquipBase(BaseItem item, BodyPart part)
        {
            // Make sure we take care of any currently equipped items before equipping the new item
            if (equipSlot != null)
                player.Player.Inventory.AddItem(equipSlot);
            equipSlot = item;
            player.Player.Inventory.DestroyItem(item);
            
            item.IsFemale = player.Player.IsFemale;
            
            CharList.charBodyParts.EquipItem(cs, part, item);
            
        }

        

        public void DropItem(BaseItem item)
        {
            // Spawn an item at the players position
            WorldController.AddItemObj(item, player.gameObject.transform.position, player.PlayerStats.LookDir);
            if (player.Player.Inventory.DestroyItem(item))
            {
                Debug.Log("Dropped Item Successfully");
                firstSort = false;
                Recount();
            }
        }

        /// <summary>
        /// Makes sure that we remove the item from the listbox and selects the next available item
        /// </summary>
        private void Recount()
        {
            if (InPlayerInv || !IsTrade)
            {
                if (Application.isEditor)
                    DestroyImmediate(btns[selectedItem].gameObject);
                else
                    Destroy(btns[selectedItem].gameObject);


                btns.RemoveAt(selectedItem);

                if (btns.Count <= selectedItem)
                {
                    selectedItem -= 1;
                    lastItem -= 1;
                    OnEnable();
                }
                else
                    Load(sortingBy.ToString());
            }
            else
            {
                if (Application.isEditor)
                    DestroyImmediate(otherBtns[selectedItem].gameObject);
                else
                    Destroy(otherBtns[selectedItem].gameObject);

                otherBtns.RemoveAt(selectedItem);

                if (otherBtns.Count <= selectedItem)
                {
                    selectedItem -= 1;
                    lastItem -= 1;
                    OnEnable();
                }
                else
                    Load(sortingBy.ToString());
            }
            
            lastItem -= 1;
            doClick = true;
        }

        public void Load(string type)
        {
            itemSelection.text = type[0].ToString().ToUpper() + type.Substring(1).ToLower();
            lastButton.GetComponentsInChildren<Image>()[1].enabled = false;

            if (type.ToUpper() == SortType.ALL.ToString())
            {
                LoadList(SortType.ALL);
                allButton.GetComponentsInChildren<Image>()[1].enabled = true;
                lastButton = allButton;
            }
            else if (type.ToUpper() == SortType.ARMOR.ToString())
            {
                LoadList(SortType.ARMOR);
                armorButton.GetComponentsInChildren<Image>()[1].enabled = true;
                lastButton = armorButton;
            }
            else if (type.ToUpper() == SortType.INGREDIENTS.ToString())
            {
                LoadList(SortType.INGREDIENTS);
                ingredButton.GetComponentsInChildren<Image>()[1].enabled = true;
                lastButton = ingredButton;
            }
            else if (type.ToUpper() == SortType.MISC.ToString())
            {
                LoadList(SortType.MISC);
                miscButton.GetComponentsInChildren<Image>()[1].enabled = true;
                lastButton = miscButton;
            }
            else if (type.ToUpper() == SortType.POTION.ToString())
            {
                LoadList(SortType.POTION);
                potionButton.GetComponentsInChildren<Image>()[1].enabled = true;
                lastButton = potionButton;
            }
            else if (type.ToUpper() == SortType.QUEST.ToString())
            {
                LoadList(SortType.QUEST);
                questButton.GetComponentsInChildren<Image>()[1].enabled = true;
                lastButton = questButton;
            }
            else if (type.ToUpper() == SortType.WEAPON.ToString())
            {
                LoadList(SortType.WEAPON);
                weaponButton.GetComponentsInChildren<Image>()[1].enabled = true;
                lastButton = weaponButton;
            }
        }
        /// <summary>
        /// Loads the Players Inventory List and sorts by the selected type
        /// </summary>
        public void LoadList(SortType type)
        {
            int pos = 0;
            List<Button> b = new List<Button>();
            List<Button> o = new List<Button>();
            Inventory inv = player.Player.Inventory;
            Navigation nav;

            gold.text = inv.Gold.ToString("0") + " Gold";
            weight.text = inv.Weight.ToString("#0.##") + "/" + player.Player.MaxCarryWeight.ToString("#0.##") + " lbs.";

            if (IsTrade)
            {
                if (!isBartering)
                    SetCost(curCost);
                else
                    SetCost(barterCost);
            }
            
            if (contentArea != null)
            {
                // Load the players inventory to the screen
                LoadList(type, inv, baseItemSlot, contentArea, true, btns);
            }
            else
            {
                if (InPlayerInv)
                    doClick = true;
                // Load the Player and the Other Inventory to the Screen
                LoadList(type, inv, baseItemSlot1, contentArea1, true, btns);

                // Settings when loading other inventory
                firstSort = false;
                if (!InPlayerInv)
                    doClick = true;
                if (otherInv != null)
                    LoadList(type, otherInv, baseItemSlot2, contentArea2, false, otherBtns);


                for (pos = 0; pos < 2; ++pos)
                {
                    if (pos == 0)
                    {
                        b = btns;
                        o = otherBtns;
                    }
                    else
                    {
                        b = otherBtns;
                        o = btns;
                    }

                    // Set the Top and Bottom Button Navigation for the Trade Menu Sections
                    if (b.Count >= 1)
                    {
                        // Set Navigation
                        nav = b[0].navigation;
                        // Change mode to explicit
                        nav.mode = Navigation.Mode.Explicit;

                        // Make sure we cannot select any other buttons before setting
                        nav.selectOnDown = null;
                        nav.selectOnUp = null;

                        if (pos == 0)
                        {
                            // Left and Right will always be the same
                            nav.selectOnLeft = lastButton;
                            if (o.Count > 0)
                                nav.selectOnRight = o[0];
                            else
                                nav.selectOnRight = null;
                        }
                        else
                        {
                            // Left and Right will always be the same
                            nav.selectOnRight = null;
                            if (o.Count > 0)
                                nav.selectOnLeft = o[o.Count - 1];
                            else
                                nav.selectOnLeft = null;
                        }
                        

                        if (b.Count > 1)
                        {
                            // Set the buttons we will select
                            nav.selectOnDown = b[1];
                            nav.selectOnUp = null;

                            // Set Navigation
                            nav = b[b.Count - 1].navigation;
                            // Change mode to explicit
                            nav.mode = Navigation.Mode.Explicit;
                            if (pos == 0)
                            {
                                // Left and Right will always be the same
                                nav.selectOnLeft = lastButton;
                                if (o.Count > 0)
                                    nav.selectOnRight = o[o.Count - 1];
                                else
                                    nav.selectOnRight = null;
                            }
                            else
                            {
                                // Left and Right will always be the same
                                nav.selectOnRight = null;
                                if (o.Count > 0)
                                    nav.selectOnLeft = o[o.Count - 1];
                                else
                                    nav.selectOnLeft = null;
                            }
                            // Set the buttons we will select
                            nav.selectOnDown = null;
                            nav.selectOnUp = b[b.Count - 2];
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Loads any Inventory List and sorts by the selected type
        /// </summary>
        public void LoadList(SortType type, Inventory inv, GameObject baseSlot, GameObject content, bool disable, List<Button> b)
        {
            GameObject obj;
            int i;
            int j;
            int count = 0;
            int otherCount = 0;
            int removed = 0;

            //if (contentArea1 != null && content == contentArea1)
            //    otherCount = tradeItemsO.Count;
            //else if (contentArea2 != null && content == contentArea2)
            //    otherCount = tradeItemsP.Count;
            
            // If we are not already sorting by the selected type, we can reload the list
            if (type != sortingBy || !firstSort)
            {
                // If we were successful in sorting the inventory
                if (inv.SortInv(type))
                {
                    count = inv.CountSort;

                    //if (otherCount > 0)
                    //    count += otherCount;

                    // Disable any current items
                    if (disable)
                        Disable();

                    // Enable the base slot when instantiating new slots
                    baseSlot.SetActive(true);

                    // Loop through each item sorted
                    for (i = 0; i < count; ++i)
                    {
                        // Store the item locally in the loop otherwise "AddListener()" will use the
                        // last referenced instance of the item. AKA all the items will be "inv.SortedItems[inv.CountSort-1]"
                        BaseItem item = new BaseItem();
                        int num = i;
                        GameObject c = content;


                        //// Add the Traded Items to the top of the list
                        //if (i < otherCount)
                        //{
                        //    if (content == contentArea1 && tradeItemsO.Count > 0)
                        //        item = tradeItemsO[i];
                        //    else if (content == contentArea2 && tradeItemsP.Count > 0)
                        //        item = tradeItemsP[i];
                        //    else
                        //        item = inv.SortedItems[i];
                        //}
                        //else
                        //{
                        //    item = inv.SortedItems[i - otherCount];
                        //}

                        item = inv.SortedItems[i];


                        //if (tradeItemsP != null && content == contentArea1)
                        //{
                        //    for (j = 0; j < tradeItemsP.Count; ++j)
                        //    {
                        //        if (item.UniqueID[0] == tradeItemsP[j].UniqueID[0])
                        //        {
                        //            removed += 1;
                        //            break;
                        //        }
                        //    }

                        //    if (j != tradeItemsP.Count)
                        //    {
                        //        continue;
                        //    }
                        //}
                        //else if (tradeItemsO != null && content == contentArea2)
                        //{
                        //    for (j = 0; j < tradeItemsO.Count; ++j)
                        //    {
                        //        if (item.UniqueID[0] == tradeItemsO[j].UniqueID[0])
                        //        {
                        //            removed += 1;
                        //            break;
                        //        }
                        //    }

                        //    if (j != tradeItemsO.Count)
                        //    {
                        //        continue;
                        //    }
                        //}
                        
                        // Instantiate the object
                        obj = Instantiate(baseSlot);
                        obj.transform.SetParent(content.transform);
                        obj.transform.localScale = new Vector3(1, 1, 1);
                        obj.GetComponent<RectTransform>().sizeDelta = baseSlot.GetComponent<RectTransform>().sizeDelta;
                        obj.transform.localPosition = new Vector3(baseSlot.transform.localPosition.x, 
                            (baseSlot.transform.localPosition.y - (75.0f * (i))), baseSlot.transform.localPosition.z);
                        obj.name = "Item" + currentItems;

                        // Set the new item slot text
                        setItemSlot(item, obj);

                        obj.GetComponent<HoverSelect>().item = item;
                        obj.GetComponent<HoverSelect>().Pos = num;
                        // Set the function that will run if we click on the item
                        obj.GetComponent<Button>().onClick.AddListener(() => { clampContentArea(c); });

                        b.Add(obj.GetComponent<Button>());

                        // Resize the content area if there are more items that can fit in the screen
                        if (obj.transform.localPosition.y < -450)
                        {
                            content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().sizeDelta.x,
                                content.GetComponent<RectTransform>().sizeDelta.y + 75);
                        }

                        currentItems++;
                    }

                    // Disable the base slot
                    baseSlot.SetActive(false);
                    sortingBy = type;
                    firstSort = true;
                    
                    // Make sure we select an item if we have made any changes to the list size
                    if (doClick && b.Count > 0 && gameObject.activeInHierarchy)
                    {
                        doClick = false;

                        if (selectedItem >= 0)
                        {
                            if (selectedItem >= b.Count)
                                selectedItem = b.Count - 1;

                            StartCoroutine(HoverSelect.SelectFirstButton(b[selectedItem]));
                        }
                        else if (contentArea == null) // Only run if this is not the Inventory and is a Trade Menu
                        {
                            selectedItem = 0;

                            if (b == btns && otherBtns.Count > 0)
                                StartCoroutine(HoverSelect.SelectFirstButton(otherBtns[selectedItem]));
                            else if (b == otherBtns && btns.Count > 0)
                                StartCoroutine(HoverSelect.SelectFirstButton(btns[selectedItem]));
                        }
                    }
                }
            }
        } // End LoadList()

        /// <summary>
        /// Clamps the scroll rect to the content area of the scroll view, AKA. Don't let the user select an item outside of their viewable area
        /// </summary>
        public void clampContentArea(GameObject content)
        {
            // If we select an item that is further down the list than the last item
            if (selectedItem > lastItem)
            {
                vec = content.transform.localPosition;

                lastItem = selectedItem;
                // If we select an item that is further down than the maxItem (out of view at bottom)
                if ((selectedItem - 5) * 75 > vec.y)
                {
                    //if (sItem < 5)
                       // sItem = 0;
                    vec.Set(0, ((selectedItem - 5) * 75), vec.z);
                    maxItem = selectedItem;
                    minItem = maxItem - 6;
                    if (vec.y < 0)
                        vec.Set(0, 0, vec.y);

                    content.transform.localPosition = vec;
                }
            }
            // If we select an item that is further up the list than the last item
            else if (selectedItem < lastItem)
            {
                vec = content.transform.localPosition;

                lastItem = selectedItem;
                // If we select an item that is further up than the minItem (out of view at top)
                if (vec.y > selectedItem * 75)
                {
                    vec.Set(0, (selectedItem * 75), vec.z);
                    minItem = selectedItem;
                    maxItem = minItem + 5;
                    content.transform.localPosition = vec;
                }
            }
            
        }
        
        /// <summary>
        /// Sets the text and image for the Item Slot
        /// </summary>
        /// <param name="item"></param>
        /// <param name="slot"></param>
        private void setItemSlot(BaseItem item, GameObject slot)
        {
            TextMeshProUGUI slotTxt = slot.GetComponentInChildren<TextMeshProUGUI>();
            Image slotImg = slot.GetComponentsInChildren<Image>()[1];
            string type = "";

            if (item.ID == "goldBarDestruction01")
            {
                type = "Misc";
                slotImg.sprite = WorldController.list.inventoryIcons[9];
            }
            else if (item.Type == ItemType.ARMOR)
            {
                type = "Armor";
                slotImg.sprite = WorldController.list.inventoryIcons[1];
            }
            else if (item.Type == ItemType.INGREDIENTS)
            {
                type = "Ingredient";
                slotImg.sprite = WorldController.list.inventoryIcons[3];
            }
            else if (item.Type == ItemType.MISC)
            {
                type = "Misc";
                slotImg.sprite = WorldController.list.inventoryIcons[4];
            }
            else if (item.Type == ItemType.POTION)
            {
                type = "Potion";
                slotImg.sprite = WorldController.list.inventoryIcons[2];
            }
            else if (item.Type == ItemType.QUEST)
            {
                type = "Quest";
                slotImg.sprite = WorldController.list.inventoryIcons[5];
            }
            else if (item.Type == ItemType.WEAPON)
            {
                type = "Weapon";
                slotImg.sprite = WorldController.list.inventoryIcons[0];
            }

            slotTxt.text = "x" + item.Quantity + ", " + type + ": " + item.Name;
            
        }
        
        /// <summary>
        /// Displays the text for the selected item
        /// </summary>
        /// <param name="item"></param>
        public void SelectItem(BaseItem item, int num, bool playerInv)
        {
            selectedItem = num;
            InPlayerInv = playerInv;

            selectedName.text = item.Name;
            selectedValue.text = item.Value.ToString("0.0");
            selectedWeight.text = item.Weight.ToString("0") + " lbs.";
            selectedDescription.text = "                       " + item.Description;

            resetText();
            
            if (item.Type == ItemType.ARMOR)
            {
                selectedResistance.gameObject.SetActive(true);
                selectedCondition.gameObject.SetActive(true);
                selectedResistance.text = item.Armor.ToString("0");
                selectedCondition.text = item.Condition[0].ToString("0.00") + "%";
            }
            else if (item.Type == ItemType.POTION)
            {
                selectedPotion.gameObject.SetActive(true);
                selectedDuration.gameObject.SetActive(true);
                selectedPotion.text = item.Effect.ToString();
                selectedDuration.text = item.Duration.ToString() + " sec.";
            }
            else if (item.Type == ItemType.WEAPON)
            {
                selectedDamage.gameObject.SetActive(true);
                selectedCondition.gameObject.SetActive(true);
                selectedDamage.text = item.Damage.ToString("0");
                selectedCondition.text = item.Condition[0].ToString("0.00") + "%";
            }
        }

        /// <summary>
        /// Resets the Text for special items
        /// </summary>
        private void resetText()
        {
            selectedResistance.gameObject.SetActive(false);
            selectedDamage.gameObject.SetActive(false);
            selectedCondition.gameObject.SetActive(false);
            selectedPotion.gameObject.SetActive(false);
            selectedDuration.gameObject.SetActive(false);
        }
        
    } // End Class
} // End Namespace

