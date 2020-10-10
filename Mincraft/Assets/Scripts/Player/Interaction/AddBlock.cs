using System;
using System.Runtime.CompilerServices;
using Core.Builder;
using Core.Chunks;
using Core.Chunks.Threading;
using Core.Managers;
using Core.Math;
using Core.Saving;
using Core.UI;
using UnityEngine;
using Core.UI.Console;
using Utilities;

namespace Core.Player.Interaction
{
    public class AddBlock : MonoBehaviour, IMouseUsable, IConsoleToggle, IFullScreenUIToggle
    {
        public float DesiredTimeUntilAction
        {
            get => timeBetweenRemove;
            set => timeBetweenRemove = value;
        }

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
        [SerializeField]
        private Camera cameraRef = null;

        [Space] 
        [SerializeField] private int mouseButtonIndex = 1;
        [SerializeField] private float raycastHitable = 1000f;
        [SerializeField] private float timeBetweenRemove = 0.1f;
        [SerializeField] private LayerMask hitMask = 0;
        [SerializeField] private BlockUV blockUV = BlockUV.Wood;
        private GameManager gameManager;

        
        private RaycastHit hit;

        private Int3 lp;

        private readonly Vector3 centerScreenNormalized = new Vector3(0.5f, 0.5f, 0f);

        private PlaceBlockHelper placer;
        private Timer timer;

        private void Start()
        {
            timer = new Timer(DesiredTimeUntilAction);

            placer = new PlaceBlockHelper
            {
                currentBlock =
                {
                    ID = blockUV
                }
            };
        }

        private void OnValidate()
            => timer.HardReset(timeBetweenRemove);

        public void SetBlock(BlockUV uv)
        {
            blockUV = uv;
            placer.currentBlock.ID = uv;
        } 

        private void Update()
        {
            if (Input.GetMouseButton(mouseButtonIndex) && timer.TimeElapsed(Time.deltaTime))
            {
                DoRaycast();
            }

            if (Input.GetMouseButtonDown(mouseButtonIndex))
            {
                DoRaycast();
                timer.Reset();
            }
        }

        private void DoRaycast()
        {
            Ray ray = cameraRef.ViewportPointToRay(centerScreenNormalized);

            if (Physics.Raycast(ray, out hit, RaycastDistance, hitMask))
            {
                ChunkReferenceHolder holder;
                if (!hit.transform.TryGetComponent(out holder))
                    return;

                Chunk currentChunk = holder.Chunk;

                placer.latestGlobalClick = MeshBuilder.CenteredClickPositionOutSide(hit.point, hit.normal);


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