using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Core.UI
{
    /// <summary>
    /// Use this interface to declare, what game-monobehaviours to turn off / on, when the fullscreen-UI is displayed
    /// </summary>
    public interface IFullScreenUIToggle
    {
        bool Enabled { get; set; }
    }
}