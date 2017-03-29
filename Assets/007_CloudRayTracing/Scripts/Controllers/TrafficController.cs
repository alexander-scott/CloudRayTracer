using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class TrafficController : MonoBehaviour
    {
        #region Singleton

        private static TrafficController _instance;

        public static TrafficController Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
                wayPoints = wayPointParent.GetComponentsInChildren<Transform>();
            }
        }

        #endregion

        public GameObject wayPointParent;
        public GameObject carPrefab;

        public Transform[] wayPoints;
    }
}
