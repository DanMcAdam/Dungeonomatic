using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

namespace ProceduralTileDungeonGenerator
{
    public class TileNode : MonoBehaviour
    {
        [SerializeField]
        bool connected = false;
        [SerializeField]
        GameObject door;
        [SerializeField]
        GameObject wall;
        [SerializeField]
        int connectionTileSize = 1;
        [SerializeField]
        Tile parentTile;
        [SerializeField]
        NavMeshLink link;
        [SerializeField]
        bool DEBUG_THIS_NODE_IS_INSIDE_COLLIDER_CHECK_POSITION = false;

        public TileNode connectedNode;
        public bool Connected { get => connected; }
        public int ConnectionTileSize { get => connectionTileSize; }
        public Tile ParentTile { get => parentTile; set => parentTile = value; }
        public GameObject Door { get => door; set => door = value; }
        public GameObject Wall { get => wall; set => wall = value; }
        public bool DEBUG_THIS_NODE_IS_INSIDE_COLLIDER_CHECK_POSITION1 { set => DEBUG_THIS_NODE_IS_INSIDE_COLLIDER_CHECK_POSITION = value; }

        void Start()
        {
            if (DEBUG_THIS_NODE_IS_INSIDE_COLLIDER_CHECK_POSITION) Debug.LogError("There is a node that is within the bounds of its room's collider, this will cause tile generation errors. " +
                "This error is thrown by a bool on the node in the prefab. If this has been fixed, rerun SetupRoom script on prefab. This node belongs to Object ID = " + parentTile.gameObject.GetInstanceID());
            ConnectToAnotherTile(false, null);
            link.enabled = false;
        }


        void Update()
        {

        }

        public void ConnectToAnotherTile(bool value, TileNode otherNode)
        {
            connected = value;
            connectedNode = otherNode;
            door.SetActive(connected);
            wall.SetActive(!connected);
            link.enabled = connected;
            
            if (value == true) link.UpdateLink();
        }


    }
}