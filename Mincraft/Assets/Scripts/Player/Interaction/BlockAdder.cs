﻿using System;
using Core.Builder;
using Core.Chunks;
using Core.Math;
using UnityEngine;
using Player.Interaction.ItemWorldAdder;
using Utilities;

namespace Core.Player.Interaction.ItemWorldAdder
{
    public class BlockAdder : IItemWorldAdder
    {
        public static event Action<BlockUV> OnAddBlock;
        
        private PlaceBlockHelper placer;
        public Vector2 ItemRange => new Vector2(0, short.MaxValue);

        public void Initialize()
        {
            placer = new PlaceBlockHelper
            {
                currentBlock =
                {
                    ID = BlockUV.Air
                }
            };
        }
        
        public void OnPlace(int itemID, ChunkReferenceHolder holder, Ray ray, RaycastHit hit)
        {
            placer.currentBlock.ID = (BlockUV) itemID;
            
            OnAddBlock?.Invoke((BlockUV) itemID);

            if (placer.currentBlock.CanFaceInDifferentDirections())
            {
                Vector3 delta = new Vector3(hit.point.x - ray.origin.x, 0.0f, hit.point.z - ray.origin.z);
                BlockDirection blockDirection = MathHelper.BlockDirectionFromSignedAngle(Vector3.SignedAngle(delta, Vector3.forward, Vector3.up));
                placer.currentBlock.Direction = blockDirection;
            }
                
            Chunk currentChunk = holder.Chunk;

            placer.latestGlobalClick = MathHelper.CenteredClickPositionOutSide(hit.point, hit.normal);


            placer.latestGlobalClickInt.X = (int) placer.latestGlobalClick.x;
            placer.latestGlobalClickInt.Y = (int) placer.latestGlobalClick.y;
            placer.latestGlobalClickInt.Z = (int) placer.latestGlobalClick.z;

            placer.GlobalToRelativeBlock(placer.latestGlobalClick, currentChunk.GlobalPosition, ref placer.LocalPosition);

            if (MathHelper.InChunkSpace(placer.LocalPosition))
            {
                placer.HandleAddBlock(currentChunk, placer.LocalPosition);
            }
            else
            {
                placer.GetDirectionPlusOne(placer.LocalPosition, ref placer.dirPlusOne);
                currentChunk = currentChunk.ChunkNeighbour(placer.dirPlusOne);
                placer.GlobalToRelativeBlock(placer.latestGlobalClick, currentChunk.GlobalPosition, ref placer.LocalPosition);

                placer.HandleAddBlock(currentChunk, placer.LocalPosition);
            }
        }
    }
}