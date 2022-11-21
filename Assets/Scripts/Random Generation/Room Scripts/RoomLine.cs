using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public enum LineType
{
    Add = -2,
    Edit = -1,
    Straight = 0,
    Bezier = 1,
    Door = 2,
}

[Serializable]
public class RoomLine
{
    private const int BezierDivisions = 16;

    [field:SerializeField]
    public LineType CurLineType { get; private set; }
    [field: SerializeField]
    public DoorType CurDoorType { get; private set; }

    public Vector2 lineStart;
    [HideInInspector]
    public Vector2 lineEnd;

    public Vector2 bezierStartHandle;
    public Vector2 bezierEndHandle;

    [SerializeField, HideInInspector]
    private int floorNum = 0;

    public RoomLine(Vector2 startPos, int floorNum)
    {
        lineStart = startPos;
        bezierStartHandle = startPos + new Vector2(Floor.GridSize, Floor.GridSize);
        bezierEndHandle = startPos - new Vector2(Floor.GridSize, Floor.GridSize);
        this.floorNum = floorNum;
    }

    public void SetLineType(LineType newType)
    {
        if(CurLineType == newType)
        {
            return;
        }

        CurLineType = newType;

        if(CurLineType == LineType.Bezier)
        {
            bezierStartHandle = lineStart + new Vector2(Floor.GridSize, Floor.GridSize);
            bezierEndHandle = lineStart - new Vector2(Floor.GridSize, Floor.GridSize);
        }
    }

    public Vector3[] GetCollisionPoints()
    {        
        switch (CurLineType)
        {
            case (LineType.Straight):
            case (LineType.Door):
                return new Vector3[] { Vec2To3(lineStart), Vec2To3(lineEnd) };

            case (LineType.Bezier):
                return Handles.MakeBezierPoints(Vec2To3(lineStart),
                    Vec2To3(lineEnd),
                    Vec2To3(bezierStartHandle),
                    Vec2To3(bezierEndHandle),
                    BezierDivisions);

            default:
                return new Vector3[0];
        }
    }

    public Vector3[] GetDrawPoints()
    {
        switch (CurLineType)
        {
            case (LineType.Straight):
            case (LineType.Door):
                return new Vector3[] { Vec2To3(lineStart), Vec2To3(lineEnd) };

            case (LineType.Bezier):
                return new Vector3[] { Vec2To3(lineStart), Vec2To3(lineEnd), Vec2To3(bezierStartHandle), Vec2To3(bezierEndHandle) };

            default:
                return new Vector3[0];
        }
    }

    private Vector3 Vec2To3(Vector2 v)
    {
        return new Vector3(v.x, Floor.FloorHeight * floorNum, v.y);
    }


    public void SetDoorType(DoorType newType)
    {
        if (CurDoorType == newType)
        {
            return;
        }

        CurDoorType = newType;
    }
}
