using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public int floors = 1;

    [HideInInspector]
    public int assignedFloor = -1;

    public List<Doorway> doorways = new List<Doorway>();

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
}
