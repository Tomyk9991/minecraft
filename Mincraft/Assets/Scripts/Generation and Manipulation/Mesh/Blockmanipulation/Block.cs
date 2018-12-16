using UnityEngine;

[System.Serializable]
public struct Block
{
    public Vector3Int Position;
    public Mesh Mesh;

    public UVSetter UVSetter { get; set; }
    public UVSetter UVSetter2 { get; set; }
    
    public bool HasUV2 { get; private set; }
    
    public Block(Vector3Int position, float tileX = 12, float tileY = 3)
    {
        HasUV2 = false;
        UVSetter = new UVSetter(12, 3);
        UVSetter2 = null;
        this.Position = position;
        
        var x = SetUVs.GetStandardMeshFilter();
        this.Mesh = new Mesh
        {
            vertices = x.vertices, 
            triangles = x.triangles, 
            name = this.Position.ToString()
        };
    }

    public void SetUV2(BlockUV blockUV)
    {
        HasUV2 = true;
        this.UVSetter2 = new UVSetter(0, 9);
    }
}
