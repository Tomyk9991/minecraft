using UnityEngine;

namespace Core.UI.Console
{
    /// <summary>
    /// Use this interface to declare, what game-monobehaviours to turn off / on, when the Console is displayed
    /// </summary>
    public interface IConsoleToggle
    {
        bool Enabled { get; set; }
    }
}
