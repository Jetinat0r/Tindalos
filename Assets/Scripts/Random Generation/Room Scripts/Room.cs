using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    //Enums for custom inspector
    [Serializable]
    public enum RoomInspectorMode
    {
        View = 0,
        Paint = 1,
        Lines = 2,
    }

    //Number of floors this room has (how tall it is)
    [Range(1, 10)]
    public int numFloors = 1;

    public Floor CurFloor { get { return mapFloors[curInspectedFloor]; } }

    [HideInInspector]
    //Bottom floor as assigned by the random generator
    public int assignedFloor = -1;

    //TODO: Deprecate / move to floor?
    public List<Doorway> doorways = new List<Doorway>();

    //Holds all floors in the room. Updated via the Update Floors button
    public List<Floor> mapFloors = new List<Floor>();


    //Editor helper values
    [HideInInspector]
    public int curInspectedFloor = -1;
    [HideInInspector]
    public TileState curPaintState = TileState.Full;
    [HideInInspector]
    public RoomInspectorMode curInspectorMode = RoomInspectorMode.View;
    [HideInInspector]
    public LineType curInspectorLineType = LineType.Straight;
    
    


    private void Start()
    {
        Init();
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

    public void SetupFloors()
    {
        if(mapFloors == null)
        {
            mapFloors = new List<Floor>();
        }

        if(mapFloors.Count < numFloors)
        {
            for(int i = mapFloors.Count; i < numFloors; i++)
            {
                mapFloors.Add(new Floor(i));
            }
        }
        else if(mapFloors.Count >= numFloors)
        {
            for(int i = mapFloors.Count; i >= numFloors; i--)
            {
                mapFloors.RemoveAt(i);
            }
        }

        curInspectedFloor = 0;
    }

    private void OnDrawGizmosSelected()
    {
        if(mapFloors == null || mapFloors.Count == 0)
        {
            return;
        }

        switch (curInspectorMode)
        {
            case (RoomInspectorMode.Paint):
                CurFloor.DrawGridOutline();
                break;

            case (RoomInspectorMode.Lines):
                CurFloor.DrawGrid();
                break;

            case (RoomInspectorMode.View):
            default:
                //Do nothing
                break;
        }
    }
}