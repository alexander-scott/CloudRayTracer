using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class ServerRTTValueProvider : AbstractValueProvider
    {
        public override float Value
        {
            get
            {
                if (DataController.Instance.performanceDictionary.ContainsKey(DataController.StatisticType.RTT))
                {
                    return DataController.Instance.performanceDictionary[DataController.StatisticType.RTT];
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
