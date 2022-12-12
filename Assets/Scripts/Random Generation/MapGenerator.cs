using JetEngine;
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

    //How many grid tiles there are in the x and y directions
    public static int GridXDimension = 1000;
    public static int GridYDimension = 1000;

    //How many rooms are on each floor
    public List<int> floorRoomCount = new List<int>();

    //How many times to attempt to place a room
    public int numRoomPlaceAttempts = 10;

    //Max distance a door can be from another door
    public float minDistanceBetweenRooms = 0.00001f;
    //Max distance a door can be from another door
    public float maxDistanceBetweenRooms = 0.0001f;

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
        int usedSeed = -1;
        if(seed == -1)
        {
            if(manualSeed == -1)
            {
                //Seed the generator with the time
                usedSeed = (int)DateTime.Now.Ticks;
            }
            else
            {
                //Seed the generator with the manual seed
                usedSeed = manualSeed;
            }
        }
        else
        {
            //Seed the generator with the given seed
            usedSeed = seed;
        }

        Debug.Log($"Seed: {usedSeed}");
        Random.InitState(usedSeed);

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
        //  rooms from other floors if they are multi-story
        List<Room> roomsOnFloor = new List<Room>();

        //Try placing the rooms for the floor
        for(int i = 0; i < floorRoomCount[floor]; i++)
        {
            Room newRoom = Instantiate(roomPrefabs[Random.Range(0, roomPrefabs.Count)], Vector3.zero, Quaternion.identity);

            if(TryPlaceRoom(newRoom, floor, ref placedRooms, ref placedRooms))
            {
                placedRooms.Add(newRoom);
            }
            else
            {
                //Debug.Log("Failed to place room!");
                Destroy(newRoom.gameObject);
            }

        }

        Debug.Log($"Successfully placed {{{placedRooms.Count}}} rooms!");
        //Return all placed room
        return placedRooms;
    }

    //Returns true if the room is placed, false otherwise
    private bool TryPlaceRoom(
        Room newRoom,
        int floor,
        ref List<Room> placedRoomsOnFloor,
        ref List<Room> allPlacedRooms)
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

        Floor curRoomFloor = newRoom.mapFloors[0];

        if (placedRoomsOnFloor.Count == 0)
        {
            //Place the first room wherever
            newRoom.transform.position = new Vector3(0f, 0f, 0f);

            return true;
        }


        //Try placing the room
        for (int i = 0; i < numRoomPlaceAttempts; i++)
        {
            //TODO: Do better
            Room selectedRoom = placedRoomsOnFloor[Random.Range(0, placedRoomsOnFloor.Count)];
            if(selectedRoom.mapFloors[0].doorways.Count == 0)
            {
                //No doors on this room, try again!
                continue;
            }
            Doorway selectedDoorway = selectedRoom.mapFloors[0].doorways[Random.Range(0, selectedRoom.mapFloors[0].doorways.Count)];

            Doorway newDoorway = newRoom.mapFloors[0].doorways[Random.Range(0, newRoom.mapFloors[0].doorways.Count)];

            //Ensure angles are compatible
            float angleDiff = Mathf.Abs(selectedDoorway.angle - newDoorway.angle);
            if(angleDiff < 180f - 0.0000001f || angleDiff > 180f + 0.0000001f)
            {
                //Attempt failed, try again!
                continue;
            }

            //Find tile pos of selected doorway. We select the end point to make it easier to set our new room's start point,
            //  though as long as the math works out, it shouldn't matter too much
            Vector3 selectedDoorwayPos = selectedDoorway.endPoint.ToVec3XZ() + selectedRoom.transform.position;

            //Get the direction to push our new room in
            Vector3 offsetDir = new Vector3(Mathf.Cos(selectedDoorway.angle * Mathf.Rad2Deg), 0f, Mathf.Sin(selectedDoorway.angle * Mathf.Rad2Deg));
            //TODO: make it actually offset
            float offsetMultiplier = Random.Range(0.0000001f, maxDistanceBetweenRooms);

            //Offset a random amount
            offsetDir *= offsetMultiplier;

            Vector3 newRoomPos = selectedDoorwayPos - newDoorway.startPoint.ToVec3XZ() + offsetDir;

            //TODO: Check Collisions
            if (CheckCollisions(allPlacedRooms, newRoom, newRoomPos))
            {
                //Place room
                newRoom.transform.position = newRoomPos;

                //Remove closed doors
                selectedRoom.mapFloors[0].doorways.Remove(selectedDoorway);
                newRoom.mapFloors[0].doorways.Remove(newDoorway);

                //Set instance vars

                return true;
            }
            //else try again
        }
        
        //If it reaches here, placement has failed
        return false;
    }

    private bool CheckCollisions(List<Room> allPlacedRooms, Room newRoom, Vector3 newRoomPos)
    {
        //TODO: use bounding boxes to optimize placement
        List<Vector3> newCollisionPoses = new List<Vector3>();
        for (int i = 0; i < newRoom.mapFloors[0].roomLines.Count; i++)
        {
            newCollisionPoses.AddRange(newRoom.mapFloors[0].roomLines[i].GetCollisionPoints());
        }

        List<Vector3> simplifiedNewCollisionPoses = new List<Vector3>();
        LineUtility.Simplify(newCollisionPoses, 0.000001f, simplifiedNewCollisionPoses);

        Vector2 placedInteriorPoint = newRoom.mapFloors[0].interiorPoint.GetXZ();

        foreach (Room placedRoom in allPlacedRooms)
        {
            List<Vector3> placedCollisionPoses = new List<Vector3>();
            for (int i = 0; i < placedRoom.mapFloors[0].roomLines.Count; i++)
            {
                placedCollisionPoses.AddRange(placedRoom.mapFloors[0].roomLines[i].GetCollisionPoints());
            }

            List<Vector3> simplifiedPlacedCollisionPoses = new List<Vector3>();
            LineUtility.Simplify(placedCollisionPoses, 0.000001f, simplifiedPlacedCollisionPoses);

            int newPointInsidePlacedCount = 0;
            int placedPointInsideNewCount = 0;

            Vector2 newInteriorPoint = placedRoom.mapFloors[0].interiorPoint.GetXZ();


            //Check if any lines intersect
            for (int i = 0; i < simplifiedPlacedCollisionPoses.Count - 1; i++)
            {
                for (int j = 0; j < simplifiedNewCollisionPoses.Count - 1; j++)
                {
                    if(LineCollisionUtils.LineIntersectsLine((placedRoom.transform.position + simplifiedPlacedCollisionPoses[i]).GetXZ(),
                        (placedRoom.transform.position + simplifiedPlacedCollisionPoses[i + 1]).GetXZ(),
                        (newRoomPos + simplifiedNewCollisionPoses[j]).GetXZ(),
                        (newRoomPos + simplifiedNewCollisionPoses[j + 1]).GetXZ(),
                        true))
                    {
                        return false;
                    }

                    if (LineCollisionUtils.LineIntersectsLine(placedRoom.transform.position.GetXZ() + placedInteriorPoint,
                        placedRoom.transform.position.GetXZ() + placedInteriorPoint + new Vector2(100f, 0f),
                        (newRoomPos + simplifiedNewCollisionPoses[j]).GetXZ(),
                        (newRoomPos + simplifiedNewCollisionPoses[j + 1]).GetXZ(),
                        true))
                    {
                        placedPointInsideNewCount++;
                    }
                }

                if (LineCollisionUtils.LineIntersectsLine(newRoomPos.GetXZ() + newInteriorPoint,
                        newRoomPos.GetXZ() + newInteriorPoint + new Vector2(100f, 0f),
                        (placedRoom.transform.position + simplifiedPlacedCollisionPoses[i]).GetXZ(),
                        (placedRoom.transform.position + simplifiedPlacedCollisionPoses[i + 1]).GetXZ(),
                        true))
                {
                    newPointInsidePlacedCount++;
                }
            }


            //Check if a point of each is inside the other
            if(newPointInsidePlacedCount % 2 == 1)
            {
                return false;
            }
            
            if(placedPointInsideNewCount % 2 == 1)
            {
                return false;
            }
        }

        return true;
    }

    private void ConnectRooms(List<Room> rooms)
    {

    }
}
