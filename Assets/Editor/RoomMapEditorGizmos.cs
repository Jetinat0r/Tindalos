using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(Room))]
public class RoomMapEditorGizmos : Editor
{
    //TODO: Clean up control panel (blegh)
    //TODO: (Out)Lines
    //TODO: Add button "Assign Tiles" to lines window. When pressed, runs through each line and
    //      determines which "partial" tiles it runs through
    //      even better, we can assign 3 points (entry, mid, exit) during this step to save even
    //      more time later (still feels like collisions, but maybe it'll be ok?)

    private int curPointIndex = -1;

    private void OnSceneGUI()
    {
        Room room = target as Room;
        if (Application.IsPlaying(room))
        {
            return;
        }

        //Vector3 center = room.transform.position;

        DrawModeButtons(room);

        switch (room.curInspectorMode)
        {
            case (Room.RoomInspectorMode.View):

                break;

            case (Room.RoomInspectorMode.Paint):
                if (room.tileGrid != null)
                {
                    DrawGrid(room, Room.GridSize, new Color(1f, 1f, 1f, 0.45f));
                }

                DrawPaintingButtons(room);
                break;

            case (Room.RoomInspectorMode.Lines):
                if(room.roomLines != null && room.roomLines.Count >= 3)
                {
                    DrawLines(room);

                    if(room.curInspectorLineType == LineType.Add)
                    {
                        DrawNewPointHandle(room);
                    }
                }

                DrawLinesButtons(room);
                break;

            default:
                Debug.LogError("Invalid Room Inspector Mode, something went horribly wrong!");
                break;
        }
    }


    private void DrawGrid(Room room, float spacing, Color color)
    {
        int widthDivs = Mathf.CeilToInt(room.tileGrid.Length);
        int heightDivs = Mathf.CeilToInt(room.tileGrid[0].Length);

        //Not *really* needed but whatever
        Handles.color = Color.white;

        //Draw Tiles
        for(int i = 0; i < widthDivs; i++)
        {
            for(int j = 0; j < heightDivs; j++)
            {
                //Get Color for each tile based on tile's current state
                switch (room.tileGrid[i][j])
                {
                    case(TileState.Full):
                        Handles.color = new Color(0f, 0f, 1f, 0.45f);
                        break;

                    case (TileState.Partial):
                        Handles.color = new Color(0f, 1f, 0f, 0.45f);
                        break;

                    case (TileState.Empty):
                        Handles.color = new Color(1f, 1f, 1f, 0.45f);
                        break;

                    default:
                        Handles.color = new Color(1f, 0f, 0f, 0.45f);
                        Debug.LogError("Invalid State in the Tile Grid!");
                        break;
                }
                
                //Create a button to click to paint the current tile
                if(Handles.Button(new Vector3((spacing * i + (spacing * 0.5f)) + room.gridOffset.x, 0f, (spacing * j + (spacing * 0.5f)) + room.gridOffset.y),
                    Quaternion.Euler(90f, 0f, 0f),
                    spacing,
                    spacing,
                    Handles.CubeHandleCap))
                {
                    //Paint the selected tile
                    room.tileGrid[i][j] = room.curPaintState;
                    UpdatePrefab();
                }
            }
        }

        //Reset Color
        Handles.color = Color.white;
    }

    private void DrawModeButtons(Room room)
    {
        Handles.BeginGUI();

        //SceneView sceneWindow = EditorWindow.GetWindow<SceneView>();
        //GUILayout.BeginArea(new Rect(10, sceneWindow.position.height - 165, 125, 125));
        GUILayout.BeginArea(new Rect(10, 150, 125, 125));

        Rect rect = EditorGUILayout.BeginVertical();
        GUI.color = Color.yellow;
        GUI.Box(rect, GUIContent.none);

        GUI.color = Color.white;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Modes");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Cur Mode = {room.curInspectorMode}");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        if (GUILayout.Button("View"))
        {
            room.curInspectorMode = Room.RoomInspectorMode.View;
            UpdatePrefab();
        }

        if (GUILayout.Button("Paint"))
        {
            room.curInspectorMode = Room.RoomInspectorMode.Paint;
            UpdatePrefab();
        }

        if (GUILayout.Button("Lines"))
        {
            room.curInspectorMode = Room.RoomInspectorMode.Lines;
            UpdatePrefab();
        }


        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();


        GUILayout.EndArea();

        Handles.EndGUI();
    }

    private void DrawPaintingButtons(Room room)
    {
        Handles.BeginGUI();

        GUILayout.BeginArea(new Rect(10, 260, 125, 200));

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

    private void DrawLinesButtons(Room room)
    {
        Handles.BeginGUI();

        GUILayout.BeginArea(new Rect(10, 260, 125, 225));

        Rect rect = EditorGUILayout.BeginVertical();
        GUI.color = Color.yellow;
        GUI.Box(rect, GUIContent.none);

        GUI.color = Color.white;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Lines");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(room.curInspectorLineType == LineType.Edit)
        {
            GUILayout.Label($"Editing Point [{curPointIndex}]");
        }
        else if(room.curInspectorLineType == LineType.Add)
        {
            GUILayout.Label($"Adding Points");
        }
        else
        {
            GUILayout.Label($"Moving Points");
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        if (room.roomLines != null && room.roomLines.Count >= 3)
        {
            if(GUILayout.Button("Toggle Edit Mode"))
            {
                if(room.curInspectorLineType == LineType.Edit)
                {
                    room.curInspectorLineType = LineType.Straight;
                }
                else
                {
                    room.curInspectorLineType = LineType.Edit;
                    curPointIndex = -1;
                }
            }

            if(GUILayout.Button("Toggle Add Mode"))
            {
                if (room.curInspectorLineType == LineType.Add)
                {
                    room.curInspectorLineType = LineType.Straight;
                }
                else
                {
                    room.curInspectorLineType = LineType.Add;
                    curPointIndex = -1;
                }
            }

            if(room.curInspectorLineType == LineType.Edit)
            {
                if (GUILayout.Button("Straight"))
                {
                    if (curPointIndex != -1)
                    {
                        room.roomLines[curPointIndex].SetLineType(LineType.Straight);
                    }
                    UpdatePrefab();
                }

                if (GUILayout.Button("Bezier"))
                {
                    if (curPointIndex != -1)
                    {
                        room.roomLines[curPointIndex].SetLineType(LineType.Bezier);
                    }
                    UpdatePrefab();
                }

                if (GUILayout.Button("Door"))
                {
                    if (curPointIndex != -1)
                    {
                        room.roomLines[curPointIndex].SetLineType(LineType.Door);
                    }
                    UpdatePrefab();
                }
            }
            else
            {
                GUILayout.Space(25 * 2);
            }

            if (room.curInspectorLineType == LineType.Edit)
            {
                if (GUILayout.Button("Delete"))
                {
                    if (room.roomLines.Count > 3)
                    {
                        //Due to size restrictions, edge cases are mutually exclusive
                        if (curPointIndex == 0)
                        {
                            //Edge case for deleting point 1
                            room.roomLines[room.roomLines.Count - 1].lineEnd = room.roomLines[curPointIndex + 1].lineStart;
                        }
                        else if (curPointIndex == room.roomLines.Count - 1)
                        {
                            //Edge case for deleting point n
                            room.roomLines[curPointIndex - 1].lineEnd = room.roomLines[0].lineStart;
                        }
                        else
                        {
                            //Normal case
                            room.roomLines[curPointIndex - 1].lineEnd = room.roomLines[curPointIndex + 1].lineStart;
                        }

                        room.roomLines.RemoveAt(curPointIndex);

                        curPointIndex = -1;
                        UpdatePrefab();
                    }
                }
            }
            else
            {
                GUILayout.Space(25);
            }

            if (GUILayout.Button("Save Lines"))
            {
                

                UpdatePrefab();
            }

            //GUILayout.Space(25);

            

            if (GUILayout.Button("Reset"))
            {


                UpdatePrefab();
            }
        }
        else
        {
            if (GUILayout.Button("Setup Lines"))
            {
                room.SetupLines();
            }
        }


        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();


        GUILayout.EndArea();

        Handles.EndGUI();
    }

    private void DrawLines(Room room)
    {
        for(int i = 0; i < room.roomLines.Count; i++)
        {
            RoomLine line = room.roomLines[i];

            switch (line.CurLineType)
            {
                case (LineType.Straight):
                    Handles.color = Color.green;
                    Vector3[] points = line.GetDrawPoints();
                    Handles.DrawAAPolyLine(points);
                    //Returns the new pos of the handle
                    if (i == 0)
                    {
                        Handles.color = Color.yellow;
                    }
                    else if(i == 1)
                    {
                        Handles.color = Color.red;
                    }
                    else
                    {
                        Handles.color = Color.green;
                    }

                    if(room.curInspectorLineType != LineType.Edit)
                    {
                        Vector3 newStraightPos = Handles.FreeMoveHandle(points[0],
                                Quaternion.identity,
                                Room.GridSize / 2,
                                new Vector3(Room.GridSize, Room.GridSize, Room.GridSize),
                                Handles.CircleHandleCap);

                        Vector2 convertedHeadPos = new Vector2(newStraightPos.x, newStraightPos.z);
                        if (convertedHeadPos != line.lineStart)
                        {
                            MoveLine(room, i, convertedHeadPos);
                            UpdatePrefab();
                        }
                    }
                    else
                    {
                        if(Handles.Button(points[0],
                            Quaternion.identity,
                            Room.GridSize / 2,
                            Room.GridSize / 2,
                            Handles.SphereHandleCap))
                        {
                            curPointIndex = i;
                        }
                    }
                    
                    break;

                case(LineType.Bezier):
                    Handles.color = Color.green;
                    Vector3[] bezierPoints = line.GetDrawPoints();
                    Handles.DrawBezier(bezierPoints[0], bezierPoints[1], bezierPoints[2], bezierPoints[3], Color.green, null, 1f);

                    if (i == 0)
                    {
                        Handles.color = Color.yellow;
                    }
                    else if (i == 1)
                    {
                        Handles.color = Color.red;
                    }
                    else
                    {
                        Handles.color = Color.green;
                    }


                    if(room.curInspectorLineType != LineType.Edit)
                    {
                        Vector3 newBezierHeadPos = Handles.FreeMoveHandle(bezierPoints[0],
                                Quaternion.identity,
                                Room.GridSize / 2,
                                new Vector3(Room.GridSize, Room.GridSize, Room.GridSize),
                                Handles.CircleHandleCap);

                        //Draw lines to show handles
                        Handles.color = Color.blue;
                        Handles.DrawAAPolyLine(new Vector3[] { bezierPoints[0], bezierPoints[2] });
                        Handles.DrawAAPolyLine(new Vector3[] { bezierPoints[1], bezierPoints[3] });

                        //Draw handles
                        Vector3 newBezierHandleOnePos = Handles.FreeMoveHandle(bezierPoints[2],
                                    Quaternion.identity,
                                    Room.GridSize / 2,
                                    new Vector3(Room.GridSize, Room.GridSize, Room.GridSize),
                                    Handles.RectangleHandleCap);
                        Vector3 newBezierHandleTwoPos = Handles.FreeMoveHandle(bezierPoints[3],
                                    Quaternion.identity,
                                    Room.GridSize / 2,
                                    new Vector3(Room.GridSize, Room.GridSize, Room.GridSize),
                                    Handles.RectangleHandleCap);


                        //Move line
                        Vector2 convertedBezierHeadPos = new Vector2(newBezierHeadPos.x, newBezierHeadPos.z);
                        if (convertedBezierHeadPos != line.lineStart)
                        {
                            MoveLine(room, i, convertedBezierHeadPos);
                            UpdatePrefab();
                        }

                        //Move start handle
                        Vector2 convertedBezierOneHandle = new Vector2(newBezierHandleOnePos.x, newBezierHandleOnePos.z);
                        if (convertedBezierOneHandle != line.bezierStartHandle)
                        {
                            line.bezierStartHandle = convertedBezierOneHandle;
                            UpdatePrefab();
                        }

                        //Move end handle
                        Vector2 convertedBezierTwoHandle = new Vector2(newBezierHandleTwoPos.x, newBezierHandleTwoPos.z);
                        if (convertedBezierTwoHandle != line.bezierEndHandle)
                        {
                            line.bezierEndHandle = convertedBezierTwoHandle;
                            UpdatePrefab();
                        }
                    }
                    else
                    {
                        if (Handles.Button(bezierPoints[0],
                            Quaternion.identity,
                            Room.GridSize / 2,
                            Room.GridSize / 2,
                            Handles.CubeHandleCap))
                        {
                            curPointIndex = i;
                        }
                    }
                    
                    break;

                case (LineType.Door):
                    Handles.color = Color.red;
                    Handles.DrawAAPolyLine(line.GetDrawPoints());
                    break;

                default:
                    Debug.LogError("Invalid Room Line Type!");
                    break;
            }
        }

        //Reset handle color
        Handles.color = Color.white;
    }

    //newPos is in (x, z) coords in world space
    private void MoveLine(Room room, int index, Vector2 newPos)
    {
        //Update self
        room.roomLines[index].lineStart = newPos;
        //Update previous
        if(index == 0)
        {
            room.roomLines[room.roomLines.Count - 1].lineEnd = room.roomLines[index].lineStart;
        }
        else
        {
            room.roomLines[(index - 1) % room.roomLines.Count].lineEnd = room.roomLines[index].lineStart;
        }
    }

    //Used to add new points to the outline
    private void DrawNewPointHandle(Room room)
    {
        //Gets the mouse pos in world space
        Vector3 mousePos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
        mousePos = new Vector3(mousePos.x, 0f, mousePos.z);

        int curStartIndex = 0;
        float closestDist = -1f;
        Vector3 closestPos = Vector3.zero;
        for(int i = 0; i < room.roomLines.Count; i++)
        {
            RoomLine line = room.roomLines[i];

            Vector3[] points = line.GetCollisionPoints();
            float newDist = Vector3.Distance(mousePos, HandleUtility.ClosestPointToPolyLine(points));

            //First iteration OR new closest
            if(closestDist < 0f || newDist < closestDist)
            {
                closestDist = newDist;
                closestPos = HandleUtility.ClosestPointToPolyLine(points);
                curStartIndex = i;
            }
        }


        if (Handles.Button(closestPos,
                            Quaternion.identity,
                            Room.GridSize / 2,
                            Room.GridSize / 2,
                            Handles.SphereHandleCap))
        {
            AddPoint(room, curStartIndex, closestPos);
        }
    }

    private void AddPoint(Room room, int startIndex, Vector3 point)
    {
        RoomLine newLine = new RoomLine(new Vector2(point.x, point.z));

        //Only 1 edge case bc of where points get added
        if(startIndex == room.roomLines.Count - 1)
        {
            //Edge case
            room.roomLines[startIndex].lineEnd = newLine.lineStart;
            newLine.lineEnd = room.roomLines[0].lineStart;
        }
        else
        {
            //Normal case
            room.roomLines[startIndex].lineEnd = newLine.lineStart;
            newLine.lineEnd = room.roomLines[startIndex + 1].lineStart;
        }

        room.roomLines.Insert(startIndex + 1, newLine);
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
