using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPool : SingletonBehaviour<BlockPool>
{
    [SerializeField] private GameObject blockPrefab = null;
    [SerializeField] private int blockThreshold = 1;
    
    public Queue<GameObject> GameObjectPool => gameObjectPool;
    
    private Queue<GameObject> gameObjectPool;
    
    private void Start()
    {
        gameObjectPool = new Queue<GameObject>();
        for (int i = 0; i < blockThreshold; i++)
        {
            GameObject g = Instantiate(blockPrefab, Vector3.up * 256, Quaternion.identity, transform);
            g.GetComponent<MeshRenderer>().enabled = false;

            this.gameObjectPool.Enqueue(g);
        }
    }
}
