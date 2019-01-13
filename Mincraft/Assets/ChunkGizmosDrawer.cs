using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGizmosDrawer : MonoBehaviour
{
    private Vector3 position;
    private Vector3 size;
    
    private void OnDrawGizmosSelected()
    {   
        if (!Application.isPlaying || !ChunkGenerator.Instance.drawChunk || gameObject.name == "Unused chunk") 
            return;
        
        if (position == default || size == default)
        {
            position = ChunkGameObjectDictionary.GetValue(this.gameObject).Position.ToVector3();
            size = Vector3.one * ChunkGenerator.GetMaxSize;
        }
        
        Gizmos.DrawWireCube(position + size / 2, size);
        
    }
}
