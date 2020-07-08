using System.Collections.Concurrent;
using System.Collections.Generic;
using Core.Builder;
using Core.Chunks;
using Core.Chunks.Threading;
using Core.Math;
using Core.UI.Console;
using UnityEngine;
using Utilities;

namespace Core.Player.Interaction
{
    public class RemoveBlock : MonoBehaviour, IMouseUsable, IConsoleToggle
    {
        private int chunkSize;

        public float RaycastDistance
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
        
        private readonly Vector3 centerScreenNormalized = new Vector3(0.5f, 0.5f, 0f);

        private RaycastHit hit;
        
        private PlaceBlockHelper placer;

        private void Start()
        {
            cameraRef = Camera.main;
            placer = new PlaceBlockHelper
            {
                currentBlock =
                {
                    ID = BlockUV.Air
                }
            };
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(mouseButtonIndex))
            {
                Ray ray = cameraRef.ViewportPointToRay(centerScreenNormalized);

                if (Physics.Raycast(ray, out hit, RaycastDistance))
                {
                    ChunkReferenceHolder holder;
                    if (!hit.transform.TryGetComponent(out holder))
                        return;

                    Chunk currentChunk = holder.Chunk;

                    placer.latestGlobalClick = MeshBuilder.CenteredClickPositionOutSide(hit.point, hit.normal) - hit.normal;

                    
                    placer.latestGlobalClickInt.X = (int) placer.latestGlobalClick.x;
                    placer.latestGlobalClickInt.Y = (int) placer.latestGlobalClick.y;
                    placer.latestGlobalClickInt.Z = (int) placer.latestGlobalClick.z;

                    placer.GlobalToRelativeBlock(placer.latestGlobalClick, currentChunk.GlobalPosition, ref placer.lp);

                    if (MathHelper.InChunkSpace(placer.lp))
                    {
                        placer.HandleAddBlock(currentChunk, placer.lp);
                    }
                    else
                    {
                        placer.GetDirectionPlusOne(placer.lp, ref placer.dirPlusOne);
                        currentChunk = currentChunk.ChunkNeighbour(placer.dirPlusOne);
                        placer.GlobalToRelativeBlock(placer.latestGlobalClick, currentChunk.GlobalPosition, ref placer.lp);

                        placer.HandleAddBlock(currentChunk, placer.lp);
                    }
                }
            }
        }
    }
}