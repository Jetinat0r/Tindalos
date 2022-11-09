using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using static Jagged2DArraySerializer;

[System.Serializable]
public enum TileState
{
    Empty = 0,
    Partial = 1,
    Full = 2,
}

public class Room : MonoBehaviour, ISerializationCallbackReceiver
{
    public int floors = 1;

    [HideInInspector]
    public int assignedFloor = -1;

    public List<Doorway> doorways = new List<Doorway>();

    [Header("Room Painting Vars")]
    public int gridWidth = 10;
    public int gridHeight = 10;

    //Used to save info for tileGrid
    [SerializeField, HideInInspector]
    private Jagged2DArrayPackage<TileState> serializable;

    [HideInInspector]
    public TileState curPaintState = TileState.Full;
    public TileState[][] tileGrid;


    private void Start()
    {
        Init();
    }

    public bool SetupGrid()
    {
        //If no changes, don't reinitialize the grid
        if(tileGrid != null && tileGrid.Length == gridWidth && tileGrid[0] != null && tileGrid[0].Length == gridHeight)
        {
            return true;
        }

        Debug.Log($"{gridWidth} {gridHeight}");

        //Ensure size is valid
        if(gridWidth <= 0)
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
        for(int i = 0; i < gridWidth; i++)
        {
            tileGrid[i] = new TileState[gridHeight];
        }

        return true;
    }

    public void Init()
    {
        foreach(Doorway d in doorways)
        {
            d.Init();
        }
    }

    private void Update()
    {
        foreach(Doorway d in doorways)
        {
            d.DrawDebugGizmos();
        }
    }

    public void OnBeforeSerialize()
    {
        //Avoid errors
        if(tileGrid == null)
        {
            return;
        }

        List<int> vSizes = new();
        List<Jagged2DArrayElementPackage<TileState>> elements = new();

        for(int i = 0; i < tileGrid.Length; i++)
        {
            //Avoid Errors
            if (tileGrid[i] == null)
            {
                return;
            }

            vSizes.Add(tileGrid[i].Length);

            for(int j = 0; j < tileGrid[i].Length; j++)
            {
                Jagged2DArrayElementPackage<TileState> package = new(i, j, tileGrid[i][j]);

                elements.Add(package);
            }
        }

        serializable = new(tileGrid.Length, vSizes, elements);
    }

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
        foreach(Jagged2DArrayElementPackage<TileState> package in serializable.Array)
        {
            tileGrid[package.Index0][package.Index1] = package.Element;
        }
    }

    public void DumpTileGrid()
    {
        string dump = "";
        for (int i = 0; i < tileGrid.Length; i++)
        {
            dump += i + ": ";
            for (int j = 0; j < tileGrid[i].Length; j++)
            {
                dump += " " + tileGrid[i][j];
            }
            dump += '\n';
        }

        Debug.Log(dump);
    }
}