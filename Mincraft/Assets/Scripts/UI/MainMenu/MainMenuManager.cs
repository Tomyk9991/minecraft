using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.UI.MainMenu
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject title;
        [SerializeField] private GameObject[] buttons;
        
        
        
        //Called by Unity
        public void OnPlayClick()
        {
            
        }
        
        //Called by Unity
        public void OnQuitClick()
        {
            Application.Quit();
        }
        
        //Called by Unity
        public void OnSettingsClick()
        {
            title.SetActive(false);
            foreach (var button in buttons)
            {
                button.SetActive(false);
            }
        }
    }
}
