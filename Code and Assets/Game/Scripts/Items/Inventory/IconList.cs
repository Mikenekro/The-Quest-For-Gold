using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace schoolRPG.Items
{
    public class IconList : MonoBehaviour
    {
        public List<Sprite> spellIcons;
        public List<Sprite> inventoryIcons;

        public static IconList list;

        // Use this for initialization
        void Awake()
        {
            list = gameObject.GetComponent<IconList>();
        }
    }
}

