using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    [Header("RNG Seed")]
    public int manualSeed = -1;

    [Header("Layout Info")]
    public int numFloors = 1;
    public float floorHeight = 1f;

    //How many rooms are on each floor
    public List<int> floorRoomCount = new List<int>();

    //How many times to attempt to place a room
    public int numRoomPlaceAttempts = 10;

    //Max distance a room can be from another room
    public float maxDistanceBetweenRooms = 10f;

    //Min angle rooms can be rotated at
    public float availableAngle = 90f;
    //Helper for generating random rotations
    //  Gets set to 360 / availableAngle
    private int numAvailableRotations = 4;

    [Header("Available Rooms")]
    //TODO: Give more control over how rooms spawn
    public List<Room> roomPrefabs = new List<Room>();

    [Header("Available Hallways")]
    //TODO: Give more control over how hallways spawn
    public List<Room> hallwayPrefabs = new List<Room>();


    private void Start()
    {
        Init();

        GenerateMansion();
    }

    public void Init(int seed = -1)
    {
        if(seed == -1)
        {
            if(manualSeed == -1)
            {
                //Seed the generator with the time
                Random.InitState((int)DateTime.Now.Ticks);
            }
            else
            {
                //Seed the generator with the manual seed
                Random.InitState(manualSeed);
            }
        }
        else
        {
            //Seed the generator with the given seed
            Random.InitState(seed);
        }

        //Use round to avoid float inconsistencies
        numAvailableRotations = Mathf.RoundToInt(360f / availableAngle);
    }

    private void GenerateMansion()
    {
        for(int i = 0; i < numFloors; i++)
        {
            List<Room> generatedRooms = GenerateFloor(i);
            for (int j = 0; j < generatedRooms.Count; j++)
            {
                generatedRooms[j].Init();
            }
        }

        
    }

    private List<Room> GenerateFloor(int floor)
    {
        //Generate a list to store successfully placed rooms
        List<Room> placedRooms = new List<Room>();

        //Create a list to temporarily store the rooms on a floor. Can be populated with
        //  rooms from placedRooms if they are multi-story
        List<Room> roomsOnFloor = new List<Room>();

        //Try placing the rooms for the floor
        for(int i = 0; i < floorRoomCount[floor]; i++)
        {
            Room newRoom = Instantiate(roomPrefabs[Random.Range(0, roomPrefabs.Count)], Vector3.zero, Quaternion.identity);

            if(TryPlaceRoom(newRoom, floor, placedRooms, placedRooms))
            {
                placedRooms.Add(newRoom);
            }
        }

        //Return all placed room
        return placedRooms;
    }

    //Returns true if the room is placed, false otherwise
    private bool TryPlaceRoom(Room room, int floor, List<Room> placedRoomsOnFloor, List<Room> allPlacedRooms)
    {
        //1. Pick a random room from placed rooms (if list is empty, no random is called and pos is set to world center
        //2. Pick a random point in/on the unit circle
        //3. Pick a random rotation according to availableAngle and rotate the room according to that
        //4. Place a room at (selectedRoomPos + (PointInUnitCircle * maxDistanceBetweenRooms)) and with rotation as found in 3
        //5. Check collisions
        //6. If no collisions, return true, else go back to step 1
        //      If out of iterations and the room is not placed, return false

        //Calls to random
        //1. 1 or 0
        //2. 1 (maybe 2? idk how it works internally)
        //3. 1


        //Try placing the room
        for(int i = 0; i < numRoomPlaceAttempts; i++)
        {
            //Step 1
            Vector3 selectedRoomPos;
            if (placedRoomsOnFloor.Count == 0)
            {
                selectedRoomPos = new Vector3(0f, floor * floorHeight, 0f);
            }
            else
            {
                selectedRoomPos = placedRoomsOnFloor[Random.Range(0, placedRoomsOnFloor.Count)].transform.position;
            }


            //Step 2
            Vector2 offset = Random.insideUnitCircle * maxDistanceBetweenRooms;
            Vector3 newRoomPos = selectedRoomPos + new Vector3(offset.x, 0f, offset.y);

            //Step 3
            float rotation = availableAngle * Random.Range(0, numAvailableRotations);

            //Step 4
            room.transform.SetPositionAndRotation(newRoomPos, Quaternion.Euler(0f, rotation, 0f));

            //Step 5
            //TODO: Check ALL collisions

            

            //Some guy's solution to my problem
            /*
            To solve this I first use Physics.OverlapSphere to get the nearby colldiers.
                Then for a more precise calculation I use Physics.ComputePenetration.

            However, some of my assets utilize concave mesh colliders. As stated in the docs,
                a concave mesh collider cannot collide with another concave mesh collider. To fix this,
                I momentarily make the mesh collider convex before calling the overlap sphere
            */
            bool isColliding = false;
            for(int j = 0; j < allPlacedRooms.Count; j++)
            {
                MeshCollider x = new MeshCollider();
                x.convex = false;
                //TODO: Fix (blegh)
                if (room.GetComponent<Collider>().bounds.Intersects(new Bounds()))
                {

                }
            }

        }
        
        //If it reaches here, placement has failed
        return false;
    }

    private void ConnectRooms(List<Room> rooms)
    {

    }
}
