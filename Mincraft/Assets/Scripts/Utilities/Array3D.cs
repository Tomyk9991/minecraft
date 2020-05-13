using System;
using System.Collections.Generic;
using Core.Math;

public class Array3D<T>
{
    public int Length => data.Length;
    public T[] RawData => data; 

    protected T[] data;
    protected readonly int width;

    public Array3D(T[] initialData) { data = initialData; }
    public Array3D(int sizeD)
    {
        this.width = sizeD;
        data = new T[sizeD * sizeD * sizeD];
    }

    public T this[int x, int y, int z]
    {
        get => data[Idx3D(x, y, z)];
        set => data[Idx3D(x, y, z)] = value;
    }

    private int Idx3D(int x, int y, int z)
        => x + width * (y + width * z);
}

public class ExtendedArray3D<T> : Array3D<T>
{
    public ExtendedArray3D(T[] initialData) : base(initialData)
    { }
    public ExtendedArray3D(int sizeD, int extension) : base(sizeD + (extension * 2))
    { }

    public new T this[int x, int y, int z]
    {
        //Map [(-1, -1, -1), (16, 16, 16)] to [(0, 0, 0), (17, 17, 17)];
        get => base[x + 1, y + 1, z + 1];
        set => base[x + 1, y + 1, z + 1] = value;
    }
}