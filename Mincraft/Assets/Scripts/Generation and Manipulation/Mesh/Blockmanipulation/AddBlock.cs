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
                Int3 centerCube = Int3.FloorToInt(ModifyMesh.CenteredClickPositionOutSide(hit.point, hit.normal));
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
                var tuple = IsBoundBlock(bounds, centerCube);
                
                // Asynchronous mesh modification and recalculation 
                if (tuple.Result)
                {
                    for (int i = 0; i < tuple.Directions.Length; i++)
                    {
                        Int3 pos = tuple.Directions[i] + temp.Position;
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
    
    public (Int3[] Directions, bool Result) IsBoundBlock((Int3 lowerBound, Int3 higherBound) tuple, Int3 pos)
    {
        List<Int3> directions = new List<Int3>();
        bool result = false;

        int maxSize = ChunkGenerator.GetMaxSize;
        
        
        if (pos.X == tuple.lowerBound.X || pos.X - 1 == tuple.lowerBound.X)
        {
            directions.Add(new Int3(-maxSize, 0, 0));
            result = true;
        }

        if (pos.Y == tuple.lowerBound.Y || pos.Y - 1 == tuple.lowerBound.Y)
        {
            directions.Add(new Int3(0, -maxSize, 0));
            result = true;
        }

        if (pos.Z == tuple.lowerBound.Z || pos.Z - 1 == tuple.lowerBound.Z)
        {
            directions.Add(new Int3(0, 0, -maxSize));
            result = true;
        }
        
        if (pos.X == tuple.higherBound.X || pos.X + 1 == tuple.higherBound.X)
        {
            directions.Add(new Int3(maxSize, 0, 0));
            result = true;
        }

        if (pos.Y == tuple.higherBound.Y || pos.Y + 1 == tuple.higherBound.Y)
        {
            directions.Add(new Int3(0, maxSize, 0));
            result = true;
        }
        
        if (pos.Z == tuple.higherBound.Z || pos.Z + 1 == tuple.higherBound.Z)
        {
            directions.Add(new Int3(0, 0, maxSize));
            result = true;
        }


        return (directions.ToArray(), result);
    }
}