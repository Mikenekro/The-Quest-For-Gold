using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace schoolRPG
{
    /// <summary>
    /// LoadPersistant will let the main scene know if this is a new game or if we should load a saved game
    /// </summary>
    public class LoadPersistant : MonoBehaviour
    {
        /// <summary>
        /// Is the game loaded yet?
        /// </summary>
        public static bool loaded { get; set; }

        /// <summary>
        /// The name of the game we will be loading
        /// </summary>
        public static string LoadName { get; set; }

        /// <summary>
        /// Is this a new game that we are starting or are we loading a game?
        /// </summary>
        public static bool NewGame { get; set; }

        private void Awake()
        {
            LoadPersistant[] objs = GameObject.FindObjectsOfType<LoadPersistant>();

            loaded = true;
            // Make sure there is only 1 LoadPersistant object in the game at all times
            if (objs.Length == 1)
                DontDestroyOnLoad(this);
            else
            {
                if (Application.isEditor)
                    DestroyImmediate(gameObject);
                else
                    Destroy(gameObject);
            }
                
            
        }
    }
}

