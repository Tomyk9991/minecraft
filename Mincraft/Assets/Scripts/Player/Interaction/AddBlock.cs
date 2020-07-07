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
        private ChunkJobManager chunkJobManager;
        private GameManager gameManager;
        private RaycastHit hit;
        private Int3 dir;
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
            chunkJobManager = ChunkJobManager.ChunkJobManagerUpdaterInstance;
            gameManager = GameManager.Instance;
            
        }

        public void SetBlock(BlockUV uv) => blockUV = uv;

        private void Update()
        {
            if (Input.GetMouseButtonDown(mouseButtonIndex))
            {
                Ray ray = cameraRef.ViewportPointToRay(centerScreenNormalized);

                if (Physics.Raycast(ray, out hit, RaycastHitable))
                {
                    Chunk currentChunk = hit.transform.GetComponent<ChunkReferenceHolder>().Chunk;
                    Int3 localPos = GlobalToRelativeBlock(MeshBuilder.CenteredClickPositionOutSide(hit.point, hit.normal), currentChunk.GlobalPosition);
                    
                    if (MathHelper.InChunkSpace(localPos))
                    {
                        currentChunk.AddBlock(new Block(blockUV), localPos);

                        if (MathHelper.BorderBlock(localPos))
                        {
                            GetDirections(localPos, ref dir);

                            if (dir.X != 0)
                            {
                                Chunk neighbourChunk = currentChunk.ChunkNeighbour(dir.X == -1 ? Chunk.Directions[4] : Chunk.Directions[5]);
                                neighbourChunk.AddBlock(new Block(blockUV), RelativeToLocalBlockMinusOneX(localPos));
                                chunkJobManager.RecalculateChunk(neighbourChunk);
                            }

                            if (dir.Y != 0)
                            {
                                Chunk neighbourChunk = currentChunk.ChunkNeighbour(dir.Y == -1 ? Chunk.Directions[3] : Chunk.Directions[2]);
                                neighbourChunk.AddBlock(new Block(blockUV), RelativeToLocalBlockMinusOneY(localPos));
                                chunkJobManager.RecalculateChunk(neighbourChunk);
                            }

                            if (dir.Z != 0)
                            {
                                Chunk neighbourChunk = currentChunk.ChunkNeighbour(dir.Z == -1 ? Chunk.Directions[1] : Chunk.Directions[0]);
                                neighbourChunk.AddBlock(new Block(blockUV), RelativeToLocalBlockMinusOneZ(localPos));
                                chunkJobManager.RecalculateChunk(neighbourChunk);
                            }
                        }
                        chunkJobManager.RecalculateChunk(currentChunk);
                    }
                    // else
                    // {
                    //     Chunk neighbourChunk = currentChunk.ChunkNeighbour(GetDirectionPlusOne(localPos));
                    //     neighbourChunk.AddBlock(new Block(blockUV), RelativeToLocalBlock(localPos));
                    //
                    //     if (MathHelper.BorderBlockPlusOne(localPos))
                    //     {
                    //         currentChunk.AddBlock(new Block(blockUV), localPos);
                    //         JobManager.JobManagerUpdaterInstance.RecalculateChunk(currentChunk);
                    //     }
                    //
                    //     JobManager.JobManagerUpdaterInstance.RecalculateChunk(neighbourChunk);
                    // }
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
        
        private void GetDirections(in Int3 localPos, ref Int3 dir)
        {
            dir.X = 0;
            dir.Y = 0;
            dir.Z = 0;
            
            if (localPos.X == 0) dir.X = -1;
            if (localPos.X >= 15) dir.X = 1;

            if (localPos.Y == 0) dir.Y = -1;
            if (localPos.Y >= 15) dir.Y = 1;

            if (localPos.Z == 0) dir.Z = -1;
            if (localPos.Z >= 15) dir.Z = 1;
        }

        private Int3 RelativeToLocalBlockMinusOneX(Int3 relativeBlockPos)
        {
            int x = relativeBlockPos.X;
            int y = relativeBlockPos.Y;
            int z = relativeBlockPos.Z;
            
            if (relativeBlockPos.X == 15) x = -1;
            if (relativeBlockPos.X == 0)  x = 16;
            
            return new Int3(x, y, z);
        }
        
        private Int3 RelativeToLocalBlockMinusOneY(Int3 relativeBlockPos)
        {
            int x = relativeBlockPos.X;
            int y = relativeBlockPos.Y;
            int z = relativeBlockPos.Z;
            
            if (relativeBlockPos.Y == 15) y = -1;
            if (relativeBlockPos.Y == 0)  y = 16;
            
            return new Int3(x, y, z);
        }
        
        private Int3 RelativeToLocalBlockMinusOneZ(Int3 relativeBlockPos)
        {
            int x = relativeBlockPos.X;
            int y = relativeBlockPos.Y;
            int z = relativeBlockPos.Z;
            
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