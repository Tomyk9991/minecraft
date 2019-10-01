using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

using Core.Builder.Generation;
using Core.Chunking;

namespace Core.Saving
{
    public class ContextIO<T> where T : Context<T>, new()
    {
        public string Path { get; private set; }
        public ContextIO(string path)
        {
            this.Path = path;
        }

        public void SaveContext(T obj, string fileName)
        {
            object data = obj.Data();
            string newCombinedPath = Path + fileName + FileEnding<T>();

            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            using (FileStream fs = new FileStream(newCombinedPath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(fs, data);
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to serialize. Reason: " + e.Message);
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        public void Swap(string copyDestination, string tempPath)
        {
            string copyDestNew = Path + copyDestination + FileEnding<T>();
            string copyDestNewOld = Path + copyDestination + "Old" + FileEnding<T>();
            string tempPathNew = Path + tempPath + FileEnding<T>();

            if (!File.Exists(copyDestNew))
            {
                File.Move(tempPathNew, copyDestNew);
            }
            else
            {
                File.Move(copyDestNew, copyDestNewOld);
                File.Move(tempPathNew, copyDestNew);
                File.Move(copyDestNewOld, tempPathNew);
            }
        }

        public T LoadContext()
        {
            if (typeof(T) != typeof(NoiseSettings))
            {
                Debug.LogError("LoadContext darf nur von SimplexNoiseSettings ausgeführt werden");
                return null;
            }

            if (!Directory.Exists(Path))
            {
                Debug.LogError("Pfad / Welt wurde nicht gefunden");
                Debug.Log(Path);
                return null;
            }

            string[] files = Directory.GetFiles(Path, "*" + FileEnding<T>(), SearchOption.TopDirectoryOnly);

            if (files.Length == 0)
            {
                return null;
            }

            return LoadContext(files[0]);
        }

        public T LoadContext(string path)
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
            if (typeof(T) != typeof(Chunk))
            {
                Debug.LogError("LoadContexts darf nur von Chunk ausgeführt werden");
                return null;
            }
            if (!Directory.Exists(Path))
            {
                Debug.LogError("Pfad / Welt wurde nicht gefunden");
                return null;
            }


            string[] files = Directory.GetFiles(Path, "*" + FileEnding<T>(), SearchOption.TopDirectoryOnly);

            return files.Select(LoadContext).ToList();
        }

        public string FileEnding<K>()
        {
            if (typeof(K) == typeof(Chunk))
            {
                return ".chk";
            }
            else if (typeof(K) == typeof(NoiseSettings))
            {
                return ".nsttngs";
            }
            else
            {
                Debug.LogError("Something went wrong with casting");
                return null;
            }
        }
    }

    public class ContextIO
    {
        public static string DefaultPath = @"C:/Users/thoma/Documents/MinecraftCloneWorlds/";
        public static void CreateDirectory(int index, string path = @"C:/Users/thoma/Documents/MinecraftCloneWorlds")
        {
            string fullPath = path + "/World" + index + "/";
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(path + "/World" + index + "/");
                Debug.Log("Path created");
                return;
            }

            Debug.Log("Path already existed");
        }
    }
}