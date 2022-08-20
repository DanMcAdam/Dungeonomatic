using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using UnityEditor.AI;
namespace ProceduralTileDungeonGenerator
{
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField]
        int baseNumberOfRooms = 4, roomNumberVariation = 2;

        [SerializeField]
        bool regenerate = false;

        [TextArea(3, 10)]
        public string debugMessage = "This is a debug message for the  cool ";

        public bool keepGoing = false, finishGen = false;

        public int BaseNumberOfRooms { get => baseNumberOfRooms; set => baseNumberOfRooms = value; }
        public int RoomNumberVariation { get => roomNumberVariation; set => roomNumberVariation = value; }

        public GameObject[] prefab;
        public GameObject spawnRoom;

        public GameObject character;
        private Tile room = null;
        private Tile newRoom = null;
        int roomNumberChoice;
        int numberOfRoomsCreated = 0;
        int targetNumberOfRooms;

        float timer = 0;


        TileNode alignNode;
        TileNode targetNode;


        private Dictionary<TileNode, Vector3> nodeMap = new Dictionary<TileNode, Vector3>();
        private Dictionary<(TileNode, TileNode), bool> attemptedPairings = new Dictionary<(TileNode, TileNode), bool>();
        private List<Tile> allTiles = new List<Tile>();

        public NavMeshBuildSettings buildSettings;

        void Start()
        {

            BeginGeneration();
        }



        private void BeginGeneration()
        {
            character.SetActive(false);
            Physics.autoSimulation = false;
            debugMessage = "Beginning generation, physics stopped";
            StartCoroutine(StartGen());
        }

        private void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
        }

        private IEnumerator StartGen()
        {
            numberOfRoomsCreated = 1;
            targetNumberOfRooms = Random.Range(baseNumberOfRooms - roomNumberVariation, baseNumberOfRooms + roomNumberVariation);

            room = Instantiate(spawnRoom).GetComponent<Tile>();

            foreach (TileNode node in room.Nodes)
            {
                nodeMap.Add(node, node.transform.position);
            }

            allTiles.Add(room);
            room.transform.SetParent(transform);

            bool finished = false;
            while (!finished)
            {
                yield return PlaceTile();
                if (numberOfRoomsCreated == targetNumberOfRooms)
                {
                    finished = true;

                    foreach (KeyValuePair<TileNode, Vector3> node in nodeMap)
                    {
                        if (node.Key.Connected == false)
                        {
                            foreach (KeyValuePair<TileNode, Vector3> innerNode in nodeMap)
                            {
                                if (node.Key != innerNode.Key && Vector3.Distance(node.Value, innerNode.Value) < .05f && node.Key.ConnectionTileSize == innerNode.Key.ConnectionTileSize)
                                {
                                    node.Key.ConnectToAnotherTile(true, innerNode.Key);
                                    innerNode.Key.ConnectToAnotherTile(true, node.Key);
                                }
                            }
                        }
                    }

                }
            }
            Physics.autoSimulation = true;
            /*
            buildSettings = new NavMeshBuildSettings() { agentRadius = 1f, agentHeight = 2, agentSlope = 45, agentClimb = .75f, agentTypeID = 0 };
            UnityEditor.AI.NavMeshBuilder.ClearAllNavMeshes();
            List<NavMeshBuildSource> navSources = new List<NavMeshBuildSource>();
            MeshFilter[] meshes = GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter mesh in meshes)
            {
                navSources.Add(new NavMeshBuildSource() { shape = NavMeshBuildSourceShape.Mesh, transform = mesh.transform.localToWorldMatrix, sourceObject = mesh.mesh });
            }
            //UnityEngine.AI.NavMeshBuilder.CollectSources(new Bounds(Vector3.zero, new Vector3(99999, 9999, 9999)), LayerMask.NameToLayer("Default"), NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(){ new NavMeshBuildMarkup() { area = 0, ignoreFromBuild = false, overrideArea = false, root = this.transform } }, navSources);
            UnityEngine.AI.NavMeshBuilder.BuildNavMeshData(buildSettings, navSources, new Bounds(Vector3.zero, new Vector3(99999, 9999, 9999)), Vector3.zero, Quaternion.identity);
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
            */
            character.transform.position = allTiles[0].spawnPoint;
            character.SetActive(true);
            character.GetComponentInChildren<NavMeshAgentTest>().goal.transform.position = allTiles[allTiles.Count - 1].Nodes[0].transform.position;
            yield break;
        }

        private void CreateRoom()
        {
            StartCoroutine(PlaceTile());
            //int roomNumberChoice = PlaceTile(newRoom);
        }

        private IEnumerator PlaceTile()
        {
            int roomToInstantiate;
            yield return GenerateNewRoom();

            Quaternion rot;

            Vector3 alignOffset;

            GameObject targetRoom = room.gameObject;
            int roomNumberChoice;

            yield return TryPlaceTile();

            IEnumerator wait()
            {
                yield return new WaitUntil(() => keepGoing == true);
                keepGoing = finishGen;
            }

            IEnumerator GenerateNewRoom()
            {
                roomToInstantiate = Random.Range(0, prefab.Length);
                newRoom = Instantiate(prefab[roomToInstantiate]).GetComponent<Tile>();
                newRoom.transform.SetParent(transform);

                yield return null;
            }

            IEnumerator TryPlaceTile()
            {
                attemptedPairings.Clear();

                foreach (TileNode node in room.Nodes)
                {
                    foreach (TileNode innerNode in newRoom.Nodes)
                    {
                        print("loop of adding node");
                        attemptedPairings.Add((innerNode, node), false);
                    }
                }
                yield return choosePointAndMove();

                Physics.Simulate(timer);
                while (newRoom.CheckForTileCollision())
                {
                    debugMessage = "Collision detected, retrying";
                    attemptedPairings[(alignNode, targetNode)] = true;
                    if (!attemptedPairings.ContainsValue(false))
                    {
                        Destroy(newRoom.gameObject);
                        Physics.Simulate(timer);
                        print("Destroyed new room, attempting again");
                        attemptedPairings.Clear();
                        yield return GenerateNewRoom();
                        foreach (TileNode node in room.Nodes)
                        {
                            foreach (TileNode innerNode in newRoom.Nodes)
                            {
                                attemptedPairings.Add((innerNode, node), false);
                            }
                        }
                    }
                    yield return choosePointAndMove();
                    Physics.Simulate(timer);
                }

                allTiles.Add(newRoom);
                room = newRoom;

                foreach (TileNode node in room.Nodes)
                {
                    nodeMap.Add(node, node.transform.position);
                }

                numberOfRoomsCreated++;
                yield break;
            }

            IEnumerator choosePointAndMove()
            {
                do
                {
                    roomNumberChoice = Random.Range(0, room.Nodes.Length);

                    targetNode = room.Nodes[roomNumberChoice];
                    alignNode = newRoom.Nodes[Random.Range(0, newRoom.Nodes.Length)];


                }
                while (attemptedPairings[(alignNode, targetNode)] == true);
                rot = Quaternion.FromToRotation(alignNode.transform.forward, -targetNode.transform.forward);
                rot.x = 0;
                rot.z = 0;

                debugMessage = "about to rotate";
                yield return wait();

                newRoom.transform.rotation = newRoom.transform.rotation * rot;

                alignOffset = newRoom.transform.position - alignNode.transform.position;

                debugMessage = "just rotated, about to place";
                yield return wait();

                newRoom.transform.position = targetNode.transform.position + alignOffset;
            }
        }

        private void OnGUI()
        {

        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(alignNode != null ? alignNode.transform.position : Vector3.zero, new Vector3(2, 2, 2));
            Gizmos.color = Color.green;
            Gizmos.DrawCube(targetNode != null ? targetNode.transform.position : Vector3.zero, new Vector3(1, 1, 1));
            for (int i = 0; i < allTiles.Count; i++)
            {
                Handles.Label(allTiles[i].transform.position, "Room " + i);
            }
        }

        private void Update()
        {

            if (regenerate)
            {
                foreach (Transform child in this.transform)
                {
                    if (regenerate)
                    {
                        GameObject.Destroy(child.gameObject);
                    }
                }
                character.SetActive(false);
                nodeMap.Clear();
                allTiles.Clear();
                Physics.autoSimulation = false;
                BeginGeneration();
                regenerate = false;
            }
        }

    }
}