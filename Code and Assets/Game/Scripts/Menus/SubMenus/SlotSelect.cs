using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;// Required when using Event data.

using schoolRPG.Items;

namespace schoolRPG
{
    public enum EquipType
    {
        HELM, SHIRT, PANTS, SHOES, GLOVES, WEAPON, SPELL
    }
    /// <summary>
    /// Highlights the SlotImage when an Equipment slot is selected or deselected
    /// </summary>
    public class SlotSelect : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [SerializeField]
        private EquipType _part;

        private static EquippedMenu menu;

        public EquipType Part { get { return _part; } }

        public void OnSelect(BaseEventData data)
        {
            if (data.selectedObject.GetComponentsInChildren<Image>()[1].enabled)
            {
                data.selectedObject.GetComponentsInChildren<Image>()[1].color =
                    data.selectedObject.GetComponent<Button>().colors.highlightedColor;

                if (menu == null)
                    menu = gameObject.GetComponentInParent<EquippedMenu>();

                if (data.selectedObject.GetComponentsInChildren<Image>()[1].color ==
                    data.selectedObject.GetComponent<Button>().colors.highlightedColor)
                {
                    if (data.selectedObject.GetComponent<SlotSelect>().Part == EquipType.GLOVES)
                        menu.OnGloves();
                    else if (data.selectedObject.GetComponent<SlotSelect>().Part == EquipType.HELM)
                        menu.OnHelmet();
                    else if (data.selectedObject.GetComponent<SlotSelect>().Part == EquipType.PANTS)
                        menu.OnPants();
                    else if (data.selectedObject.GetComponent<SlotSelect>().Part == EquipType.SHIRT)
                        menu.OnShirt();
                    else if (data.selectedObject.GetComponent<SlotSelect>().Part == EquipType.SHOES)
                        menu.OnBoots();
                    else if (data.selectedObject.GetComponent<SlotSelect>().Part == EquipType.SPELL)
                        menu.OnSpell();
                    else if (data.selectedObject.GetComponent<SlotSelect>().Part == EquipType.WEAPON)
                        menu.OnWeapon();
                }
            }
        }
        public void OnDeselect(BaseEventData data)
        {
            if (data.selectedObject.GetComponentsInChildren<Image>()[1].enabled)
                data.selectedObject.GetComponentsInChildren<Image>()[1].color =
                    data.selectedObject.GetComponent<Button>().colors.normalColor;
        }
    }
}

