using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RemoveBlock : MonoBehaviour, IMouseUsable, IRemoveChunk
{
    public ChunkGameObjectPool GoPool { get; set; }
    
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

    private void Start()
    {
        cameraRef = Camera.main;
        GoPool = ChunkGameObjectPool.Instance;
        modifier = new MeshModifier();
        meshDatas = new ConcurrentQueue<MeshData>();
        modifier.MeshAvailable += (s, data) => meshDatas.Enqueue(data);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(mouseButtonIndex))
        {
            Ray ray = cameraRef.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, RaycastHitable))
            {
                Int3 centerCube = Int3.FloorToInt(ModifyMesh.CenteredClickPositionOutSide(hit.point, hit.normal) - hit.normal);
                IChunk chunk = ChunkGameObjectDictionary.GetValue(hit.transform.gameObject);
                chunk.RemoveBlock(centerCube);

                MeshData data = ModifyMesh.Combine(chunk);
                modifier.RedrawMeshFilter(data.GameObject, data);
                
                var bounds = chunk.GetChunkBounds();
                var tuple = IsBoundBlock(bounds, centerCube);
                
                if (tuple.HasDirections)
                {
                    for (int i = 0; i < tuple.Directions.Length; i++)
                    {
                        Int3 pos = tuple.Directions[i] + chunk.Position;
                        IChunk neigbourChunk = ChunkDictionary.GetValue(pos);

                        if (neigbourChunk != null)
                        {
                            MeshData data1 = ModifyMesh.Combine(neigbourChunk);
                            modifier.RedrawMeshFilter(data1.GameObject, data1);
                        }
                    }
                }

                if (CheckIfNeedsToBeRemoved(chunk))
                {
                    RemoveChunk(chunk);

                    if (tuple.HasDirections)
                    {
                        for (int i = 0; i < tuple.Directions.Length; i++)
                        {
                            Int3 pos = tuple.Directions[i] + chunk.Position;
                            IChunk neigbourChunk = ChunkDictionary.GetValue(pos);

                            neigbourChunk?.CalculateNeigbours();
                        }
                    }
                }
            }
        }
    }


    public bool CheckIfNeedsToBeRemoved(IChunk chunk)
        => chunk.GetBlocks().All(b => b == null);

    public void RemoveChunk(IChunk chunk)
    {
        GameObject goToAddToPoolAgain = chunk.CurrentGO;
        
        ChunkGameObjectDictionary.Remove(goToAddToPoolAgain);
        ChunkDictionary.Remove(chunk.Position);
        
        //Füge GameObject dem Pool wieder hinzu
        GoPool.SetGameObject(goToAddToPoolAgain);

        chunk.CalculateNeigbours();
    }

    private (Int3[] Directions, bool HasDirections) IsBoundBlock((Int3 lowerBound, Int3 higherBound) tuple, Int3 pos)
    {
        List<Int3> directions = new List<Int3>();
        int maxSize = ChunkGenerator.GetMaxSize;
        bool result = false;
        
        
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
