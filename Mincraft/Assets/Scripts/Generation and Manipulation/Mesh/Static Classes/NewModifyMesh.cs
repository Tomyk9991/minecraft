using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class NewModifyMesh
{
//    public static MeshData Combine(IChunk chunk)
//    {
//        List<Vector3> vertices = new List<Vector3>();
//        List<int> triangles = new List<int>();
//
//        List<Block> blocks = chunk.GetBlocks();
//        
//        for (int i = 0; i < blocks.Count; i++)
//        {
//            // Betrachte jeden Block. Sollte der gegebene Block in irgendeine Richtung keinen Nachbarn haben
//            // so ist es ein Block, der außen liegt.
//            // Das funktioniert sogar chunkunabhängig. Sprich "seitliche Wände" werden nicht gerendert, obwohl der Chunk
//            // da aufhört.
//            bool result = blocks[i].Neigbours.Any(results => results == false);
//
//            if (result == true)
//            {
//                
//            }
//            // ↑
//            vertices.AddRange(blocks[i].Mesh.vertices);
//
//            int[] newTri = blocks[i].Mesh.triangles;
//            
//            for (int j = 0; j < newTri.Length; j++)
//                newTri[j] += ((blocks.Count - 1) * 24);
//            
////            triangles.AddRange(blocks[i].Mesh.triangles);
//            triangles.AddRange(newTri);
//        }
//
//        return new MeshData(vertices, triangles);
//    }
}
