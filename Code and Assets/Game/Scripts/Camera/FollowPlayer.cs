using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using schoolRPG;
using TeamUtility.IO;

namespace schoolRPG.CamScripts
{
    public class FollowPlayer : MonoBehaviour
    {
        private bool good;
        private Camera cam;
        private Vector3 vec;

        public PlayerController player;
        [Tooltip("The Target that this Camera will follor")]
        public Transform target;

        public float dampen;
        public Vector3 velocity = Vector3.zero;

        // Use this for initialization
        void Awake()
        {
            good = true;
            cam = gameObject.GetComponent<Camera>();
        }
        void Start()
        {
            if (cam == null)
                good = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (!WorldController.Paused && !WorldController.InMenu && good)
            {
                vec = cam.ScreenToViewportPoint(cam.WorldToScreenPoint(target.transform.position));

                ////Debug.Log("Vec: " + vec);

                if (vec.x >= 0.6f)
                {
                    // Move Camera Right
                    moveCam(new Vector3(player.Speed, 0));
                }
                else if (vec.x <= 0.4f)
                {
                    // Move Camera Left
                    moveCam(new Vector3(-player.Speed, 0));
                }
                else if (vec.y >= 0.6f)
                {
                    // Move Camera Up
                    moveCam(new Vector3(0, player.Speed));
                }
                else if (vec.y <= 0.4f)
                {
                    // Move Camera Down
                    moveCam(new Vector3(0, -player.Speed));
                }
                else
                {
                    // Uncomment this if you want to add in a "Smooth Follow" element to the players screen position
                    smoothFollow();
                }

                if (InputManager.PlayerOneConfiguration == InputManager.GetInputConfiguration("KeyboardAndMouse"))
                {
                    if (InputManager.GetAxis("Mouse ScrollWheel") > 0)
                    {
                        if (Camera.main.orthographicSize > 6.0f)
                            Camera.main.orthographicSize -= 0.1f;
                    }
                    else if (InputManager.GetAxis("Mouse ScrollWheel") < 0)
                    {
                        if (Camera.main.orthographicSize < 10.0f)
                            Camera.main.orthographicSize += 0.1f;
                    }
                }
                else
                {
                    if (InputManager.GetButton("Left Stick Button"))
                    {
                        if (InputManager.GetAxis("rVertical") > 0.1f)
                        {
                            if (Camera.main.orthographicSize > 6.0f)
                                Camera.main.orthographicSize -= 0.1f;
                        }
                        else if (InputManager.GetAxis("rVertical") < -0.1f)
                        {
                            if (Camera.main.orthographicSize < 10.0f)
                                Camera.main.orthographicSize += 0.1f;
                        }
                    }
                }
                    
                
            }
        }

        /// <summary>
        /// Smoothly follows the target
        /// </summary>
        public void smoothFollow()
        {
            if (target)
            {
                Vector3 point = cam.WorldToViewportPoint(target.position);
                Vector3 delta = target.position - cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
                Vector3 destination = transform.position + delta;
                //transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampen);
                transform.position = Vector3.Lerp(transform.position, destination, dampen * Time.deltaTime);
            }
        }

        /// <summary>
        /// Flat out moves the Camera using transform.Translate
        /// </summary>
        /// <param name="dir"></param>
        public void moveCam(Vector3 dir)
        {
            transform.Translate(dir * Time.deltaTime);
        }
    }
}

