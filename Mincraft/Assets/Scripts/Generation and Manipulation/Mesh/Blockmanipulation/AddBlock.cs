using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class AddBlock : MonoBehaviour, IMouseUsable
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
    
    [SerializeField] private int mouseButtonIndex = 1;
    [SerializeField] private float raycastHitable = 1000f;
    [SerializeField] private BlockUV blockUV = BlockUV.Dirt;
    
    private (Vector3Int lowerBound, Vector3Int higherBound) tuple;
    
    private Camera cameraRef;
    
    private MeshModifier modifier;
    private ConcurrentQueue<MeshData> meshDatas;
    
    private void Start()
    {
        cameraRef = Camera.main;
        modifier = new MeshModifier();
        meshDatas = new ConcurrentQueue<MeshData>();
        modifier.MeshAvailable += (s, data) => meshDatas.Enqueue(data);
    }

    public void SetBlock(BlockUV uv) => blockUV = uv;

    private void Update()
    {
        if (Input.GetMouseButtonDown(mouseButtonIndex))
        {
            Ray ray = cameraRef.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f))
            {
                Vector3Int centerCube = Vector3Int.FloorToInt(ModifyMesh.CenteredClickPositionOutSide(hit.point, hit.normal));
                Block block = new Block(centerCube)
                {
                    ID = (int) blockUV
                };

                IChunk temp = ChunkGameObjectDictionary.GetValue(hit.transform.gameObject);
                IChunk chunk = temp.TryAddBlock(block, hit.normal);

                if (chunk != temp)
                {
                    chunk.TryAddBlock(block, hit.normal);
                    var data1 = ModifyMesh.Combine(chunk);
                    modifier.RedrawMeshFilter(chunk.CurrentGO, data1);
                }

                if (chunk == null)
                    throw new Exception("Kein Nachbar. Erstelle neuen Chunk genau dort. Also hier.");

                //Synchronous mesh modification and recalculation
                var data = ModifyMesh.Combine(temp);
                modifier.RedrawMeshFilter(temp.CurrentGO, data);

                var bounds = temp.GetChunkBounds();
                this.tuple = bounds;
                var tuple = IsBoundBlock(bounds, centerCube);
                
                // Asynchronous mesh modification and recalculation 
                if (tuple.Result)
                {
                    for (int i = 0; i < tuple.Directions.Length; i++)
                    {
                        Vector3Int pos = tuple.Directions[i] + temp.Position;
                        IChunk neigbourChunk = ChunkDictionary.GetValue(pos);

                        if (neigbourChunk != null)
                        {
                            modifier.Combine(neigbourChunk);
                        }
                    }
                }
            }
        }

        if (meshDatas.Count > 0)
        {
            while (meshDatas.TryDequeue(out var data))
            {
                modifier.RedrawMeshFilter(data.GameObject, data);
            }
        }
    }
    
    public (Vector3Int[] Directions, bool Result) IsBoundBlock((Vector3Int lowerBound, Vector3Int higherBound) tuple, Vector3Int pos)
    {
        List<Vector3Int> directions = new List<Vector3Int>();
        bool result = false;

        int maxSize = ChunkGenerator.GetMaxSize;
        
        
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