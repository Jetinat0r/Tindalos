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

    public Vector2 lineStart;
    [HideInInspector]
    public Vector2 lineEnd;

    public Vector2 bezierStartHandle;
    public Vector2 bezierEndHandle;

    public RoomLine(Vector2 startPos)
    {
        lineStart = startPos;
        bezierStartHandle = startPos + new Vector2(Room.GridSize, Room.GridSize);
        bezierEndHandle = startPos - new Vector2(Room.GridSize, Room.GridSize);
    }

    public void SetLineType(LineType newType)
    {
        if(CurLineType == newType)
        {
            return;
        }

        //TODO: Mess with the line
        CurLineType = newType;

        if(CurLineType == LineType.Bezier)
        {
            bezierStartHandle = lineStart + new Vector2(Room.GridSize, Room.GridSize);
            bezierEndHandle = lineStart - new Vector2(Room.GridSize, Room.GridSize);
        }
    }

    public Vector3[] GetCollisionPoints()
    {        
        switch (CurLineType)
        {
            case (LineType.Straight):
                return new Vector3[] { Vec2To3(lineStart), Vec2To3(lineEnd) };

            case (LineType.Bezier):
                return Handles.MakeBezierPoints(Vec2To3(lineStart), Vec2To3(lineEnd), Vec2To3(bezierStartHandle), Vec2To3(bezierEndHandle), BezierDivisions);

            case(LineType.Door):
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
        return new Vector3(v.x, 0f, v.y);
    }
}
