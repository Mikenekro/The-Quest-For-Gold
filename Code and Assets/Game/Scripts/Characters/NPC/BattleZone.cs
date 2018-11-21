using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace schoolRPG
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class BattleZone : MonoBehaviour
    {
        [SerializeField, Tooltip("All NPC's that you want effected by this Battle Zone")]
        private List<GameObject> npcs;

        private bool active;

        /// <summary>
        /// Is this Battle Zone active?
        /// </summary>
        public bool Active { get { return active; } set { active = value; } }

        // Use this for initialization
        public void Start()
        {
            int i = 0;
            NpcController n = null;

            active = false;

            for (i = 0; i < npcs.Count; ++i)
            {
                n = npcs[i].GetComponent<NpcController>();

                if (n != null && n.enabled)
                {
                    n.enabled = false;
                    npcs[i].GetComponent<AILerp>().canMove = false;
                }
                else if (n == null)
                {
                    npcs.RemoveAt(i);
                    i -= 2;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            if (col.tag == "Player")
            {
                if (!active)
                    Enable();
            }
        }

        /// <summary>
        /// Checks if there are any Nested BattleZones or if this is a Nested Battle Zone and activates/de-activates it accordingly
        /// </summary>
        public void CheckNested()
        {
            BattleZone parent = GetComponentInParent<BattleZone>();
            BattleZone nest = GetComponentsInChildren<BattleZone>()[1];

            if (parent != null)
            {
                parent.Active = active;
            }
            else if (nest != null)
            {
                nest.Active = active;
            }
        }

        /// <summary>
        /// Enable each of the enemies pathing actions
        /// </summary>
        private void Enable()
        {
            int i = 0;
            NpcController n = null; 

            active = true;
            CheckNested();

            for (i = 0; i < npcs.Count; ++i)
            {
                n = npcs[i].GetComponent<NpcController>();

                // Enable if we have an NPC controller
                if (n != null)
                {
                    n.enabled = true;
                    npcs[i].GetComponent<AILerp>().canMove = true;
                }
            }
        }

        /// <summary>
        /// Disable each of the enemies pathing actions
        /// </summary>
        private void Disable()
        {
            int i = 0;
            NpcController n = null;

            active = false;
            CheckNested();

            for (i = 0; i < npcs.Count; ++i)
            {
                n = npcs[i].GetComponent<NpcController>();

                if (n != null)
                {
                    // Disable if we don't have an enemy target or the enemy is not the player
                    if (n.Enemy == null || (n.Enemy != null && n.Enemy.name != "Player"))
                    {
                        n.enabled = false;
                        npcs[i].GetComponent<AILerp>().canMove = false;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if this NPC is in the list and if the rest of the NPCs are disabled so we can disable it
        /// </summary>
        /// <param name="checkID"></param>
        public void CheckDisable(Guid checkID)
        {
            int i = 0;
            NpcController n = null;

            // If the rest of the NPCs are disabled
            if (!active)
            {
                for (i = 0; i < npcs.Count; ++i)
                {
                    n = npcs[i].GetComponent<NpcController>();

                    if (n != null)
                    {
                        if (n.NPC.UniqueID == checkID)
                        {
                            n.enabled = false;
                            break;
                        }
                    }
                }
            }
            
        }
    }
}

