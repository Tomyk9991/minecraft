using System.Collections;
using System.Collections.Generic;
using Core.Saving;
using TMPro;
using UnityEngine;

namespace Core.UI.MainMenu
{
    public class PlayMenuManager : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private GameObject mainMenuParent = null;
        [SerializeField] private GameObject playMenuParent = null;
        

        [Space(10)] 
        [SerializeField] private GameObject viewPortParent = null;
        [SerializeField] private GameObject itemPrefab = null;


        private void Start()
        {
            var worlds = SavingManager.LoadWorldInformation();

            if (worlds != null)
            {
                foreach (var t in worlds)
                {
                    GameObject g = Instantiate(itemPrefab, viewPortParent.transform);
                    g.GetComponentInChildren<TMP_Text>().text = $"{t.WorldName}\n" +
                                                                $"<size=17>World Size: {t.Size:F}MB";
                }
            }
        }
        
        //Called from Unity
        public void OnBackClick()
        {
            Save();
            playMenuParent.SetActive(false);
            mainMenuParent.SetActive(true);
        }
        
        private void Save()
        {
            
        }
    }

    public class WorldInformation : IDataContext
    {
        public DataContextFinder Finder => DataContextFinder.Settings;
        public float Size { get; set; }
        public string WorldName { get; set; }

        public WorldInformation()
        {
        }

        public WorldInformation(float size, string worldName)
        {
            Size = size;
            WorldName = worldName;
        }
    }
}