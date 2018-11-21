
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using schoolRPG.Items;
using schoolRPG.SaveLoad;
using schoolRPG.Stats;
using TMPro;
using UnityEngine.UI;
using schoolRPG.Quests;

namespace schoolRPG
{
    /// <summary>
    /// Put this on any button that will be repeated within a Listbox
    /// </summary>
    public class HoverSelect : MonoBehaviour, ISelectHandler
    {
        [Tooltip("Only worry about this if this is a Trade Menu Item Button")]
        public bool isPlayerInv;

        /// <summary>
        /// Only needed if we will be using this in the Inventory Menu
        /// </summary>
        public BaseItem item { get; set; }
        /// <summary>
        /// Only needed if we will be using this in one of the Save/Load Menus
        /// </summary>
        public string savePath { get; set; }
        /// <summary>
        /// Only needed if we will be using this in the Quests Menu
        /// </summary>
        public bool IsObj { get; set; }
        public int Pos { get; set; }

        public void OnSelect(BaseEventData eventData)
        {
            if (gameObject.GetComponentInParent<SkillsMenu>() != null)
                gameObject.GetComponentInParent<SkillsMenu>().hoverSkill(Pos);
            else if (gameObject.GetComponentInParent<InventoryMenu>() != null)
                gameObject.GetComponentInParent<InventoryMenu>().SelectItem(item, Pos, isPlayerInv);
            else if (gameObject.GetComponentInParent<SaveMenu>() != null)
                gameObject.GetComponentInParent<SaveMenu>().SelectSave(Pos);
            else if (gameObject.GetComponentInParent<LoadMenu>() != null)
                gameObject.GetComponentInParent<LoadMenu>().SelectSave(Pos);
            else if (gameObject.GetComponentInParent<StatsMenu>() != null)
                gameObject.GetComponentInParent<StatsMenu>().selectedItem = Pos;
            else if (gameObject.GetComponentInParent<QuestsMenu>() != null)
                gameObject.GetComponentInParent<QuestsMenu>().SelectQuest(IsObj, Pos);
        }

        /// <summary>
        /// Call this Coroutine to select a button when a screen first opens
        /// </summary>
        /// <param name="btn"></param>
        /// <returns></returns>
        public static IEnumerator SelectFirstButton(Button btn)
        {
            yield return null;
            if (btn != null)
                btn.Select();
        }
        public static IEnumerator SelectInput(TMP_InputField input)
        {
            yield return null;
            if (input != null)
                input.Select();
        }
        public static IEnumerator SelectSelectable(Selectable itm)
        {
            yield return null;
            if (itm != null)
                itm.Select();
        }
    }
}

