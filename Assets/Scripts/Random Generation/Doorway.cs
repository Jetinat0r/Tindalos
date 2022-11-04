using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum DoorType
{
    Normal = 0,
}

[Serializable]
public class Doorway
{
    [Header("User Vars")]
    public Transform bottomLeft;
    public Transform topRight;

    public DoorType doorType = DoorType.Normal;

    [Header("Derived Vars")]
    public float width;
    public float height;
    public Quaternion direction;


    public void Init()
    {
        //Ensure no errors (by throwing an error)
        if(bottomLeft == null || topRight  == null)
        {
            Debug.LogError($"Doorway Info Unset!");
        }

        //Get the vector between the two
        Vector3 dist = topRight.position - bottomLeft.position;
        
        //Get the width of the door regardless of the angle it's at
        width = Mathf.Sqrt(Mathf.Pow(dist.x, 2) + Mathf.Pow(dist.z, 2));
        //Height of door is always the y difference
        height = dist.y;

        //Get just the horizontal vectors for crossing
        Vector3 xzVector = topRight.position - bottomLeft.position;
        xzVector.Set(xzVector.x, 0f, xzVector.z);
        //Create just a vertical vector for crossing
        Vector3 yVector = new Vector3(0f, topRight.position.y - bottomLeft.position.y, 0f);
        
        
        Vector3 crossVector = Vector3.Cross(xzVector, yVector).normalized;

        float yRot = Vector3.SignedAngle(Vector3.forward, crossVector, Vector3.up);

        direction = Quaternion.Euler(new Vector3(0f, yRot, 0f));
    }

    public void DrawDebugGizmos()
    {
        Vector3 doorCenter = (topRight.position - bottomLeft.position) / 2;
        Debug.DrawRay(doorCenter + bottomLeft.position, direction * Vector3.forward * 3, Color.red);
    }
}
