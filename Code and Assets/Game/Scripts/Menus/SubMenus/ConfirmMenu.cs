using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace schoolRPG.Stats
{
    /// <summary>
    /// Confirms that the player made the correct choices during character creation
    /// </summary>
    public class ConfirmMenu : MonoBehaviour
    {

        [SerializeField]
        private PlayerController pc;
        private CharStats cs;

        [SerializeField]
        private TextMeshProUGUI pName;
        [SerializeField]
        private TextMeshProUGUI pGender;
        [SerializeField]
        private TextMeshProUGUI pRace;
        [SerializeField]
        private Button startSel;

        void Start()
        {
            cs = pc.gameObject.GetComponent<CharStats>();
            OnEnable();
        }

        // Called when the object the script is attached to is activated
        void OnEnable()
        {
            if (pc.Player.Gender == "" || pc.Player.Gender == null)
                pc.Player.Gender = "Male";
            if (pc.Player.Race == "" || pc.Player.Race == null)
                pc.Player.Race = "Light Human";

            // Set the players stats
            pName.text = "Name: " + pc.Player.Name;
            pGender.text = "Gender: " + pc.Player.Gender;
            pRace.text = "Race: " + pc.Player.Race;

            // Set the base colors of each item that we can change the colors for
            if (cs != null)
            {
                cs.baseHairColor = new Color(cs.hairColor.r, cs.hairColor.g, cs.hairColor.b, cs.hairColor.a);
                cs.basePantsColor = new Color(cs.pantsColor.r, cs.pantsColor.g, cs.pantsColor.b, cs.pantsColor.a);
                cs.baseShirtColor = new Color(cs.shirtColor.r, cs.shirtColor.g, cs.shirtColor.b, cs.shirtColor.a);
                cs.baseShoesColor = new Color(cs.shoesColor.r, cs.shoesColor.g, cs.shoesColor.b, cs.shoesColor.a);
            }

            // Make sure our stats are correct
            pc.Player.SetBaseStats(true);

            // Select the Button when starting
            StartCoroutine(HoverSelect.SelectFirstButton(startSel));
        }
        
    }

}
