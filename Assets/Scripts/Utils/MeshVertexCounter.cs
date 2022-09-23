using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshVertexCounter : MonoBehaviour
{
    [SerializeField]
    private Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"Vertices: {mesh.vertexCount}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
