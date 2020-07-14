using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Core.UI.MainMenu;
using UnityEngine;

namespace Core.Saving
{
    public static class SavingManager
    {
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

        public static WorldInformation[] LoadWorldInformation()
        {
            string targetDirectory = Path.Combine(Application.persistentDataPath, paths[(int) DataContextFinder.WorldInformation]);

            string tempJson = "";
            if (Directory.Exists(targetDirectory))
            {
                string[] directories = Directory.GetDirectories(targetDirectory);
                WorldInformation[] information = new WorldInformation[directories.Length];
                
                for (int i = 0; i < directories.Length; i++)
                {
                    information[i] = new WorldInformation(DirSize(new DirectoryInfo(directories[i])), directories[i].Split('\\').Last());
                }

                return information;
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