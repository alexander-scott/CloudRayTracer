using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleClass : MonoBehaviour
{
    void OnPreRender()
    {
        GL.wireframe = true;
    }
    void OnPostRender()
    {
        GL.wireframe = false;
    }
}
