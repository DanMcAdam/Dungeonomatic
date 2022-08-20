using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ProceduralTileDungeonGenerator
{
    public class Tile : MonoBehaviour
    {
        [SerializeField]
        TileNode[] nodes = new TileNode[0];

        [SerializeField]
        List<TileNode> connectedNodes = new List<TileNode>();

        [SerializeField]
        Collider[] tileCollider;

        public TileNode[] Nodes { get => nodes; set => nodes = value; }
        public List<TileNode> ConnectedNodes { get => connectedNodes; set => connectedNodes = value; }
        public Collider[] Collider { get => tileCollider; set => tileCollider = value; }
        public bool isSpawnRoom = false;

        //add way to assign
        public Vector3 spawnPoint;

        LayerMask mask;

        void Start()
        {
            mask = LayerMask.GetMask("TileGenLayer");

            foreach (TileNode node in Nodes)
            {
                node.ParentTile = this;
            }
        }

        public bool CheckForTileCollision()
        {
            print("Checking for collisions");
            foreach (Collider col in tileCollider)
            {
                Collider[] hitColliders = Physics.OverlapBox(col.bounds.center, col.bounds.extents, transform.rotation, mask);

                for (int i = 0; i < hitColliders.Length; i++)
                {
                    if (hitColliders[i].gameObject.GetInstanceID() != col.gameObject.GetInstanceID())
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        void Update()
        {

        }

        public List<Tile> GetConnectedTiles()
        {
            List<Tile> connectedTiles = new List<Tile>();
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].Connected)
                {
                    connectedTiles[i] = nodes[i].connectedNode.ParentTile;
                }
            }
            return connectedTiles;
        }



        private void OnDrawGizmosSelected()
        {

            Gizmos.color = Color.yellow;
            //Gizmos.DrawCube(collider[0].bounds.center, collider[0].bounds.extents * 2);
            for (int i = 0; i < connectedNodes.Count; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(connectedNodes[i].transform.position, new Vector3(5, 5, 5));
            }
        }
    }

}