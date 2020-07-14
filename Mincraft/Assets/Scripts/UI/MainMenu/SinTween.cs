using System.Collections;
using System.Collections.Generic;
using Core.Math;
using TMPro;
using UnityEditor.U2D;
using UnityEngine;

namespace Core.UI.MainMenu
{
    public class SinTween : MonoBehaviour
    {
        [SerializeField] private float minTween = 0.0f;
        [SerializeField] private float maxTween = 0.3f;
        
        [SerializeField] private TMP_Text text = null;
        
        private void Update()
        {
            text.outlineWidth = MathHelper.Map(Mathf.Sin(Time.time), -1f, 1f, minTween, maxTween);
        }
    }
}
