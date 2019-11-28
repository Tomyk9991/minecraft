using System;
using Core.Chunking;
using Core.Math;
using Extensions;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerMovementTracker : SingletonBehaviour<PlayerMovementTracker>
{
    public static event Action<Direction> OnDirectionModified;
    public static event Action<int, int> OnChunkPositionChanged;

    public int xPlayerPos = 0;
    public int zPlayerPos = 0;
    [Space]
    public int prevXPlayerPos = 0;
    public int prevZPlayerPos = 0;
    
    private int chunkSize;

    private Int3 latestPlayerPosition;

    private void Start()
    {
        chunkSize = ChunkSettings.ChunkSize;
        latestPlayerPosition = transform.position.ToInt3();
        
        xPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.X, chunkSize);
        zPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);
        prevXPlayerPos = xPlayerPos;
        prevZPlayerPos = zPlayerPos;
        OnChunkPositionChanged?.Invoke(xPlayerPos, zPlayerPos);
    }
    private void Update()
    {
        Direction d = Direction.None;
        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.position += Vector3.left * 8;
            d = Direction.Left;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            transform.position += Vector3.right * 8;
            d = Direction.Right;
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            transform.position += Vector3.forward * 8;
            d = Direction.Forward;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            transform.position += Vector3.back * 8;
            d = Direction.Back;
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            transform.position = new Vector3(0, 100, 0);
        }
        
        latestPlayerPosition = transform.position.ToInt3();
        xPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.X, chunkSize);
        zPlayerPos = MathHelper.ClosestMultiple(latestPlayerPosition.Z, chunkSize);

        if (prevXPlayerPos != xPlayerPos || prevZPlayerPos != zPlayerPos)
        {
            OnDirectionModified(d);
            prevXPlayerPos = xPlayerPos;
            prevZPlayerPos = zPlayerPos;
            OnChunkPositionChanged?.Invoke(xPlayerPos, zPlayerPos);
        }
    }

    public string PlayerPos()
    {
        return latestPlayerPosition.ToString();
    }
}
