using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(Room))]
public class RoomMapEditorGizmos : Editor
{
    private const float GridSize = 0.1f;

    //TODO: Make Handles Permanent in Prefab View
    //TODO: Center grid
    //TODO: (Out)Lines

    private void OnSceneGUI()
    {
        Room room = target as Room;
        if (Application.IsPlaying(room))
        {
            return;
        }

        //Vector3 center = room.transform.position;
        
        if(room.tileGrid != null)
        {
            DrawGrid(room, GridSize, new Color(1f, 1f, 1f, 0.25f));
        }

        DrawButtons(room);
    }


    private void DrawGrid(Room room, float spacing, Color color)
    {
        int widthDivs = Mathf.CeilToInt(room.tileGrid.Length);
        int heightDivs = Mathf.CeilToInt(room.tileGrid[0].Length);

        //Handles.BeginGUI();
        Handles.color = new Color(1, 1, 1, 1);

        for (int i = 0; i < widthDivs; i++)
        {
            //TODO: Account for different floors
            //Handles.DrawLine(new Vector3(0f, 0f, 0f + spacing * i), new Vector3(room.gridHeight * GridSize, 0f, spacing * i));
        }

        for (int i = 0; i < heightDivs; i++)
        {
            //TODO: Account for different floors
            //Handles.DrawLine(new Vector3(0f + spacing * i, 0f, 0f), new Vector3(spacing * i, 0f, room.gridWidth * GridSize));
        }

        for(int i = 0; i < widthDivs; i++)
        {
            for(int j = 0; j < heightDivs; j++)
            {
                //Draw a guide for the handles
                Vector3[] lowerTileVerts = {
                    new Vector3(spacing * i, -spacing * 0.5f, spacing * j),
                    new Vector3(spacing * (i + 1), -spacing * 0.5f, spacing * j),
                    new Vector3(spacing * (i + 1), -spacing * 0.5f, spacing *  (j + 1)),
                    new Vector3(spacing * i, -spacing * 0.5f, spacing * (j + 1))
                };
                Handles.color = Color.white;
                Handles.DrawSolidRectangleWithOutline(lowerTileVerts, new Color(1f, 1f, 1f, 0.1f), new Color(0f, 0f, 0f, 1f));
                Vector3[] upperTileVerts = {
                    new Vector3(spacing * i, spacing * 0.5f, spacing * j),
                    new Vector3(spacing * (i + 1), spacing * 0.5f, spacing * j),
                    new Vector3(spacing * (i + 1), spacing * 0.5f, spacing *  (j + 1)),
                    new Vector3(spacing * i, spacing * 0.5f, spacing * (j + 1))
                };
                //Handles.DrawSolidRectangleWithOutline(upperTileVerts, new Color(0f, 0f, 0f, 0f), new Color(0f, 0f, 0f, 1f));
                //Handles.color = new Color(1f, 1f, 1f, 0.25f);

                Handles.color = Color.black;
                Handles.DrawWireCube(new Vector3(spacing * i + (spacing * 0.5f), 0f, spacing * j + (spacing * 0.5f)), new Vector3(spacing, spacing, spacing));


                switch (room.tileGrid[i][j])
                {
                    case(TileState.Full):
                        Handles.color = new Color(0f, 0f, 1f, 0.25f);
                        break;

                    case (TileState.Partial):
                        Handles.color = new Color(0f, 1f, 0f, 0.25f);
                        break;

                    case (TileState.Empty):
                        Handles.color = new Color(1f, 1f, 1f, 0.25f);
                        break;

                    default:
                        Handles.color = new Color(1f, 0f, 0f, 0.25f);
                        Debug.LogError("Invalid State in the Tile Grid!");
                        break;
                }
                
                if(Handles.Button(new Vector3(spacing * i + (spacing * 0.5f), 0f, spacing * j + (spacing * 0.5f)), Quaternion.Euler(90f, 0f, 0f), spacing, spacing, Handles.CubeHandleCap))
                {
                    room.tileGrid[i][j] = room.curPaintState;
                    UpdatePrefab();
                }



                //HandleUtility.
                //Handles.DrawLine(HandleUtility.WorldToGUIPointWithDepth(new Vector3(spacing * i, 0f, spacing * j)), Vector3.zero);
                //Handles.PositionHandle(new Vector3(spacing * i, 0f, spacing * j), Quaternion.identity);
            }
        }
        Handles.color = Color.white;
        //Handles.EndGUI();
    }


    private void DrawButtons(Room room)
    {
        Handles.BeginGUI();

        GUILayout.BeginArea(new Rect(10, 250, 125, 200));

        Rect rect = EditorGUILayout.BeginVertical();
        GUI.color = Color.yellow;
        GUI.Box(rect, GUIContent.none);

        GUI.color = Color.white;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Paint");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Cur Paint = {room.curPaintState}");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        if(room.tileGrid != null)
        {
            if (GUILayout.Button("Full"))
            {
                //room.DumpTileGrid();
                room.curPaintState = TileState.Full;
                UpdatePrefab();
            }

            if (GUILayout.Button("Partial"))
            {
                room.curPaintState = TileState.Partial;
                UpdatePrefab();
            }

            if (GUILayout.Button("Empty"))
            {
                room.curPaintState = TileState.Empty;
                UpdatePrefab();
            }

            GUILayout.Space(25);

            if (GUILayout.Button("Fill Empty"))
            {
                for (int i = 0; i < room.tileGrid.Length; i++)
                {
                    for (int j = 0; j < room.tileGrid[i].Length; j++)
                    {
                        if (room.tileGrid[i][j] == TileState.Empty)
                        {
                            room.tileGrid[i][j] = room.curPaintState;
                        }
                    }
                }

                UpdatePrefab();
            }

            if (GUILayout.Button("Reset"))
            {
                if (room.SetupGrid())
                {
                    for (int i = 0; i < room.tileGrid.Length; i++)
                    {
                        for (int j = 0; j < room.tileGrid[i].Length; j++)
                        {
                            room.tileGrid[i][j] = TileState.Empty;
                        }
                    }
                }

                UpdatePrefab();
            }
        }
        else
        {
            if(GUILayout.Button("Setup Grid"))
            {
                room.SetupGrid();
            }
        }
        

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();


        GUILayout.EndArea();

        Handles.EndGUI();
    }

    public static void UpdatePrefab()
    {
        PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            EditorSceneManager.MarkSceneDirty(prefabStage.scene);
        }
    }
}
