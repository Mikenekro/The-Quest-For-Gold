using schoolRPG.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main Namespace for RPG
/// </summary>
namespace schoolRPG
{
    // TODO: Same as I did with SaveVector3 with SaveColor

    /// <summary>
    /// Holds data about the characters looks and player animations
    /// </summary>
    [System.Serializable]
    public class CharStats : MonoBehaviour
    {
        [SerializeField]
        private CStats storeStats;
        
        private int inner;
        private int outer;
        private bool killing;
        private bool respawning;
        private bool canRespawn;

        /// <summary>
        /// Can we Destroy this GameObject
        /// </summary>
        [Tooltip("Can we Destroy the GameObject once it dies?")]
        public bool canDestroy = true;
        public bool isPlayer = false;

        /// <summary>
        /// The Serializable Stats for this Character
        /// </summary>
        public CStats StoreStats {
            get { return storeStats; }
            set { storeStats = value; SetColors(); }
        }
        /// <summary>
        /// The Spawn Position of this GameObject
        /// </summary>
        public Vector3 spawnPos { get; set; }
        

        // Base Colors selected in the Character Creation Menu
        public Color baseHairColor;
        public Color baseShirtColor;
        public Color basePantsColor;
        public Color baseShoesColor;
        
        // Colors for the Body Parts that will change color
        public Color helmColor;
        public Color hairColor;
        public Color shirtColor;
        public Color pantsColor;
        public Color shoesColor;
        public Color glovesColor;
        public Color weaponsColor;

        [SerializeField, Tooltip("Should we use the NPC List to set the NPC's Sprite? (Note: Only effects the Body Sprite)")]
        private bool UseNpcList;
        [SerializeField]
        private GameObject bodyObj;
        [SerializeField]
        private GameObject helmObj;
        [SerializeField]
        private GameObject hairObj;
        [SerializeField]
        private GameObject shirtObj;
        [SerializeField]
        private GameObject pantsObj;
        [SerializeField]
        private GameObject shoesObj;
        [SerializeField]
        private GameObject glovesObj;
        [SerializeField]
        private GameObject weaponsObj;

        /// <summary>
        /// The current animation that is playing
        /// </summary>
        public Dictionary<string, bool> animPlaying;

        /// <summary>
        /// Names of each Animation
        /// </summary>
        private string[,] anims;
        private Animator[] anim;
        private int lastDir;
        private SpriteSwitcher[] switchers;
        private Vector3 lDir;
        private Vector3 mDir;
        private BoxCollider2D[] cols;
        private SpriteRenderer[] rends;

        public bool IsMale { get { return StoreStats.IsMale; } set { StoreStats.IsMale = value; } }

        /// <summary>
        /// Is this Character dead? (Plays the death animation when set
        /// </summary>
        public bool IsDead { get { return StoreStats.IsDead; } set { StoreStats.IsDead = value; } }
        /// <summary>
        /// The direction the character is looking (does not animate the character)
        /// </summary>
        public Vector3 LookDir
        {
            get { return lDir; }
            set
            {
                if (lDir != value && value != Vector3.zero)
                {
                    storeStats.MoveDir = new SaveVector3(value.x, value.y, value.z);
                    lDir = value;   
                }
            }
        }
        /// <summary>
        /// The direction the character is moving (Will animate the character)
        /// </summary>
        public Vector3 MoveDir
        {
            get { return mDir; }
            set
            {
                mDir = value;
                if (value != Vector3.zero && value != LookDir)
                {
                    LookDir = value;

                    // Set the Last Direction ONLY when we are going to move
                    if (MoveDir.y < 0)
                        lastDir = 0;
                    else if (MoveDir.y > 0)
                        lastDir = 1;
                    else if (MoveDir.x < 0)
                        lastDir = 2;
                    else if (MoveDir.x > 0)
                        lastDir = 3;
                }
            }
        }
        public bool CanMove { get; set; }
        public bool IsAttacking { get; set; }
        public bool IsPowerAttacking { get; set; }
        public bool IsCasting { get; set; }

        public bool UseNPC { get { return UseNpcList; } }
        // Positions in Array where this character gets their Body Parts
        public int BodyPos { get { return storeStats.BodyPos; } set { ChangeBody(value); } }
        public int HelmPos { get { return storeStats.HelmPos; } set { ChangeHelm(value); } }
        public int HairPos { get { return storeStats.HairPos; } set { ChangeHair(value); } }
        public int ShirtPos { get { return storeStats.ShirtPos; } set { ChangeShirt(value); } }
        public int PantsPos { get { return storeStats.PantsPos; } set { ChangePants(value); } }
        public int ShoesPos { get { return storeStats.ShoesPos; } set { ChangeShoes(value); } }
        public int GlovesPos { get { return storeStats.GlovesPos; } set { ChangeGloves(value); } }
        public int WeaponPos { get { return storeStats.WeaponPos; } set { ChangeWeapon(value); } }
        
        void OnTriggerEnter2D(Collider2D other)
        {
            // If we are colliding with the player or another npc
            if (gameObject.layer == 9)
            {
                if (canRespawn && (other.gameObject.layer == 8 || other.gameObject.layer == 9))
                {
                    canRespawn = false;
                }
            }
            
        }
        void OnTriggerExit2D(Collider2D other)
        {
            // If we are colliding with the player or another npc
            if (gameObject.layer == 9)
            {
                 if (!canRespawn && (other.gameObject.layer == 8 || other.gameObject.layer == 9))
                {
                    canRespawn = true;
                }
            }
            
        }

        // Use this for initialization
        void Start()
        {
            bool first = true;
            int remove = 0;
            Animator[] tempAnim;
            string vals = "Body Helmet Hair Shirt Pants Shoes Gloves Weapon";

            if (storeStats == null)
                storeStats = new CStats();
            anim = gameObject.GetComponentsInChildren<Animator>();
            animPlaying = new Dictionary<string, bool>();
            switchers = gameObject.GetComponentsInChildren<SpriteSwitcher>();
            spawnPos = gameObject.transform.position;
            IsMale = true;
            UpdateColors();
            MoveDir = Vector3.zero;
            storeStats.MoveDir = new SaveVector3(MoveDir.x, MoveDir.y, MoveDir.z);
            canRespawn = true;

            tempAnim = new Animator[anim.Length];
            
            for (int i = 0; i < anim.Length; ++i)
            {
                if (first && !vals.Contains(anim[i].gameObject.tag))
                {
                    remove++;
                }
                else if (!first && i < tempAnim.Length)
                {
                    tempAnim[i] = anim[i];
                }

                if (first && i == anim.Length - 1)
                {
                    if (remove > 0)
                    {
                        tempAnim = new Animator[anim.Length - remove];
                        first = false;
                        i = -1;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (!first)
            {
                anim = tempAnim;
            }
            
            // Create each Animation Name
            anims = new string[5, 4];

            anims[0, 0] = "Idle";
            anims[0, 1] = "IdleUp";
            anims[0, 2] = "IdleLeft";
            anims[0, 3] = "IdleRight";

            anims[1, 0] = "WalkDown";
            anims[1, 1] = "WalkUp";
            anims[1, 2] = "WalkLeft";
            anims[1, 3] = "WalkRight";

            anims[2, 0] = "AttackDown";
            anims[2, 1] = "AttackUp";
            anims[2, 2] = "AttackLeft";
            anims[2, 3] = "AttackRight";

            anims[3, 0] = "MagicDown";
            anims[3, 1] = "MagicUp";
            anims[3, 2] = "MagicLeft";
            anims[3, 3] = "MagicRight";

            anims[4, 0] = "Dead";
            anims[4, 1] = "Dead1";
            anims[4, 2] = "Dead2";
            anims[4, 3] = "Dead3";

            inner = anims.GetUpperBound(0);
            outer = anims.Length / inner;

            // Create a state for each Animation Name
            for (int i = 0; i < outer; ++i)
            {
                for (int k = 0; k < inner; ++k)
                {
                    animPlaying.Add(anims[i,k], false);
                }
            }

            // Get any colliders an sprite renderers in this Character
            cols = gameObject.GetComponentsInChildren<BoxCollider2D>();
            rends = gameObject.GetComponentsInChildren<SpriteRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!WorldController.Paused)
            {
                AnimateCharacter();
            }

        }

        /// <summary>
        /// Updates each Color in the cStats script
        /// </summary>
        public void UpdateColors()
        {
            if (hairObj != null)
                storeStats.BaseHairColor = new SaveColor(baseHairColor.r, baseHairColor.g, baseHairColor.b, baseHairColor.a);
            if (pantsObj != null)
                storeStats.BasePantsColor = new SaveColor(basePantsColor.r, basePantsColor.g, basePantsColor.b, basePantsColor.a);
            if (shirtObj != null)
                storeStats.BaseShirtColor = new SaveColor(baseShirtColor.r, baseShirtColor.g, baseShirtColor.b, baseShirtColor.a);
            if (shoesObj != null)
                storeStats.BaseShoesColor = new SaveColor(baseShoesColor.r, baseShoesColor.g, baseShoesColor.b, baseShoesColor.a);

            if (glovesObj != null)
                storeStats.GlovesColor = new SaveColor(glovesColor.r, glovesColor.g, glovesColor.b, glovesColor.a);
            if (hairObj != null)
                storeStats.HairColor = new SaveColor(hairColor.r, hairColor.g, hairColor.b, hairColor.a);
            if (helmObj != null)
                storeStats.HelmColor = new SaveColor(helmColor.r, helmColor.g, helmColor.b, helmColor.a);
            if (pantsObj != null)
                storeStats.PantsColor = new SaveColor(pantsColor.r, pantsColor.g, pantsColor.b, pantsColor.a);
            if (shirtObj != null)
                storeStats.ShirtColor = new SaveColor(shirtColor.r, shirtColor.g, shirtColor.b, shirtColor.a);
            if (shoesObj != null)
                storeStats.ShoesColor = new SaveColor(shoesColor.r, shoesColor.g, shoesColor.b, shoesColor.a);
            if (weaponsObj != null)
                storeStats.WeaponsColor = new SaveColor(weaponsColor.r, weaponsColor.g, weaponsColor.b, weaponsColor.a);
        }

        /// <summary>
        /// Sets each color to the values in the cStats object
        /// </summary>
        public void SetColors()
        {
            if (hairObj != null)
                baseHairColor = new Color(storeStats.BaseHairColor.R, storeStats.BaseHairColor.G, storeStats.BaseHairColor.B, storeStats.BaseHairColor.A);
            if (pantsObj != null)
                basePantsColor = new Color(storeStats.BasePantsColor.R, storeStats.BasePantsColor.G, storeStats.BasePantsColor.B, storeStats.BasePantsColor.A);
            if (shirtObj != null)
                baseShirtColor = new Color(storeStats.BaseShirtColor.R, storeStats.BaseShirtColor.G, storeStats.BaseShirtColor.B, storeStats.BaseShirtColor.A);
            if (shoesObj != null)
                baseShoesColor = new Color(storeStats.BaseShoesColor.R, storeStats.BaseShoesColor.G, storeStats.BaseShoesColor.B, storeStats.BaseShoesColor.A);

            if (glovesObj != null)
                glovesColor = new Color(storeStats.GlovesColor.R, storeStats.GlovesColor.G, storeStats.GlovesColor.B, storeStats.GlovesColor.A);
            if (hairObj != null)
                hairColor = new Color(storeStats.HairColor.R, storeStats.HairColor.G, storeStats.HairColor.B, storeStats.HairColor.A);
            if (helmObj != null)
                helmColor = new Color(storeStats.HelmColor.R, storeStats.HelmColor.G, storeStats.HelmColor.B, storeStats.HelmColor.A);
            if (pantsObj != null)
                pantsColor = new Color(storeStats.PantsColor.R, storeStats.PantsColor.G, storeStats.PantsColor.B, storeStats.PantsColor.A);
            if (shirtObj != null)
                shirtColor = new Color(storeStats.ShirtColor.R, storeStats.ShirtColor.G, storeStats.ShirtColor.B, storeStats.ShirtColor.A);
            if (shoesObj != null)
                shoesColor = new Color(storeStats.ShoesColor.R, storeStats.ShoesColor.G, storeStats.ShoesColor.B, storeStats.ShoesColor.A);
            if (weaponsObj != null)
                weaponsColor = new Color(storeStats.WeaponsColor.R, storeStats.WeaponsColor.G, storeStats.WeaponsColor.B, storeStats.WeaponsColor.A);
        }

        /// <summary>
        /// Changes the characters Body to match the selection
        /// </summary>
        /// <param name="pos"></param>
        public void ChangeBody(int pos)
        {
            if (bodyObj != null)
            {
                if (!UseNpcList)
                {
                    if (pos >= CharList.charBodyParts.body.Count)
                        pos = 0;
                    else if (pos == -1)
                        pos = CharList.charBodyParts.body.Count - 1;
                    else if (pos < -1)
                    {
                        pos = -2;
                        bodyObj.SetActive(false);
                    }
                }
                else
                {
                    if (pos >= CharList.charBodyParts.npc.Count)
                        pos = 0;
                    else if (pos == -1)
                        pos = CharList.charBodyParts.npc.Count - 1;
                    else if (pos < -1)
                    {
                        pos = -2;
                        bodyObj.SetActive(false);
                    }
                }
                

                storeStats.BodyPos = pos;

                if (pos > -1)
                {
                    bodyObj.SetActive(true);
                    // Make sure we update the Sprite Switcher
                    switchers[1].SwitchList();
                }
            }
            
        }

        /// <summary>
        /// Changes the characters Helmet to match the selection
        /// </summary>
        /// <param name="pos"></param>
        public void ChangeHelm(int pos)
        {
            if (helmObj != null)
            {
                if (pos >= CharList.charBodyParts.helmet.Count)
                    pos = 0;
                else if (pos == -1)
                    pos = CharList.charBodyParts.helmet.Count - 1;
                else if (pos < -1)
                {
                    pos = -2;
                    helmObj.SetActive(false);
                }

                storeStats.HelmPos = pos;

                if (pos > -1)
                {
                    helmObj.SetActive(true);
                    // Make sure we update the Sprite Switcher
                    switchers[0].SwitchList();
                }
            }
        }

        /// <summary>
        /// Changes the characters Hair to match the selection
        /// </summary>
        /// <param name="pos"></param>
        public void ChangeHair(int pos)
        {
            if (hairObj != null)
            {
                if (pos >= CharList.charBodyParts.hair.Count)
                    pos = 0;
                else if (pos == -1)
                    pos = CharList.charBodyParts.hair.Count - 1;
                else if (pos < -1)
                {
                    pos = -2;
                    hairObj.SetActive(false);
                }

                storeStats.HairPos = pos;

                if (pos > -1)
                {
                    hairObj.SetActive(true);
                    // Make sure we update the Sprite Switcher
                    switchers[2].SwitchList();
                }
            }
        }

        /// <summary>
        /// Changes the characters Shirt to match the selection
        /// </summary>
        /// <param name="pos"></param>
        public void ChangeShirt(int pos)
        {
            if (shirtObj != null)
            {
                if (pos >= CharList.charBodyParts.shirt.Count)
                    pos = 0;
                else if (pos == -1)
                    pos = CharList.charBodyParts.shirt.Count - 1;
                else if (pos < -1)
                {
                    pos = -2;
                    shirtObj.SetActive(false);
                }

                storeStats.ShirtPos = pos;

                if (pos > -1)
                {
                    shirtObj.SetActive(true);
                    // Make sure we update the Sprite Switcher
                    switchers[3].SwitchList();
                }
            }
        }

        /// <summary>
        /// Changes the characters Pants to match the selection
        /// </summary>
        /// <param name="pos"></param>
        public void ChangePants(int pos)
        {
            if (pantsObj != null)
            {
                if (pos >= CharList.charBodyParts.pants.Count)
                    pos = 0;
                else if (pos == -1)
                    pos = CharList.charBodyParts.pants.Count - 1;
                else if (pos < -1)
                {
                    pos = -2;
                    pantsObj.SetActive(false);
                }

                storeStats.PantsPos = pos;

                if (pos > -1)
                {
                    pantsObj.SetActive(true);
                    // Make sure we update the Sprite Switcher
                    switchers[4].SwitchList();
                }
            }
        }

        /// <summary>
        /// Changes the characters Shoes to match the selection
        /// </summary>
        /// <param name="pos"></param>
        public void ChangeShoes(int pos)
        {
            if (shoesObj != null)
            {
                if (pos >= CharList.charBodyParts.shoes.Count)
                    pos = 0;
                else if (pos == -1)
                    pos = CharList.charBodyParts.shoes.Count - 1;
                else if (pos < -1)
                {
                    pos = -2;
                    shoesObj.SetActive(false);
                }

                storeStats.ShoesPos = pos;

                if (pos > -1)
                {
                    shoesObj.SetActive(true);
                    // Make sure we update the Sprite Switcher
                    switchers[5].SwitchList();
                }
            }
        }

        /// <summary>
        /// Changes the characters Gloves to match the selection
        /// </summary>
        /// <param name="pos"></param>
        public void ChangeGloves(int pos)
        {
            if (glovesObj != null)
            {
                if (pos >= CharList.charBodyParts.gloves.Count)
                    pos = 0;
                else if (pos == -1)
                    pos = CharList.charBodyParts.gloves.Count - 1;
                else if (pos < -1)
                {
                    pos = -2;
                    glovesObj.SetActive(false);
                }

                storeStats.GlovesPos = pos;

                if (pos > -1)
                {
                    glovesObj.SetActive(true);
                    // Make sure we update the Sprite Switcher
                    switchers[6].SwitchList();
                }
            }
        }

        /// <summary>
        /// Changes the characters Weapon to match the selection
        /// </summary>
        /// <param name="pos"></param>
        public void ChangeWeapon(int pos)
        {
            if (weaponsObj != null)
            {
                if (pos >= CharList.charBodyParts.weapons.Count)
                    pos = 0;
                else if (pos == -1)
                    pos = CharList.charBodyParts.weapons.Count - 1;
                else if (pos < -1)
                {
                    pos = -2;
                    weaponsObj.SetActive(false);
                }

                storeStats.WeaponPos = pos;

                if (pos > -1)
                {
                    weaponsObj.SetActive(true);
                    // Make sure we update the Sprite Switcher
                    switchers[7].SwitchList();
                }
            }
        }

        /// <summary>
        /// Animates the Player Character based on the last direction, if they are moving, 
        /// if they are using magic, if they are attacking, or if they are dying
        /// </summary>
        private void AnimateCharacter()
        {
            // Make sure each animation state is false
            ResetDictionary();

            for (int i = 0; i < anim.Length; ++i)
            {
                if (!IsDead && anim[i].gameObject.activeInHierarchy)
                {
                    if (!CanMove)
                    {
                        // TODO: Uncomment this once you get LookDir to not reset attacking to LookDir = Down
                        //if (LookDir.y < 0)
                        //    lastDir = 0;
                        //else if (LookDir.y > 0)
                        //    lastDir = 1;
                        //else if (LookDir.x < 0)
                        //    lastDir = 2;
                        //else if (LookDir.x > 0)
                        //    lastDir = 3;

                        if (IsAttacking || IsPowerAttacking)
                        {
                            // Play Attacking Animation
                            anim[i].Play(anims[2, lastDir]);
                            animPlaying[anims[2, lastDir]] = true;
                        }
                        else if (IsCasting)
                        {
                            // Play Casting Animation
                            anim[i].Play(anims[3, lastDir]);
                            animPlaying[anims[3, lastDir]] = true;
                        }
                        else 
                        {
                            // Play Idle Animation
                            anim[i].Play(anims[0, lastDir]);
                            animPlaying[anims[0, lastDir]] = true;
                        }
                    }
                    else
                    {
                        
                        // Play Walking Animation
                        anim[i].Play(anims[1, lastDir]);
                        animPlaying[anims[1, lastDir]] = true;
                    }
                }
                else if (IsDead) // The character is dead
                {
                    // Play Dead and kill the character if possible
                    anim[i].Play(anims[4, 0]);
                    animPlaying[anims[4, 0]] = true;
                    StartCoroutine(KillCharacter());
                }

            }
        }

        /// <summary>
        /// Kills the character and cleans up the GameObject Mess
        /// </summary>
        /// <returns></returns>
        private IEnumerator KillCharacter()
        {
            bool wait = true;
            WaitForSeconds wfs = new WaitForSeconds(4);
            
            if (!killing)
            {
                killing = true;
                // Wait a few seconds until we clean up the GameObject
                while (wait)
                {
                    yield return wfs;

                    wait = false;
                }

                // Clean up GameObject
                if (!isPlayer)
                {
                    cols[0].isTrigger = true;
                    cols[1].enabled = false;
                    for (int i = 0; i < rends.Length; ++i)
                        rends[i].enabled = false;

                    // Destroy the gameObject if we can
                    if (canDestroy)
                    {
                        // Destroy the object based on play mode
                        if (Application.isEditor)
                            DestroyImmediate(gameObject);
                        else
                            Destroy(gameObject);
                    }
                    else
                    {
                        if (!respawning)
                        {
                            // Respawn the character after X Seconds
                            StartCoroutine(Respawn());
                        }
                    }
                } // End if !IsPlayer
                
            }
            
            yield return null;
        }

        /// <summary>
        /// Respawns this Character after X Seconds. 
        /// <para></para>
        /// NOTE: Only NPC's should respawn. Player must load a Saved game
        /// </summary>
        /// <param name="xSeconds"></param>
        /// <returns></returns>
        public IEnumerator Respawn()
        {
            bool wait = true;
            WaitForSeconds wfs = new WaitForSeconds(WorldController.RespawnRate);

            killing = true;
            respawning = true;

            Debug.Log("Starting Respawn...");
            while (wait)
            {
                yield return wfs;

                // Don't respawn until we are not colliding with another NPC or the Player
                if (canRespawn)
                    wait = false;
            }
            
            Spawn();
            
            yield return null;
        }

        /// <summary>
        /// Respawns the npc
        /// </summary>
        private void Spawn()
        {
            NpcController npc;
            
            IsDead = false;
            IsCasting = false;
            IsAttacking = false;
            IsPowerAttacking = false;
            gameObject.transform.position = spawnPos;
            gameObject.SetActive(true);
            // Re-Enable this NPC's colliders and renderers
            cols[0].isTrigger = false;
            cols[1].enabled = true;
            for (int i = 0; i < rends.Length; ++i)
                rends[i].enabled = true;

            if (gameObject.GetComponent<NpcController>() != null)
            {
                npc = gameObject.GetComponent<NpcController>();
                npc.NPC.AddHealth(npc.NPC.MaxHealth, true, false);
                npc.NPC.ResetHitByPlayer();
                npc.NPC.WorldPos = new SaveVector3(spawnPos.x, spawnPos.y, spawnPos.z);
                npc.transform.position = spawnPos;

                // Checks if we should disable this NPCs controller on respawn (In case we were chasing the player when the rest were disabled)
                if (GetComponentInParent<BattleZone>() != null)
                    GetComponentInParent<BattleZone>().CheckDisable(npc.NPC.UniqueID);
                Debug.Log("Respawned: " + npc.NPC.Name);
            }

            respawning = false;
            killing = false;
        }

        /// <summary>
        /// Reset each Animation State to False
        /// </summary>
        private void ResetDictionary()
        {
            // Create a state for each Animation Name
            for (int i = 0; i < outer; ++i)
            {
                for (int k = 0; k < inner; ++k)
                {
                    animPlaying[anims[i, k]] = false;
                }
            }
        }

    }
}

