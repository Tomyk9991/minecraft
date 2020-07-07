using System.Collections.Concurrent;
using System.Collections.Generic;
using Core.Builder;
using Core.Chunks;
using Core.Chunks.Threading;
using Core.Math;
using Core.UI.Console;
using UnityEngine;

namespace Core.Player.Interaction
{
    public class RemoveBlock : MonoBehaviour, IMouseUsable, IConsoleToggle
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

        public bool Enabled
        {
            get => this.enabled;
            set => this.enabled = value;
        }

        [Header("References")] 
        [SerializeField] private Camera cameraRef;

        [Space] 
        [SerializeField] private float raycastHitable = 1000f;
        [SerializeField] private int mouseButtonIndex = 0;

        private ChunkJobManager chunkJobManager;
        
        private readonly Vector3 centerScreenNormalized = new Vector3(0.5f, 0.5f, 0f);

        private RaycastHit hit;
        private Int3 dir;
        private Int3 dirPlusOne;
        private Int3 blockPos;
        private Vector3 latestGlobalClick;
        private Int3 latestGlobalClickInt;
        private Block currentBlock;

        private Int3 lp;

        private void Start()
        {
            cameraRef = Camera.main;
            chunkJobManager = ChunkJobManager.ChunkJobManagerUpdaterInstance;
            chunkSize = 0x10;
            currentBlock.ID = BlockUV.Air;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(mouseButtonIndex))
            {
                Ray ray = cameraRef.ViewportPointToRay(centerScreenNormalized);

                if (Physics.Raycast(ray, out hit, RaycastHitable))
                {
                    ChunkReferenceHolder holder;
                    if (!hit.transform.TryGetComponent(out holder))
                        return;

                    Chunk currentChunk = holder.Chunk;

                    latestGlobalClick = MeshBuilder.CenteredClickPositionOutSide(hit.point, hit.normal) - hit.normal;

                    latestGlobalClickInt.X = (int) latestGlobalClick.x;
                    latestGlobalClickInt.Y = (int) latestGlobalClick.y;
                    latestGlobalClickInt.Z = (int) latestGlobalClick.z;

                    GlobalToRelativeBlock(latestGlobalClick, currentChunk.GlobalPosition, ref lp);

                    if (MathHelper.InChunkSpace(lp))
                    {
                        HandleAddBlock(currentChunk, lp);
                    }
                    else
                    {
                        GetDirectionPlusOne(lp, ref dirPlusOne);
                        currentChunk = currentChunk.ChunkNeighbour(dirPlusOne);
                        GlobalToRelativeBlock(latestGlobalClick, currentChunk.GlobalPosition, ref lp);

                        HandleAddBlock(currentChunk, lp);
                    }
                }
            }
        }

        private void HandleAddBlock(Chunk currentChunk, Int3 localPos)
        {
            if (PlayerMovementTracker.CurrentStandingBlock == latestGlobalClickInt) return;

            currentChunk.AddBlock(new Block(), localPos);

            if (MathHelper.BorderBlock(localPos))
            {
                GetDirections(localPos, ref dir);

                if (dir.X != 0)
                {
                    Chunk neighbourChunk =
                        currentChunk.ChunkNeighbour(dir.X == -1 ? Chunk.Directions[4] : Chunk.Directions[5]);
                    RelativeToLocalBlockMinusOneX(localPos, ref blockPos);
                    neighbourChunk.AddBlock(currentBlock, blockPos);
                    chunkJobManager.RecalculateChunk(neighbourChunk);
                }

                if (dir.Y != 0)
                {
                    Chunk neighbourChunk =
                        currentChunk.ChunkNeighbour(dir.Y == -1 ? Chunk.Directions[3] : Chunk.Directions[2]);
                    RelativeToLocalBlockMinusOneY(localPos, ref blockPos);
                    neighbourChunk.AddBlock(currentBlock, blockPos);
                    chunkJobManager.RecalculateChunk(neighbourChunk);
                }

                if (dir.Z != 0)
                {
                    Chunk neighbourChunk =
                        currentChunk.ChunkNeighbour(dir.Z == -1 ? Chunk.Directions[1] : Chunk.Directions[0]);
                    RelativeToLocalBlockMinusOneZ(localPos, ref blockPos);
                    neighbourChunk.AddBlock(currentBlock, blockPos);
                    chunkJobManager.RecalculateChunk(neighbourChunk);
                }
            }

            chunkJobManager.RecalculateChunk(currentChunk);
        }

        private void GetDirectionPlusOne(in Int3 localPos, ref Int3 dir)
        {
            dir.X = 0;
            dir.Y = 0;
            dir.Z = 0;

            if (localPos.X < 0) dir.X = -1;
            if (localPos.X >= chunkSize) dir.X = 1;

            if (localPos.Y < 0) dir.Y = -1;
            if (localPos.Y >= chunkSize) dir.Y = 1;

            if (localPos.Z < 0) dir.Z = -1;
            if (localPos.Z >= chunkSize) dir.Z = 1;
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

        private void RelativeToLocalBlockMinusOneX(Int3 relativeBlockPos, ref Int3 blockPos)
        {
            blockPos.X = relativeBlockPos.X;
            blockPos.Y = relativeBlockPos.Y;
            blockPos.Z = relativeBlockPos.Z;

            if (relativeBlockPos.X == 15) blockPos.X = -1;
            if (relativeBlockPos.X == 0) blockPos.X = 16;
        }

        private void RelativeToLocalBlockMinusOneY(Int3 relativeBlockPos, ref Int3 blockPos)
        {
            blockPos.X = relativeBlockPos.X;
            blockPos.Y = relativeBlockPos.Y;
            blockPos.Z = relativeBlockPos.Z;

            if (relativeBlockPos.Y == 15) blockPos.Y = -1;
            if (relativeBlockPos.Y == 0) blockPos.Y = 16;
        }

        private void RelativeToLocalBlockMinusOneZ(Int3 relativeBlockPos, ref Int3 blockPos)
        {
            blockPos.X = relativeBlockPos.X;
            blockPos.Y = relativeBlockPos.Y;
            blockPos.Z = relativeBlockPos.Z;

            if (relativeBlockPos.Z == 15) blockPos.Z = -1;
            if (relativeBlockPos.Z == 0) blockPos.Z = 16;
        }

        private void GlobalToRelativeBlock(Vector3 globalBlockPos, Int3 globalChunkPos, ref Int3 globalToRel)
        {
            globalToRel.X = (int) globalBlockPos.x - globalChunkPos.X;
            globalToRel.Y = (int) globalBlockPos.y - globalChunkPos.Y;
            globalToRel.Z = (int) globalBlockPos.z - globalChunkPos.Z;
        }
    }
}