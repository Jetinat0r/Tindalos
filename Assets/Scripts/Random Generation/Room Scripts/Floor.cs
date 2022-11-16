using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Jagged2DArraySerializer;

[Serializable]
public enum TileState
{
    Empty = 0,
    Partial = 1,
    Full = 2,
}

[Serializable]
public class Floor : ISerializationCallbackReceiver
{
    public const float FloorHeight = 1f;

    [SerializeField, HideInInspector]
    private int floorNum = 0;

    // -------- Painting -----------
    [HideInInspector]
    public const float GridSize = 0.1f;

    [Header("Room Painting Vars")]
    public int gridWidth = 10;
    public int gridHeight = 10;
    [Space(10)]
    public Vector2 gridOffset = Vector2.zero;


    //Used to save info for tileGrid
    [SerializeField, HideInInspector]
    private Jagged2DArrayPackage<TileState> serializable;
    public TileState[][] tileGrid;
    

    // ----------- Lines ------------
    public List<RoomLine> roomLines;
    [SerializeField, HideInInspector]
    private Vector3[] points;


    public Floor(int floorNum)
    {
        this.floorNum = floorNum;

        SetupGrid();
        SetupLines();
    }

    public bool SetupGrid()
    {
        //If no changes, don't reinitialize the grid
        if (tileGrid != null && tileGrid.Length == gridWidth && tileGrid[0] != null && tileGrid[0].Length == gridHeight)
        {
            return true;
        }

        //Ensure size is valid
        if (gridWidth <= 0)
        {
            Debug.LogError("Invalid Room Grid Width!");
            return false;
        }

        //Ensure size is valid
        if (gridHeight <= 0)
        {
            Debug.LogError("Invalid Room Grid Height!");
            return false;
        }


        //Setup the array
        tileGrid = new TileState[gridWidth][];
        for (int i = 0; i < gridWidth; i++)
        {
            tileGrid[i] = new TileState[gridHeight];
        }

        return true;
    }

    //Used to quickly draw the outline of the grid in Paint mode (sorry DRY)
    public void DrawGridOutline()
    {
        if(tileGrid == null)
        {
            return;
        }

        int widthDivs = Mathf.CeilToInt(tileGrid.Length);
        int heightDivs = Mathf.CeilToInt(tileGrid[0].Length);

        Gizmos.color = Color.black;
        //Draw Tiles
        for (int i = 0; i < widthDivs; i++)
        {
            for (int j = 0; j < heightDivs; j++)
            {
                Gizmos.DrawWireCube(new Vector3((GridSize * i + (GridSize * 0.5f) + gridOffset.x),
                    FloorHeight * floorNum,
                    (GridSize * j + (GridSize * 0.5f)) + gridOffset.y),
                    new Vector3(GridSize, GridSize, GridSize));
            }
        }

        //Reset Color
        Gizmos.color = Color.white;
    }

    //Used to toggle on the grid in Line mode
    //Has some weird issues w/ depth and transparency
    public void DrawGrid()
    {
        if (tileGrid == null)
        {
            return;
        }

        int widthDivs = Mathf.CeilToInt(tileGrid.Length);
        int heightDivs = Mathf.CeilToInt(tileGrid[0].Length);

        //Not *really* needed but whatever
        Handles.color = Color.white;

        //Draw Tiles
        for (int i = 0; i < widthDivs; i++)
        {
            for (int j = 0; j < heightDivs; j++)
            {
                //Get Color for each tile based on tile's current state
                switch (tileGrid[i][j])
                {
                    case (TileState.Full):
                        Gizmos.color = new Color(0f, 0f, 1f, 1f);
                        break;

                    case (TileState.Partial):
                        Gizmos.color = new Color(0f, 1f, 0f, 1f);
                        break;

                    case (TileState.Empty):
                        Gizmos.color = new Color(1f, 1f, 1f, 1f);
                        break;

                    default:
                        Gizmos.color = new Color(1f, 0f, 0f, 1f);
                        Debug.LogError("Invalid State in the Tile Grid!");
                        break;
                }

                Gizmos.DrawCube(new Vector3((GridSize * i + (GridSize * 0.5f) + gridOffset.x),
                    FloorHeight * floorNum,
                    (GridSize * j + (GridSize * 0.5f)) + gridOffset.y),
                    new Vector3(GridSize, GridSize, GridSize));


                Gizmos.color = Color.black;
                Gizmos.DrawWireCube(new Vector3((GridSize * i + (GridSize * 0.5f) + gridOffset.x),
                    FloorHeight * floorNum,
                    (GridSize * j + (GridSize * 0.5f)) + gridOffset.y),
                    new Vector3(GridSize, GridSize, GridSize));
            }
        }

        //Reset Color
        Gizmos.color = Color.white;
    }


    public void SetupLines()
    {
        if (roomLines == null)
        {
            roomLines = new List<RoomLine>();
        }
        else
        {
            //Should never happen, but I want to be careful
            roomLines.Clear();
        }

        //Set up the first 3 lines
        RoomLine lineOne = new RoomLine(new Vector2(0f, 0f), floorNum);
        RoomLine lineTwo = new RoomLine(new Vector2(1f, 0f), floorNum);
        RoomLine lineThree = new RoomLine(new Vector2(0.5f, -1f), floorNum);

        //Ensure that they point in a loop
        lineOne.lineEnd = lineTwo.lineStart;
        lineTwo.lineEnd = lineThree.lineStart;
        lineThree.lineEnd = lineOne.lineStart;

        //Add the lines to the list
        roomLines.Add(lineOne);
        roomLines.Add(lineTwo);
        roomLines.Add(lineThree);
    }




    //Special Serialization for the Jagged array
    public void OnBeforeSerialize()
    {
        //Avoid errors
        if (tileGrid == null)
        {
            return;
        }

        List<int> vSizes = new();
        List<Jagged2DArrayElementPackage<TileState>> elements = new();

        for (int i = 0; i < tileGrid.Length; i++)
        {
            //Avoid Errors
            if (tileGrid[i] == null)
            {
                return;
            }

            vSizes.Add(tileGrid[i].Length);

            for (int j = 0; j < tileGrid[i].Length; j++)
            {
                Jagged2DArrayElementPackage<TileState> package = new(i, j, tileGrid[i][j]);

                elements.Add(package);
            }
        }

        serializable = new(tileGrid.Length, vSizes, elements);
    }
    //Special DeSerialization for the Jagged array
    public void OnAfterDeserialize()
    {
        //Avoid errors
        if (serializable.Array == null)
        {
            return;
        }

        //Generate a new 2D array
        tileGrid = new TileState[serializable.HorizontalSize][];
        for (int i = 0; i < serializable.HorizontalSize; i++)
        {
            tileGrid[i] = new TileState[serializable.VerticalSizes[i]];
        }

        //Fill the array
        foreach (Jagged2DArrayElementPackage<TileState> package in serializable.Array)
        {
            tileGrid[package.Index0][package.Index1] = package.Element;
        }
    }
}
