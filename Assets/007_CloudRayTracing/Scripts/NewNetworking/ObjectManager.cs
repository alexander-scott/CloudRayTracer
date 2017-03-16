using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    private static ObjectManager _instance;

    public static ObjectManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public Dictionary<Vector3, GameObject> m_instanceMap = new Dictionary<Vector3, GameObject>();

    // Use this for initialization
    void Start ()
    {
        List<GameObject> gos = new List<GameObject>();

        foreach (DetectableObject go in FindObjectsOfType<DetectableObject>())
        {
            if (gos.Contains(go.gameObject))
            {
                continue;
            }

            gos.Add(go.gameObject);
            m_instanceMap[go.transform.position] = go.gameObject;
        }
    }

    public GameObject GetGameObject(Vector3 objectID)
    {
        return m_instanceMap[objectID];
    }

    public void UpdateKey(Vector3 oldKey)
    {
        GameObject val = m_instanceMap[oldKey];
        m_instanceMap.Remove(oldKey);
        m_instanceMap[val.transform.position] = val;
    }
}
