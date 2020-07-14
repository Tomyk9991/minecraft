using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.UI.MainMenu
{
    public class SliderChangeListener : MonoBehaviour
    {
        [SerializeField] private TMP_Text target = null;
             
        //Called from Unity
        public void ValueChanged(float value)
        {
            target.text = ((int) value).ToString();
        }
    }
}