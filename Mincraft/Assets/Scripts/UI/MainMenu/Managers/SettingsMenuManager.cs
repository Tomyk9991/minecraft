﻿using System;
using Core.Saving;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.MainMenu
{
    public class SettingsMenuManager : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private GameObject settingsParent = null;
        [SerializeField] private GameObject mainMenuParent = null;
        

        [SerializeField] private Slider fovSlider;
        [SerializeField] private Slider renderDistanceSlider;

        private void Start()
        {
            var data = SavingManager.Load<SettingsData>(DataContextFinder.Settings);
            this.fovSlider.value = data.fovSlider;
            this.renderDistanceSlider.value = data.renderDistance;
        }

        //Called from Unity
        public void OnBackClick()
        {
            Save();
            settingsParent.SetActive(false);
            mainMenuParent.SetActive(true);
        }

        private void Save()
        {
            SavingManager.Save(new SettingsData((int) fovSlider.value, (int) renderDistanceSlider.value));
        }
    }

    [Serializable]
    public class SettingsData : IDataContext
    {
        public DataContextFinder Finder => DataContextFinder.Settings;
        public int fovSlider;
        public int renderDistance;

        public SettingsData() { }
        
        public SettingsData(int fovSlider, int renderDistance)
        {
            this.fovSlider = fovSlider;
            this.renderDistance = renderDistance;
        }

    }
}