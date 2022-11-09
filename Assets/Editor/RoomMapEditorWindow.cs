using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class RoomMapEditorWindow : EditorWindow
{
    private Room curRoom;
    private Vector2 editorPanOffset;
    public const int GridSize = 9;
    public float curZoom = 1f;
    public float minZoom = 0.25f;
    public float maxZoom = 4f;
    private Vector2 TESTINGdrawPos = Vector2.zero;

    //Render Room Prefab
    private PreviewRenderUtility previewRenderer;


    [MenuItem("Custom Tools/Room Map Editor")]
    private static void OpenWindow()
    {
        RoomMapEditorWindow window = GetWindow<RoomMapEditorWindow>();
        window.titleContent = new GUIContent("Room Map Editor", "Paint Room Grids");
        //window.titleContent.tooltip = "Paint Room Grids";
    }

    private void SetCurRoom(Room room)
    {
        if(curRoom == room)
        {
            return;
        }

        curRoom = room;

        editorPanOffset = Vector2.zero;
        curZoom = 1f;
        //TODO: modify what I'm looking at? may be unecessary
    }

    private void OnEnable()
    {
        wantsMouseMove = true;

        editorPanOffset = Vector2.zero;


        Selection.selectionChanged += CheckRoom;
        CheckRoom();
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= CheckRoom;
    }

    private void CheckRoom()
    {
        string folderPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(folderPath);

        if(prefab == null)
        {
            return;
        }

        Room newRoom = prefab.GetComponent<Room>();

        if(curRoom == null)
        {
            SetCurRoom(newRoom);
        }
        else if (newRoom != null)
        {
            SetCurRoom(newRoom);
        }
    }

    private void OnGUI()
    {
        if (curRoom == null)
        {
            EditorGUILayout.LabelField("No Room Selected.");
            CheckRoom();
            return;
        }

        DrawRoomPrefab();

        DrawGrid(GridSize, 0.2f, Color.gray);
        DrawGrid(GridSize * 5, 0.4f, Color.gray);

        //DrawConnectors();
        //DrawNodes();

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

    public void InitializePreviewRenderer()
    {
        previewRenderer = new PreviewRenderUtility();
        previewRenderer.cameraFieldOfView = 60;
        previewRenderer.camera.clearFlags = CameraClearFlags.Skybox;
        previewRenderer.camera.transform.position = new Vector3(0, 0, -5);
        previewRenderer.camera.farClipPlane = 1000;

        //Make the camera render the prefab flat
        previewRenderer.camera.orthographic = true;

        //My attempt at lights. May need to override
        //previewRenderer.lights[0] = new Light();
        //previewRenderer.lights[0].type = LightType.Directional;
        previewRenderer.lights[0].transform.rotation = FindDirectionalLights()[0].transform.rotation;
        previewRenderer.lights[0].intensity = 1;
        for (int i = 1; i < previewRenderer.lights.Length; i++)
        {
            previewRenderer.lights[i].intensity = 0;
        }
    }

    private Light[] FindDirectionalLights()
    {
        return GameObject.FindObjectsOfType<Light>().Where(light => light.type == LightType.Directional).ToArray();
    }

    private void DrawRoomPrefab()
    {
        MeshFilter[] meshFilters = curRoom.GetComponentsInChildren<MeshFilter>();
        SkinnedMeshRenderer[] skinnedMeshRenderers = curRoom.GetComponentsInChildren<SkinnedMeshRenderer>();

        if (meshFilters == null)
        {
            EditorGUILayout.LabelField("Selected prefab does not contain any meshes!");
            return; // The necessary components aren't present. Skip.
        }

        if (previewRenderer == null)
        {
            InitializePreviewRenderer();
        }

        //Put the camera above the room and point it down
        //TODO: Adjust for zoom & pan & height/floor
        previewRenderer.camera.transform.SetPositionAndRotation(curRoom.transform.position + new Vector3(0f, 10f, 0f) + new Vector3(editorPanOffset.x / (this.position.width / 2), 0f, editorPanOffset.y / (this.position.height / 2)) * curZoom, Quaternion.Euler(90f, 0f, 0f));

        Rect boundaries = new Rect(0, 0, this.position.width, this.position.height);
        previewRenderer.BeginPreview(boundaries, GUIStyle.none);

        //Draw all meshes
        foreach (MeshFilter filter in meshFilters)
        {
            MeshRenderer meshRenderer = filter.GetComponent<MeshRenderer>();
            if (meshRenderer)
            {
                DrawSelectedMesh(filter.sharedMesh, meshRenderer.sharedMaterial, filter.gameObject.transform);
            }
        }

        //Draw all skins (what are skins)
        foreach (SkinnedMeshRenderer skin in skinnedMeshRenderers)
        {
            Mesh mesh = new Mesh();
            skin.BakeMesh(mesh);
            DrawSelectedMesh(mesh, skin.sharedMaterial, skin.gameObject.transform);
        }

        previewRenderer.camera.Render();
        Texture render = previewRenderer.EndPreview();
        GUI.DrawTexture(new Rect(0, 0, boundaries.width, boundaries.height), render);
    }

    private void DrawSelectedMesh(Mesh mesh, Material material, Transform transform)
    {
        //Originally used local scale, caused issues
        previewRenderer.DrawMesh(mesh, Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale * curZoom), material, 0);
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
        //editorPanOffset = editorPanOffset + mousePos * scale;
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