using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using JetEngine;

[CustomEditor(typeof(Room))]
public class RoomMapEditorGizmos : Editor
{
    //TODO: LIMIT SIZE OF DOORS (use enum extension?)
    //TODO: LIMIT ANGLE OF DOORS
    //TODO: LOCK DOOR START POS TO A GRID LINE, X OR Y (use mod?)

    private int curPointIndex = -1;

    private void OnSceneGUI()
    {
        Room room = target as Room;
        if (Application.IsPlaying(room))
        {
            return;
        }

        //Above other things to maintain correct draw order
        //Draws Line stuff
        switch (room.curInspectorMode)
        {
            case (Room.RoomInspectorMode.View):

                break;

            case (Room.RoomInspectorMode.Paint):
                if (room.CurFloor.tileGrid != null)
                {
                    DrawGrid(room, Floor.GridSize);
                }

                DrawPaintingButtons(room);
                break;

            case (Room.RoomInspectorMode.Lines):
                if (room.CurFloor.roomLines != null && room.CurFloor.roomLines.Count >= 3)
                {
                    DrawLines(room);

                    if (room.curInspectorLineType == LineType.Add)
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

        DrawFloorButtons(room);

        if(room.mapFloors == null || room.mapFloors.Count == 0)
        {
            return;
        }

        DrawModeButtons(room);

        if (room.curInspectorMode == Room.RoomInspectorMode.Lines &&
            room.curInspectorLineType == LineType.Edit  &&
            curPointIndex != -1 &&
            room.CurFloor.roomLines[curPointIndex].CurLineType == LineType.Door)
        {
            DrawDoorsButtons(room);
        }
    }


    private void DrawFloorButtons(Room room)
    {
        Handles.BeginGUI();

        //SceneView sceneWindow = EditorWindow.GetWindow<SceneView>();
        //GUILayout.BeginArea(new Rect(10, sceneWindow.position.height - 165, 125, 125));
        GUILayout.BeginArea(new Rect(10, 10, 125, 125));

        Rect rect = EditorGUILayout.BeginVertical();
        GUI.color = Color.yellow;
        GUI.Box(rect, GUIContent.none);

        GUI.color = Color.white;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Floors");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Cur Floor = {room.curInspectedFloor}");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1f);


        if(room.mapFloors != null && room.mapFloors.Count != 0)
        {
            if (GUILayout.Button("Go Up"))
            {
                room.curInspectedFloor += 1;
                if(room.curInspectedFloor >= room.mapFloors.Count)
                {
                    room.curInspectedFloor = room.mapFloors.Count - 1;
                }
                UpdatePrefab();
            }

            if (GUILayout.Button("Go Down"))
            {
                room.curInspectedFloor -= 1;
                if (room.curInspectedFloor <= 0)
                {
                    room.curInspectedFloor = 0;
                }
                UpdatePrefab();
            }

            if (GUILayout.Button("Update Floors"))
            {
                room.SetupFloors();
                UpdatePrefab();
            }
        }
        else
        {
            if (GUILayout.Button("Setup Floors"))
            {
                room.SetupFloors();
                UpdatePrefab();
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();


        GUILayout.EndArea();

        Handles.EndGUI();
    }


    private void DrawGrid(Room room, float spacing)
    {
        int widthDivs = Mathf.CeilToInt(room.CurFloor.tileGrid.Length);
        int heightDivs = Mathf.CeilToInt(room.CurFloor.tileGrid[0].Length);

        //Not *really* needed but whatever
        Handles.color = Color.white;

        //Draw Tiles
        for(int i = 0; i < widthDivs; i++)
        {
            for(int j = 0; j < heightDivs; j++)
            {
                //Get Color for each tile based on tile's current state
                switch (room.CurFloor.tileGrid[i][j])
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
                if(Handles.Button(new Vector3((spacing * i + (spacing * 0.5f)) + room.gridOffset.x,
                    Floor.FloorHeight * room.curInspectedFloor,
                    (spacing * j + (spacing * 0.5f)) + room.gridOffset.y),
                    Quaternion.identity,
                    spacing,
                    spacing,
                    Handles.CubeHandleCap))
                {
                    //Paint the selected tile
                    room.CurFloor.tileGrid[i][j] = room.curPaintState;
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

        if(room.CurFloor.tileGrid != null)
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
                for (int i = 0; i < room.CurFloor.tileGrid.Length; i++)
                {
                    for (int j = 0; j < room.CurFloor.tileGrid[i].Length; j++)
                    {
                        if (room.CurFloor.tileGrid[i][j] == TileState.Empty)
                        {
                            room.CurFloor.tileGrid[i][j] = room.curPaintState;
                        }
                    }
                }

                UpdatePrefab();
            }

            if (GUILayout.Button("Reset"))
            {
                if (room.CurFloor.SetupGrid())
                {
                    for (int i = 0; i < room.CurFloor.tileGrid.Length; i++)
                    {
                        for (int j = 0; j < room.CurFloor.tileGrid[i].Length; j++)
                        {
                            room.CurFloor.tileGrid[i][j] = TileState.Empty;
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
                room.CurFloor.SetupGrid();
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

        if (room.CurFloor.roomLines != null && room.CurFloor.roomLines.Count >= 3)
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
                        room.CurFloor.roomLines[curPointIndex].SetLineType(LineType.Straight);
                    }
                    UpdatePrefab();
                }

                if (GUILayout.Button("Bezier"))
                {
                    if (curPointIndex != -1)
                    {
                        room.CurFloor.roomLines[curPointIndex].SetLineType(LineType.Bezier);
                    }
                    UpdatePrefab();
                }

                if (GUILayout.Button("Door"))
                {
                    if (curPointIndex != -1)
                    {
                        room.CurFloor.roomLines[curPointIndex].SetLineType(LineType.Door);
                    }
                    UpdatePrefab();
                }
            }
            else
            {
                if (GUILayout.Button("Update Lines"))
                {
                    UpdateLines(room);

                    UpdatePrefab();
                }

                GUILayout.Space(40);
            }

            if (room.curInspectorLineType == LineType.Edit)
            {
                if (GUILayout.Button("Delete"))
                {
                    if (room.CurFloor.roomLines.Count > 3)
                    {
                        //Due to size restrictions, edge cases are mutually exclusive
                        if (curPointIndex == 0)
                        {
                            //Edge case for deleting point 1
                            room.CurFloor.roomLines[^1].lineEnd = room.CurFloor.roomLines[curPointIndex + 1].lineStart;
                        }
                        else if (curPointIndex == room.CurFloor.roomLines.Count - 1)
                        {
                            //Edge case for deleting point n
                            room.CurFloor.roomLines[curPointIndex - 1].lineEnd = room.CurFloor.roomLines[0].lineStart;
                        }
                        else
                        {
                            //Normal case
                            room.CurFloor.roomLines[curPointIndex - 1].lineEnd = room.CurFloor.roomLines[curPointIndex + 1].lineStart;
                        }

                        room.CurFloor.roomLines.RemoveAt(curPointIndex);

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
                room.CurFloor.SaveLines(room.gridOffset);

                UpdatePrefab();
            }

            if (GUILayout.Button("Reset"))
            {
                room.CurFloor.SetupLines();

                UpdatePrefab();
            }
        }
        else
        {
            if (GUILayout.Button("Setup Lines"))
            {
                room.CurFloor.SetupLines();
            }
        }


        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();


        GUILayout.EndArea();

        Handles.EndGUI();
    }

    //Preconditions: The current point being edited is of type door
    private void DrawDoorsButtons(Room room)
    {
        Handles.BeginGUI();

        GUILayout.BeginArea(new Rect(155, 10, 125, 65));

        Rect rect = EditorGUILayout.BeginVertical();
        GUI.color = Color.yellow;
        GUI.Box(rect, GUIContent.none);

        GUI.color = Color.white;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Doors");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Door Type [{room.CurFloor.roomLines[curPointIndex].CurDoorType}]");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1f);


        if (GUILayout.Button("Normal"))
        {
            room.CurFloor.roomLines[curPointIndex].SetDoorType(DoorType.Normal);

            UpdatePrefab();
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();


        GUILayout.EndArea();

        Handles.EndGUI();
    }

    private void DrawLines(Room room)
    {
        for(int i = 0; i < room.CurFloor.roomLines.Count; i++)
        {
            RoomLine line = room.CurFloor.roomLines[i];

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
                                Floor.GridSize / 2,
                                new Vector3(Floor.GridSize, Floor.GridSize, Floor.GridSize),
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
                            Floor.GridSize / 2,
                            Floor.GridSize / 2,
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
                                Floor.GridSize / 2,
                                new Vector3(Floor.GridSize, Floor.GridSize, Floor.GridSize),
                                Handles.CircleHandleCap);

                        //Draw lines to show handles
                        Handles.color = Color.blue;
                        Handles.DrawAAPolyLine(new Vector3[] { bezierPoints[0], bezierPoints[2] });
                        Handles.DrawAAPolyLine(new Vector3[] { bezierPoints[1], bezierPoints[3] });

                        //Draw handles
                        Vector3 newBezierHandleOnePos = Handles.FreeMoveHandle(bezierPoints[2],
                                    Quaternion.identity,
                                    Floor.GridSize / 2,
                                    new Vector3(Floor.GridSize, Floor.GridSize, Floor.GridSize),
                                    Handles.RectangleHandleCap);
                        Vector3 newBezierHandleTwoPos = Handles.FreeMoveHandle(bezierPoints[3],
                                    Quaternion.identity,
                                    Floor.GridSize / 2,
                                    new Vector3(Floor.GridSize, Floor.GridSize, Floor.GridSize),
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
                            Floor.GridSize / 2,
                            Floor.GridSize / 2,
                            Handles.CubeHandleCap))
                        {
                            curPointIndex = i;
                        }
                    }
                    
                    break;

                case (LineType.Door):
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
                    Vector3[] doorPoints = line.GetDrawPoints();
                    if (room.curInspectorLineType != LineType.Edit)
                    {
                        Vector3 newStraightPos = Handles.FreeMoveHandle(doorPoints[0],
                                Quaternion.identity,
                                Floor.GridSize / 2,
                                new Vector3(Floor.GridSize, Floor.GridSize, Floor.GridSize),
                                Handles.RectangleHandleCap);

                        Vector2 convertedHeadPos = new Vector2(newStraightPos.x, newStraightPos.z);
                        if (convertedHeadPos != line.lineStart)
                        {
                            MoveLine(room, i, convertedHeadPos);
                            UpdatePrefab();
                        }
                    }
                    else
                    {
                        if (Handles.Button(doorPoints[0],
                            Quaternion.identity,
                            Floor.GridSize / 2,
                            Floor.GridSize / 2,
                            Handles.CubeHandleCap))
                        {
                            curPointIndex = i;
                        }
                    }

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
        room.CurFloor.roomLines[index].lineStart = newPos;
        //Update previous
        if(index == 0)
        {
            room.CurFloor.roomLines[^1].lineEnd = room.CurFloor.roomLines[index].lineStart;
        }
        else
        {
            room.CurFloor.roomLines[(index - 1) % room.CurFloor.roomLines.Count].lineEnd = room.CurFloor.roomLines[index].lineStart;
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
        for(int i = 0; i < room.CurFloor.roomLines.Count; i++)
        {
            RoomLine line = room.CurFloor.roomLines[i];

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
                            Floor.GridSize / 2,
                            Floor.GridSize / 2,
                            Handles.SphereHandleCap))
        {
            AddPoint(room, curStartIndex, closestPos);
        }
    }

    private void AddPoint(Room room, int startIndex, Vector3 point)
    {
        RoomLine newLine = new RoomLine(new Vector2(point.x, point.z), room.curInspectedFloor);

        //Only 1 edge case bc of where points get added
        if(startIndex == room.CurFloor.roomLines.Count - 1)
        {
            //Edge case
            room.CurFloor.roomLines[startIndex].lineEnd = newLine.lineStart;
            newLine.lineEnd = room.CurFloor.roomLines[0].lineStart;
        }
        else
        {
            //Normal case
            room.CurFloor.roomLines[startIndex].lineEnd = newLine.lineStart;
            newLine.lineEnd = room.CurFloor.roomLines[startIndex + 1].lineStart;
        }

        room.CurFloor.roomLines.Insert(startIndex + 1, newLine);
    }

    public static void UpdatePrefab()
    {
        PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            EditorSceneManager.MarkSceneDirty(prefabStage.scene);
        }
    }

    private void UpdateLines(Room room)
    {
        //Cached for fast
        int numLines = room.CurFloor.roomLines.Count;

        //Allows editing Lines points in inspector
        for (int i = 0; i < numLines; i++)
        {
            //Cache the line and it's next set
            RoomLine curLine = room.CurFloor.roomLines[i];
            RoomLine endPointLine = room.CurFloor.roomLines[(i + 1) % numLines];

            //If we're dealing with a door, limit it
            if (curLine.CurLineType == LineType.Door)
            {
                //Force line start on to a grid line
                
                //How far a given point can be from a grid line
                float give = 0.00001f;


                int closestXLine = (int)Mathf.Clamp(
                    JetEngine.MathUtils.GetClosestEvenDivisor(curLine.lineStart.x - room.gridOffset.x, Floor.GridSize) / Floor.GridSize,
                    0f,
                    room.CurFloor.gridWidth);
                float distToX = Mathf.Abs(curLine.lineStart.x - ((closestXLine * Floor.GridSize) + room.gridOffset.x));
                //Debug.Log($"pos: {curLine.lineStart.x}; distToX: {distToX}; closest line? {closestXLine}; re point: {((closestXLine * Floor.GridSize) + room.gridOffset.x)}");
                
                //TODO: Allow for "Internal Offsets" i.e. doors shifted away from the grid.
                //      This is out of scope for now


                //If the point is not on an X line, try again!
                if(!(distToX > -give && distToX < give))
                {
                    int closestYLine = (int)Mathf.Clamp(
                    JetEngine.MathUtils.GetClosestEvenDivisor(curLine.lineStart.y - room.gridOffset.y, Floor.GridSize) / Floor.GridSize,
                    0f,
                    room.CurFloor.gridWidth);
                    float distToY = Mathf.Abs(curLine.lineStart.y - ((closestYLine * Floor.GridSize) + room.gridOffset.y));

                    //If the point is not on a Y line, snap to closest!
                    if (!(distToY > -give && distToY < give))
                    {
                        //Allows snapping to closest line, X or Y
                        //NOTE: Make something that allows the user to choose?
                        /*
                        if(distToX < distToY)
                        {
                            curLine.lineStart.x = ((closestXLine * Floor.GridSize) + room.gridOffset.x);
                        }
                        else
                        {
                            curLine.lineStart.y = ((closestYLine * Floor.GridSize) + room.gridOffset.y);
                        }
                        */

                        //Snaps to both X and Y
                        curLine.lineStart.x = ((closestXLine * Floor.GridSize) + room.gridOffset.x);
                        curLine.lineStart.y = ((closestYLine * Floor.GridSize) + room.gridOffset.y);

                        //Notify previous point about this
                        int prevIndex = (i == 0) ? room.CurFloor.roomLines.Count - 1 : i - 1;
                        room.CurFloor.roomLines[prevIndex].lineEnd = curLine.lineStart;
                    }
                }


                //Clamp end point distance and angle
                endPointLine.lineStart = curLine.CurDoorType.FixEndPoint(curLine.lineStart, endPointLine.lineStart);
            }

            curLine.lineEnd = room.CurFloor.roomLines[(i + 1) % numLines].lineStart;
        }
    }
}
