using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Unity.Services.Analytics
{

    public class AnalyticEventsManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        public static void RecordCustomEventWithNoParameters()
        {
            AnalyticsService.Instance.CustomData("myEvent");
        }
    }
}
