using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// RPG 
using schoolRPG.Stats;
using schoolRPG.Items;

// Input manager
using TeamUtility.IO;
// TextMesh Pro
using TMPro;
using System;
using Pathfinding;
using schoolRPG.Quests;
using schoolRPG.Sound;

/// <summary>
/// Main Namespace for RPG
/// </summary>
namespace schoolRPG
{
    
    [System.Serializable]
    public class PlayerController : MonoBehaviour
    {
        private static PlayerController _pc;
        private static List<string> notificationQueue;
        private static List<bool> notificationSound;
        public static PlayerController Pc { get { return _pc; } }
        /// <summary>
        /// Which of the players nodes are in use
        /// </summary>
        public static bool[] NodeUse { get; set; }
        public static bool jumpPoint { get; set; }

        [SerializeField]
        private Camera mainCam;
        [SerializeField, Tooltip("Set the Quest Track Gameobject to point the player in the correct direction")]
        private GameObject questTracker;
        [SerializeField]
        private Character _player;
        private Rigidbody2D rb;
        private Vector3 move;
        private Vector3 faceDir;
        private CharStats cs;
        private bool inQueue;
        private float healthMod;
        private float staminaMod;
        private float magickaMod;

        private WaitForSeconds wfs1;
        private WaitForSeconds wfs;

        private Bounds bounds;
        private GraphUpdateObject guo;
        private GraphUpdateScene gus;

        private ControllerBox cb;

        public GameObject hoverT;
        public TextMeshProUGUI notif;
        public Slider healthSlider;
        public Slider staminaSlider;
        public Slider magickaSlider;
        public TextMeshProUGUI health;
        public TextMeshProUGUI stamina;
        public TextMeshProUGUI magicka;
        /// <summary>
        /// The total time a Notification will appear on the screen
        /// </summary>
        [Tooltip("The total time a Notification will appear on the screen"), Range(0, 100)]
        public float displayTime;

        /// <summary>
        /// Speed the player is moving
        /// </summary>
        public float Speed { get; set; }
        /// <summary>
        /// Speed the player walks through the world
        /// </summary>
        public float WalkSpeed { get; set; }
        /// <summary>
        /// Speed the player runs through the world
        /// </summary>
        public float RunSpeed { get; set; }
        /// <summary>
        /// Is the player moving
        /// </summary>
        public bool Moving { get; set; }
        public bool Running { get; set; }
        public bool InStaminaUpdate { get; set; }


        public Character Player { get { return _player; } }
        public CharStats PlayerStats { get { return cs; } }

        [SerializeField, Tooltip("List of items that player will start with in their inventory")]
        private List<Item> _items;
        /// <summary>
        /// The Base Items that the player will start out with
        /// </summary>
        public List<Item> Items { get { return _items; } }

        /// <summary>
        /// A Gameobject that will point towards a Quest Marker
        /// </summary>
        public GameObject QuestTracker { get { return questTracker; } }
        
        // Use this for initialization
        public void Awake()
        {
            // Subscribe to Delegates
            Skill.SkillLevelupDelegate += DisplayText;
            CharDelegates.LevelupDelegate += DisplayText;
            CharDelegates.DeathDelegate += PlayerDeath;
            WorldController.PlayerGetData += StoreData;

            NodeUse = new bool[4];
            _pc = this;
            hoverT.SetActive(false);
            OnEnable();
            
            rb = gameObject.GetComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            notif.text = "";
            if (cs == null)
                cs = gameObject.GetComponent<CharStats>();

            questTracker.SetActive(false);
            
        }

        public void Start()
        {
            // Load the Players saved Data
            if (WorldController.IsLoaded)
            {
                // NOTE: This is now done through delegates
                //_player = WorldController.Data.Player;
                //cs.StoreStats = WorldController.Data.PlayerStats;
                //cs.setColors();

                //// Make sure the player loads into their last saved position (This means using the private _player variable instead of player)
                //if (_player.WorldPos.x != 0 || _player.WorldPos.y != 0)
                //{
                //    transform.position = new Vector3(_player.WorldPos.x, _player.WorldPos.y, _player.WorldPos.z);
                //    mainCam.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
                //}
                //if (cs.lookDir.x != 0 || cs.lookDir.y != 0)
                //{
                //    // Set the move direction to the look direction so it will spawn the player facing the same way as they saved the game
                //    cs.moveDir = cs.lookDir;
                //}
                //else
                //{
                //    cs.lookDir = new Vector3(0, -1, 0);
                //}
            }
            else // This is a new game
            {
                BaseItem main;
                BaseItem item;
                _player.Init(true, false);
                _player.WorldPos = new SaveVector3(0, 0, 0);
                _player.AddAttribute(AttributeEnum.ENDURANCE, 9);
                _player.AddAttribute(AttributeEnum.INTELLIGENCE, 9);
                _player.AddAttribute(AttributeEnum.SPEED, 9);
                _player.AddAttribute(AttributeEnum.STRENGTH, 9);
                _player.SetBaseStats(true);

                cs.MoveDir = new Vector3(0, -1, 0);

                WorldController.Data.Player = _player;
                WorldController.Data.PlayerStats = cs.StoreStats;

                healthSlider.maxValue = _player.MaxHealth;
                staminaSlider.maxValue = _player.MaxStamina;
                magickaSlider.maxValue = _player.MaxMagicka;

                healthSlider.value = healthSlider.maxValue;
                staminaSlider.value = staminaSlider.maxValue;
                magickaSlider.value = magickaSlider.maxValue;

                // Add each referenced item to the players inventory
                for (int i = 0; i < Items.Count; ++i)
                {
                    Items[i].Start();
                    main = Items[i].item;
                    item = new BaseItem();
                    // Create a new item for the Inventory with the selected quantity
                    item.CreateItem(main.IsStackable, main.ArrayPos, main.ID, main.Name, main.Description,
                        main.Value, main.Weight, main.Quantity, main.ItemColor, main.UniqueID, main.Condition,
                        main.ArmorType, main.Armor, main.WeaponType, main.Damage, main.Effect, main.EffectValue,
                        main.Duration, main.Type);
                    Player.Inventory.AddItem(item);
                }
            }
            // Clear the items even if they did not need to be used
            _items.Clear();

            wfs = new WaitForSeconds(displayTime);
            wfs1 = new WaitForSeconds(0.1f);
            notificationQueue = new List<string>();
            notificationSound = new List<bool>();

            WalkSpeed = Player.WalkSpeed;
            RunSpeed = Player.RunSpeed;
            move = new Vector3(0, 0, 0);
            Speed = WalkSpeed;

            inQueue = false;
            Moving = true;
            Running = false;

            if (GetComponentInChildren<ControllerBox>() != null)
                cb = GetComponentInChildren<ControllerBox>();

            Player.Inventory.PlayerInv = true;

            StatsUpdate();
        }

        void OnEnable()
        {
            
        }
        
        void OnDestroy()
        {
            // Un-Subscribe from the Delegate
            Skill.SkillLevelupDelegate -= DisplayText;
            CharDelegates.LevelupDelegate -= DisplayText;
            CharDelegates.DeathDelegate -= PlayerDeath;
            WorldController.PlayerGetData -= StoreData;
        }

        /// <summary>
        /// Register the item delegate for displaying text (Used by an Unknown/Dynamic number of items spawned in the Game-World)
        /// </summary>
        /// <param name="item"></param>
        public void RegisterDelegate(Item item)
        {
            item.HoverText += HoverText;
        }
        /// <summary>
        /// Unregister the item delegate for displaying text (Used by an Unknown/Dynamic number of items spawned in the Game-World)
        /// </summary>
        /// <param name="item"></param>
        public void UnregisterDelegate(Item item)
        {
            item.HoverText -= HoverText;
        }

        /// <summary>
        /// Called automatically
        /// </summary>
        /// <param name="setData"></param>
        public void StoreData(bool setData)
        {
            // If we are Saving a Game
            if (setData)
            {
                // Get World Position before storing Player Character
                Player.WorldPos = new SaveVector3(transform.position.x, transform.position.y, transform.position.z);
                WorldController.Data.Player = Player;
                // Update colors before storing Player Stats
                cs.UpdateColors();
                WorldController.Data.PlayerStats = cs.StoreStats;
            }
            else // If we are Loading a Game
            {
                SetPCData(WorldController.Data.Player, WorldController.Data.PlayerStats);

                // Make sure the player loads into their last saved position (This means using the private _player variable instead of player)
                if (_player.WorldPos.X != 0 || _player.WorldPos.Y != 0)
                {
                    transform.position = new Vector3(_player.WorldPos.X, _player.WorldPos.Y, _player.WorldPos.Z);
                    mainCam.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
                }
                if (cs.LookDir.x != 0 || cs.LookDir.y != 0)
                {
                    // Set the move direction to the look direction so it will spawn the player facing the same way as they saved the game
                    cs.MoveDir = cs.LookDir;
                }
                else
                {
                    cs.LookDir = new Vector3(0, -1, 0);
                }

                // Make sure we are not using the last Attacking/Casting State
                Player.IsPowerAttacking = false;
                Player.IsAttacking = false;
                Player.IsCasting = false;
            }
            
        }

        /// <summary>
        /// Sets the Players Data when it needs to be changed
        /// </summary>
        /// <param name="p"></param>
        /// <param name="c"></param>
        public void SetPCData(Character p, CStats c)
        {
            if (cs == null)
                cs = new CharStats();
            _player = p;
            cs.StoreStats = c;
            cs.SetColors();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // Run player actions when the world is not paused
            if (!WorldController.Paused && !WorldController.InMenu && !CreationMenu.Creating)
            {
                MovePlayer();

                if (healthSlider.maxValue != Player.MaxHealth)
                    healthSlider.maxValue = Player.MaxHealth;
                if (staminaSlider.maxValue != Player.MaxStamina)
                    staminaSlider.maxValue = Player.MaxStamina;
                if (magickaSlider.maxValue != Player.MaxMagicka)
                    magickaSlider.maxValue = Player.MaxMagicka;

                if (!InStaminaUpdate)
                {
                    InStaminaUpdate = true;
                    StartCoroutine(StatsTimeUpdate());
                }
            }
            else if (cs.CanMove && Dialogue.DialogueMenuController.IsOpen)
            {
                cs.CanMove = false;
            }

        }

        // Call this to Drink a Potion Item
        public void DrinkPotion(BaseItem item)
        {
            StartCoroutine(drinkPotion(item));
        }
        private IEnumerator drinkPotion(BaseItem item)
        {
            float duration = 0.0f;
            WaitForSecondsRealtime wfsr = new WaitForSecondsRealtime(0.01f);
            
            if (item.Effect == PotionEffect.INC_CARRYWEIGHT)
                Player.ModCarryWeight += item.EffectValue;
            else if (item.Effect == PotionEffect.INC_MAXHEALTH)
                Player.ModHealth += item.EffectValue;
            else if (item.Effect == PotionEffect.INC_MAXMAGICKA)
                Player.ModMagicka += item.EffectValue;
            else if (item.Effect == PotionEffect.INC_MAXSTAMINA)
                Player.ModStamina += item.EffectValue;

            while (duration < item.Duration)
            {
                yield return wfsr;

                if (!WorldController.Paused && !WorldController.InMenu)
                {
                    if (item.Effect == PotionEffect.HEAL)
                    {
                        // Add 1 tenth of the effect value to the players health for the total duration
                        Player.AddHealth((item.EffectValue / 100.0f), true, false);
                    }
                    else if (item.Effect == PotionEffect.MAGICKA)
                    {
                        Player.AddMagicka((item.EffectValue / 100.0f), false);
                    }
                    else if (item.Effect == PotionEffect.STAMINA)
                    {
                        Player.AddStamina((item.EffectValue / 100.0f));
                    }

                    duration += 0.01f;
                }
            }

            if (item.Effect == PotionEffect.INC_CARRYWEIGHT)
                Player.ModCarryWeight -= item.EffectValue;
            else if (item.Effect == PotionEffect.INC_MAXHEALTH)
                Player.ModHealth -= item.EffectValue;
            else if (item.Effect == PotionEffect.INC_MAXMAGICKA)
                Player.ModMagicka -= item.EffectValue;
            else if (item.Effect == PotionEffect.INC_MAXSTAMINA)
                Player.ModStamina -= item.EffectValue;
        }

        /// <summary>
        /// Updates Health, Stamina, and Magicka
        /// </summary>
        /// <returns></returns>
        private void StatsUpdate()
        {
            if (healthMod != 0)
            {
                Player.AddHealth(healthMod, true, false); // Health takes a fixed loss when healthMod is set
                healthMod = 0;
                health.text = "Health: " + Mathf.Floor(Player.Health).ToString("0") + "/" + Mathf.Floor(Player.MaxHealth).ToString("0");
                if (healthSlider.maxValue != Player.MaxHealth)
                    healthSlider.maxValue = Player.MaxHealth;
                healthSlider.value = Player.Health;
            }
            
            if (staminaMod != 0)
            {
                Player.AddStamina(staminaMod);
                staminaMod = 0;
            }
            if (magickaMod != 0 && (Player.ActiveSpell != null || jumpPoint))
            {
                Player.AddMagicka(magickaMod, Player.IsCasting);
                magickaMod = 0;
            }
            
        }

        /// <summary>
        /// Stats that can be drained over time will be drained here.
        /// Coroutine can be used to drain it by x every y seconds
        /// </summary>
        /// <returns></returns>
        private IEnumerator StatsTimeUpdate()
        {
            float mod = 0.0f;
            float mmod = 0.25f;
            byte speedb4 = Player.Speed;

            while (!WorldController.Paused && !WorldController.InMenu)
            {
                if (Moving && Running)
                    mod = -0.40f + (Player.Athletics.Level * 0.002f); // Athletics helps with stamina loss 
                else if (Moving && !Running)
                    mod = 0.25f + (Player.Athletics.Level * 0.001f); // Athletics helps with stamina gain
                else
                    mod = 0.50f + (Player.Athletics.Level * 0.01f); // Stamina regenerates the fastest while not moving

                if (mod != 0.0f)
                    Player.AddStamina(mod);

                // Give XP to Stamina
                if (Moving || Running && mod != 0)
                {
                    Player.StaminaSkills(((mod < 0) ? (mod) : (-mod)), 0);

                    // Reset speed when the player levels up the Speed Attribute
                    if (Player.Speed != speedb4)
                    {
                        speedb4 = Player.Speed;
                        WalkSpeed = Player.WalkSpeed;
                        RunSpeed = Player.RunSpeed;
                        if (!Running)
                            Speed = WalkSpeed;
                        else
                            Speed = RunSpeed;
                    }
                }

                // Add Magicka to the Magicka pool
                if (mmod != 0.0f)
                    Player.AddMagicka(mmod, false);

                if (health.text.Substring(8, Player.Health.ToString().Split('.')[0].Length) != Player.Health.ToString().Split('.')[0])
                    health.text = "Health: "   + Mathf.Floor(Player.Health).ToString("0") +
                                           "/" + Mathf.Floor(Player.MaxHealth).ToString("0");
                if (healthSlider.maxValue != Player.MaxHealth)
                    healthSlider.maxValue = Player.MaxHealth;
                if (healthSlider.value != Player.Health)
                    healthSlider.value = Player.Health;

                if (stamina.text.Substring(9, Player.Stamina.ToString().Split('.')[0].Length) != Player.Stamina.ToString().Split('.')[0])
                    stamina.text = "Stamina: " + Mathf.Floor(Player.Stamina).ToString("0") + 
                                           "/" + Mathf.Floor(Player.MaxStamina).ToString("0");
                if (staminaSlider.maxValue != Player.MaxStamina)
                    staminaSlider.maxValue = Player.MaxStamina;
                if (staminaSlider.value != Player.Stamina)
                    staminaSlider.value = Player.Stamina;

                if (magicka.text.Substring(9, Player.Magicka.ToString().Split('.')[0].Length) != Player.Magicka.ToString().Split('.')[0])
                    magicka.text = "Magicka: " + Mathf.Floor(Player.Magicka).ToString("0") + 
                                           "/" + Mathf.Floor(Player.MaxMagicka).ToString("0");
                if (magickaSlider.maxValue != Player.MaxMagicka)
                    magickaSlider.maxValue = Player.MaxMagicka;
                if (magickaSlider.value != Player.Magicka)
                    magickaSlider.value = Player.Magicka;

                // Wait for wfs1 seconds before we continue
                yield return wfs1;
            }

            InStaminaUpdate = false;
            yield return null;
        }

        /// <summary>
        /// Runs when the player dies
        /// </summary>
        private void PlayerDeath(Guid uniqueID)
        {
            if (Player.UniqueID == uniqueID)
            {
                Debug.Log("Player is Dead!!!");
                // Set the Player Death animation to run
                cs.IsDead = true;
                // Don't allow the game to autosave
                WorldController.playerAlive = false;
                // Bring up the load game menu after a certain amount of time
                StartCoroutine(KillPlayer());
            }
        }

        private IEnumerator KillPlayer()
        {
            yield return new WaitForSeconds(3.5f);

            WorldController.DeathLoad();
            WorldController.TimeScale(false);
        }

        /// <summary>
        /// Function that will be called to display text when we hover over an object
        /// </summary>
        private void HoverText(bool display, string text1, string text2, string text3, string text4, string text5)
        {
            int i;
            TextMeshProUGUI[] txt;
            txt = hoverT.GetComponentsInChildren<TextMeshProUGUI>();
            
            // Display the text area
            if (display)
            {
                hoverT.SetActive(true);
                if (txt.Length == 5)
                {
                    txt[0].text = text1;
                    txt[1].text = text2;
                    txt[2].text = text3;
                    txt[3].text = text4;
                    txt[4].text = text5;
                }
            }
            else // Hide the text area
            {
                for (i = 0; i < txt.Length; ++i)
                    txt[i].text = "";

                hoverT.SetActive(false);
            }
            
        }

        /// <summary>
        /// Displays the input text to the screen
        /// </summary>
        /// <param name="text"></param>
        public static void DisplayTextToScreen(string text, bool playSound)
        {
            try
            {
                Pc.DisplayText(text, playSound);
            }
            catch (Exception ex)
            {
                Debug.LogError("PlayerController.DisplayTextToscreen(" + text + ", " + playSound + "): " + ex.Message);
            }
            
        }

        /// <summary>
        /// Adds the Notification Text to the Queue
        /// </summary>
        /// <param name="text"></param>
        private void DisplayText(string text, bool playSound)
        {
            notificationQueue.Add(text);
            notificationSound.Add(playSound);

            Debug.Log("Added To Queue: " + text);

            if (!inQueue)
                StartCoroutine(NotificationQueue());
        }

        /// <summary>
        /// Coroutine that displays any items in the Notification Queue in FIFO order
        /// </summary>
        /// <returns></returns>
        private IEnumerator NotificationQueue()
        {
            inQueue = true;

            // Loop while there are Notifications in the Queue
            while(notificationQueue.Count > 0)
            {
                // Display the text
                notif.text = notificationQueue[0];
                if (notificationSound[0])
                    SoundController.Audio.Levelup.Play();
                // Wait for specified time
                yield return wfs;
                // Remove the Text from the Queue and display no text
                notif.text = "";
                notificationQueue.RemoveAt(0);
                notificationSound.RemoveAt(0);
            }

            inQueue = false;
            yield return null;
        }

        /// <summary>
        /// Checks if the Character hit an enemy
        /// </summary>
        /// <returns></returns>
        public bool CheckHit()
        {
            float damAfter = 0;
            float damage = Player.CDamage;
            float distance = 1.5f;
            bool cHit = false;
            NpcController npc;
            RaycastHit2D hit;

            if (Player.EquipWeapon != null)
                distance = 2.5f;

            Physics2D.queriesHitTriggers = true;
            Physics2D.queriesStartInColliders = false;

            hit = Physics2D.Raycast(transform.position, faceDir, distance, 1 << 9);
            if (hit)
            {
                npc = hit.collider.attachedRigidbody.gameObject.GetComponent<NpcController>();
                if (npc != null)
                {
                    // We hit an enemy
                    cHit = true;

                    // Double the damage if we are power attacking
                    if (Player.IsPowerAttacking)
                        damage *= 2.0f;

                    // Remove health from NPC (Will also update the NPC's Health Bar)
                    damAfter = npc.NPC.AddHealth(-damage, false, true);

                    // Add to total damage
                    WorldController.Data.StatValue.DamageDealt += -damAfter;
                    // Add to Amount of Damage the enemy resisted
                    WorldController.Data.StatValue.DamageMissed += (damage + damAfter);

                    // Check if this is the Highest Damage the player has done to an NPC
                    if (-damAfter > WorldController.Data.StatValue.HighestDamageDealt)
                        WorldController.Data.StatValue.HighestDamageDealt = -damAfter;

                    // Check if we hit an enemy or friendly NPC
                    if (npc.Type == NPCType.ENEMY)
                        WorldController.Data.StatValue.HitEnemy += 1;
                    else
                        WorldController.Data.StatValue.HitFriendly += 1;

                    // Give the player some skillz
                    if (Player.IsAttacking || Player.IsPowerAttacking)
                    {
                        Player.StaminaSkills(0, damAfter);
                        // Decrease the Condition of the Players Weapon
                        if (Player.EquipWeapon != null)
                            Player.EquipWeapon.Condition[0] -= 0.1f;
                    }
                }
            }

            return cHit;
        }

        /// <summary>
        /// Move the player when the world is not paused
        /// </summary>
        /// <returns></returns>
        private void MovePlayer()
        {
            float val = 0.0f;

            if (!Player.IsPowerAttacking && !Player.IsAttacking && !Player.IsCasting)
            {
                bool run = false;

                if (InputManager.PlayerOneConfiguration == InputManager.GetInputConfiguration("KeyboardAndMouse"))
                    run = InputManager.GetButton("Run");
                else
                    run = InputManager.GetButton("NextMenu");

                if (run && (InputManager.GetButtonDown("Attack") || InputManager.GetAxis("Attack") != 0) && Player.Stamina >= 10)
                {
                    staminaMod = -10;
                    Player.IsPowerAttacking = true;
                    cs.IsPowerAttacking = true;
                    CheckHit();
                    SetMove(0, 0, false);
                    StatsUpdate();
                }
                else if (!run && (InputManager.GetButtonDown("Attack") || InputManager.GetAxis("Attack") != 0) && Player.Stamina >= 5)
                {
                    if (Player.Stamina >= 5)
                        staminaMod = -5;
                    else
                        staminaMod = WorldController.TechN9ne;

                    Player.IsAttacking = true;
                    cs.IsAttacking = true;
                    CheckHit();
                    SetMove(0, 0, false);
                    StatsUpdate();
                }
                else if ((InputManager.GetButtonDown("Cast") || InputManager.GetAxis("Cast") != 0) && Player.Magicka >= Player.ActiveSpell.Cost)
                {
                    magickaMod = -Player.ActiveSpell.Cost;
                    Player.IsCasting = true;
                    cs.IsCasting = true;
                    SetMove(0, 0, false);
                    StatsUpdate();
                    // TESTING
                    // END TESTING
                }
                else
                {
                    if (run)
                    {

                        // Make sure the player cannot keep running after they are out of stamina
                        // Also makes sure Stamina does not regenerate if the run key is held down
                        if (!Running && Player.Stamina >= 0.75f)
                            Speed = Player.RunSpeed;
                        else if (Running && Player.Stamina < 0.75f)
                            Speed = Player.WalkSpeed;

                        Running = true;
                    }
                    else if (Running)
                    {

                        Speed = Player.WalkSpeed;
                        Running = false;
                    }
                    else if (!Running && Speed != Player.WalkSpeed)
                    {
                        Speed = Player.WalkSpeed;
                    }

                    if (InputManager.GetAxis("Vertical") != 0)
                    {
                        val = SetVal(InputManager.GetAxis("Vertical"), false);

                        if (val > 0)
                        {
                            faceDir = Vector3.up;
                            cb.MoveBox(Look.UP); // Direction of the Interact Box when using a Controller
                        }
                        else
                        {
                            faceDir = Vector3.down;
                            cb.MoveBox(Look.DOWN); // Direction of the Interact Box when using a Controller
                        }

                        SetMove(0, val, true);
                    }
                    else if (InputManager.GetAxis("Horizontal") != 0)
                    {
                        val = SetVal(InputManager.GetAxis("Horizontal"), true);

                        if (val > 0)
                        {
                            faceDir = Vector3.right;
                            cb.MoveBox(Look.RIGHT); // Direction of the Interact Box when using a Controller
                        }
                        else
                        {
                            faceDir = Vector3.left;
                            cb.MoveBox(Look.LEFT); // Direction of the Interact Box when using a Controller
                        }

                        SetMove(val, 0, true);
                    }
                    else
                    {
                        SetMove(0, 0, false);
                    }
                    
                    //rb.MovePosition(transform.position + move);
                    rb.velocity = move * 100;

                    // Make sure we update the graph when the player moves so AI Knows where they are
                    if (move != Vector3.zero)
                    {
                        //UpdatePath();
                    }
                    //if (move != Vector3.zero)

                    //transform.Translate(move);
                } // End Attacking, Casting, or Moving
            }
            
        } // End movePlayer()

        private void UpdatePath()
        {
            //Set the Update Bounds to the players collider
                    bounds = gus.GetBounds();
            // Expand the bounds along the Z axis
            bounds.Expand(Vector3.forward * 1000);

            // Create a new GUO for the current Player Bounds with a walking penalty of 1000
            guo = new GraphUpdateObject(bounds)
            {
                modifyTag = true,
                setTag = 3
            };



            // change some settings on the object
            AstarPath.active.UpdateGraphs(guo);
        }

        //private bool apply;
        //private GraphUpdateScene gus2;

        //private void UpdatePath()
        //{
        //    if (gus == null)
        //        gus = gameObject.GetComponentInChildren<GraphUpdateScene>();

        //    //if (prog == -1 || prog >= 100)
        //    //{
        //    //    StartCoroutine(ScanGraphs());
        //    //}

        //    if (apply)
        //    {
        //        if (gus2 == null)
        //            gus2 = GameObject.Find("GUO").GetComponent<GraphUpdateScene>();
        //        gus2.Apply();
        //    }
        //    else
        //    {
        //        // Set the Update Bounds to the players collider
        //        bounds = gus.GetBounds();
        //        // Expand the bounds along the Z axis
        //        bounds.Expand(Vector3.forward * 1000);

        //        // Create a new GUO for the current Player Bounds with a walking penalty of 1000
        //        guo = new GraphUpdateObject(bounds)
        //        {
        //            modifyTag = false,
        //            setTag = 3,
        //            trackChangedNodes = false,
        //            addPenalty = 1000
        //        };
        //    }
        //    // Remove the previous penalty from the grid graph
        //    if (guo != null && bounds != gus.GetBounds())
        //    {
        //        gus.Apply();
        //    }



        //    // change some settings on the object
        //    AstarPath.active.UpdateGraphs(guo);
        //}

        /// <summary>
        /// Sets the move vector to move the character
        /// </summary>
        /// <param name="x">X Axis</param>
        /// <param name="y">Y Axis</param>
        /// <param name="m">Are we moving or should we just turn?</param>
        private void SetMove(float x, float y, bool m)
        {
            Moving = m;

            if (y != 0)
                y = (SetVal(y, false) * Speed) * Time.fixedDeltaTime;
            else if (x != 0)
                x = (SetVal(x, true) * Speed) * Time.fixedDeltaTime;
            else // Otherwise, only turn the players animation in the specified direction
                SetVal(0, false);

            move.y = y;
            move.x = x;
            
            if (m && Running)
                Player.IsRunning = true;
            else
                Player.IsRunning = false;
        }

        /// <summary>
        /// Set the move direction based on the input axis and also set the players animation direction
        /// </summary>
        /// <param name="val">Current Axis value</param>
        /// <param name="x">Is this the X Axis?</param>
        /// <returns></returns>
        private float SetVal(float val, bool x)
        {
            // Clamp the value within 1.0 of zero
            if (val > 1.0f)
                val = 1.0f;
            else if (val < -1.0f)
                val = -1.0f;

            // Set players animation direction
            if (x)
            {
                if (val > 0)
                    cs.MoveDir = Vector3.right;
                else if (val < 0)
                    cs.MoveDir = Vector3.left;
            }
            else
            {
                if (val > 0)
                    cs.MoveDir = Vector3.up;
                else if (val < 0)
                    cs.MoveDir = Vector3.down;
            }

            cs.LookDir = cs.MoveDir;

            if (val == 0)
                cs.CanMove = false;
            else
                cs.CanMove = true;

            return val;
        }

    }
}

