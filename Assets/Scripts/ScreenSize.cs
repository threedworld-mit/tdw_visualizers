using System;
using UnityEngine;


/// <summary>
///  Set the screen size at start.
/// </summary>
public class ScreenSize : MonoBehaviour
{
    private void Awake()
    {
        Screen.SetResolution(1280, 720, false);
    }
}
