using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Core.UI.MainMenu;
using UnityEngine;

namespace Core.Saving
{
    public static class MainMenuSavingManager
    {
        private static readonly string WORLDJSON = "world.json";
        private static List<string> paths = new List<string>()
        {
            "settings.json",
            "Worlds"
        };

        public static void Save(IDataContext context)
        {
            File.WriteAllBytes(Path.Combine(Application.persistentDataPath, paths[(int) context.Finder]),
                Encoding.UTF8.GetBytes(JsonUtility.ToJson(context, true)));
        }
        
        public static void Save(IDataContext context, string worldDirectory = "")
        {
            if (worldDirectory == "")
            {
                File.WriteAllBytes(Path.Combine(Application.persistentDataPath, paths[(int) context.Finder]),
                    Encoding.UTF8.GetBytes(JsonUtility.ToJson(context, true)));
            }
            else
            {
                string directoryPath = Path.Combine(Application.persistentDataPath, paths[(int) context.Finder], worldDirectory);
                Directory.CreateDirectory(directoryPath);
                
                File.WriteAllBytes(Path.Combine(directoryPath, WORLDJSON), Encoding.UTF8.GetBytes(JsonUtility.ToJson(context, true)));
            }
        }

        public static T Load<T>(DataContextFinder finder) where T : new()
        {
            string json = "";
            try
            {
                json = File.ReadAllText(Path.Combine(Application.persistentDataPath, paths[(int) finder]));
            }
            catch (FileNotFoundException e)
            {
                Debug.Log(e);
                return new T();
            }
            return JsonUtility.FromJson<T>(json);
        }
        
        public static void Delete(WorldInformation worldInformation)
        {
            string path = Path.Combine(Application.persistentDataPath, paths[(int) worldInformation.Finder], worldInformation.WorldName);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        public static List<WorldInformation> LoadWorldInformation()
        {
            string targetDirectory = Path.Combine(Application.persistentDataPath, paths[(int) DataContextFinder.WorldInformation]);

            List<WorldInformation> info = new List<WorldInformation>();
            
            string tempJson = "";
            if (Directory.Exists(targetDirectory))
            {
                string[] directories = Directory.GetDirectories(targetDirectory);

                foreach (string directory in directories)
                {
                    string json = "";
                    try
                    {
                        json = File.ReadAllText(Path.Combine(directory, WORLDJSON));
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Could not load {WORLDJSON} from {directory}");
                    }

                    if (json != "")
                    {
                        try
                        {
                            WorldInformation w = JsonUtility.FromJson<WorldInformation>(json);
                            w.Size = DirSize(new DirectoryInfo(directory));
                            info.Add(w);
                        }
                        catch (ArgumentException e)
                        {
                            Debug.Log($"<color=#ff5555>Warning: \"{WORLDJSON}\" in directory: {directory} is corrupted</color>");
                        }
                    }
                }

                return info;
            }

            Debug.Log("Directory doesn't exist. Creating new directory at: " + targetDirectory);
            Directory.CreateDirectory(targetDirectory);
            return null;
        }
        
        private static float DirSize(DirectoryInfo d) 
        {    
            long size = 0;    

            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis) 
            {      
                size += fi.Length;    
            }
            
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis) 
            {
                size += (long) DirSize(di);   
            }

            float result = (float) size / 1_048_576l;
            return result;
        }
    }
}