using System.Collections;
using UnityEngine;
using UnityEditor;

public class RemoveBlock : MonoBehaviour, IMouseUsable
{
    public int MouseButtonIndex
    {
        get => mouseButtonIndex;
        set => mouseButtonIndex = value;
    }
    
    [SerializeField] private int mouseButtonIndex = 0;
    private ChunkManager chunkManager;

    private void Start()
    {
        this.chunkManager = ChunkManager.Instance;
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(mouseButtonIndex))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f))
            {
                int[] triangles = hit.transform.GetComponent<MeshFilter>().mesh.triangles;
                Vector3[] vertices = hit.transform.GetComponent<MeshFilter>().mesh.vertices;
                
                Vector3 centerCube = ModifyMesh.CenteredClickPosition(triangles, vertices, hit.normal, hit.triangleIndex) + hit.transform.position;
                Transform obj = ChunkDictionary.GetValue(centerCube);

                chunkManager.RemoveBlock(hit.transform.gameObject, obj);
            }
        }
    }

}
