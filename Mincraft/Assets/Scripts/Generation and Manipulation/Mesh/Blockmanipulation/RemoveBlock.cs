using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class RemoveBlock : MonoBehaviour, IMouseUsable
{
    private int chunkSize;
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

    private Camera cameraRef;
    
    private MeshModifier modifier;
    private ConcurrentQueue<MeshData> meshDatas;
    private (Vector3Int lowerBound, Vector3Int higherBound) tuple;

    private Vector3Int latest;

    private void Start()
    {
        cameraRef = Camera.main;
        modifier = new MeshModifier();
        meshDatas = new ConcurrentQueue<MeshData>();
        chunkSize = ChunkGenerator.GetMaxSize;
        modifier.MeshAvailable += (s, data) => meshDatas.Enqueue(data);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(mouseButtonIndex))
        {
            Ray ray = cameraRef.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, RaycastHitable))
            {
                //GET BLOCK POSITION
                Vector3Int centerCube = Vector3Int.FloorToInt(ModifyMesh.CenteredClickPositionOutSide(hit.point, hit.normal) - hit.normal);
                //GET CHUNK TO OPERATE IN
                IChunk chunk = ChunkGameObjectDictionary.GetValue(hit.transform.gameObject);
                chunk.RemoveBlock(centerCube);

                MeshData data = ModifyMesh.Combine(chunk);
                modifier.RedrawMeshFilter(data.GameObject, data);
                
                var bounds = chunk.GetChunkBounds();
                this.tuple = bounds;
                var tuple = IsBoundBlock(bounds, centerCube);
                
                if (tuple.Result)
                {
                    for (int i = 0; i < tuple.Directions.Length; i++)
                    {
                        Vector3Int pos = tuple.Directions[i] + chunk.Position;
                        IChunk neigbourChunk = ChunkDictionary.GetValue(pos);

                        if (neigbourChunk != null)
                        {
                            MeshData data1 = ModifyMesh.Combine(neigbourChunk);
                            modifier.RedrawMeshFilter(data1.GameObject, data1);
                        }
                    }
                }
            }
        }
    }
    
    private (Vector3Int[] Directions, bool Result) IsBoundBlock((Vector3Int lowerBound, Vector3Int higherBound) tuple, Vector3Int pos)
    {
        List<Vector3Int> directions = new List<Vector3Int>();
        int maxSize = ChunkGenerator.GetMaxSize;
        bool result = false;
        
        
        if (pos.x == tuple.lowerBound.x || pos.x - 1 == tuple.lowerBound.x)
        {
            directions.Add(new Vector3Int(-maxSize, 0, 0));
            result = true;
        }

        if (pos.y == tuple.lowerBound.y || pos.y - 1 == tuple.lowerBound.y)
        {
            directions.Add(new Vector3Int(0, -maxSize, 0));
            result = true;
        }

        if (pos.z == tuple.lowerBound.z || pos.z - 1 == tuple.lowerBound.z)
        {
            directions.Add(new Vector3Int(0, 0, -maxSize));
            result = true;
        }
        
        if (pos.x == tuple.higherBound.x || pos.x + 1 == tuple.higherBound.x)
        {
            directions.Add(new Vector3Int(maxSize, 0, 0));
            result = true;
        }

        if (pos.y == tuple.higherBound.y || pos.y + 1 == tuple.higherBound.y)
        {
            directions.Add(new Vector3Int(0, maxSize, 0));
            result = true;
        }
        
        if (pos.z == tuple.higherBound.z || pos.z + 1 == tuple.higherBound.z)
        {
            directions.Add(new Vector3Int(0, 0, maxSize));
            result = true;
        }


        return (directions.ToArray(), result);
    }
}
