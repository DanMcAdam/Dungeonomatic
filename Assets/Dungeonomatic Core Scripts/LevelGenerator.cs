using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using UnityEditor.AI;
using Cysharp.Threading.Tasks;

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
            StartGen();
        }

        private void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
        }

        private async UniTaskVoid StartGen()
        {
            Tile oldTile;
            Tile newTile;
            numberOfRoomsCreated = 1;
            targetNumberOfRooms = Random.Range(baseNumberOfRooms - roomNumberVariation, baseNumberOfRooms + roomNumberVariation);

            oldTile = Instantiate(spawnRoom).GetComponent<Tile>();

            foreach (TileNode node in oldTile.Nodes)
            {
                nodeMap.Add(node, node.transform.position);
            }

            allTiles.Add(oldTile);
            oldTile.transform.SetParent(transform);

            bool finished = false;
            while (!finished)
            {
                newTile = ChooseTile();
                newTile = await PlaceTile(newTile, oldTile);
                oldTile = newTile;
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

            character.transform.position = allTiles[0].spawnPoint;
            character.SetActive(true);
            character.GetComponentInChildren<NavMeshAgentTest>().goal.transform.position = allTiles[allTiles.Count - 1].Nodes[0].transform.position;
            await UniTask.Yield();
        }

        private Tile ChooseTile()
        {
            int roomToInstantiate = Random.Range(0, prefab.Length);
            Tile newTile = Instantiate(prefab[roomToInstantiate]).GetComponent<Tile>();
            newTile.transform.SetParent(transform);
            return newTile;
        }

        private async UniTask<Tile> PlaceTile(Tile newRoom, Tile room)
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
            await ChoosePointAndMove(newRoom, room);

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
                    newRoom = ChooseTile();
                    foreach (TileNode node in room.Nodes)
                    {
                        foreach (TileNode innerNode in newRoom.Nodes)
                        {
                            attemptedPairings.Add((innerNode, node), false);
                        }
                    }
                }
                await ChoosePointAndMove(newRoom, room);
                Physics.Simulate(timer);
            }

            allTiles.Add(newRoom);

            foreach (TileNode node in newRoom.Nodes)
            {
                nodeMap.Add(node, node.transform.position);
            }

            numberOfRoomsCreated++;
            return newRoom;
        }

        private async UniTask ChoosePointAndMove(Tile newRoom, Tile room)
        {
            Quaternion rot;
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
            await Wait();

            newRoom.transform.rotation = newRoom.transform.rotation * rot;

            Vector3 alignOffset = newRoom.transform.position - alignNode.transform.position;

            debugMessage = "just rotated, about to place";
            await Wait();

            newRoom.transform.position = targetNode.transform.position + alignOffset;
            await UniTask.Yield();
        }

        private async UniTask Wait()
        {
            keepGoing = finishGen;
            await UniTask.WaitUntil(() => keepGoing == true);
            await UniTask.Yield();
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