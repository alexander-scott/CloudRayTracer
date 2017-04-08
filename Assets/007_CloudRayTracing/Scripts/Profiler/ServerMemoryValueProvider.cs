using System;

namespace BMW.Verification.CloudRayTracing
{
    public class ServerMemoryValueProvider : AbstractValueProvider
    {
        public override float Value
        {
            get
            {
                if (DataController.Instance.performanceDictionary.ContainsKey(DataController.StatisticType.MEM))
                {
                    return DataController.Instance.performanceDictionary[DataController.StatisticType.MEM];
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
