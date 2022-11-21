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
    public Vector3 leftEdge;
    public Vector3 rightEdge;

    public DoorType doorType = DoorType.Normal;

    [Header("Derived Vars")]
    public float width;
    public float height;
    public Quaternion direction;

    public Doorway(Vector3 leftPos, Vector3 rightPos)
    {
        leftEdge = leftPos;
        rightEdge = rightPos;
        doorType = DoorType.Normal;
    }

    public void Init()
    {
        //Ensure no errors (by throwing an error)
        if(leftEdge == null || rightEdge == null)
        {
            Debug.LogError($"Doorway Info Unset!");
        }

        //Get the vector between the two
        Vector3 dist = rightEdge - leftEdge;
        
        //Get the width of the door regardless of the angle it's at
        width = Mathf.Sqrt(Mathf.Pow(dist.x, 2) + Mathf.Pow(dist.z, 2));
        //Height of door is always the y difference
        height = dist.y;

        //Get just the horizontal vectors for crossing
        Vector3 xzVector = rightEdge - leftEdge;
        xzVector.Set(xzVector.x, 0f, xzVector.z);
        //Create just a vertical vector for crossing
        Vector3 yVector = new Vector3(0f, rightEdge.y - leftEdge.y, 0f);
        
        
        Vector3 crossVector = Vector3.Cross(xzVector, yVector).normalized;

        float yRot = Vector3.SignedAngle(Vector3.forward, crossVector, Vector3.up);

        direction = Quaternion.Euler(new Vector3(0f, yRot, 0f));
    }

    public void DrawDebugGizmos()
    {
        Vector3 doorCenter = (rightEdge - leftEdge) / 2;
        Debug.DrawRay(doorCenter + leftEdge, direction * Vector3.forward * 3, Color.red);
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
        Debug.Log(closestAngle);

        //Construct a new point based off of the given points and derived vars
        return startPoint + new Vector2(doorSize * Mathf.Cos(Mathf.Deg2Rad * closestAngle), doorSize * Mathf.Sin(Mathf.Deg2Rad * closestAngle));
    }
}
