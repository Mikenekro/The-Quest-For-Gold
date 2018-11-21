using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace schoolRPG.Items
{
    /// <summary>
    /// The Container Class will be placed onto Containers, such as a chest, to hold their inventory
    /// </summary>
    [System.Serializable]
    public class Container : MonoBehaviour
    {
        private Inventory _inv;

        /// <summary>
        /// The Container ID will be used to identify this container instance from the PlayerData's Container List
        /// </summary>
        public string containerID;

        public string containerName;

        public Inventory Inv { get { return _inv; } set { _inv = value; } }
        
        void Start()
        {
            _inv = new Inventory();

            if (WorldController.Data.IsLoaded)
            {
                // UNCOMMENT WHEN WE CREATE A MONOBEHAVIORLESS CONTAINER CLASS
                //for (int i = 0; i < WorldController.Data.Cont.Count; ++i)
                //{
                //    // Set the loaded Data for this Container
                //    if (WorldController.Data.Cont[i].containerID == containerID)
                //        c = WorldController.Data.Cont[i];
                //}
            }
        }
    }
}

