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
        //Initialize a grid for the floor
        //TODO: Move out so that every floor has access to every other floor
        TileState[][] floorGrid = new TileState[GridXDimension][];
        for (int i = 0; i < floorGrid.Length; i++)
        {
            floorGrid[i] = new TileState[GridYDimension];
            Array.Fill(floorGrid[i], TileState.Empty);
        }

        List<ConflictingTileData> partialTiles = new List<ConflictingTileData>();

        //Generate a list to store successfully placed rooms
        List<Room> placedRooms = new List<Room>();

        //Create a list to temporarily store the rooms on a floor. Can be populated with
        //  rooms from other floors if they are multi-story
        List<Room> roomsOnFloor = new List<Room>();

        //Try placing the rooms for the floor
        for(int i = 0; i < floorRoomCount[floor]; i++)
        {
            Room newRoom = Instantiate(roomPrefabs[Random.Range(0, roomPrefabs.Count)], Vector3.zero, Quaternion.identity);

            if(TryPlaceRoom(newRoom, floor, ref floorGrid, ref partialTiles, ref placedRooms, ref placedRooms))
            {
                placedRooms.Add(newRoom);
            }
            else
            {
                Debug.Log("Failed to place room!");
                Destroy(newRoom.gameObject);
            }

        }

        Debug.Log($"Successfully placed {{{placedRooms.Count}}} rooms!");
        //Return all placed room
        return placedRooms;
    }

    //Returns true if the room is placed, false otherwise
    private bool TryPlaceRoom(
        Room room,
        int floor,
        ref TileState[][] floorGrid,
        ref List<ConflictingTileData> partialTiles,
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

        Floor curRoomFloor = room.mapFloors[0];
        int topTile = curRoomFloor.topMostTile;
        int bottomTile = curRoomFloor.bottomMostTile;
        int leftTile = curRoomFloor.leftMostTile;
        int rightTile = curRoomFloor.rightMostTile;

        if (placedRoomsOnFloor.Count == 0)
        {
            //Place the first room wherever
            Tuple<int, int> firstRoomGridPos = GetValidGridSpace(bottomTile, topTile, leftTile, rightTile);

            Vector2 firstRoomPos = ConvertPlacedRoomGridToWorldSpace(room.gridOffset, firstRoomGridPos.Item1, firstRoomGridPos.Item2);
            room.transform.position = new Vector3(firstRoomPos.x, 0f, firstRoomPos.y);

            //Fill floor grid
            FillGrid(curRoomFloor, firstRoomGridPos.Item1, firstRoomGridPos.Item2, ref floorGrid, ref partialTiles);

            return true;
        }

        //Try placing the room
        for (int i = 0; i < numRoomPlaceAttempts; i++)
        {
            Tuple<int, int> newRoomGridPos = GetValidGridSpace(bottomTile, topTile, leftTile, rightTile);
            if (CheckGridCollisions(curRoomFloor, newRoomGridPos.Item1, newRoomGridPos.Item2, floorGrid, ref partialTiles))
            {
                //Place room
                Vector2 newRoomPos = ConvertPlacedRoomGridToWorldSpace(room.gridOffset, newRoomGridPos.Item1, newRoomGridPos.Item2);
                room.transform.position = new Vector3(newRoomPos.x, 0f, newRoomPos.y);

                //Fill floor grid
                FillGrid(curRoomFloor, newRoomGridPos.Item1, newRoomGridPos.Item2, ref floorGrid, ref partialTiles);

                return true;
            }
            //else try again
        }
        
        //If it reaches here, placement has failed
        return false;
    }

    private void ConnectRooms(List<Room> rooms)
    {

    }

    //Returns a bottom-left point that does not extend past the bounds of the grid
    public Tuple<int, int> GetValidGridSpace(int bottom, int top, int left, int right)
    {
        int verticalDiff = top - bottom;
        int horizontalDiff = right - left;

        //If the points given are too big, fail!
        if (verticalDiff > GridYDimension || horizontalDiff > GridXDimension)
        {
            Debug.LogError("Given room too big for the grid!");
            return new Tuple<int, int>(-1, -1);
        }

        //Debug.Log($"T: {top}; B: {bottom}; L: {left}; R: {right};");

        //The - 1 clamps it because it is 0 indexed
        int x = Random.Range(0, GridXDimension - horizontalDiff - 1);
        int y = Random.Range(0, GridYDimension - verticalDiff - 1);

        //Debug.Log($"({x}, {y})");
        return new Tuple<int, int>(x, y);
    }

    public static float ConvertGridToWorldSpace(int desiredTile)
    {
        return (desiredTile * Floor.GridSize);
    }

    public Vector2 ConvertPlacedRoomGridToWorldSpace(Vector2 gridOffset, int tileX, int tileY)
    {
        gridOffset.x = ConvertGridToWorldSpace(tileX) - gridOffset.x;
        gridOffset.y = ConvertGridToWorldSpace(tileY) - gridOffset.y;
        return gridOffset;
    }

    //Returns true if there is no collisions between the given floor and floorGrid
    public bool CheckGridCollisions(in Floor roomFloor, int x, int y, in TileState[][] floorGrid, ref List<ConflictingTileData> partialTiles)
    {
        TileState[][] roomGrid = roomFloor.tileGrid;

        for (int i = roomFloor.leftMostTile; i <= roomFloor.rightMostTile; i++)
        {
            for (int j = roomFloor.bottomMostTile; j <= roomFloor.topMostTile; j++)
            {
                TileState otherTile = floorGrid[x + i][y + j];
                //Was a switch the best idea for this? Probably not
                switch (roomGrid[i][j])
                {
                    case (TileState.Partial):
                        //Check collisions
                        if(otherTile == TileState.Full)
                        {
                            return false;
                        }

                        if(otherTile == TileState.Partial)
                        {
                            //Grab the two colliding tiles
                            ConflictingTileData newTile = Array.Find(roomFloor.partialTileData.ToArray(), (tile) => (tile.xIndex == i && tile.yIndex == j));
                            ConflictingTileData existingTile = Array.Find(partialTiles.ToArray(), (tile) => (tile.xIndex == x + i && tile.yIndex == y + j));

                            //Check collisions
                            for(int o = 0; o < newTile.lines.Count; o++)
                            {
                                for(int p = 0; p < existingTile.lines.Count; p++)
                                {
                                    if (LineCollisionUtils.LineIntersectsLine(
                                        newTile.lines[o].list[0],
                                        newTile.lines[o].list[1],
                                        existingTile.lines[p].list[0],
                                        existingTile.lines[p].list[1]))
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        break;

                    case (TileState.Full):
                        if(otherTile == TileState.Full || otherTile == TileState.Partial)
                        {
                            //Collision!
                            return false;
                        }
                        break;

                    case (TileState.Empty):
                    default:
                        //Do nothings
                        break;
                }
            }
        }

        return true;
    }

    //Preconditions: All collision checking has already been completed
    //x is the desired x pos in floorGrid
    //y is the desired y pos in floorGrid
    public static void FillGrid(in Floor roomFloor, int x, int y, ref TileState[][] floorGrid, ref List<ConflictingTileData> partialTiles)
    {
        TileState[][] roomGrid = roomFloor.tileGrid;

        for(int i = roomFloor.leftMostTile; i <= roomFloor.rightMostTile; i++)
        {
            for(int j = roomFloor.bottomMostTile; j <= roomFloor.topMostTile; j++)
            {
                //Was a switch the best idea for this? Probably not
                switch (roomGrid[i][j])
                {
                    case (TileState.Partial):
                        if (floorGrid[x + i][y + j] == TileState.Partial)
                        {
                            floorGrid[x + i][y + j] = TileState.Full;

                            //Find and remove now irrelevant partialTileData (lambda magic!)
                            partialTiles.Remove(Array.Find(partialTiles.ToArray(), (tile) => (tile.xIndex == x + i && tile.yIndex == y + j)));
                        }
                        else
                        {
                            floorGrid[x + i][y + j] = TileState.Partial;

                            //Find and add the now relevant partialTileData (lambda magic!)
                            //TODO: Could result in null? Check it out
                            //  though maybe it doesn't. Could go either way
                            partialTiles.Add(Array.Find(roomFloor.partialTileData.ToArray(), (tile) => (tile.xIndex == x + i && tile.yIndex == y + j)));
                        }
                        break;

                    case (TileState.Full):
                        floorGrid[x + i][y + j] = TileState.Full;
                        break;

                    case (TileState.Empty):
                    default:
                        //Do nothings
                        break;
                }
            }
        }
    }
}
