using System.Collections.Generic;
using UnityEngine;

namespace Core.Builder
{
    public class MeshGrassAdder : IMeshAdder
    {
        public void AddMesh(MeshAdderParameter parameters)
        {
            AddGrassNorthSouth(parameters.BlockPos, parameters.Vertices, parameters.TransparentTriangles, parameters.Uvs, parameters.CurrentUVData);
            AddGrassWestEast(parameters.BlockPos, parameters.Vertices, parameters.TransparentTriangles, parameters.Uvs, parameters.CurrentUVData);
        }
        
        private static void AddGrassWestEast(in Vector3 blockPos, List<Vector3> vertices, List<int> transparentTriangles, List<Vector2> uvs, UVData[] currentUVData)
        {
            int vc = vertices.Count;

            float offset = 0.146447f;
            float offsetMinusOne = 0.853553f;
            
            vertices.Add(new Vector3(blockPos.x + offset, blockPos.y, blockPos.z + offsetMinusOne));
            vertices.Add(new Vector3(blockPos.x + offsetMinusOne, blockPos.y, blockPos.z + offset));
            vertices.Add(new Vector3(blockPos.x + offsetMinusOne, blockPos.y + 0.8f, blockPos.z + offset));
            vertices.Add(new Vector3(blockPos.x + offset, blockPos.y + 0.8f, blockPos.z + offsetMinusOne));
            
            transparentTriangles.Add(vc + 0);
            transparentTriangles.Add(vc + 1);
            transparentTriangles.Add(vc + 3);
            
            transparentTriangles.Add(vc + 1);
            transparentTriangles.Add(vc + 2); 
            transparentTriangles.Add(vc + 3);

            vc += 4;
            
            vertices.Add(new Vector3(blockPos.x + offset, blockPos.y, blockPos.z + offsetMinusOne));
            vertices.Add(new Vector3(blockPos.x + offsetMinusOne, blockPos.y, blockPos.z + offset));
            vertices.Add(new Vector3(blockPos.x + offsetMinusOne, blockPos.y + 0.8f, blockPos.z + offset));
            vertices.Add(new Vector3(blockPos.x + offset, blockPos.y + 0.8f, blockPos.z + offsetMinusOne));
            
            transparentTriangles.Add(vc + 0);
            transparentTriangles.Add(vc + 3);
            transparentTriangles.Add(vc + 1);
            
            transparentTriangles.Add(vc + 1);
            transparentTriangles.Add(vc + 3); 
            transparentTriangles.Add(vc + 2);
            
            
            UVData uvdata = currentUVData[0];

            
            uvs.Add(new Vector2(uvdata.TileX + uvdata.SizeX, uvdata.TileY));
            uvs.Add(new Vector2(uvdata.TileX, uvdata.TileY));
            uvs.Add(new Vector2(uvdata.TileX, uvdata.TileY + uvdata.SizeY));
            uvs.Add(new Vector2(uvdata.TileX + uvdata.SizeX,
                uvdata.TileY + uvdata.SizeY));
            
            uvs.Add(new Vector2(uvdata.TileX + uvdata.SizeX, uvdata.TileY));
            uvs.Add(new Vector2(uvdata.TileX, uvdata.TileY));
            uvs.Add(new Vector2(uvdata.TileX, uvdata.TileY + uvdata.SizeY));
            uvs.Add(new Vector2(uvdata.TileX + uvdata.SizeX,
                uvdata.TileY + uvdata.SizeY));
        }

        private static void AddGrassNorthSouth(in Vector3 blockPos, List<Vector3> vertices, List<int> transparentTriangles, List<Vector2> uvs, UVData[] currentUVData)
        {
            int vc = vertices.Count;

            float offset = 0.146447f;
            float offsetMinusOne = 0.853553f;
            
            vertices.Add(new Vector3(blockPos.x + offset, blockPos.y, blockPos.z + offset));
            vertices.Add(new Vector3(blockPos.x + offsetMinusOne, blockPos.y, blockPos.z + offsetMinusOne));
            vertices.Add(new Vector3(blockPos.x + offsetMinusOne, blockPos.y + 0.8f, blockPos.z + offsetMinusOne));
            vertices.Add(new Vector3(blockPos.x + offset, blockPos.y + 0.8f, blockPos.z + offset));
            
            transparentTriangles.Add(vc + 0);
            transparentTriangles.Add(vc + 1);
            transparentTriangles.Add(vc + 3);
            
            transparentTriangles.Add(vc + 1);
            transparentTriangles.Add(vc + 2); 
            transparentTriangles.Add(vc + 3);

            vc += 4;
            
            vertices.Add(new Vector3(blockPos.x + offset, blockPos.y, blockPos.z + offset));
            vertices.Add(new Vector3(blockPos.x + offsetMinusOne, blockPos.y, blockPos.z + offsetMinusOne));
            vertices.Add(new Vector3(blockPos.x + offsetMinusOne, blockPos.y + 0.8f, blockPos.z + offsetMinusOne));
            vertices.Add(new Vector3(blockPos.x + offset, blockPos.y + 0.8f, blockPos.z + offset));
            
            transparentTriangles.Add(vc + 0);
            transparentTriangles.Add(vc + 3);
            transparentTriangles.Add(vc + 1);
            
            transparentTriangles.Add(vc + 1);
            transparentTriangles.Add(vc + 3); 
            transparentTriangles.Add(vc + 2);
            
            
            UVData uvdata = currentUVData[0];

            
            uvs.Add(new Vector2(uvdata.TileX + uvdata.SizeX, uvdata.TileY));
            uvs.Add(new Vector2(uvdata.TileX, uvdata.TileY));
            uvs.Add(new Vector2(uvdata.TileX, uvdata.TileY + uvdata.SizeY));
            uvs.Add(new Vector2(uvdata.TileX + uvdata.SizeX,
                uvdata.TileY + uvdata.SizeY));
            
            uvs.Add(new Vector2(uvdata.TileX + uvdata.SizeX, uvdata.TileY));
            uvs.Add(new Vector2(uvdata.TileX, uvdata.TileY));
            uvs.Add(new Vector2(uvdata.TileX, uvdata.TileY + uvdata.SizeY));
            uvs.Add(new Vector2(uvdata.TileX + uvdata.SizeX,
                uvdata.TileY + uvdata.SizeY));
        }
    }
}