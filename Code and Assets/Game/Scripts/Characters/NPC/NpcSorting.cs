using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace schoolRPG
{
    public class NpcSorting : MonoBehaviour
    {
        private Transform pc;
        private Transform nc;
        private SpriteRenderer sr;
        private WaitForSeconds wfs;
        private bool inSorting;
        private bool above;

        // Use this for initialization
        void Start()
        {
            above = false;
            if (GameObject.Find("Player") != null)
                pc = GameObject.Find("Player").transform;
            nc = gameObject.transform;
            sr = gameObject.GetComponent<SpriteRenderer>();
            wfs = new WaitForSeconds(1.0f);
            Debug.Log("Layer ID: " + sr.sortingLayerID);
        }

        void Update()
        {
            if (!inSorting && pc != null)
            {
                StartCoroutine(SetSorting());
            }
        }
        
        private IEnumerator SetSorting()
        {
            inSorting = true;

            while (!WorldController.Paused && !WorldController.InMenu)
            {
                if (!above && pc.position.y > nc.position.y)
                {
                    above = true;
                    // UI Layer
                    sr.sortingOrder = -10;
                    sr.sortingLayerID = -1558561717;
                    
                }
                else if (above && pc.position.y < nc.position.y)
                {
                    above = false;
                    // Body Layer
                    sr.sortingOrder = -2;
                    sr.sortingLayerID = -1948909699;
                }

                // Run every second so this doesn't take up much CPU time
                yield return wfs;
            }

            inSorting = false;
            yield return null;
        }
    }
}

