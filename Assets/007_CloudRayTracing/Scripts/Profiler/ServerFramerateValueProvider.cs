using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class ServerFramerateValueProvider : AbstractValueProvider
    {
        public override float Value
        {
            get
            {
                if (DataController.Instance.performanceDictionary.ContainsKey(DataController.StatisticType.FPS))
                {
                    return DataController.Instance.performanceDictionary[DataController.StatisticType.FPS];
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
