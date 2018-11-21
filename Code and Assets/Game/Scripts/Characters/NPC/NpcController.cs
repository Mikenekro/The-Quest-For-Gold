using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using schoolRPG.Stats;
using schoolRPG.Items;
using schoolRPG.Quests;
using schoolRPG.Dialogue;
using schoolRPG.Sound;

// TODO: Figure out a way to make NPCs update the A* Pathfinding graph without effecting their own pathing 

namespace schoolRPG
{
    public enum NPCType
    {
        ENEMY, MERCHANT, GUARD, CITIZEN, NEUTRAL
    }

    /// <summary>
    /// A route this NPC will take for a specific amount of time
    /// </summary>
    [System.Serializable]
    public class NpcRoute
    {
        [SerializeField, Tooltip("The Direction this NPC will look when they arrive at their destination")]
        private Vector2 directionOnArrive;
        [SerializeField]
        private GameObject target;
        private Waypoint wayp;
        [SerializeField]
        private float time;
        private float timeSpent;
        private bool none;

        /// <summary>
        /// Direction to look when the NPC arrives at their destination
        /// </summary>
        public Vector2 DirOnArrive { get { return directionOnArrive; } set { directionOnArrive = value; } }
        public GameObject Target { get { return target; } set { target = value; } }
        public Waypoint Waypoints { get { if (wayp == null && target != null) { wayp = target.GetComponent<Waypoint>(); } return wayp; } }
        public bool NoWaypoint { get { return none; } set { none = value; } }
        public float Time { get { return time; } set { time = value; } }
        public float TimeSpent { get { return timeSpent; } set { timeSpent = value; } }

        public NpcRoute(GameObject ta, float tm)
        {
            target = ta;
            time = tm;
            timeSpent = 0;
        }
    }

    [System.Serializable]
    [RequireComponent(typeof(NPCDialogue))]
    public class NpcController : MonoBehaviour
    {
        private QuestMarker[] qm;
        public static int nNum = 0;
        /// <summary>
        /// Position that this NPC holds in the World Controller
        /// </summary>
        [SerializeField, Tooltip("Set the Position that this NPC will load and save from")]
        private int npcPos;
        private TextMeshProUGUI baseDamage;
        private WaitForSeconds wfs;
        private float time = 0.1f;
        private float dmgVisible = 1.5f;
        private AILerp path;

        private bool enemyDead;
        private PlayerController enemyPC;
        private NpcController enemyNPC;
        private RaycastHit2D[] hits;
        private Transform newTarget;
        private bool inHit;
        private int node;

        /// <summary>
        /// When this is true, the NPC sees an enemy
        /// </summary>
        private bool seeEnemy;
        /// <summary>
        /// The GameObject of the enemy in this NPCs sight
        /// </summary>
        private GameObject enemy;
        [SerializeField, Tooltip("Add each Waypoint for the NPC to stop at during the day")]
        private List<NpcRoute> waypoint;
        private NpcRoute current;
        private int waypointPos;

        private bool moving;

        [SerializeField]
        private NPCType type;
        [SerializeField, Tooltip("Place the NPC's health bar here")]
        private Slider healthBar;
        [SerializeField, Tooltip("Place the Item that this NPC will use as a weapon")]
        private Item weapon;
        [SerializeField, Tooltip("Place the Item that this NPC will use as a helmet")]
        private Item helm;
        [SerializeField, Tooltip("Place the Item that this NPC will use as Body Armor")]
        private Item bodyArmor;
        [SerializeField, Tooltip("Place the Item that this NPC will use as pants")]
        private Item pants;
        [SerializeField, Tooltip("Place the Item that this NPC will use as gloves")]
        private Item gloves;
        [SerializeField, Tooltip("Place the Item that this NPC will use as boots")]
        private Item boots;
        
        [SerializeField]
        private Character npc;
        private CharStats cs;
        private NPCDialogue npcDialogue;

        [SerializeField]
        private float speed = 7.0f;

        private bool splash;
        private bool registered;

        public Vector3 MoveDir { get; set; }
        /// <summary>
        /// What is this NPC's reaction to the Player Character?
        /// </summary>
        public NPCType Type { get { return type; } }

        /// <summary>
        /// The Enemy that this NPC is chasing
        /// </summary>
        public GameObject Enemy { get { return enemy; } }

        /// <summary>
        /// Which of the NPCs nodes are in use
        /// </summary>
        public bool[] NodeUse { get; set; }

        public Character NPC
        {
            get
            {
                if (!splash)
                {
                    if (WorldController.Data.NPC.Count == 0 || WorldController.Data.NPC.Count <= npcPos || WorldController.Data.NPC[npcPos].Name != npc.Name)
                        npcPos = GetDataPos();
                    return WorldController.Data.NPC[npcPos];
                }
                else
                    return npc;
            }
        }

        // Use this for initialization
        void Awake()
        {
            // Register this NPC for death
            CharDelegates.DeathDelegate += OnNpcDeath;
            // Resigter this NPC to update its health bar
            CharDelegates.NpcHealthDelegate += OnHealthBar;
            // Register this NPC to save its data on command
            WorldController.NpcGetData += StoreNPCData;

            registered = true;

            NodeUse = new bool[4];

            if (GameObject.Find("SplashTimer") != null)
            {
                splash = true;
                CreationMenu.Creating = false;
                WorldController.Paused = false;
                WorldController.InMenu = false;
                LoadPersistant.loaded = true;
            }
            else
                splash = false;

            // Check if we have a Quest Marker attached
            if (GetComponent<QuestMarker>() != null)
                qm = GetComponents<QuestMarker>();

            //if (npcPos <= -1)
            //    npcPos = -1;
            // Get total number of NPCs in the world
            if (nNum == 0)
                nNum = FindObjectsOfType<NpcController>().Length;
            
            if (gameObject.GetComponent<AILerp>() == null)
                gameObject.AddComponent<AILerp>();
            path = gameObject.GetComponent<AILerp>();
            path.enableRotation = false;
            path.speed = 3;
            baseDamage = GetComponentInChildren<TextMeshProUGUI>();
            baseDamage.gameObject.SetActive(false);
            wfs = new WaitForSeconds(time);

            // Store character stats and NPC Dialogue 
            cs = GetComponent<CharStats>();
            if (GetComponent<NPCDialogue>() != null)
                npcDialogue = GetComponent<NPCDialogue>();
            cs.spawnPos = gameObject.transform.position;
            npc.Init(false, false);
            inHit = false;
            
            if (waypoint == null || waypoint.Count <= 0)
            {
                // Make sure there is always at least 1 route option (The NPC's position)
                waypoint = new List<NpcRoute>
                {
                    new NpcRoute(gameObject, 5.0f)
                };

                waypoint[0].NoWaypoint = true;
            }
        }

        public void Start()
        {
            if (WorldController.IsLoaded)
            {
                // NOTE: This is now done through delegates
                //// Load the data for this NPC
                //LoadNPCData();

                //if (cs.IsDead)
                //{
                //    if (cs.canDestroy)
                //    {
                //        if (Application.isEditor)
                //            DestroyImmediate(gameObject);
                //        else
                //            Destroy(gameObject);
                //    }
                //    else
                //    {

                //    }
                    
                //}
            }
            else
            {
                npcPos = GetDataPos();
                StoreNPCData(true);
            }

            npc.Inventory.PlayerInv = false;
            // Don't show the following change to the health bar
            StartCoroutine(healthBar.GetComponent<NpcHealthBar>().NoShow());
            healthBar.maxValue = npc.MaxHealth;
            healthBar.value = npc.Health;
        }

        void OnEnable()
        {
        }
        void OnDisable()
        {
            if (registered)
            {
                // Un-Register this NPC for death
                CharDelegates.DeathDelegate -= OnNpcDeath;
                // Un-Register this NPC from updating the health bar
                CharDelegates.NpcHealthDelegate -= OnHealthBar;
                // Un-Register this NPC from saving its data on command
                WorldController.NpcGetData -= StoreNPCData;

                registered = false;
            }
        }
        private void OnDestroy()
        {
            if (registered)
            {
                // Un-Register this NPC for death
                CharDelegates.DeathDelegate -= OnNpcDeath;
                // Un-Register this NPC from updating the health bar
                CharDelegates.NpcHealthDelegate -= OnHealthBar;
                // Un-Register this NPC from saving its data on command
                WorldController.NpcGetData -= StoreNPCData;

                registered = false;
            }
        }

        // Update is called once per frame
        private void Update()
        {

            if (!CreationMenu.Creating && !WorldController.Paused && !WorldController.InMenu && LoadPersistant.loaded)
            {
                // Make sure we have a Waypoint so we are not causing unneccesary allocations
                if (!waypoint[0].NoWaypoint)
                {
                    if (npc.IsRunning)
                        speed = npc.RunSpeed;
                    else
                        speed = npc.WalkSpeed;

                    path.speed = speed;
                    CheckEnemyDead();

                    if (npc.IsAttacking || npc.IsCasting || npc.IsTalking)
                    {
                        path.canMove = false;
                        path.canSearch = false;
                    }
                    else if (!cs.IsDead)
                    {
                        path.canMove = true;
                        path.canSearch = true;
                    }

                    if (!moving && !cs.IsDead && !npc.IsAttacking && !npc.IsCasting && !npc.IsTalking)
                    {
                        StartCoroutine(MoveNPC());
                    }
                    else if (cs.IsDead && npc.IsAttacking)
                    {
                        // Stop attacking
                        npc.IsAttacking = false;
                        path.canMove = false;
                        // stop chasing the enemy
                        seeEnemy = false;
                    }
                    else if (npc.IsTalking)
                    {
                        // Start talking to the player
                        cs.CanMove = false;
                        DialogueMenuController.npcC = this;
                    }
                }

            }
            else
            {
                cs.CanMove = false;
                path.canMove = false;
                path.canSearch = false;
            }
        }

        private void CheckEnemyDead()
        {
            if (seeEnemy)
            {
                if (enemyNPC != null && enemyNPC.NPC.Health <= 0)
                {
                    enemyDead = true;
                }
                else if (enemyPC != null && enemyPC.Player.Health <= 0)
                {
                    enemyDead = true;
                }
            }
            else if (enemyDead)
            {
                enemyDead = false;
            }
        }

        /// <summary>
        /// Moves the NPC and checks to see what their next action should be
        /// </summary>
        /// <returns></returns>
        private IEnumerator MoveNPC()
        {
            Vector2 nextPos = Vector2.zero;
            bool inPos = false;
            bool inUse = false;
            
            // Only set the current target if the current target is not the waypoint and we do not see an enemy
            if (current != waypoint[waypointPos] && !seeEnemy)
            {
                // Make sure the current Waypoint is not in use
                if (!waypoint[waypointPos].Waypoints.InUse)
                {
                    current = waypoint[waypointPos];
                    waypoint[waypointPos].Waypoints.InUse = true;
                }
                else
                {
                    if (waypointPos < waypoint.Count - 1)
                        waypointPos++;
                    else
                        waypointPos = 0;
                    current = waypoint[waypointPos];
                    current.TimeSpent = 0;
                }
            }
            
            moving = true;

            // While we are not in position and we are not dead
            while (!inPos && npc.Health > 0)
            {
                // If we do not see an enemy, continue with the waypoints
                if (!seeEnemy)
                {
                    // Check for an enemy
                    seeEnemy = CheckForEnemy();

                    path.targetHasCollider = false;

                    // Check if the NPC is within 2 units of their waypoint
                    if (Vector2.Distance(transform.position, current.Target.transform.position) <= 1f)
                    {
                        inPos = true;
                        
                        // Loop while we are at the current waypoint
                        while (current.TimeSpent < current.Time)
                        {
                            cs.CanMove = false;
                            path.canMove = false;
                            current.TimeSpent += time;

                            // Check for an enemy while we are patrolling
                            seeEnemy = CheckForEnemy();
                            if (seeEnemy)
                            {
                                //current.TimeSpent = 0;
                                cs.CanMove = true;
                                path.canMove = true;
                                inPos = false;
                                break;
                            }
                            yield return wfs;

                            // Look in the specified direction
                            cs.MoveDir = current.DirOnArrive;
                            cs.LookDir = current.DirOnArrive;
                        }

                        // If we have spent the correct amount of time at the waypoint
                        if (current.TimeSpent >= current.Time)
                        {
                            cs.CanMove = true;
                            path.canMove = true;
                            current.TimeSpent = 0;
                            current.Waypoints.InUse = false;

                            if (waypointPos < waypoint.Count - 1)
                                waypointPos++;
                            else
                                waypointPos = 0;

                            // If the Waypoint is in use, Loop until we find a waypoint that is not in use
                            while (waypoint[waypointPos].Waypoints.InUse)
                            {
                                if (waypointPos < waypoint.Count - 1)
                                    waypointPos++;
                                else
                                    waypointPos = 0;
                            }

                            current = waypoint[waypointPos];
                            current.Waypoints.InUse = true;
                            current.TimeSpent = 0;
                            
                        }
                    }

                    // Set the target for this NPC
                    path.target = current.Target.transform;
                }
                else // Fight or Run away from the enemy, depending on the NPCType
                {

                    if (type == NPCType.CITIZEN || type == NPCType.MERCHANT)
                    {
                        // Run
                        // TODO: Create a waypoint in the opposite direction of the enemy
                    }
                    else if (enemy != null) // If the NPC is a Guard, Neutral or an Enemy and they have an Enemy in their sights
                    {
                        // Fight
                        if (path.target != newTarget || newTarget == null)
                            SetOtherTarget(enemy.transform);
                        // See if we are targeting the correct node
                        else
                            CheckForNode();

                        // Make sure we found a good target position above
                        if (newTarget == null)
                            continue;

                        // Attack If we are within 1.55 units of the enemy and the enemy is not dead
                        if ((Vector2.Distance(transform.position, newTarget.position) <= 0.5f) && !enemyDead)
                        {
                            Vector2 direction = Vector2.zero; // This is now the normalized direction.
                            

                            // If we found the target name and no other NPC is already using the node
                            if (newTarget.name == "RightNode")
                                direction = Vector2.left;
                            else if (newTarget.name == "LeftNode")
                                direction = Vector2.right;
                            else if (newTarget.name == "DownNode")
                                direction = Vector2.up;
                            else if (newTarget.name == "UpNode")
                                direction = Vector2.down;

                            if (direction != Vector2.zero)
                            {
                                path.canMove = false;
                                cs.CanMove = false;
                                cs.MoveDir = direction;
                                cs.LookDir = direction;
                                npc.IsAttacking = true;
                                cs.MoveDir = Vector3.zero;

                                // Run when we are not in hit
                                if (!inHit)
                                    StartCoroutine(CheckHit());
                            }
                            
                        }  
                        else // If we are not within 2 units of the enemy
                        {
                            // Stop attacking
                            npc.IsAttacking = false;
                            path.canMove = true;
                            
                        }

                    }

                    // Must get 10 units away from an enemy before they stop chasing you
                    // Or if the enemy is dead, we can go back to the normal route
                    if ((Vector2.Distance(transform.position, enemy.transform.position) >= 10) || enemyDead)
                    {
                        // stop chasing the enemy
                        seeEnemy = false;
                        inPos = true;
                        // Release the target node from use state
                        if (newTarget != null)
                        {
                            if (enemy.name == "Player")
                                newTarget.gameObject.GetComponent<TargetNode>().ReleaseNode(true);
                            else
                                newTarget.gameObject.GetComponent<TargetNode>().ReleaseNode(false);
                            newTarget = null;
                            SoundController.CheckMusicType();
                        }
                    }

                }

                yield return wfs;
            }

            moving = false;
            yield return null;
        }

        /// <summary>
        /// Checks if we should hit another Character while attacking
        /// </summary>
        /// <returns></returns>
        private IEnumerator CheckHit()
        {
            // Looks for colliders on layer 8 or 9 (Player and NPC Layers)
            int layerMask = (1 << 8 | 1 << 9);
            float damAfter = 0;
            float damage = npc.Damage;
            float distance = 1.75f;
            RaycastHit2D hit;
            Rigidbody2D rb2;
            PlayerController pc;
            NpcController nc;
            WaitForSeconds wait = new WaitForSeconds(1.4f);
            inHit = true;

            // While this NPC is attacking
            while (npc.IsAttacking && !cs.IsDead)
            {
                // Set values
                cs.IsAttacking = true;
                path.canMove = false;
                cs.CanMove = false;
                if (npc.IsPowerAttacking)
                    damage = npc.Damage * 2.0f;
                else
                    damage = npc.Damage;

                Physics2D.queriesHitTriggers = true;
                Physics2D.queriesStartInColliders = false;
                // Look for the closest Character (Even if it isn't the enemy
                hit = Physics2D.Raycast(transform.position, cs.LookDir, distance, layerMask);
                if (hit)
                {
                    rb2 = hit.collider.gameObject.GetComponentInParent<Rigidbody2D>();
                    // If we hit the player
                    if (rb2.gameObject.tag == "Player")
                    {
                        pc = rb2.gameObject.GetComponent<PlayerController>();
                        if (pc != null)
                        {
                            // Damage the player
                            damAfter = pc.Player.AddHealth(-damage, false, false);

                            // Add to the Total Damage the player has taken
                            WorldController.Data.StatValue.DamageTaken += -damAfter;
                            // Add to the Total Damage the player has resisted
                            WorldController.Data.StatValue.DamageBlocked += (damage + damAfter);

                            // Check if this is the highest damage done to the player
                            if (-damAfter > WorldController.Data.StatValue.HighestDamageTaken)
                                WorldController.Data.StatValue.HighestDamageTaken = -damAfter;

                            // Check if this is the highest damage resisted by the player
                            if (-(damage - damAfter) > WorldController.Data.StatValue.HighestDamageResisted)
                                WorldController.Data.StatValue.HighestDamageResisted = (damage + damAfter);

                            // Damage the Players Equipment
                            if (pc.Player.EquipBody != null)
                                pc.Player.EquipBody.Condition[0] -= 0.05f;
                            if (pc.Player.EquipBoots != null)
                                pc.Player.EquipBoots.Condition[0] -= 0.05f;
                            if (pc.Player.EquipGloves != null)
                                pc.Player.EquipGloves.Condition[0] -= 0.05f;
                            if (pc.Player.EquipHelm != null)
                                pc.Player.EquipHelm.Condition[0] -= 0.05f;
                            if (pc.Player.EquipPants != null)
                                pc.Player.EquipPants.Condition[0] -= 0.05f;

                            if (npc.IsPowerAttacking)
                                npc.StaminaSkills(0, damAfter);
                            else
                                npc.StaminaSkills(0, damAfter);
                        }
                    }
                    // If we hit an NPC
                    else if (rb2.gameObject.tag == "NPC")
                    {
                        nc = rb2.gameObject.GetComponent<NpcController>();
                        if (nc != null)
                        {
                            // Damage the NPC
                            damage = nc.npc.AddHealth(-damage, false, false);

                            if (npc.IsPowerAttacking)
                                npc.StaminaSkills(0, damage);
                            else
                                npc.StaminaSkills(0, damage);
                        }
                    }

                }

                yield return wait;

                cs.IsAttacking = false;
                cs.CanMove = true;
                path.canMove = true;
                yield return null;
            }

            cs.CanMove = true;
            path.canMove = true;
            inHit = false;
            yield return null;
        }

        /// <summary>
        /// Sets the target based on this NPCs position to the target
        /// <para></para>
        /// NOTE: Uses a Node based system to prevent the Player from being attacked by more than 4 enemies at a time 
        /// as well as to prevent enemies from attacking on top of each other
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="target"></param>
        private void SetOtherTarget(Transform target, string nNode = "NONE")
        {
            int i = 0;
            bool usingNode;
            string search = "";
            TargetNode[] tNode;
            Transform[] targets;

            // Find the target node of transform that was passed to this function
            tNode = target.gameObject.GetComponentsInChildren<TargetNode>();
            targets = new Transform[tNode.Length];
            for (i = 0; i < tNode.Length; ++i)
                targets[i] = tNode[i].gameObject.transform;

            if (nNode != "NONE")
            {
                for (i = 0; i < targets.Length; ++i)
                {
                    // Make sure this node is not in use by another NPC
                    usingNode = (tNode[i].InUse && tNode[i].UsingID != npc.UniqueID);

                    if (targets[i].gameObject.name == nNode && !usingNode)
                    {
                        newTarget = targets[i];
                        node = 0;
                        break;
                    }
                }
            }
            else
            {
                // If we are to the right of the target and the X position is closer than the Y position
                if (transform.position.x > target.position.x && (transform.position.x - target.position.x) > Mathf.Abs(Mathf.Abs(transform.position.y) - Mathf.Abs(target.position.y)))
                {
                    search = "RightNode";
                    for (i = 0; i < targets.Length; ++i)
                    {
                        // Make sure this node is not in use by another NPC
                        usingNode = (tNode[i].InUse && tNode[i].UsingID != npc.UniqueID);

                        if (targets[i].gameObject.name == search && !usingNode)
                        {
                            newTarget = targets[i];
                            node = 0;
                            break;
                        }
                        else if (usingNode)
                        {
                            if (search == "RightNode")
                                search = "UpNode";
                            else if (search == "UpNode")
                                search = "DownNode";
                            else if (search == "DownNode")
                                search = "LeftNode";
                            else
                            {
                                // We cannot attack this target yet
                                seeEnemy = false;
                                enemy = null;
                                newTarget = null;
                                break;
                            }

                            // Restart the search
                            i = 0;
                        }
                    }
                }
                // If we are to the left of the target and the X position is closer than the Y position
                else if (transform.position.x < target.position.x && (target.position.x - transform.position.x) > Mathf.Abs(Mathf.Abs(transform.position.y) - Mathf.Abs(target.position.y)))
                {
                    search = "LeftNode";

                    for (i = 0; i < targets.Length; ++i)
                    {
                        // Make sure this node is not in use by another NPC
                        usingNode = (tNode[i].InUse && tNode[i].UsingID != npc.UniqueID);

                        if (targets[i].gameObject.name == search && !usingNode)
                        {
                            newTarget = targets[i];
                            node = 1;
                            break;
                        }
                        else if (usingNode)
                        {
                            if (search == "LeftNode")
                                search = "UpNode";
                            else if (search == "UpNode")
                                search = "DownNode";
                            else if (search == "DownNode")
                                search = "RightNode";
                            else
                            {
                                // We cannot attack this target yet
                                seeEnemy = false;
                                enemy = null;
                                newTarget = null;
                                break;
                            }

                            // Restart the search
                            i = 0;
                        }
                    }
                }
                else if (transform.position.y > target.position.y)
                {
                    search = "UpNode";
                    for (i = 0; i < targets.Length; ++i)
                    {
                        // Make sure this node is not in use by another NPC
                        usingNode = (tNode[i].InUse && tNode[i].UsingID != npc.UniqueID);

                        if (targets[i].gameObject.name == search && !usingNode)
                        {
                            newTarget = targets[i];
                            node = 2;
                            break;
                        }
                        else if (usingNode)
                        {
                            if (search == "UpNode")
                                search = "LeftNode";
                            else if (search == "LeftNode")
                                search = "RightNode";
                            else if (search == "RightNode")
                                search = "DownNode";
                            else
                            {
                                // We cannot attack this target yet
                                seeEnemy = false;
                                enemy = null;
                                newTarget = null;
                                break;
                            }

                            // Restart the search
                            i = 0;
                        }
                    }
                }
                else
                {
                    search = "DownNode";

                    for (i = 0; i < targets.Length; ++i)
                    {
                        // Make sure this node is not in use by another NPC
                        usingNode = (tNode[i].InUse && tNode[i].UsingID != npc.UniqueID);

                        if (targets[i].gameObject.name == search && !usingNode)
                        {
                            newTarget = targets[i];
                            node = 3;
                            break;
                        }
                        else if (usingNode)
                        {
                            if (search == "DownNode")
                                search = "LeftNode";
                            else if (search == "LeftNode")
                                search = "RightNode";
                            else if (search == "RightNode")
                                search = "UpNode";
                            else
                            {
                                // We cannot attack this target yet
                                seeEnemy = false;
                                enemy = null;
                                newTarget = null;
                                break;
                            }

                            // Restart the search
                            i = 0;
                        }
                    }
                }
            }

            
            // Set the new target
            if (newTarget != null)
            {
                // Set this node as in use
                newTarget.gameObject.GetComponent<TargetNode>().UseNode((enemy.name == "Player"), node, npc.UniqueID);
                path.target = newTarget;
            }
        }
        
        /// <summary>
        /// Checks the NPC's position against the Enemies position to see if the current Node is the most efficient one
        /// </summary>
        /// <returns></returns>
        private void CheckForNode()
        {
            int i;
            string nNode = "";
            TargetNode[] tNode;
            Transform[] targets;
            string[] nodes = { "LeftNode", "RightNode", "DownNode", "UpNode" };

            if (newTarget != null)
            {
                // Find the target node of transform that was passed to this function
                tNode = enemy.transform.gameObject.GetComponentsInChildren<TargetNode>();
                targets = new Transform[tNode.Length];
                for (i = 0; i < tNode.Length; ++i)
                    targets[i] = tNode[i].gameObject.transform;

                for (i = 0; i < tNode.Length; ++i)
                {
                    // If there is a node that is closer than the current selected node, move to that node if possible
                    if (Mathf.Abs(Vector2.Distance(transform.position, targets[i].position)) < Mathf.Abs(Vector2.Distance(transform.position, newTarget.position)) &&
                        targets[i].gameObject.name != newTarget.gameObject.name && !tNode[i].InUse)
                    {
                        nNode = nodes[i];
                    }
                }
            }
            

            //// Check for the closest node to the NPC
            //if (transform.position.x > enemy.transform.position.x && 
            //    (transform.position.x - enemy.transform.position.x) > Mathf.Abs(Mathf.Abs(transform.position.y) - Mathf.Abs(enemy.transform.position.y)) && 
            //    newTarget.gameObject.name != "RightNode")
            //{
            //    nNode = "RightNode";
            //}
            //else if (transform.position.x < enemy.transform.position.x && 
            //    (enemy.transform.position.x - transform.position.x) > Mathf.Abs(Mathf.Abs(transform.position.y) - Mathf.Abs(enemy.transform.position.y)) &&
            //    newTarget.gameObject.name != "LeftNode")
            //{
            //    nNode = "LeftNode";
            //}
            //else if (transform.position.y > enemy.transform.position.y && newTarget.gameObject.name != "UpNode")
            //{
            //    nNode = "UpNode";
            //}
            //else if (transform.position.y < enemy.transform.position.y && newTarget.gameObject.name != "DownNode")
            //{
            //    nNode = "DownNode";
            //}

            if (nNode != "")
            {
                if (newTarget != null)
                {
                    if (enemy.name == "Player")
                        newTarget.gameObject.GetComponent<TargetNode>().ReleaseNode(true);
                    else
                        newTarget.gameObject.GetComponent<TargetNode>().ReleaseNode(false);
                    newTarget = null;
                    SoundController.CheckMusicType();
                }

                // Set the new Node that we should target
                SetOtherTarget(enemy.transform, nNode);
            }
        }
        
        /// <summary>
        /// Do we want to Force an Attack on the Player or another NPC?
        /// </summary>
        public void ForceAttack(bool player, NpcController nc = null)
        {
            // Make sure we are forcing an attack correctly
            if (player || (!player && nc != null))
            {
                seeEnemy = true;

                if (player)
                {
                    // Play fight music
                    SoundController.SetMusicType(MusicType.FIGHT);
                    enemyPC = PlayerController.Pc;
                    enemy = enemyPC.gameObject;
                }
                else
                {
                    enemyNPC = nc;
                    enemy = nc.gameObject;
                }
            }
            else
            {
                enemy = null;
                seeEnemy = false;
            }
        }

        /// <summary>
        /// Resets this NPCs relation to the player and other NPCs
        /// </summary>
        /// <param name="t"></param>
        public void SetRelation(NPCType t)
        {
            type = t;
            WorldController.Data.ChangedTypes.Add((int)type + "_Changed" + npcPos + npc.Name);
        }

        /// <summary>
        /// Checks if we can see an enemy
        /// </summary>
        /// <returns></returns>
        private bool CheckForEnemy()
        {
            int maskNPC = 1 << 9;
            int maskPlayer = 1 << 8;
            int finalMask = maskNPC | maskPlayer; // (1 << layer1) or (1 << layer2)
            bool wasPlayer = false;
            bool inUse = false;
            Vector2 boxSize = new Vector2(6, 6);
            Vector3 offset = new Vector3(3 * cs.LookDir.x, 3 * cs.LookDir.y, 0);
            Vector3 pos = transform.position + offset;
            RaycastHit2D[] hits;

            seeEnemy = false;

            if (enemy != null && enemy.name == "Player")
                wasPlayer = true;

            Physics2D.queriesHitTriggers = true;
            Physics2D.queriesStartInColliders = true;
            hits = Physics2D.CircleCastAll(pos, 8, cs.LookDir, 1.0f, finalMask);
            // Look at any and all colliders that can be a target enemy
            //hits = Physics2D.BoxCastAll(pos, boxSize, 0, cs.LookDir, 6.0f, finalMask);

            // Check if any of the Raycasts hit an enemy
            for (int i = 0; i < hits.Length; ++i)
            {
                // Don't target self
                if (hits[i].collider.gameObject.name != gameObject.name)
                {
                    if (type == NPCType.CITIZEN || type == NPCType.MERCHANT || type == NPCType.GUARD)
                    {
                        if (hits[i].collider.gameObject.tag == "NPC")
                        {
                            enemyNPC = hits[i].collider.gameObject.GetComponent<NpcController>();
                            // Enemy and Neutral are considered enemies to other NPCs
                            if (enemyNPC.Type == NPCType.ENEMY || enemyNPC.type == NPCType.NEUTRAL)
                            {
                                seeEnemy = true;
                                enemy = enemyNPC.gameObject;
                                break;
                            }
                            else if (seeEnemy)
                            {
                                seeEnemy = false;
                                enemy = null;
                            }
                        }
                    }
                    else
                    {
                        // If we see the player and we are not neutral
                        if (hits[i].collider.gameObject.tag == "Player" && type != NPCType.NEUTRAL)
                        {
                            // Play fight music
                            SoundController.SetMusicType(MusicType.FIGHT);
                            seeEnemy = true;
                            if (enemyPC == null)
                                enemyPC = hits[i].collider.gameObject.GetComponent<PlayerController>();
                            enemy = enemyPC.gameObject;
                        }
                        else if (hits[i].collider.gameObject.tag == "NPC")
                        {
                            enemyNPC = hits[i].collider.gameObject.GetComponent<NpcController>();
                            if (enemyNPC.Type == NPCType.CITIZEN || enemyNPC.Type == NPCType.MERCHANT || enemyNPC.Type == NPCType.GUARD)
                            {
                                seeEnemy = true;
                                enemy = enemyNPC.gameObject;
                                break;
                            }
                            //else if (seeEnemy)
                            //{
                            //    seeEnemy = false;
                            //    enemy = null;
                            //}
                        }
                    }
                }
            }

            // Free up the last used node for the music if this is not the player anymore
            if (wasPlayer && ((enemy != null && enemy.name != "Player") || enemy == null))
            {
                PlayerController.NodeUse[node] = false;

                // Check if we should disable the battle music
                for (int i = 0; i < PlayerController.NodeUse.Length; ++i)
                {
                    if (PlayerController.NodeUse[i])
                    {
                        inUse = true;
                        break;
                    }
                }

                // Change the music type if no nodes are in use
                if (!inUse)
                    SoundController.SetMusicType(MusicType.EXPLORATION);
            }

            return seeEnemy;
        }
        
        private void EquipItems()
        {
            // Equip any items this NPC has referenced
            if (weapon != null)
                npc.EquipWeapon = weapon.item;
            if (helm != null)
                npc.EquipHelm = helm.item;
            if (bodyArmor != null)
                npc.EquipBody = bodyArmor.item;
            if (pants != null)
                npc.EquipPants = pants.item;
            if (gloves != null)
                npc.EquipGloves = gloves.item;
            if (boots != null)
                npc.EquipBoots = boots.item;
        }
        
        /// <summary>
        /// Automatically stores the NPC data
        /// <paramref name="setData">Are we Saving or Loading? (Saving = true, Loading = false)</paramref>
        /// </summary>
        public void StoreNPCData(bool setData)
        {
            SpriteRenderer[] rends;

            if (!splash)
            {
                // If we are saving the data
                if (setData)
                {
                    // Equip any items before we save this NPC's state
                    EquipItems();
                    // Set the World Position
                    npc.WorldPos = new SaveVector3(transform.position.x, transform.position.y, transform.position.z);
                    npc.Name = npc.Name;

                    // Update or add this NPC to the PlayerData
                    if (npcPos -1 > WorldController.Data.NPC.Count)
                        npcPos = GetDataPos();
                    else
                    {
                        WorldController.Data.NPC[npcPos] = npc;
                        WorldController.Data.NPCStats[npcPos] = cs.StoreStats;
                        WorldController.Data.NPCNum[npcPos] = waypointPos;
                        // Store the Dialogue to save any changes
                        WorldController.Data.NPCGreeting[npcPos] = npcDialogue.Greet;
                        WorldController.Data.NPCDialogue[npcPos] = npcDialogue.DialogueList;
                    }
                    //Debug.Log("Set NPC: " + npcPos);
                }
                else
                {
                    LoadNPCData();

                    // Make sure the NPCs are loaded with their last used waypoints
                    if (path.target != waypoint[waypointPos].Target.transform && !seeEnemy)
                    {
                        current = waypoint[waypointPos];
                        if (path.target != current.Target.transform)
                        {
                            path.target = current.Target.transform;
                            Debug.Log("Set Target for: " + npc.Name);
                        }
                    }

                    // Try to load any changed type
                    for (int i = 0; i < WorldController.Data.ChangedTypes.Count; ++i)
                    {
                        if (WorldController.Data.ChangedTypes[i].Split('_')[1] == "Changed" + npcPos + npc.Name)
                        {
                            int val = -1;

                            int.TryParse(WorldController.Data.ChangedTypes[i].Split('_')[0], out val);

                            if (val > -1)
                                type = (NPCType)val;

                            break;
                        }
                    }

                    Debug.Log("Loaded NPC: " + npc.Name);

                    if (cs.IsDead)
                    {
                        if (cs.canDestroy)
                        {
                            // Force an instant kill if the NPC can NOT respawn and they are loaded dead
                            if (Application.isEditor)
                                DestroyImmediate(gameObject);
                            else
                                Destroy(gameObject);
                        }
                        else
                        {
                            // Force an instant respawn if the NPC can respawn and they are loaded dead
                            rends = gameObject.GetComponentsInChildren<SpriteRenderer>();
                            gameObject.GetComponentsInChildren<BoxCollider2D>()[0].isTrigger = true;
                            gameObject.GetComponentsInChildren<BoxCollider2D>()[0].enabled = false;
                            for (int i = 0; i < rends.Length; ++i)
                                rends[i].enabled = false;
                            StartCoroutine(cs.Respawn());
                        }

                    }
                }
            }
        }
        /// <summary>
        /// Load this NPCs data from the PlayerData
        /// </summary>
        public void LoadNPCData()
        {
            // Make sure this NPC is in the data list
            GetDataPos();

            npc = WorldController.Data.NPC[npcPos];
            transform.position = new Vector3(npc.WorldPos.X, npc.WorldPos.Y, npc.WorldPos.Z);
            cs.StoreStats = WorldController.Data.NPCStats[npcPos];
            // Load the Dialogue
            npcDialogue.LoadDialogue(WorldController.Data.NPCGreeting[npcPos], WorldController.Data.NPCDialogue[npcPos]);

            if (waypoint.Count > waypointPos)
                waypointPos = WorldController.Data.NPCNum[npcPos];
            cs.SetColors();
        }

        /// <summary>
        /// Gets the position in the PlayerData that this NPC resides. 
        /// <para></para>
        /// Will add the NPC to the PlayerData if it isn't there yet
        /// </summary>
        /// <returns></returns>
        private int GetDataPos()
        {
            int i = 0;

            if (!splash)
            {
                List<Character> nums = WorldController.Data.NPC;

                // Make sure there are enough NPCs in the list to save the current NPC if it is saving out of order
                while (nums.Count < nNum)
                {
                    WorldController.Data.NPC.Add(new Character());
                    WorldController.Data.NPCStats.Add(new CStats());
                    WorldController.Data.NPCNum.Add(-1);
                    WorldController.Data.NPCDialogue.Add(new List<Dialogue.Dialogue>());
                    WorldController.Data.NPCGreeting.Add(new Greeting());
                }

                // Store this NPC in the Player Data if they are not already stored
                if (nums.Count - 1 >= npcPos && WorldController.Data.NPC[npcPos].Name != npc.Name)
                {
                    WorldController.Data.NPC[npcPos] = npc;
                    WorldController.Data.NPCStats[npcPos] = cs.StoreStats;
                    WorldController.Data.NPCNum[npcPos] = waypointPos;
                    // Store the Dialogue to save any changes
                    WorldController.Data.NPCGreeting[npcPos] = npcDialogue.Greet;
                    WorldController.Data.NPCDialogue[npcPos] = npcDialogue.DialogueList;
                }

                // Look for this NPCs number in the PlayerData list
                for (i = 0; i < nums.Count; ++i)
                {
                    if (i == npcPos)
                        break;
                }
            }

            return i;
        }

        /// <summary>
        /// Called when the NPC has died
        /// </summary>
        public void OnNpcDeath(Guid uniqueID)
        {
            if (npc.UniqueID == uniqueID)
            {
                Debug.Log("NPC Has Died!");

                path.canMove = false;
                path.canSearch = false;

                // stop chasing the enemy
                seeEnemy = false;
                // Release the target node from use state
                if (newTarget != null)
                {
                    if (enemy.name == "Player")
                        newTarget.gameObject.GetComponent<TargetNode>().ReleaseNode(true);
                    else
                        newTarget.gameObject.GetComponent<TargetNode>().ReleaseNode(false);
                    newTarget = null;

                    if (enemy.name == "Player")
                    SoundController.CheckMusicType();
                }

                // If we have a quest marker attached when we die
                if (qm != null)
                {
                    for (int i = 0; i < qm.Length; ++i)
                    {
                        // If the quest stage meets this objective stage
                        if (QuestController.Quests[qm[i].questPos].Stage == qm[i].obj.Stage)
                        {
                            // If the NPC is supposed to be killed (by the player or not)
                            if (QuestController.Quests[qm[i].questPos].Objectives[QuestController.Quests[qm[i].questPos].ObjectiveAt].KillUniqueNPC == gameObject)
                            {
                                // Make sure they can be destroyed and will not respawn
                                cs.canDestroy = true;
                                // Move to the next stage in the quest
                                QuestController.Quests[qm[i].questPos].SetStage(true, false);
                            }
                            // else if the NPC is one of the NPCS we can kill and it can be destroyed
                            else if (qm[i].type == ObjectiveType.KILLNPC_ANY && npc.PlayerHitMe && cs.canDestroy)
                            {
                                // Kills more NPCs
                                QuestController.Quests[qm[i].questPos].Objectives[qm[i].objPos].KillAny(true);
                                QuestController.Quests[qm[i].questPos].CheckFinishKillCount();
                            }
                        }
                        else // If we are not at the correct stage yet
                        {
                            // Only let specific objectives advance
                            if (qm[i].type == ObjectiveType.KILLNPC_UNIQUE && qm[i].obj.KillUniqueNPC == gameObject)
                            {
                                cs.canDestroy = true;
                                QuestController.Quests[qm[i].questPos].Objectives[qm[i].objPos].CompleteBeforeActive = true;
                                Debug.Log("Completed Before Active, Quest: " + qm[i].quest.Name + ", Objective: " + qm[i].obj.ObjectiveDisplay);
                            }
                        }
                    }
                }

                cs.IsDead = true;

                // Store the NPCs data to save the dead NPCs state
                if (!splash)
                    StoreNPCData(true);
            }
        }

        /// <summary>
        /// Sets this NPC's health bar value 
        /// </summary>
        /// <param name="value"></param>
        public void OnHealthBar(float value, Guid uniqueID)
        {
            // Make sure we only update the correct NPC
            if (npc.UniqueID == uniqueID)
            {
                // Display damage if we are losing health
                if (value < healthBar.value)
                    InstantiateDamage((healthBar.value - value));

                // Set the health bar value
                healthBar.value = value;
            }
        }

        /// <summary>
        /// Instantiates the damage text over the NPC
        /// </summary>
        private void InstantiateDamage(float dmg)
        {
            GameObject obj = Instantiate(baseDamage.gameObject);
            obj.transform.SetParent(baseDamage.transform.parent);
            obj.GetComponent<RectTransform>().sizeDelta = baseDamage.GetComponent<RectTransform>().sizeDelta;
            obj.GetComponent<RectTransform>().localScale = baseDamage.GetComponent<RectTransform>().localScale;
            obj.transform.position = baseDamage.transform.position;
            obj.GetComponent<TextMeshProUGUI>().text = dmg.ToString("-0.00");
            obj.SetActive(true);
            // Start the damage text coroutine
            StartCoroutine(DamageTxt(obj));
        }

        private IEnumerator DamageTxt(GameObject obj)
        {
            float curVisible = 0.0f;
            Transform tr = obj.transform;

            while (curVisible < dmgVisible)
            {
                yield return wfs;
                curVisible += time;
                tr.Translate(Vector3.up * Time.deltaTime);
            }

            if (Application.isEditor)
                DestroyImmediate(obj);
            else
                Destroy(obj);

            yield return null;
        }
    }
}

