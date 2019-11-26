using Core.Chunking;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    public class TestChunkBuffer : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        private List<GameObject> gameObjects = new List<GameObject>();

        public void DrawTest()
        {
            gameObjects.ForEach(Destroy);
            gameObjects.Clear();
            int dimension = ChunkBuffer.dimension;
            for (int x = 0; x < dimension; x++)
            {
                for (int y = 0; y < dimension; y++)
                {
                    var column = ChunkBuffer.GetChunkColumn(x, y);
                    GameObject g = Instantiate(prefab, column.GlobalPosition.ToVector3(), Quaternion.identity, this.transform);
                    g.GetComponent<TextMesh>().text = column.GlobalPosition.ToString();
                    g.GetComponentInChildren<TextMesh>().text = column.LocalPosition.ToString();

                    g.GetComponent<TextMesh>().color = column.DesiredForVisualization ? Color.green : Color.red;
                    g.GetComponentInChildren<TextMesh>().color = column.DesiredForVisualization ? Color.green : Color.red;

                    gameObjects.Add(g);
                }
            }
        }
    }
}
