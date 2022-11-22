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
    public float width;
    public float height;
    public Quaternion direction;

    public Doorway(Vector2 leftPos, Vector2 rightPos, float xStartLine, float yStartLine, float xEndLine, float yEndLine)
    {
        startPoint = leftPos;
        endPoint = rightPos;
        this.xStartLine = xStartLine;
        this.yStartLine = yStartLine;
        this.xEndLine = xEndLine;
        this.yEndLine = yEndLine;

        Debug.Log($"({xStartLine}, {yStartLine}); ({xEndLine}, {yEndLine})");
        
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
        Vector3 dist = endPoint - startPoint;
        
        //Get the width of the door regardless of the angle it's at
        width = Mathf.Sqrt(Mathf.Pow(dist.x, 2) + Mathf.Pow(dist.z, 2));
        //Height of door is always the y difference
        height = dist.y;

        //Get just the horizontal vectors for crossing
        Vector3 xzVector = endPoint - startPoint;
        xzVector.Set(xzVector.x, 0f, xzVector.z);
        //Create just a vertical vector for crossing
        Vector3 yVector = new Vector3(0f, endPoint.y - startPoint.y, 0f);
        
        
        Vector3 crossVector = Vector3.Cross(xzVector, yVector).normalized;

        float yRot = Vector3.SignedAngle(Vector3.forward, crossVector, Vector3.up);

        direction = Quaternion.Euler(new Vector3(0f, yRot, 0f));
    }

    public void DrawDebugGizmos()
    {
        //TODO: Fix lol
        Vector3 doorCenter = (endPoint - startPoint) / 2;
        Debug.DrawRay(doorCenter + new Vector3(startPoint.x, doorCenter.y, startPoint.y), direction * Vector3.forward * 3, Color.red);
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
                doorSize = 1f;
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
