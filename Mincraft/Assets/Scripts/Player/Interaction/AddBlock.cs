using System;
using Core.Builder;
using Core.Chunks;
using Core.Chunks.Threading;
using Core.Managers;
using Core.Math;
using UnityEngine;
using Core.UI.Console;

namespace Core.Player.Interaction
{
    public class AddBlock : MonoBehaviour, IMouseUsable, IConsoleToggle
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

        [SerializeField] private int mouseButtonIndex = 1;
        [SerializeField] private float raycastHitable = 1000f;
        [SerializeField] private BlockUV blockUV = BlockUV.Wood;

        private readonly int chunkSize = 0x10;

        private Camera cameraRef;
        private GameManager gameManager;
        private RaycastHit[] hit = new RaycastHit[1];
        private readonly Vector3 centerScreenNormalized = new Vector3(0.5f, 0.5f, 0f);

        public bool Enabled
        {
            get => this.enabled;
            set => this.enabled = value;
        }

        private void Start()
        {
            GoPool = ChunkGameObjectPool.Instance;
            cameraRef = Camera.main;
            gameManager = GameManager.Instance;
        }

        public void SetBlock(BlockUV uv) => blockUV = uv;

        private void Update()
        {
            if (Input.GetMouseButtonDown(mouseButtonIndex))
            {
                Ray ray = cameraRef.ViewportPointToRay(centerScreenNormalized);

                if (Physics.RaycastNonAlloc(ray, hit, RaycastHitable) > 0)
                {
                    Chunk currentChunk = hit[0].transform.GetComponent<ChunkReferenceHolder>().Chunk;

                    Int3 localPos = GlobalToRelativeBlock(MeshBuilder.CenteredClickPositionOutSide(hit[0].point, hit[0].normal), currentChunk.GlobalPosition);
                    
                    if (MathHelper.InChunkSpace(localPos))
                    {
                        currentChunk.AddBlock(new Block(blockUV), localPos);

                        if (MathHelper.BorderBlock(localPos))
                        {
                            Chunk neighbourChunk = currentChunk.ChunkNeighbour(GetDirection(localPos));
                            JobManager.JobManagerUpdaterInstance.RecalculateChunk(neighbourChunk);
                        }
                        JobManager.JobManagerUpdaterInstance.RecalculateChunk(currentChunk);
                    }
                    else
                    {
                        Chunk neighbourChunk = currentChunk.ChunkNeighbour(GetDirectionPlusOne(localPos));
                        neighbourChunk.AddBlock(new Block(blockUV), RelativeToLocalBlock(localPos));

                        if (MathHelper.BorderBlockPlusOne(localPos))
                        {
                            currentChunk.AddBlock(new Block(blockUV), localPos);
                            JobManager.JobManagerUpdaterInstance.RecalculateChunk(currentChunk);
                        }

                        JobManager.JobManagerUpdaterInstance.RecalculateChunk(neighbourChunk);
                    }
                }
            }
        }

        private Int3 GetDirectionPlusOne(in Int3 localPos)
        {
            if (localPos.X < 0) return Int3.Left;
            if (localPos.X >= chunkSize) return Int3.Right;
            
            if (localPos.Y < 0) return Int3.Down;
            if (localPos.Y >= chunkSize) return Int3.Up;
            
            if (localPos.Z < 0) return Int3.Back;
            if (localPos.Z >= chunkSize) return Int3.Forward;

            throw new Exception("Chunk not found");
        }
        
        private Int3 GetDirection(in Int3 localPos)
        {
            const int chunksizeMinusOne = 15;
            if (localPos.X == 0) return Int3.Left;
            if (localPos.X >= chunksizeMinusOne) return Int3.Right;
            
            if (localPos.Y == 0) return Int3.Down;
            if (localPos.Y >= chunksizeMinusOne) return Int3.Up;
            
            if (localPos.Z == 0) return Int3.Back;
            if (localPos.Z >= chunksizeMinusOne) return Int3.Forward;

            throw new Exception("Chunk not found");
        }

        private Int3 RelativeToLocalBlockMinusOne(Int3 relativeBlockPos)
        {
            int x = relativeBlockPos.X;
            int y = relativeBlockPos.Y;
            int z = relativeBlockPos.Z;
            
            if (relativeBlockPos.X == 15) x = -1;
            if (relativeBlockPos.X == 0)  x = 16;

            if (relativeBlockPos.Y == 15) y = -1;
            if (relativeBlockPos.Y == 0)  y = 16;
            
            if (relativeBlockPos.Z == 15) z = -1;
            if (relativeBlockPos.Z == 0)  z = 16;

            return new Int3(x, y, z);
        }
        
        private Int3 RelativeToLocalBlock(Int3 relativeBlockPos)
        {
            int x = relativeBlockPos.X % chunkSize;
            int y = relativeBlockPos.Y % chunkSize;
            int z = relativeBlockPos.Z % chunkSize;
            if (x < 0) x += chunkSize;
            if (y < 0) y += chunkSize;
            if (z < 0) z += chunkSize;

            return new Int3(x, y, z);
        }

        private Int3 GlobalToRelativeBlock(Vector3 globalBlockPos, Int3 globalChunkPos) 
            => new Int3((int) globalBlockPos.x - globalChunkPos.X, (int) globalBlockPos.y - globalChunkPos.Y, (int) globalBlockPos.z - globalChunkPos.Z);
    }
}