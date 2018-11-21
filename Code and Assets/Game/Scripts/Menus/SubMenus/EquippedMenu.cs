using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using schoolRPG.Items;
using schoolRPG.Stats;

using TeamUtility.IO;
using TMPro;
using System;

namespace schoolRPG.Items
{
    /// <summary>
    /// Runs when the Equipped Items menu is open
    /// </summary>
    public class EquippedMenu : MonoBehaviour
    {
        private CharStats cs;

        public TextMeshProUGUI warningTxt;
        public Button firstSelect;

        public PlayerController player;
        public TextMeshProUGUI playerName;

        public TextMeshProUGUI selectedName;
        public TextMeshProUGUI selectedValue;
        public TextMeshProUGUI selectedWeight;
        public TextMeshProUGUI selectedDamage;
        public TextMeshProUGUI selectedResistance;
        public TextMeshProUGUI selectedPotion;
        public TextMeshProUGUI selectedCondition;
        public TextMeshProUGUI selectedDuration;
        public TextMeshProUGUI selectedDescription;

        public TextMeshProUGUI totValue;
        public TextMeshProUGUI totWeight;
        public TextMeshProUGUI totDam;
        public TextMeshProUGUI totResist;
        public TextMeshProUGUI totSpellDam;
        public TextMeshProUGUI avgCond;

        public GameObject helmSlot;
        public GameObject shirtSlot;
        public GameObject glovesSlot;
        public GameObject pantsSlot;
        public GameObject bootsSlot;
        public GameObject weaponSlot;
        public GameObject spellSlot;

        private bool helmSelect;
        private bool shirtSelect;
        private bool glovesSelect;
        private bool pantsSelect;
        private bool bootsSelect;
        private bool weaponSelect;
        private bool spellSelect;

        // Use this for initialization
        void Start()
        {
            cs = player.gameObject.GetComponent<CharStats>();
            LoadSlots();
            //StartCoroutine(InvItem.SelectFirstButton(firstSelect));
        }

        void OnEnable()
        {
            warningTxt.text = "";
            //if (load)
            LoadSlots();
        }

        void Update()
        {
            if (WorldController.InMenu)
            {
                if (InputManager.GetButtonUp("Use")) // Un-Equip the item
                {
                    if (helmSelect && player.Player.EquipHelm != null)
                        player.Player.EquipHelm = unEquip(player.Player.EquipHelm, BodyPart.HELMET);
                    else if (shirtSelect && player.Player.EquipBody != null)
                        player.Player.EquipBody = unEquip(player.Player.EquipBody, BodyPart.SHIRT);
                    else if (pantsSelect && player.Player.EquipPants != null)
                        player.Player.EquipPants = unEquip(player.Player.EquipPants, BodyPart.PANTS);
                    else if (glovesSelect && player.Player.EquipGloves != null)
                        player.Player.EquipGloves = unEquip(player.Player.EquipGloves, BodyPart.GLOVES);
                    else if (bootsSelect && player.Player.EquipBoots != null)
                        player.Player.EquipBoots = unEquip(player.Player.EquipBoots, BodyPart.SHOES);
                    else if (weaponSelect && player.Player.EquipWeapon != null)
                        player.Player.EquipWeapon = unEquip(player.Player.EquipWeapon, BodyPart.WEAPONS);
                    else if (spellSelect && player.Player.ActiveSpell != null)
                        unEquip(player.Player.ActiveSpell);

                    LoadSlots();
                }
                else if (InputManager.GetButtonUp("Drop")) // Drop the item from the Inventory
                {
                    if (helmSelect && player.Player.EquipHelm != null)
                        player.Player.EquipHelm = drop(player.Player.EquipHelm, BodyPart.HELMET);
                    else if (shirtSelect && player.Player.EquipBody != null)
                        player.Player.EquipBody = drop(player.Player.EquipBody, BodyPart.SHIRT);
                    else if (pantsSelect && player.Player.EquipPants != null)
                        player.Player.EquipPants = drop(player.Player.EquipPants, BodyPart.PANTS);
                    else if (glovesSelect && player.Player.EquipGloves != null)
                        player.Player.EquipGloves = drop(player.Player.EquipGloves, BodyPart.GLOVES);
                    else if (bootsSelect && player.Player.EquipBoots != null)
                        player.Player.EquipBoots = drop(player.Player.EquipBoots, BodyPart.SHOES);
                    else if (weaponSelect && player.Player.EquipWeapon != null)
                        player.Player.EquipWeapon = drop(player.Player.EquipWeapon, BodyPart.WEAPONS);
                    else if (spellSelect && player.Player.ActiveSpell != null)
                        StartCoroutine(dropSpell(player.Player.ActiveSpell));

                    LoadSlots();
                }
            }
            
        }

        /// <summary>
        /// Calculate the Totals of the Equipped Items
        /// </summary>
        private void CalcTotals()
        {
            double val = 0;
            float weight = 0;
            float dam = 0;
            float resist = 0;
            float spellDam = 0;
            float cond = 0;
            int totItems = 0;

            if (player.Player.EquipBody != null)
            {
                totItems++;
                cond += player.Player.EquipBody.Condition[0];
                val += player.Player.EquipBody.Value;
                weight += player.Player.EquipBody.Weight;
                resist += player.Player.EquipBody.Armor;
            }
            if (player.Player.EquipBoots != null)
            {
                totItems++;
                cond += player.Player.EquipBoots.Condition[0];
                val += player.Player.EquipBoots.Value;
                weight += player.Player.EquipBoots.Weight;
                resist += player.Player.EquipBoots.Armor;
            }
            if (player.Player.EquipGloves != null)
            {
                totItems++;
                cond += player.Player.EquipGloves.Condition[0];
                val += player.Player.EquipGloves.Value;
                weight += player.Player.EquipGloves.Weight;
                resist += player.Player.EquipGloves.Armor;
            }
            if (player.Player.EquipHelm != null)
            {
                totItems++;
                cond += player.Player.EquipHelm.Condition[0];
                val += player.Player.EquipHelm.Value;
                weight += player.Player.EquipHelm.Weight;
                resist += player.Player.EquipHelm.Armor;
            }
            if (player.Player.EquipPants != null)
            {
                totItems++;
                cond += player.Player.EquipPants.Condition[0];
                val += player.Player.EquipPants.Value;
                weight += player.Player.EquipPants.Weight;
                resist += player.Player.EquipPants.Armor;
            }
            if (player.Player.EquipWeapon != null)
            {
                totItems++;
                cond += player.Player.EquipWeapon.Condition[0];
                val += player.Player.EquipWeapon.Value;
                weight += player.Player.EquipWeapon.Weight;
                dam += player.Player.EquipWeapon.Damage;
            }
            if (player.Player.ActiveSpell != null)
            {
                val += player.Player.ActiveSpell.Value;
                spellDam += player.Player.ActiveSpell.Power;
            }


            totValue.text = val.ToString("###,##0.##") + " Gold";
            totWeight.text = weight.ToString("###,##0.##") + " Lbs.";
            totDam.text = dam.ToString("###,##0.##");
            totResist.text = resist.ToString("###,##0.##");
            totSpellDam.text = spellDam.ToString("###,##0.##") + " / Sec.";
            avgCond.text = (cond / totItems).ToString("##0.##") + "%";
    }

        /// <summary>
        /// Unequips the item that is input
        /// </summary>
        /// <param name="item"></param>
        private BaseItem unEquip(BaseItem item, BodyPart part)
        {
            string n = item.Name;
            // Reset any of the Base Item Colors
            if (shirtSelect)
                cs.shirtColor = cs.baseShirtColor;
            else if (pantsSelect)
                cs.pantsColor = cs.basePantsColor;
            else if (bootsSelect)
                cs.shoesColor = cs.baseShoesColor;
            
            // Add the item to the Players Inventory
            player.Player.Inventory.AddItem(item);

            // Change the in-game sprite
            CharList.charBodyParts.EquipItem(cs, part, null);

            item = null;

            StartCoroutine(unequipItem(n, false));

            return item;
        }
        private IEnumerator unequipItem(string name, bool isSpell)
        {
            string t = ((isSpell)?("spell"):("item"));
            warningTxt.text = "Un-Equipped " + t + " <color=green>" + name + "</color>!";
            yield return new WaitForSeconds(5.0f);
            warningTxt.text = "";
            yield return null;
        }
        /// <summary>
        /// Unequips the Spell that the Player has equipped
        /// </summary>
        /// <param name="spell"></param>
        private void unEquip(Spell spell)
        {
            string n = spell.Name;

            // Don't need to add the spell to any inventory
            player.Player.ActiveSpell = null;
            if (player.Player.ActiveSpell == null)
                Debug.Log("No Spell Attached!");

            StartCoroutine(unequipItem(n, true));

            LoadSlots();
        }

        private BaseItem drop(BaseItem item, BodyPart part)
        {
            string n = item.Name;
            // Spawn an item at the players position
            WorldController.AddItemObj(item, player.gameObject.transform.position, player.PlayerStats.LookDir);
            // Change the in-game sprite
            CharList.charBodyParts.EquipItem(cs, part, null);
            // Destroy the item
            item = null;
            // Display text that dropping was successful
            StartCoroutine(dropItem(n));
            
            return item;
        }
        

        private IEnumerator dropItem(string name)
        {
            warningTxt.text = "Sucessfully dropped item <color=green>" + name + "</color>!";
            yield return new WaitForSeconds(5.0f);
            warningTxt.text = "";
            yield return null;
        }
        private IEnumerator dropSpell(Spell spell)
        {
            warningTxt.text = "<color=red>Warning!</color> Cannot destroy spells from this menu!";
            yield return new WaitForSeconds(5.0f);
            warningTxt.text = "";
            yield return null;
        }

        /// <summary>
        /// Load each Slot with the Players Equipped items
        /// </summary>
        private void LoadSlots()
        {
            playerName.text = player.Player.Name;

            ResetItemDesc();

            // Disable any images over the slots
            helmSlot.GetComponentsInChildren<Image>()[1].enabled = false;
            shirtSlot.GetComponentsInChildren<Image>()[1].enabled = false;
            glovesSlot.GetComponentsInChildren<Image>()[1].enabled = false;
            pantsSlot.GetComponentsInChildren<Image>()[1].enabled = false;
            bootsSlot.GetComponentsInChildren<Image>()[1].enabled = false;
            weaponSlot.GetComponentsInChildren<Image>()[1].enabled = false;
            spellSlot.GetComponentsInChildren<Image>()[1].enabled = false;

            // Enable the Empty text on each slot
            helmSlot.GetComponentsInChildren<TextMeshProUGUI>()[1].enabled = true;
            shirtSlot.GetComponentsInChildren<TextMeshProUGUI>()[1].enabled = true;
            glovesSlot.GetComponentsInChildren<TextMeshProUGUI>()[1].enabled = true;
            pantsSlot.GetComponentsInChildren<TextMeshProUGUI>()[1].enabled = true;
            bootsSlot.GetComponentsInChildren<TextMeshProUGUI>()[1].enabled = true;
            weaponSlot.GetComponentsInChildren<TextMeshProUGUI>()[1].enabled = true;
            spellSlot.GetComponentsInChildren<TextMeshProUGUI>()[1].enabled = true;

            StartCoroutine(HoverSelect.SelectFirstButton(firstSelect));
            // Check for any equipped items
            if (player.Player.ActiveSpell != null)
                OnSpell();
            if (player.Player.EquipPants != null)
                OnPants();
            if (player.Player.EquipBoots != null)
                OnBoots();
            if (player.Player.EquipWeapon != null)
                OnWeapon();
            if (player.Player.EquipGloves != null)
                OnGloves();
            if (player.Player.EquipBody != null)
                OnShirt();
            if (player.Player.EquipHelm != null)
                OnHelmet();

            CalcTotals();
        }


        public void ResetItemDesc()
        {
            selectedName.text = "[No Item Selected]";
            // De-activate any item descriptors
            selectedValue.gameObject.SetActive(false);
            selectedWeight.gameObject.SetActive(false);
            selectedDamage.gameObject.SetActive(false);
            selectedResistance.gameObject.SetActive(false);
            selectedPotion.gameObject.SetActive(false);
            selectedCondition.gameObject.SetActive(false);
            selectedDuration.gameObject.SetActive(false);
            selectedDescription.gameObject.SetActive(false);

            selectedValue.text = "";
            selectedWeight.text = "";
            selectedDamage.text = "";
            selectedResistance.text = "";
            selectedPotion.text = "";
            selectedCondition.text = "";
            selectedDuration.text = "";
            selectedDescription.text = "";
        }

        public void ResetSelect()
        {
            helmSelect = false;
            shirtSelect = false;
            pantsSelect = false;
            glovesSelect = false;
            bootsSelect = false;
            weaponSelect = false;
            spellSelect = false;
        }

        public void OnHelmet()
        {
            ResetSelect();
            helmSelect = true;
            ResetItemDesc();
            // Load if the player has a helm equipped
            if (player.Player.EquipHelm != null)
            {
                Image img = helmSlot.GetComponentsInChildren<Image>()[1];
                // Set and Enable the Sprite for the Item
                img.enabled = true;
                img.sprite =
                    CharList.charBodyParts.helmet[player.Player.EquipHelm.ArrayPos].inventoryIcon;
                helmSlot.GetComponentsInChildren<TextMeshProUGUI>()[1].enabled = false;

                //setButton(helmSlot);
                setBaseItem(player.Player.EquipHelm, "0.00", "0 lbs.", "0", "%", true);
            }
        }
        public void OnShirt()
        {
            ResetSelect();
            shirtSelect = true;
            ResetItemDesc();
            // Load if the player has a shirt equipped
            if (player.Player.EquipBody != null)
            {
                Image img = shirtSlot.GetComponentsInChildren<Image>()[1];
                // Set and Enable the Sprite for the Item
                img.enabled = true;
                img.sprite =
                    CharList.charBodyParts.shirt[player.Player.EquipBody.ArrayPos].inventoryIcon;
                shirtSlot.GetComponentsInChildren<TextMeshProUGUI>()[1].enabled = false;
                
                setBaseItem(player.Player.EquipBody, "0.00", "0 lbs.", "0", "%", true);
            }
        }
        public void OnGloves()
        {
            ResetSelect();
            glovesSelect = true;
            ResetItemDesc();
            // Load if the player has gloves equipped
            if (player.Player.EquipGloves != null)
            {
                Image img = glovesSlot.GetComponentsInChildren<Image>()[1];
                // Set and Enable the Sprite for the Item
                img.enabled = true;
                img.sprite =
                    CharList.charBodyParts.gloves[player.Player.EquipGloves.ArrayPos].inventoryIcon;
                glovesSlot.GetComponentsInChildren<TextMeshProUGUI>()[1].enabled = false;
                
                setBaseItem(player.Player.EquipGloves, "0.00", "0 lbs.", "0", "%", true);
            }
        }
        public void OnWeapon()
        {
            ResetSelect();
            weaponSelect = true;
            ResetItemDesc();
            // Load if the player has a weapon equipped
            if (player.Player.EquipWeapon != null)
            {
                Image img = weaponSlot.GetComponentsInChildren<Image>()[1];
                List<CharParts> weps = CharList.charBodyParts.weapons;
                // Set and Enable the Sprite for the Item
                img.enabled = true;
                img.sprite = weps[player.Player.EquipWeapon.ArrayPos].inventoryIcon;
                weaponSlot.GetComponentsInChildren<TextMeshProUGUI>()[1].enabled = false;
                
                setBaseItem(player.Player.EquipWeapon, "0.00", "0 lbs.", "0", "%", false);
            }
        }
        public void OnPants()
        {
            ResetSelect();
            pantsSelect = true;
            ResetItemDesc();
            // Load if the player has pants equipped
            if (player.Player.EquipPants != null)
            {
                Image img = pantsSlot.GetComponentsInChildren<Image>()[1];
                // Set and Enable the Sprite for the Item
                img.enabled = true;
                img.sprite =
                    CharList.charBodyParts.pants[player.Player.EquipPants.ArrayPos].inventoryIcon;
                pantsSlot.GetComponentsInChildren<TextMeshProUGUI>()[1].enabled = false;
                
                setBaseItem(player.Player.EquipPants, "0.00", "0 lbs.", "0", "%", true);
            }
        }
        public void OnBoots()
        {
            ResetSelect();
            bootsSelect = true;
            ResetItemDesc();
            // Load if the player has boots equipped
            if (player.Player.EquipBoots != null)
            {
                Image img = bootsSlot.GetComponentsInChildren<Image>()[1];
                // Set and Enable the Sprite for the Item
                img.enabled = true;
                img.sprite =
                    CharList.charBodyParts.shoes[player.Player.EquipBoots.ArrayPos].inventoryIcon;
                bootsSlot.GetComponentsInChildren<TextMeshProUGUI>()[1].enabled = false;
                
                setBaseItem(player.Player.EquipBoots, "0.00", "0 lbs.", "0", "%", true);
            }
        }

        public void setButton(GameObject slot)
        {
            StartCoroutine(HoverSelect.SelectFirstButton(slot.GetComponent<Button>()));
        }

        /// <summary>
        /// Selects the Item on the equipped screen
        /// </summary>
        private void setBaseItem(BaseItem item, string valueStr, string weightStr, string armorStr, string condStr, bool armor)
        {
            // Enable the correct text items
            selectedValue.gameObject.SetActive(true);
            selectedWeight.gameObject.SetActive(true);
            selectedCondition.gameObject.SetActive(true);
            selectedDescription.gameObject.SetActive(true);
            if (armor)
                selectedResistance.gameObject.SetActive(true);
            else
                selectedDamage.gameObject.SetActive(true);

            // Set the item specific text
            if (item != null)
            {
                selectedName.text = item.Name;
                selectedValue.text = item.Value.ToString(valueStr);
                selectedWeight.text = item.Weight.ToString(weightStr);
                selectedCondition.text = item.Condition[0].ToString("0") + condStr;
                // 23 spaces before to account for the title
                selectedDescription.text = "                       " + item.Description;
                // Is this item armor or a weapon?
                if (armor)
                    selectedResistance.text = item.Armor.ToString(armorStr);
                else
                    selectedDamage.text = item.Damage.ToString(armorStr);
            }
        }

        public void OnSpell()
        {
            ResetSelect();
            spellSelect = true;
            ResetItemDesc();
            selectedValue.gameObject.SetActive(true);
            selectedWeight.gameObject.SetActive(true);
            selectedDamage.gameObject.SetActive(true);
            selectedCondition.gameObject.SetActive(true);
            selectedDescription.gameObject.SetActive(true);

            if (player.Player.ActiveSpell != null)
            {
                // Enable the Sprite for the spell
                spellSlot.GetComponentsInChildren<Image>()[1].enabled = true;
                spellSlot.GetComponentsInChildren<Image>()[1].sprite = 
                    IconList.list.spellIcons[Spells.GetSpellLoc(player.Player.ActiveSpell)];
                spellSlot.GetComponentsInChildren<TextMeshProUGUI>()[1].enabled = false;

                selectedName.text = player.Player.ActiveSpell.Name;
                selectedValue.text = player.Player.ActiveSpell.Value.ToString("00.00");
                selectedWeight.text = player.Player.ActiveSpell.Power.ToString("0") + "/sec for " + 
                                      player.Player.ActiveSpell.Time.ToString("0") + " sec";
                selectedDamage.text = player.Player.ActiveSpell.Range.ToString("0") + " feet";
                // 23 spaces before the text to account for the title
                selectedDescription.text = "                       " + player.Player.ActiveSpell.Description;
            }
        }
    }
}

