using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    #region Singleton

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

    #endregion

    public Dictionary<Vector3, GameObject> objectDictionary = new Dictionary<Vector3, GameObject>();

    // Use this for initialization
    void Start ()
    {
        List<GameObject> gos = new List<GameObject>();

        foreach (DetectableObject detectObject in FindObjectsOfType<DetectableObject>())
        {
            if (gos.Contains(detectObject.gameObject))
            {
                continue;
            }

            gos.Add(detectObject.gameObject);
            objectDictionary[detectObject.transform.position] = detectObject.gameObject;
        }
    }

    public GameObject GetGameObject(Vector3 objectID)
    {
        return objectDictionary[objectID];
    }

    public void UpdateKey(Vector3 oldKey)
    {
        GameObject val = objectDictionary[oldKey];
        objectDictionary.Remove(oldKey);
        objectDictionary[val.transform.position] = val;
    }
}
