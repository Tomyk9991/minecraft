using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.UI.MainMenu
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenuParent = null;
        [SerializeField] private GameObject settingsParent = null;
        [SerializeField] private GameObject playMenuParent = null;
        
        //Called by Unity
        public void OnPlayClick()
        {
            mainMenuParent.SetActive(false);
            playMenuParent.SetActive(true);
        }
        
        //Called by Unity
        public void OnQuitClick()
        {
            Application.Quit();
        }
        
        //Called by Unity
        public void OnSettingsClick()
        {
            mainMenuParent.SetActive(false);
            settingsParent.SetActive(true);
        }
    }
}
