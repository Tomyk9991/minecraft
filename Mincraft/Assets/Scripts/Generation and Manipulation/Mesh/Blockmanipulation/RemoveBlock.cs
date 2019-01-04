using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.Rendering;

public class RemoveBlock : MonoBehaviour, IMouseUsable
{
    private Vector3Int chunkSize;
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
        chunkSize = ChunkManager.GetMaxSize;
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

                Vector3Int centerCube = Vector3Int.FloorToInt(
                    ModifyMesh.CenteredClickPosition(triangles, vertices, hit.normal, hit.triangleIndex) +
                    hit.transform.position);

                chunkManager.RemoveBlock(hit.transform.gameObject, chunk.GetBlock(centerCube));

                var bounds = chunk.GetChunkBounds();
                var tuple = ChunkManager.IsBoundBlock(bounds, centerCube);

                if (tuple.Result)
                {
                    for (int i = 0; i < tuple.Directions.Length; i++)
                    {
                        Vector3Int pos = tuple.Directions[i] + chunk.ChunkOffset;
                        IChunk neigbourChunk = ChunkDictionary.GetValue(pos);

                        if (neigbourChunk != null)
                        {
                            MeshData data = ModifyMesh.Combine(neigbourChunk);
                            ModifyMesh.RedrawMeshFilter(neigbourChunk.CurrentGO, data);
                        }
                    }
                }
            }
        }
    }
}
