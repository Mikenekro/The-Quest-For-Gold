using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;

namespace schoolRPG
{
    /// <summary>
    /// Use this script to create a Jump Point in the game
    /// </summary>
    public class JumpPoint : MonoBehaviour
    {
        private GameObject player;
        private WaitForSeconds wfs;
        private bool inCollider;

        [SerializeField, Tooltip("Place the Point that this Jump Point will spawn the player at")]
        private JumpPoint jumpTo;

        public Vector2 Point { get; set; }
        public bool HasJumped { get; set; }

        // Use this for initialization
        void Start()
        {
            Point = transform.position;
            player = GameObject.Find("Player");
            wfs = new WaitForSeconds(1.0f);
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.GetComponent<PlayerController>() != null)
            {
                PlayerController.jumpPoint = true;
                inCollider = true;
            }
        }
        // Update is called once per frame
        void OnTriggerStay2D(Collider2D col)
        {
            if (col.GetComponent<PlayerController>() != null)
            {
                WorldController.UseMsg.text = "Press the \"Cast\" button to use Jump Point.";
                PlayerController.jumpPoint = true;
                inCollider = true;
            }
        }
        private void OnTriggerExit2D(Collider2D col)
        {
            if (col.GetComponent<PlayerController>() != null)
            {
                WorldController.UseMsg.text = "";
                PlayerController.jumpPoint = false;
                inCollider = false;
            }
        }

        public void Update()
        {
            if (inCollider)
            {
                // If we press the Cast key 
                if (InputManager.GetButtonDown("Cast") && !HasJumped)
                {
                    player.GetComponent<PlayerController>().enabled = false;
                    // Move the player to the jump point
                    player.transform.position = jumpTo.Point;
                    HasJumped = true;
                    jumpTo.HasJumped = true;

                    StartCoroutine(ProcessJump());
                }
            }
        }

        private IEnumerator ProcessJump()
        {
            yield return wfs;
            player.GetComponent<PlayerController>().enabled = true;
            HasJumped = false;
            jumpTo.HasJumped = false;

            // Increase the number of times we used a Jump Point;
            WorldController.Data.StatValue.JumpPointsUsed += 1;
            yield return null;
        }
    }
}

