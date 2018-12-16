using System.Collections;
using UnityEngine;
using UnityEditor;

public class RemoveBlock : MonoBehaviour, IMouseUsable
{
    public float RaycastHitable
    {
        get => raycastHitable;
        set => raycastHitable = value;
    }

    public int MouseButtonIndex
    {
        get => mouseButtonIndex;
        set => mouseButtonIndex = value;
    }
    
    [SerializeField] private float raycastHitable = 1000f;
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

            if (Physics.Raycast(ray, out RaycastHit hit, RaycastHitable))
            {
                int[] triangles = hit.transform.GetComponent<MeshFilter>().mesh.triangles;
                Vector3[] vertices = hit.transform.GetComponent<MeshFilter>().mesh.vertices;
                IChunk chunk = hit.transform.GetComponent<IChunk>();
                
                Vector3 centerCube = ModifyMesh.CenteredClickPosition(triangles, vertices, hit.normal, hit.triangleIndex) + hit.transform.position;
                Vector3Int obj = ChunkDictionary.GetValue(Vector3Int.FloorToInt(centerCube));

                chunkManager.RemoveBlock(hit.transform.gameObject, chunk.GetBlock(obj));
            }
        }
    }
}
