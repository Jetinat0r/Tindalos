using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Jagged2DArraySerializer;
using static ListWrappers;

[Serializable]
public enum TileState
{
    Empty = 0,
    Partial = 1,
    Full = 2,
}

//Used to cut down on collision check time
[Serializable]
public class ConflictingTileData
{
    public ConflictingTileData(int x, int y)
    {
        xIndex = x;
        yIndex = y;

        lines = new List<Vector2ListWrapper>();
    }

    //Where on the tileGrid does this fall?
    public int xIndex;
    public int yIndex;

    //Stores the points of each non-consecutive line passing through the given tile
    //List 1: Non-consecutive lines
    //List 2: Points on each line
    public List<Vector2ListWrapper> lines;
}


[Serializable]
public class Floor : ISerializationCallbackReceiver
{
    public const float FloorHeight = 2.785f;

    [SerializeField, HideInInspector]
    private int floorNum = 0;

    // -------- Painting -----------
    [HideInInspector]
    public const float GridSize = 0.1f;

    [Header("Room Painting Vars")]
    public int gridWidth = 10;
    public int gridHeight = 10;


    //Used to save info for tileGrid
    [SerializeField, HideInInspector]
    private Jagged2DArrayPackage<TileState> serializable;
    public TileState[][] tileGrid;
    [SerializeField, HideInInspector, Obsolete("Deprecated; No longer in use")]
    public List<ConflictingTileData> partialTileData = new();

    [SerializeField, HideInInspector]
    public int leftMostTile;
    [SerializeField, HideInInspector]
    public int rightMostTile;
    [SerializeField, HideInInspector]
    public int bottomMostTile;
    [SerializeField, HideInInspector]
    public int topMostTile;

    // ----------- Lines ------------
    public List<RoomLine> roomLines;
    [SerializeField, HideInInspector]
    private Vector3[] points;

    //One point inside the polgon, useful to determine if 2 polygons completely overlap
    [SerializeField, HideInInspector]
    public Vector3 interiorPoint;

    //Every door on the respective floor
    public List<Doorway> doorways = new List<Doorway>();

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
    public void DrawGridOutline(Vector2 gridOffset)
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
    public void DrawGrid(Vector2 gridOffset)
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

    public void SaveGrid()
    {
        //Clamps search area

        //Find lowest possible tile
        bool isFound = false;
        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                if (tileGrid[j][i] != TileState.Empty)
                {
                    bottomMostTile = i;
                    isFound = true;
                    break;
                }
            }

            if (isFound)
            {
                break;
            }
        }

        //Find highest possible tile
        isFound = false;
        for (int i = gridHeight - 1; i >= 0; i--)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                if (tileGrid[j][i] != TileState.Empty)
                {
                    topMostTile = i;
                    isFound = true;
                    break;
                }
            }

            if (isFound)
            {
                break;
            }
        }

        //Find leftmost possible tile
        isFound = false;
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                if (tileGrid[i][j] != TileState.Empty)
                {
                    leftMostTile = i;
                    isFound = true;
                    break;
                }
            }

            if (isFound)
            {
                break;
            }
        }

        //Find rightmost possible tile
        isFound = false;
        for (int i = gridWidth - 1; i >= 0; i--)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                if (tileGrid[i][j] != TileState.Empty)
                {
                    rightMostTile = i;
                    isFound = true;
                    break;
                }
            }

            if (isFound)
            {
                break;
            }
        }
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

    //WARNING: Abandon all hope, ye who enter here
    public void SaveLines(Vector2 gridOffset)
    {
        //Generates doorways from lines
        doorways.Clear();

        //Also collects points!
        List<Vector2> points = new List<Vector2>();
        foreach (RoomLine line in roomLines)
        {
            if (line.CurLineType == LineType.Door)
            {
                //Convert physical pos to grid pos
                //startLine X
                int closestXSLine = (int)Mathf.Clamp(
                    JetEngine.MathUtils.GetClosestEvenDivisor(line.lineStart.x - gridOffset.x, GridSize) / GridSize,
                    0f,
                    gridWidth);
                float distToXS = line.lineStart.x - ((closestXSLine * GridSize) + gridOffset.x);
                float xLineStart = (distToXS / GridSize) + closestXSLine;
                xLineStart = Mathf.Clamp(xLineStart, 0f, gridWidth);

                //startLine Y
                int closestYSLine = (int)Mathf.Clamp(
                    JetEngine.MathUtils.GetClosestEvenDivisor(line.lineStart.y - gridOffset.y, GridSize) / GridSize,
                    0f,
                    gridWidth);
                float distToYS = line.lineStart.y - ((closestYSLine * GridSize) + gridOffset.y);
                float yLineStart = (distToYS / GridSize) + closestYSLine;
                yLineStart = Mathf.Clamp(yLineStart, 0f, gridHeight);

                //endLine X
                int closestXELine = (int)Mathf.Clamp(
                    JetEngine.MathUtils.GetClosestEvenDivisor(line.lineEnd.x - gridOffset.x, GridSize) / GridSize,
                    0f,
                    gridWidth);
                float distToXE = line.lineEnd.x - ((closestXELine * GridSize) + gridOffset.x);
                float xLineEnd = (distToXE / GridSize) + closestXELine;
                xLineEnd = Mathf.Clamp(xLineEnd, 0f, gridWidth);

                //endLine Y
                int closestYELine = (int)Mathf.Clamp(
                    JetEngine.MathUtils.GetClosestEvenDivisor(line.lineEnd.y - gridOffset.y, GridSize) / GridSize,
                    0f,
                    gridWidth);
                float distToYE = line.lineEnd.y - ((closestYELine * GridSize) + gridOffset.y);
                float yLineEnd = (distToYE / GridSize) + closestYELine;
                yLineEnd = Mathf.Clamp(yLineEnd, 0f, gridHeight);


                //Create the new door object
                Doorway newDoor = new Doorway(line.lineStart, line.lineEnd, xLineStart, yLineStart, xLineEnd, yLineEnd);
                newDoor.Init();
                doorways.Add(newDoor);
            }

            Vector3[] curLinePoints = line.GetCollisionPoints();
            for (int i = 0; i < curLinePoints.Length; i++)
            {
                points.Add(new Vector2(curLinePoints[i].x, curLinePoints[i].z));
            }
        }

        //Clean up duplicate points
        for (int i = 0; i < points.Count - 1; i++)
        {
            if (points[i] == points[i + 1])
            {
                points.RemoveAt(i);
                i--;
            }
        }

        //Clean up final point if duplicate
        if (points[0] == points[^1])
        {
            points.RemoveAt(points.Count - 1);
        }

        //Generates collision data for partial tiles
        partialTileData.Clear();

        //Generates a list of rects to check and the data associated with them
        List<Tuple<Rect, ConflictingTileData>> partialTiles = new List<Tuple<Rect, ConflictingTileData>>();
        for(int i = 0; i < tileGrid.Length; i++)
        {
            for(int j = 0; j < tileGrid[i].Length; j++)
            {
                if (tileGrid[i][j] == TileState.Partial)
                {
                    Rect newRect = new Rect();
                    Vector2 bottomLeft = new Vector2(gridOffset.x + (i * GridSize), gridOffset.y + (j * GridSize));
                    Vector2 topRight = new Vector2(gridOffset.x + ((i + 1) * GridSize), gridOffset.y + ((j + 1) * GridSize));

                    newRect.yMin = bottomLeft.y;
                    newRect.xMin = bottomLeft.x;
                    newRect.yMax = topRight.y;
                    newRect.xMax = topRight.x;

                    partialTiles.Add(new Tuple<Rect, ConflictingTileData>(newRect, new ConflictingTileData(i, j)));
                }
            }
        }

        //Finds every line that intersects with each tile, and adds it to that respective tile's collision data
        for(int i = 0; i < points.Count; i++)
        {
            Vector2 point1 = points[i];
            Vector2 point2 = (i != points.Count - 1) ? points[i + 1] : points[0];

            foreach (Tuple<Rect, ConflictingTileData> tuple in partialTiles)
            {
                if(LineCollisionUtils.LineIntersectsRect(point1, point2, tuple.Item1))
                {
                    tuple.Item2.lines.Add(new Vector2ListWrapper());
                    tuple.Item2.lines[^1].list.Add(point1);
                    tuple.Item2.lines[^1].list.Add(point2);
                }
            }
        }
        
        //Finally add the partial tile data to the list
        for(int i = 0; i < partialTiles.Count; i++)
        {
            partialTileData.Add(partialTiles[i].Item2);
        }
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
