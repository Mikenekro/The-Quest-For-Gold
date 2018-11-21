using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace schoolRPG
{
    /// <summary>
    /// The TargetNode is attached to all nodes for a Character that determines if it's currently in use
    /// </summary>
    public class TargetNode : MonoBehaviour
    {
        /// <summary>
        /// Is this node in use?
        /// </summary>
        public bool InUse { get; set; }

        public int Node { get; set; }

        public Guid UsingID { get; set; }

        public void Awake()
        {
            InUse = false;
        }

        /// <summary>
        /// Start Using this Node. If the Node is already in use, returns false
        /// </summary>
        /// <param name="node"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool UseNode(bool player, int node, Guid id)
        {
            bool used = false;

            if (!InUse)
            {
                used = true;
                InUse = true;
                Node = node;
                UsingID = id;
                if (player)
                    PlayerController.NodeUse[node] = true;
                else
                    GetComponentInParent<NpcController>().NodeUse[node] = true;
            }
            
            return used;
        }

        /// <summary>
        /// Release the currently used Node
        /// </summary>
        public void ReleaseNode(bool player)
        {
            if (InUse)
            {
                InUse = false;
                UsingID = new Guid();
                if (player)
                    PlayerController.NodeUse[Node] = false;
                else
                    GetComponentInParent<NpcController>().NodeUse[Node] = false;
            }
        }
    }
}

