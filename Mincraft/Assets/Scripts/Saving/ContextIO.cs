using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class ContextIO<T> where T : Context<T>, new()
{
    private string path;
    //Als Kontext kann man z. B. Chunks betrachten
    //Als Kontext kann man auch den Serializer und Deserializer betrachten, denn der wird immer of type T zurückgeben

    public ContextIO(string path, int worldIndex)
    {
        this.path = path;
    }

    public void SaveContext(T obj)
    {
        object data = obj.Data();

        string newCombinedPath = Path.Combine(path + "/chunk0.chk");

        using (FileStream fs = new FileStream(newCombinedPath, FileMode.Create))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, data);
            }
            catch (Exception e)
            {
                Debug.Log("Failed to serialize. Reason: " + e.Message);
            }
            finally
            {
                fs.Close();
            }
        }
    }

    public T LoadContext()
    {
        string newCombinedPath = Path.Combine(path + "/chunk0.chk");

        using (FileStream fs = new FileStream(newCombinedPath, FileMode.Open))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                object data = formatter.Deserialize(fs);
                T obj = new T();
                return obj.Caster(data);
            }
            catch (Exception e)
            {
                Debug.Log("Failed to deserialize. Reason: " + e.Message);
            }
            finally
            {
                fs.Close();
            }
        }

        return null;
    }
}
public class Context<T>
{
    private int ChunkCounter = 0;

    /// <summary>
    /// Returns null in case of not overwriting
    /// </summary>
    /// <returns></returns>
    public virtual object Data()
    {
        return null;
    }

    public virtual T Caster(object data)
    {
        return default(T);
    }
}
[System.Serializable]
public class ChunkSerializeHelper
{
    public Int3 ChunkPosition;
    public Block[] localBlocks;
}


