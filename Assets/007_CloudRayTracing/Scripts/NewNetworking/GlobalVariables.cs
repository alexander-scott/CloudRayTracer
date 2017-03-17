using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalVariables
{
    public static bool isClient = false;
    public static bool activated = false;

    public static string ipAddress = PlayerPrefs.GetString("IPAddress", "127.0.0.1");
}
