using System;
using System.Collections.Generic;
using System.IO;
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

        static MainMenuSavingManager()
        {
            Directory.CreateDirectory(Application.persistentDataPath);
        }

        public static void SaveSettings(SettingsData context)
        {
            File.WriteAllBytes(Path.Combine(Application.persistentDataPath, paths[(int) context.Finder]),
                Encoding.UTF8.GetBytes(JsonUtility.ToJson(context, true)));
        }

        public static void SaveWorldInformation(WorldInformation context, string worldDirectory)
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

        public static SettingsData LoadSettings()
        {
            string json = "";
            try
            {
                json = File.ReadAllText(Path.Combine(Application.persistentDataPath, paths[(int) DataContextFinder.Settings]));
            }
            catch (FileNotFoundException e)
            {
                Debug.Log(e);
                return new SettingsData(60, 5);
            }
            return JsonUtility.FromJson<SettingsData>(json);
        }
        
        public static void DeleteWorldInformation(WorldInformation worldInformation)
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
                    catch (Exception)
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
                        catch (ArgumentException)
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

            float result = (float) size / 0x100000;
            return result;
        }
    }
}