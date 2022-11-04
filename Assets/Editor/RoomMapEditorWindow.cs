using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class RoomMapEditorWindow : EditorWindow
{
    private Room curRoom;
    private Vector2 editorPanOffset;
    public const int GridSize = 10;
    public float curZoom = 1f;
    public float minZoom = 0.2f;
    public float maxZoom = 4f;
    private Vector2 TESTINGdrawPos = Vector2.zero;

    [MenuItem("Custom Tools/Room Map Editor")]
    private static void OpenWindow()
    {
        RoomMapEditorWindow window = GetWindow<RoomMapEditorWindow>();
        window.titleContent = new GUIContent("Room Map Editor");
    }


    private void SetCurRoom(Room room)
    {
        curRoom = room;

        editorPanOffset = Vector2.zero;
        //TODO: modify what I'm looking at? may be unecessary
    }

    private void OnEnable()
    {
        wantsMouseMove = true;

        editorPanOffset = Vector2.zero;
    }

    private void OnGUI()
    {
        if (curRoom == null)
        {
            //return;
        }

        DrawGrid(GridSize, 0.2f, Color.gray);
        DrawGrid(GridSize * 5, 0.4f, Color.gray);

        DrawConnectors();
        DrawNodes();

        ProcessEvents(Event.current);


        DrawTile(ConvertGridToScreen(TESTINGdrawPos), Color.red);

        //ScaleWindow();

        if (GUI.changed)
        {
            Repaint();
            Debug.Log($"Offset {{{editorPanOffset}}} Zoom {{{curZoom}}} Tile Pos {{{TESTINGdrawPos}}}");
        }
        //Repaint();
    }

    private void DrawConnectors()
    {
        //if (curWorld.nodes != null)
        //{
        //    for (int i = 0; i < curWorld.nodes.Count; i++)
        //    {
        //        curWorld.nodes[i].DrawConnectors();
        //    }
        //}
    }

    private void DrawNodes()
    {
        //if (curWorld.nodes != null)
        //{
        //    for (int i = 0; i < curWorld.nodes.Count; i++)
        //    {
        //        curWorld.nodes[i].Draw();
        //    }
        //}
    }

    private void ProcessEvents(Event e)
    {
        //TODO: is necessary?
        //EditorUtility.SetDirty(curRoom);

        if (e.type == EventType.Used)
        {
            return;
        }

        switch (e.type)
        {
            case (EventType.KeyDown):

                break;

            case (EventType.MouseDown):
                if(e.button == 0)
                {
                    //curWorld.scale *= 2;
                    TESTINGdrawPos = ConvertScreenToGrid(e.mousePosition);
                    GUI.changed = true;
                }
                if (e.button == 1)
                {
                    ProcessContextMenu(e.mousePosition);
                }
                break;

            case (EventType.MouseDrag):
                if (e.button == 2)
                {
                    OnDrag(e.delta);
                }
                break;

            case (EventType.ScrollWheel):
                if (e.delta.y < 0)
                {
                    // Scroll up
                    // Zoom in
                    curZoom *= 2;
                }
                else
                {
                    // Scroll down
                    // Zoom out
                    curZoom /= 2;
                }

                curZoom = Mathf.Clamp(curZoom, minZoom, maxZoom);

                Zoom(curZoom, e.mousePosition);
                break;
        }
    }

    private void DrawTile(Vector2 blCorner, Color c)
    {
        EditorGUI.DrawRect(new Rect(blCorner.x, blCorner.y, GridSize * curZoom, GridSize * curZoom), c);
    }

    private void ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();

        //genericMenu.AddItem(new GUIContent("Add Node"), false, () => OnClickAddNode(mousePosition));
        genericMenu.ShowAsContext();
    }

    private void ScaleWindow()
    {
        //TODO: Get scale to work
        //GUIUtility.ScaleAroundPivot(new Vector2(curWorld.scale, curWorld.scale), Vector2.zero);
    }

    private void OnDrag(Vector2 delta)
    {
        editorPanOffset += delta;// / curZoom;
        GUI.changed = true;
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        gridSpacing *= curZoom;

        int widthDivs = Mathf.FloorToInt((position.width / gridSpacing));
        int heightDivs = Mathf.FloorToInt((position.height / gridSpacing));

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        Vector2 gridOffset = editorPanOffset;
        Vector3 newOffset = new Vector3(gridOffset.x % gridSpacing, gridOffset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void Zoom(float scale, Vector2 mousePos)
    {
        GUI.changed = true;
        //TODO: Get zoom to work
        //curWorld.offset = curWorld.offset * scale + mousePos;
    }

    #region Mouse Pos Helpers
    private Vector2 ConvertScreenToEditor(Vector2 mousePos)
    {
        //TODO: Account for zoom
        return (mousePos - editorPanOffset) / curZoom;
    }

    private Vector2 ConvertScreenToGrid(Vector2 mousePos)
    {
        return ConvertEditorToGrid(ConvertScreenToEditor(mousePos));
    }

    private Vector2 ConvertEditorToScreen(Vector2 editorPos)
    {
        return (editorPos * curZoom) + editorPanOffset;
    }

    private Vector2 ConvertEditorToGrid(Vector2 editorPos)
    {
        return new Vector2(Mathf.Floor((editorPos.x) / GridSize), Mathf.Floor((editorPos.y) / GridSize));
    }

    private Vector2 ConvertGridToEditor(Vector2 gridPos)
    {
        return (gridPos * GridSize);
    }

    private Vector2 ConvertGridToScreen(Vector2 gridPos)
    {
        return ConvertEditorToScreen(ConvertGridToEditor(gridPos));
    }
    #endregion
}