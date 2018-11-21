using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using schoolRPG.Items;
using schoolRPG.Sound;

/// <summary>
/// Main Namespace for RPG
/// </summary>
namespace schoolRPG
{
    /// <summary>
    /// Switches the Sprites from an Animation at runtime based of of the specified position in the Parent.
	/// This allows a pre-built animation to use a different sprite than it was originally assigned.
    /// <para></para>
    /// Note: Throw this on an object that has animated sprites and is a Child of an object with the Character class. 
    /// </summary>
    public class SpriteSwitcher : MonoBehaviour
    {
        [SerializeField]
        private bool isPlayer;

        [SerializeField]
        private CharStats _charStat;

        private AudioSource source;
        private int lastSwing;

        public CharStats C { get; set; }
        /// <summary>
        /// Used to set which Item a Character currently has equipped (Default Color if none is set)
        /// </summary>
        public BaseItem Item { get; set; }
        public string SpriteSheet { get; set; }
        public string PartTag { get; set; }
        public bool UseNpc { get; set; }

        // Sprite Data
        private Sprite newSprite;
        private Color newColor;
        private int spritePos;
        private List<CharParts> sprites;
        private List<Sprite> allSprites;
        private SpriteRenderer[] sRenderer;
        private Sprite[] rendSprite;

        private string spriteName;
        private int spriteCount;
        private int i;
        private int j;
        private int k;

        // Reflection Vars
        private Type type;
        private PropertyInfo[] info;

        // Player or NPC vars
        private PlayerController pc;
        private NpcController nc;
        private CharStats cs;

        // Audio Source on the Sprite Switcher
        private AudioSource audio;

        /// <summary>
        /// Last Position (For Steps and Distance)
        /// </summary>
        private Vector3 lastP;

        // ***** Un-Comment if you need to change Clothes from inspector
        //Type character;
        //PropertyInfo[] charInfo;


        // Use this for initialization
        private void Start()
        {
            // Initialization of main variables
            // Make sure we always have CharStats
            C = GetComponentInParent<CharStats>();
            if (C == null)
                C = _charStat;
            sRenderer = GetComponents<SpriteRenderer>();
            allSprites = new List<Sprite>();

            if (gameObject.GetComponentInParent<NpcController>() != null)
                nc = gameObject.GetComponentInParent<NpcController>();
            else
                pc = gameObject.GetComponentInParent<PlayerController>();

            cs = gameObject.GetComponentInParent<CharStats>();

            // Try to get the Audio Source
            if (GetComponent<AudioSource>() != null)
                source = GetComponent<AudioSource>();

            if (source != null)
            {
                source.spatialize = true;
            }

            // Get the AudioSource for the body parts that contain Audio
            if (GetComponent<AudioSource>() != null)
            {
                SoundMenu.ChangeVolume += Volume;
                audio = GetComponent<AudioSource>();
            }

            rendSprite = new Sprite[sRenderer.Length];
            for (i = 0; i < sRenderer.Length; ++i)
            {
                rendSprite[i] = sRenderer[i].sprite;
            }
            // ***** Un-Comment if you need to change Clothes from inspector
            //// Gather Character data for reflection over properties
            //character = c.GetType();
            //charInfo = character.GetProperties();


            lastP = gameObject.transform.position;
            SetList();
        }

        public void OnDisable()
        {
            if (audio != null)
            {
                SoundMenu.ChangeVolume -= Volume;
            }
        }

        /// <summary>
        /// Sets the Volume for any Audio Source when the Volume is changed
        /// </summary>
        public void Volume()
        {
            // Set the Audio Volume if it has an Audio Source
            if (audio != null)
                audio.volume = SoundController.enviro * SoundController.master;
        }

        /// <summary>
        /// Call this function whenever a change is made to a Characters clothing
        /// </summary>
        public void SwitchList()
        {
            SetList();
        }

        /// <summary>
        /// Runs when the Attack or Casting Animation finishes
        /// </summary>
        public void FinishAnim()
        {
            if (pc != null)
            {
                pc.Player.IsAttacking = false;
                pc.Player.IsPowerAttacking = false;
                pc.Player.IsCasting = false;
            }
            else
            {
                nc.NPC.IsAttacking = false;
                nc.NPC.IsPowerAttacking = false;
                nc.NPC.IsCasting = false;
            }

            cs.IsAttacking = false;
            cs.IsPowerAttacking = false;
            cs.IsCasting = false;
        }

        /// <summary>
        /// Plays the Attack Sound
        /// </summary>
        public void PlayAttack()
        {
            // Play Attacking Sound
            if (source != null)
                lastSwing = SoundController.SwingSword(source, lastSwing);
        }
        
        /// <summary>
        /// Plays the Walking sound for left foot
        /// </summary>
        public void PlayWalkLeft()
        {
            if (source != null)
            {
                SoundController.Footstep(source, false);

                // Add 2 feet to total distance traveled if is player
                if (isPlayer)
                {
                    float dist = Vector3.Distance(gameObject.transform.position, lastP);
                    lastP = gameObject.transform.position;
                    //Debug.Log("Speed: " + pc.Speed + ", Distance: " + dist + " ft., Steps: " + ((dist < 2) ? (1) : (Mathf.RoundToInt(dist) / 2)));
                    WorldController.Data.StatValue.StepsTaken += ((dist < 2) ? (1) : (Mathf.RoundToInt(dist)));
                    WorldController.Data.StatValue.DistanceTraveled += dist;
                }
            }
        }
        /// <summary>
        /// Plays the Walking sound for right foot
        /// </summary>
        public void PlayWalkRight()
        {
            if (source != null)
            {
                SoundController.Footstep(source, true);

                // Add 2 feet to total distance traveled if is player
                if (isPlayer)
                {
                    float dist = Vector3.Distance(gameObject.transform.position, lastP);
                    lastP = gameObject.transform.position;
                    //Debug.Log("Speed: " + pc.Speed + ", Distance: " + dist + " ft., Steps: " + ((dist < 2) ? (1) : (Mathf.RoundToInt(dist) / 2)));
                    WorldController.Data.StatValue.StepsTaken += ((dist < 2) ? (1) : (Mathf.RoundToInt(dist)));
                    WorldController.Data.StatValue.DistanceTraveled += dist;
                }
            }
        }

        //public void Update()
        //{
        //    // ***** Un-Comment if you need to change Clothes from inspector
        //    //// Loop through each Property in Character
        //    //for (i = 1; i < 9; ++i)
        //    //{
        //    //    // Check if any property is different from spritechange
        //    //    if (spriteChange[i-1] != (int)charInfo[i].GetValue(c, null))
        //    //    {
        //    //        // Set value and change sprite
        //    //        spriteChange[i-1] = (int)charInfo[i].GetValue(c, null);
        //    //        switchList();
        //    //    }
        //    //}
        //}
        
        // LateUpdate is called after Update() and before rendering
        private void LateUpdate()
        {
            SetSprite();
        }

        /// <summary>
        /// Runs in LateUpdate() to update the characters Animation Sprites with a specific style
        /// </summary>
        private void SetSprite()
        {
            // Set the sprite based on the current "Animations Sprite Name"
            for (i = 0; i < sRenderer.Length; ++i)
            {
                if (sRenderer[i].sprite != null)
                {
                    spriteName = sRenderer[i].sprite.name;

                    sRenderer[i].sprite = SetArea();

                    if (sRenderer[i].color != newColor)
                        sRenderer[i].color = newColor;
                }
            }
        }

        /// <summary>
        /// Set the sprite based on sprite name
        /// </summary>
        private Sprite SetArea()
        {
            for (j = 0; j < spriteCount; ++j)
            {
                if (allSprites[j].name == spriteName)
                {
                    newSprite = allSprites[j];
                    break;
                }
            }

            return newSprite;
        }

        /// <summary>
        /// Set the list where we will get our sprites from. List contains each sprite from the currently active Body Part
        /// <para></para>
        /// Note: This is ONLY called when a character changes clothes.
        /// </summary>
        private void SetList()
        {
            PartTag = gameObject.tag;
            if (allSprites.Count > 0)
                allSprites.Clear();

            if (CharList.charBodyParts == null)
                CharList.charBodyParts = GameObject.Find("BodyParts").GetComponent<CharList>();

            switch (PartTag)
            {
                case "Body":
                    // Get list of sprites, sprite position, and the gender of the sprite
                    if (cs == null || !cs.UseNPC)
                        sprites = CharList.charBodyParts.body;
                    else
                        sprites = CharList.charBodyParts.npc;

                    spritePos = C.BodyPos;
                    // Cannot change Body Color
                    newColor = Color.white;
                    break;
                case "Hair":
                    sprites = CharList.charBodyParts.hair;
                    spritePos = C.HairPos;
                    newColor = new Color(C.hairColor.r, C.hairColor.g, C.hairColor.b, 1.0f);
                    break;
                case "Shirt":
                    sprites = CharList.charBodyParts.shirt;
                    spritePos = C.ShirtPos;
                    newColor = new Color(C.shirtColor.r, C.shirtColor.g, C.shirtColor.b, 1.0f);
                    break;
                case "Pants":
                    sprites = CharList.charBodyParts.pants;
                    spritePos = C.PantsPos;
                    newColor = new Color(C.pantsColor.r, C.pantsColor.g, C.pantsColor.b, 1.0f);
                    break;
                case "Shoes":
                    sprites = CharList.charBodyParts.shoes;
                    spritePos = C.ShoesPos;
                    newColor = new Color(C.shoesColor.r, C.shoesColor.g, C.shoesColor.b, 1.0f);
                    break;
                case "Gloves":
                    sprites = CharList.charBodyParts.gloves;
                    spritePos = C.GlovesPos;
                    newColor = new Color(C.glovesColor.r, C.glovesColor.g, C.glovesColor.b, 1.0f);
                    break;
                case "Helmet":
                    sprites = CharList.charBodyParts.helmet;
                    spritePos = C.HelmPos;
                    newColor = new Color(C.helmColor.r, C.helmColor.g, C.helmColor.b, 1.0f);
                    break;
                case "Weapon":
                    sprites = CharList.charBodyParts.weapons;
                    spritePos = C.WeaponPos;
                    newColor = new Color(C.weaponsColor.r, C.weaponsColor.g, C.weaponsColor.b, 1.0f);
                    break;
                default:
                    sprites = CharList.charBodyParts.body;
                    spritePos = C.BodyPos;
                    newColor = Color.white;
                    break;
            }

            // ***
            // Use reflection to avoid creating 17 different loops to add to allSprites
            // ***

            if (spritePos > -1)
            {
                // Get the Property Info of the current instance of CharParts
                type = sprites[spritePos].GetType();
                info = type.GetProperties();
                
                // Loop through each Property in the CharParts list
                for (k = 0; k < info.Length; ++k)
                {
                    // Get the value of the current List of Sprites in CharParts
                    List<Sprite> value = (List<Sprite>)info[k].GetValue(sprites[spritePos], null);

                    // Loop through the List of Sprites
                    for (int x = 0; x < value.Count; ++x)
                    {
                        // Add the current Sprite to the allSprites list
                        allSprites.Add(value[x]);
                    }
                }
            }

            spriteCount = allSprites.Count;
        } // End setList()
    }
}

