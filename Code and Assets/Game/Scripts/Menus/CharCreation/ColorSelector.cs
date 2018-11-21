using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TeamUtility.IO;

namespace schoolRPG
{
    public class ColorSelector : MonoBehaviour
    {
        [SerializeField]
        private CreationMenu create;

        private Color col;
        private Vector3 mouse;

        public Vector3 origin;

        void Start()
        {
            
        }

        public void OnMouseOver()
        {
            if (InputManager.GetButton("Attack"))
            {
                SelectColor(false);
            }
            
        }

        /// <summary>
        /// Used to select the color for the Players Items when Creating a Character
        /// </summary>
        /// <param name="slider"></param>
        public void SelectColor(bool slider)
        {
            Ray ray;
            RaycastHit hit;
            Vector3 mouse = Camera.main.ScreenToWorldPoint(InputManager.mousePosition);

            if (!slider)
                origin = new Vector3(mouse.x, mouse.y, -10);
            ray = new Ray(origin, Vector3.forward);
            

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.name == "Colors")
                {
                    Texture2D textMap = (Texture2D)hit.transform.GetComponent<MeshRenderer>().material.mainTexture;
                    Vector2 pixelUV = hit.textureCoord;
                    pixelUV.x *= textMap.width;
                    pixelUV.y *= textMap.height;

                    col = textMap.GetPixel((int)pixelUV.x, (int)pixelUV.y);

                    // Set the color of the selected object
                    create.SetColor(col);
                }
            }
        }
    }
}

