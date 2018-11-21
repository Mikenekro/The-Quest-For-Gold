using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace schoolRPG
{
    public enum Locations
    {
        JARDAAN, PINEFOREST, DAWNSPIRE, DS_TOWNSQUARE, DRETH, DR_TOWNSQUARE, PONTAR, LAVAFIELDS, NOVIGRAD
    }

    /// <summary>
    /// The Location Script will be places on gameobjects whos location will display once the player reaches this area
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Location : MonoBehaviour
    {
        [SerializeField, Tooltip("Is this a Jump Point? (Jump Points will not use the Exit name)")]
        private bool isJump;

        [SerializeField, Tooltip("Enter the name of this Location")]
        private string locationName;

        [SerializeField, Tooltip("Enter the name of the Location surrounding this Location")]
        private string leaveName;

        [SerializeField, Tooltip("What Location is this?")]
        private Locations locationType;

        [SerializeField, Tooltip("What Location will we enter when we leave this Location?")]
        private Locations leaveLocation;

        [SerializeField, Tooltip("Add the Parent that has a Quest Marker if needed")]
        private Quests.QuestMarker parentMarker;

        /// <summary>
        /// The name of this Location
        /// </summary>
        public string Name { get { return locationName; } }

        public void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.name == "Player")
            {
                if ((isJump && WorldController.Data.Loc != locationType) || WorldController.Data.Loc != locationType)
                {
                    WorldController.loc = locationType;
                    WorldController.Data.Loc = locationType;
                    PlayerController.DisplayTextToScreen("Entered " + locationName, false);
                    Sound.SoundController.CheckMusicType();

                    if (parentMarker != null)
                        parentMarker.OnTriggerEnter2D(col);
                }
                else if (WorldController.Data.Loc != leaveLocation)
                {
                    WorldController.loc = leaveLocation;
                    WorldController.Data.Loc = leaveLocation;
                    
                    PlayerController.DisplayTextToScreen("Entered " + leaveName, false);

                    Sound.SoundController.CheckMusicType();
                }
            }
        }
    }
}

