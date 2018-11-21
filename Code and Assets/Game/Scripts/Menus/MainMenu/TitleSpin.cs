using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace schoolRPG
{
    public class TitleSpin : MonoBehaviour
    {
        public bool mouseOver;
        public float speed = 5.0f;

        // Use this for initialization
        void Start()
        {
            mouseOver = false;
        }

        public void OnMouseOver()
        {
            mouseOver = true;
        }
        public void OnMouseExit()
        {
            mouseOver = false;
        }

        // Update is called once per frame
        void Update()
        {
            // Random Title animation for looks
            if (!mouseOver)
                transform.Rotate(new Vector3(0.0f, speed * Time.deltaTime, 0.0f));
            else
                transform.localRotation = new Quaternion(0, 0, 0, 0);
        }
    }
}

