using UnityEngine;

/// <summary>
/// Use this interface to declare, what components to turn off / on, when the Console is displayed
/// </summary>
public interface IConsoleToggle
{
    bool Enabled { get; set; }
}
