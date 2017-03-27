using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorLine : MonoBehaviour
{
    private LineRenderer _lineRenderer;

    public LineRenderer LineRenderer
    {
        get
        {
            if (_lineRenderer == null)
            {
                _lineRenderer = GetComponent<LineRenderer>();
            }

            return _lineRenderer;
        }
    }
	
}
