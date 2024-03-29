﻿using UnityEngine;

namespace Core.UI
{
    /// <summary>
    /// Use this interface to declare, what game-monobehaviours to turn off / on, when the console is displayed
    /// </summary>
    public interface IConsoleToggle
    {
        bool Enabled { get; set; }
    }
}
