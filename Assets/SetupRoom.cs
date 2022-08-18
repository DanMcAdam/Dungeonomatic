using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralTileDungeonGenerator
{
    [ExecuteAlways]
    public class SetupRoom : MonoBehaviour
    {
        /*
         * In order to run this script, the following setup will need to be done: 
            - The script must be put on a GameObject with a Tile component.
            - The script depends on the following tags, they can be changed below but must exist in the project for this implementation: "Node", "Door", "Wall"
            - Somewhere in the children, a GameObject must have a TileNode component. It will be tagged with nodeTag as part of setup.
                |- It is recommended to have all node object be children of a master gameobject, for organization.
                |- Each Node object needs to have 2 GameObjects as children, with the doorTag and wallTag respectively. These will be enabled and disabled during generation.
                |- Node connection size must be manually assigned, and will limit the placement of nodes to other nodes with matching size. Default is 1.
                |- Node placement is important, as that's the point where the tiles will connect. It is recommended that it be at the exact edge of the floor, and right in the center of the door.  
            - There must be at least one Collider attached to the Tile's GameObject in order for it to generate properly.
                |- The colliders should not extend past the bounds of the object.
                |- If the colliders overlap a Node, that Node will not be able to connect to other nodes. This will generate an error message.
                |- Nodes have a bool called DEBUG_THIS_NODE_IS_INSIDE_COLLIDER_CHECK_POSITION that is exposed, if you get a message check for nodes with the bool set to true.
         * This script will assign the layer of the GameObject it is attached to to tileGenLayer. There must be a tileGenLayer for the tile generation to prevent rooms overlapping. 
           |-Do not change the layer to something else after running this script or assign it to any children objects
         */

        private Tile thisTile;


        //You can rename these strings as you see fit to match your existing tags/layers, but they should not be assigned to objects outside the context of the generator

        private string tileGenLayer = "TileGenLayer";
        private string nodeTag = "Node";
        private string doorTag = "Door";
        private string wallTag = "Wall";
        private bool errorFound = false;

        private void Start()
        {
            //Checks to make sure required tags and layers are assigned
            if (!AllTagsAndLayersExist()) return;
            //Finds Tile Component on attached Gameobject
            thisTile = GetComponent<Tile>();
            if (thisTile == null)
            {
                Debug.LogError("No Tile Component Found On Gameobject. This will abort setup, this script needs a tile component to operate.");
                errorFound = true;
                RemoveThisScript();
                return;
            }

            //Assigns to correct layer
            thisTile.gameObject.layer = LayerMask.NameToLayer(tileGenLayer);

            //Finds and assigns Colliders (must be on the Tile GameObject!)
            thisTile.Collider = GetComponents<Collider>();
            if (thisTile.Collider.Length == 0) { Debug.LogError("No colliders were found on the tile's gameobject, this will cause generation errors"); errorFound = true; }

            TileNode[] nodes = gameObject.GetComponentsInChildren<TileNode>();
            for (int i = 0; i < nodes.Length; i++)
            {
                TileNode currentNode = nodes[i];
                currentNode.transform.tag = nodeTag;

                currentNode.ParentTile = thisTile;
                foreach (Transform nodeChild in currentNode.transform)
                {
                    if (nodeChild.CompareTag(doorTag)) currentNode.Door = nodeChild.gameObject;
                    else if (nodeChild.CompareTag(wallTag)) currentNode.Wall = nodeChild.gameObject;
                }
                if (currentNode.Door == null) { Debug.LogError("No door assigned to this node, please check tags"); errorFound = true; }
                if (currentNode.Wall == null) { Debug.LogError("No wall assigned to this node, please check tags"); errorFound = true; }

                //this makes sure the node is not within the bounds of a collider
                foreach (Collider col in thisTile.Collider)
                {
                    if (col.bounds.Contains(currentNode.transform.position))
                    {
                        currentNode.DEBUG_THIS_NODE_IS_INSIDE_COLLIDER_CHECK_POSITION1 = true;
                        Debug.LogError("One of the nodes is within the bounds of a tile generation collider. DEBUG_THIS_NODE_IS_INSIDE_COLLIDER_CHECK_POSITION will be set to true on this node, please " +
                            "check collider bounds and node position. The node is at array position " + i);
                        errorFound = true;
                    }
                    else currentNode.DEBUG_THIS_NODE_IS_INSIDE_COLLIDER_CHECK_POSITION1 = false;
                }
            }
            if (nodes.Length == 0) { Debug.LogError("No nodes were found, please check that your intended room attachment points have a TileNode script"); errorFound = true; }

            thisTile.Nodes = nodes;

            RemoveThisScript();
        }

        private bool AllTagsAndLayersExist()
        {
            try
            {
                GameObject.FindGameObjectWithTag(nodeTag);
            }
            catch
            {
                Debug.LogError("An error was found, the following tag does not exist. Please add to the taglist for the tile generator to function. Required tag: " + nodeTag);
                errorFound = true;
                RemoveThisScript();
                return false;
            }
            try
            {
                GameObject.FindGameObjectWithTag(doorTag);
            }
            catch
            {
                Debug.LogError("An error was found, the following tag does not exist. Please add to the taglist for the tile generator to function. Required tag: " + doorTag);
                errorFound = true;
                RemoveThisScript();
                return false;
            }
            try
            {
                GameObject.FindGameObjectWithTag(wallTag);
            }
            catch
            {
                Debug.LogError("An error was found, the following tag does not exist. Please add to the taglist for the tile generator to function. Required tag: " + wallTag);
                errorFound = true;
                RemoveThisScript();
                return false;
            }

            var newLayer = LayerMask.NameToLayer(tileGenLayer);
            if (newLayer < 0)
            {
                Debug.LogError("An error was found, tile generation layer does not exist. Without this, the tile generator won't be able to work. Please create a layer with the name " +
                    tileGenLayer + " or reassign the tileGenLayer in the script to your desired layer.");
                errorFound = true;
                RemoveThisScript();
                return false;
            }
            return true;
        }

        private void RemoveThisScript()
        {
            if (errorFound) { Debug.LogError("The script's function has detected at least 1 error. Please check error logs and fix issues. This script will likely need to be run again"); }
            DestroyImmediate(this);
        }
    }
}