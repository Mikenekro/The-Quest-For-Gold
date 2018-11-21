using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using schoolRPG.Items;

/// <summary>
/// Main Namespace for RPG
/// </summary>
namespace schoolRPG
{
    public enum BodyPart
    {
        BODY, HELMET, HAIR, SHIRT, PANTS, SHOES, GLOVES, WEAPONS
    }

    /// <summary>
    /// CharParts contains each Body Part for the Player and NPCs in the game
    /// </summary>
    [System.Serializable]
    public class CharParts
    {
        public string name;
        public string race;
        public Texture2D spriteSheet;

        /// <summary>
        /// The Icon that will appear in the Inventory and Equipped screens
        /// </summary>
        [Tooltip("The Icon that will appear in the Inventory and Equipped screens")]
        public Sprite inventoryIcon;

        [SerializeField]
        private List<Sprite> idleUp;
        [SerializeField]
        private List<Sprite> idleLeft;
        [SerializeField]
        private List<Sprite> idleDown;
        [SerializeField]
        private List<Sprite> idleRight;

        [SerializeField]
        private List<Sprite> walkUp;
        [SerializeField]
        private List<Sprite> walkLeft;
        [SerializeField]
        private List<Sprite> walkDown;
        [SerializeField]
        private List<Sprite> walkRight;

        [SerializeField]
        private List<Sprite> attackUp;
        [SerializeField]
        private List<Sprite> attackLeft;
        [SerializeField]
        private List<Sprite> attackDown;
        [SerializeField]
        private List<Sprite> attackRight;

        [SerializeField]
        private List<Sprite> magicUp;
        [SerializeField]
        private List<Sprite> magicLeft;
        [SerializeField]
        private List<Sprite> magicDown;
        [SerializeField]
        private List<Sprite> magicRight;

        [SerializeField]
        private List<Sprite> death;

        public List<Sprite> IdleUp { get { return idleUp; } }
        public List<Sprite> IdleLeft { get { return idleLeft; } }
        public List<Sprite> IdleDown { get { return idleDown; } }
        public List<Sprite> IdleRight { get { return idleRight; } }

        public List<Sprite> WalkUp { get { return walkUp; } }
        public List<Sprite> WalkLeft { get { return walkLeft; } }
        public List<Sprite> WalkDown { get { return walkDown; } }
        public List<Sprite> WalkRight { get { return walkRight; } }

        public List<Sprite> AttackUp { get { return attackUp; } }
        public List<Sprite> AttackLeft { get { return attackLeft; } }
        public List<Sprite> AttackDown { get { return attackDown; } }
        public List<Sprite> AttackRight { get { return attackRight; } }

        public List<Sprite> MagicUp { get { return magicUp; } }
        public List<Sprite> MagicLeft { get { return magicLeft; } }
        public List<Sprite> MagicDown { get { return magicDown; } }
        public List<Sprite> MagicRight { get { return magicRight; } }

        public List<Sprite> Death { get { return death; } }

    }

    /// <summary>
    /// CharList creates a list of character parts separated by Animation type
    /// </summary>
    public class CharList : MonoBehaviour
    {
        public static CharList charBodyParts;

        public List<CharParts> body;
        public List<CharParts> helmet;
        public List<CharParts> hair;
        public List<CharParts> shirt;
        public List<CharParts> pants;
        public List<CharParts> shoes;
        public List<CharParts> gloves;
        public List<CharParts> weapons;

        [Tooltip("A List for each NPC Sprite")]
        public List<CharParts> npc;

        public void Start()
        {
            charBodyParts = gameObject.GetComponent<CharList>();
        }

        /// <summary>
        /// Equips the selected body part based on the item entered. Also changes the color of the item
        /// <para></para>
        /// Note: Pass in "null" for the item to remove an item and bring the color back to normal
        /// </summary>
        public void EquipItem(CharStats stats, BodyPart part, BaseItem item)
        {
            int pos;
            Color col;

            if (item == null)
            {
                pos = 0;
                if (part == BodyPart.SHIRT)
                    col = stats.baseShirtColor;
                else if (part == BodyPart.PANTS)
                    col = stats.basePantsColor;
                else if (part == BodyPart.SHOES)
                    col = stats.baseShoesColor;
                else
                {
                    col = Color.white;
                    pos = -2;
                }
            }
            else
            {
                pos = item.ArrayPos;
                col = new Color(item.ItemColor.R, item.ItemColor.G, item.ItemColor.B, item.ItemColor.A);
            }

            if (part == BodyPart.HELMET)
            {
                stats.helmColor = col;
                stats.HelmPos = pos;
            }
            else if (part == BodyPart.HAIR)
            {
                stats.hairColor = col;
                stats.HairPos = pos;
            }
            else if (part == BodyPart.SHIRT)
            {
                stats.shirtColor = col;
                stats.ShirtPos = pos;
            }
            else if (part == BodyPart.PANTS)
            {
                stats.pantsColor = col;
                stats.PantsPos = pos;
            }
            else if (part == BodyPart.SHOES)
            {
                stats.shoesColor = col;
                stats.ShoesPos = pos;
            }
            else if (part == BodyPart.GLOVES)
            {
                stats.glovesColor = col;
                stats.GlovesPos = pos;
            }
            else if (part == BodyPart.WEAPONS)
            {
                stats.weaponsColor = col;
                stats.WeaponPos = pos;
            }

            stats.UpdateColors();
        }

        /// <summary>
        /// Call this when you want to change genders
        /// </summary>
        /// <param name="male"></param>
        /// <returns></returns>
        public void ChangeGenderPart(CharStats playerStats)
        {
            int genderMove;
            if (playerStats.IsMale)
                genderMove = -1;
            else
                genderMove = 1;

            if (playerStats.BodyPos > -1)
                playerStats.BodyPos += genderMove;
            if (playerStats.GlovesPos > -1)
                playerStats.GlovesPos += genderMove;
            if (playerStats.HairPos > -1)
                playerStats.HairPos += genderMove;
            if (playerStats.HelmPos > -1)
                playerStats.HelmPos += genderMove;
            if (playerStats.PantsPos > -1)
                playerStats.PantsPos += genderMove;
            if (playerStats.ShirtPos > -1)
                playerStats.ShirtPos += genderMove;
            if (playerStats.ShoesPos > -1)
                playerStats.ShoesPos += genderMove;
            if (playerStats.WeaponPos > -1)
                playerStats.WeaponPos += genderMove;
            
        }

        public void ChangeHairPart(bool next, int pos, CharStats playerStats)
        {
            if (next)
            {
                if (pos + 2 <= hair.Count - 1)
                    pos += 2;
                else
                    pos = 0;
            }
            else
            {
                if (pos - 2 >= 0)
                    pos -= 2;
                else
                    pos = hair.Count - 1;
            }

            // Make sure we are displaying the correct gender specific hair
            if (pos == 0 && !playerStats.IsMale)
                pos = 1;
            else if (pos == hair.Count - 1 && playerStats.IsMale)
                pos -= 1;

            if (playerStats.HairPos > -1)
                playerStats.HairPos = pos;
        }

        /// <summary>
        /// Call this to change the Characters race
        /// </summary>
        /// <param name="male"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public void ChangeRacePart(bool next, int pos, CharStats playerStats)
        {
            if (next)
            {
                if (pos + 2 <= body.Count - 1)
                    pos += 2;
                else
                    pos = 0;
            }
            else
            {
                if (pos - 2 >= 0)
                    pos -= 2;
                else
                    pos = body.Count - 1;
            }

            // Make sure we don't display a male character when the races roll over
            if (pos == 0 && !playerStats.IsMale)
                pos = 1;
            else if (pos == body.Count - 1 && playerStats.IsMale)
                pos -= 1;

            if (playerStats.BodyPos > -1)
                playerStats.BodyPos = pos;
            
        }

    }
}

