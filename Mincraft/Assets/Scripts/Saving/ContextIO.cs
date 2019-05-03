using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;

public class ContextIO<T> where T : Context<T>, new()
{
    public int WorldIndex { get; private set; }
    private string path;
    public ContextIO(string path, int worldIndex)
    {
        this.path = path;
        this.WorldIndex = worldIndex;
    }

    public void SaveContext(T obj, string fileName)
    {
        object data = obj.Data();
        string directoryPath = path + "/World" + WorldIndex + "/";
        string newCombinedPath = directoryPath + fileName + FileEnding(obj);

        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

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
        Debug.Log(path);
        string directoryPath = path + "/World" + WorldIndex + "/";

        if (!Directory.Exists(directoryPath))
        {
            Debug.LogError("Pfad / Welt wurde nicht gefunden");
            Debug.Log(directoryPath);
            return null;
        }

        string[] files = Directory.GetFiles(directoryPath, "*" + FileEnding(new T()), SearchOption.TopDirectoryOnly);

        Debug.Log("Noise Settings");
        for (int i = 0; i < files.Length; i++)
        {
            Debug.Log(files[i]);
        }

        return LoadContext(files[0]);
    }

    private T LoadContext(string path)
    {
        using (FileStream fs = new FileStream(path, FileMode.Open))
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

    public List<T> LoadContexts()
    {
        string directoryPath = path + "/World" + WorldIndex + "/";

        if (!Directory.Exists(directoryPath))
        {
            Debug.LogError("Pfad / Welt wurde nicht gefunden");
            return null;
        }


        string[] files = Directory.GetFiles(directoryPath, "*" + FileEnding(new T()), SearchOption.TopDirectoryOnly);

        return files.Select(LoadContext).ToList();
    }

    private string FileEnding(T obj)
    {
        switch (obj)
        {
            case Chunk _:
                return ".chk"; //CH un K
            case SimplexNoiseSettings _:
                return ".nsttngs"; //N oise S e TT i NGS
            default:
                Debug.LogError("Something went wrong with casting");
                return "";
        }
    }
}