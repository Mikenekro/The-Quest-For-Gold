using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;

namespace schoolRPG
{
    public enum Look
    {
        UP, DOWN, LEFT, RIGHT
    }

    [RequireComponent(typeof(BoxCollider2D))]
    public class ControllerBox : MonoBehaviour
    {
        private static BoxCollider2D box;
        private Look lastDir;
        
        // Use this for initialization
        public void Start()
        {
            lastDir = Look.DOWN;
            box = GetComponent<BoxCollider2D>();
        }

        /// <summary>
        /// Set the players Interact Box when the controller is/isnt in use
        /// </summary>
        /// <param name="active"></param>
        public static void SetBox(bool active)
        {
            box.enabled = active;
        }

        public void MoveBox(Look dir)
        {
            if (dir != lastDir)
            {
                if (dir == Look.UP)
                {
                    transform.rotation = new Quaternion(0, 0, 1, 0);
                }
                else if (dir == Look.DOWN)
                {
                    transform.rotation = new Quaternion(0, 0, 0, 0);
                }
                else if (dir == Look.LEFT)
                {
                    transform.rotation = new Quaternion(0, 0, 90, -90);
                }
                else if (dir == Look.RIGHT)
                {
                    transform.rotation = new Quaternion(0, 0, 90, 90);
                }

                lastDir = dir;
            }
            
        }
        
    }
}

