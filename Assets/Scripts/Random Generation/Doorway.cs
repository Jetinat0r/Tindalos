using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JetEngine;

[Serializable]
public enum DoorType
{
    Normal = 0,
}

[Serializable]
public class Doorway
{
    [Header("User Vars")]
    public Vector2 startPoint;
    public Vector2 endPoint;

    //Door start and end pos as [gridLine].[% towards next grid line] (i.e. 3.89)
    public float xStartLine;
    public float yStartLine;
    public float xEndLine;
    public float yEndLine;

    public DoorType doorType = DoorType.Normal;

    [Header("Derived Vars")]
    public float width; //Kind of irrelevant?
    public float height; //100% irrelevant
    public float angle;

    public Doorway(Vector2 leftPos, Vector2 rightPos, float xStartLine, float yStartLine, float xEndLine, float yEndLine)
    {
        startPoint = leftPos;
        endPoint = rightPos;
        this.xStartLine = xStartLine;
        this.yStartLine = yStartLine;
        this.xEndLine = xEndLine;
        this.yEndLine = yEndLine;

        //Debug.Log($"({xStartLine}, {yStartLine}); ({xEndLine}, {yEndLine})");
        
        doorType = DoorType.Normal;
    }

    public void Init()
    {
        //Ensure no errors (by throwing an error)
        if(startPoint == null || endPoint == null)
        {
            Debug.LogError($"Doorway Info Unset!");
        }

        //Get the vector between the two
        Vector2 dist = endPoint - startPoint;
        
        //Get the width of the door regardless of the angle it's at
        //TODO: Get rid of?
        width = dist.magnitude;


        //Get angle that dist vector is at
        float doorAngle = Mathf.Atan2(dist.y, dist.x);
        //Make the angle degrees
        doorAngle *= Mathf.Rad2Deg;
        //Ensure that the angle is always positive
        doorAngle += 360f;

        //Find the perpendicualr angle
        doorAngle += 90f;

        //Clamp the angle between 0f and 360f
        angle = doorAngle % 360f;
    }

    public void DrawDebugGizmos(Room r, Transform transform)
    {
        Vector2 doorCenter = ((endPoint - startPoint) / 2) + transform.position.GetXZ() + startPoint;
        float doorY = Floor.FloorHeight * r.assignedFloor;
        Debug.DrawRay(new Vector3(doorCenter.x, doorY, doorCenter.y), 3 * new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad)), Color.red);
    }
}

public static class DoorwayTypeExtensions
{
    public static Vector2 FixEndPoint(this DoorType doorType, Vector2 startPoint, Vector2 endPoint)
    {
        float doorSize;
        float validAngles;

        //Assign vars based on door type
        switch (doorType)
        {
            case (DoorType.Normal):
                doorSize = 1.3125f;
                validAngles = 45f;
                break;

            default:
                doorSize = 1f;
                validAngles = 90f;
                Debug.LogError("Door type does not have vars in FixEndPoint extension method!");
                break;
        }

        //Get the current angle that the two given points create
        Vector2 direction = endPoint - startPoint;
        float curAngle = Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x);

        //Get the clamped angle based on the assigned vars above
        float closestAngle = MathUtils.GetClosestEvenDivisor(curAngle, validAngles);

        //Construct a new point based off of the given points and derived vars
        return startPoint + new Vector2(doorSize * Mathf.Cos(Mathf.Deg2Rad * closestAngle), doorSize * Mathf.Sin(Mathf.Deg2Rad * closestAngle));
    }
}
