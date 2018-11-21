using System;
using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;

namespace schoolRPG
{
    /// <summary>
    /// Allows this object to point towards a Quest Marker
    /// </summary>
    public class LookToObjective : MonoBehaviour
    {
        [SerializeField, Tooltip("The Pointer for the Objective")]
        private GameObject arrow;
        [SerializeField, Tooltip("The Object we will be looking at")]
        private GameObject lookAt;

        private static GameObject qDir;

        private Vector2 dir;
        private float angle;
        private Transform player;
        private Transform mark;


        private float distance;
        private Vector2 direction;

        private Vector3 lastArrow;
        private Vector3 lastLookAt;

        /// <summary>
        /// The Marker we will look at
        /// </summary>
        public GameObject Marker { get; set; }

        public string QuestID { get; set; }

        /// <summary>
        /// A Unique ID to identify which object this Marker belongs to
        /// </summary>
        public Guid UniqueID { get; set; }

        private void Start()
        {
            qDir = GameObject.Find("QuestDirection");
        }

        private void OnEnable()
        {
            player = PlayerController.Pc.gameObject.transform;
            //camPos = new Vector2(Camera.main.pixelWidth / 8.0f, Camera.main.pixelHeight / 8.0f);

            if (qDir == null)
                qDir = GameObject.Find("QuestDirection");

            if (gameObject.name != "QuestPointer")  
                arrow.transform.SetParent(qDir.transform);

            arrow.transform.position = player.position;
        }

        private void OnDestroy()
        {
            if (Application.isEditor)
                DestroyImmediate(arrow);
            else
                Destroy(arrow);
        }

        // Update is called once per frame
        void Update()
        {
            if (!WorldController.Paused && !WorldController.InMenu)
            {
                dir = mark.position - player.position;
                distance = dir.magnitude;

                if (distance > 10)
                {
                    if (!arrow.activeInHierarchy)
                        arrow.SetActive(true);

                    angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    lookAt.transform.position = mark.position;
                    arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                }
                else
                {
                    arrow.SetActive(false);
                }
                
            }
        }



        /// <summary>
        /// Sets the direction this Arrow should point at
        /// </summary>
        /// <param name="Marker"></param>
        public void SetLookAt(Transform Marker)
        {
            mark = Marker;
        }
    }
}

