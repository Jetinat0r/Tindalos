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

    //Angle forgiveness
    public float angleEpsilon = 0.001f;
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
        //Generate a list to store successfully placed rooms
        List<Room> placedRooms = new List<Room>();

        for (int i = 0; i < numFloors; i++)
        {
            GenerateFloor(i, ref placedRooms);
        }

        for (int j = 0; j < placedRooms.Count; j++)
        {
            placedRooms[j].Init();
        }
    }

    private void GenerateFloor(int floor, ref List<Room> placedRooms)
    {
        int initialCount = placedRooms.Count;

        //Create a list to temporarily store the rooms on a floor. Can be populated with
        //  rooms from other floors if they are multi-story
        List<Room> roomsOnFloor = new List<Room>();

        foreach (Room r in placedRooms)
        {
            int localFloor = floor - r.assignedFloor;
            if (localFloor < r.numFloors && localFloor >= 0)
            {
                roomsOnFloor.Add(r);
            }
        }

        //Try placing the rooms for the floor
        for (int i = 0; i < floorRoomCount[floor]; i++)
        {
            Room newRoom = Instantiate(roomPrefabs[Random.Range(0, roomPrefabs.Count)], Vector3.zero, Quaternion.identity);
            newRoom.assignedFloor = floor;

            if(TryPlaceRoom(newRoom, floor, ref roomsOnFloor, ref placedRooms))
            {
                placedRooms.Add(newRoom);
                roomsOnFloor.Add(newRoom);
            }
            else
            {
                //Debug.Log("Failed to place room!");
                Destroy(newRoom.gameObject);
            }
        }

        Debug.Log($"Successfully placed {{{placedRooms.Count - initialCount}}} rooms!");
    }

    //Returns true if the room is placed, false otherwise
    private bool TryPlaceRoom(
        Room newRoom,
        int floor,
        ref List<Room> placedRoomsOnFloor,
        ref List<Room> allPlacedRooms)
    {
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
            if(selectedRoom.mapFloors[floor - selectedRoom.assignedFloor].doorways.Count == 0)
            {
                //No doors on this room, try again!
                continue;
            }

            //floor - selectedRoom.assignedFloor gets us the floor on level that we're looking at.
            //This room is guaranteed to contain this floor
            Doorway selectedDoorway = selectedRoom.mapFloors[floor - selectedRoom.assignedFloor].doorways[Random.Range(0, selectedRoom.mapFloors[floor - selectedRoom.assignedFloor].doorways.Count)];

            //We grab the bottom floor of the new room
            Doorway newDoorway = newRoom.mapFloors[0].doorways[Random.Range(0, newRoom.mapFloors[0].doorways.Count)];

            //Ensure angles are compatible
            float angleDiff = Mathf.Abs(selectedDoorway.angle - newDoorway.angle);
            if(angleDiff < 180f - angleEpsilon || angleDiff > 180f + angleEpsilon)
            {
                //Attempt failed, try again!
                continue;
            }

            //Find tile pos of selected doorway. We select the end point to make it easier to set our new room's start point,
            //  though as long as the math works out, it shouldn't matter too much
            Vector3 selectedDoorwayPos = selectedDoorway.endPoint.ToVec3XZ() + selectedRoom.transform.position;

            //Get the direction to push our new room in
            Vector3 offsetDir = new Vector3(Mathf.Cos(selectedDoorway.angle * Mathf.Rad2Deg), 0f, Mathf.Sin(selectedDoorway.angle * Mathf.Rad2Deg));
            //Little bit of offset perpendicular to the doorway for fun and maybe profit?
            float offsetMultiplier = Random.Range(minDistanceBetweenRooms, maxDistanceBetweenRooms);

            //Offset a random amount
            offsetDir *= offsetMultiplier;

            Vector3 newRoomPos = selectedDoorwayPos - newDoorway.startPoint.ToVec3XZ() + offsetDir;

            //TODO: Check Collisions
            if (CheckCollisions(allPlacedRooms, floor, newRoom, newRoomPos))
            {
                //Place room
                newRoom.transform.position = new Vector3(newRoomPos.x, Floor.FloorHeight * floor, newRoomPos.z);

                //Remove closed doors
                selectedRoom.mapFloors[floor - selectedRoom.assignedFloor].doorways.Remove(selectedDoorway);
                newRoom.mapFloors[0].doorways.Remove(newDoorway);

                //Set instance vars

                return true;
            }
            //else try again
        }
        
        //If it reaches here, placement has failed
        return false;
    }

    //Bottom floor is the bottom floor of the new room, in global space
    private bool CheckCollisions(List<Room> allPlacedRooms, int bottomFloor, Room newRoom, Vector3 newRoomPos)
    {
        for(int curFloor = 0; curFloor < newRoom.numFloors; curFloor++)
        {
            //TODO: use bounding boxes to optimize placement
            List<Vector3> newCollisionPoses = new List<Vector3>();
            for (int i = 0; i < newRoom.mapFloors[curFloor].roomLines.Count; i++)
            {
                newCollisionPoses.AddRange(newRoom.mapFloors[curFloor].roomLines[i].GetCollisionPoints());
            }

            for (int m = 1; m < newCollisionPoses.Count; m++)
            {
                if (newCollisionPoses[m - 1] == newCollisionPoses[m])
                {
                    newCollisionPoses.RemoveAt(m);
                    m--;
                }
            }

            Vector2 placedInteriorPoint = newRoom.mapFloors[curFloor].interiorPoint.GetXZ();

            //The floor to be checked in global space
            int checkingFloor = bottomFloor + curFloor;
            foreach (Room placedRoom in allPlacedRooms)
            {
                int localFloor = checkingFloor - placedRoom.assignedFloor;
                if (localFloor >= placedRoom.numFloors)
                {
                    //Falls outside range, don't bother checking
                    continue;
                }

                List<Vector3> placedCollisionPoses = new List<Vector3>();
                for (int i = 0; i < placedRoom.mapFloors[localFloor].roomLines.Count; i++)
                {
                    placedCollisionPoses.AddRange(placedRoom.mapFloors[localFloor].roomLines[i].GetCollisionPoints());
                }

                for (int m = 1; m < placedCollisionPoses.Count; m++)
                {
                    if (placedCollisionPoses[m - 1] == placedCollisionPoses[m])
                    {
                        placedCollisionPoses.RemoveAt(m);
                        m--;
                    }
                }

                int newPointInsidePlacedCount = 0;
                int placedPointInsideNewCount = 0;

                Vector2 newInteriorPoint = placedRoom.mapFloors[localFloor].interiorPoint.GetXZ();


                //Check if any lines intersect
                for (int i = 0; i < placedCollisionPoses.Count - 1; i++)
                {
                    for (int j = 0; j < newCollisionPoses.Count - 1; j++)
                    {
                        if (LineCollisionUtils.LineIntersectsLine((placedRoom.transform.position + placedCollisionPoses[i]).GetXZ(),
                            (placedRoom.transform.position + placedCollisionPoses[i + 1]).GetXZ(),
                            (newRoomPos + newCollisionPoses[j]).GetXZ(),
                            (newRoomPos + newCollisionPoses[j + 1]).GetXZ(),
                            true))
                        {
                            return false;
                        }

                        if (LineCollisionUtils.LineIntersectsLine(placedRoom.transform.position.GetXZ() + placedInteriorPoint,
                            placedRoom.transform.position.GetXZ() + placedInteriorPoint + new Vector2(100f, 0f),
                            (newRoomPos + newCollisionPoses[j]).GetXZ(),
                            (newRoomPos + newCollisionPoses[j + 1]).GetXZ(),
                            true))
                        {
                            placedPointInsideNewCount++;
                        }
                    }

                    if (LineCollisionUtils.LineIntersectsLine(newRoomPos.GetXZ() + newInteriorPoint,
                            newRoomPos.GetXZ() + newInteriorPoint + new Vector2(100f, 0f),
                            (placedRoom.transform.position + placedCollisionPoses[i]).GetXZ(),
                            (placedRoom.transform.position + placedCollisionPoses[i + 1]).GetXZ(),
                            true))
                    {
                        newPointInsidePlacedCount++;
                    }
                }


                //Check if a point of each is inside the other
                if (newPointInsidePlacedCount % 2 == 1)
                {
                    return false;
                }

                if (placedPointInsideNewCount % 2 == 1)
                {
                    return false;
                }
            }
        }
        

        return true;
    }

    private void ConnectLeftoverRooms(List<Room> rooms)
    {

    }
}
