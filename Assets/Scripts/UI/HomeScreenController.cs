using UnityEngine;
using BilliardMasterAi.Routines;

namespace BilliardMasterAi.UI
{
    public class HomeScreenController : MonoBehaviour
    {
        public RoutineBannerController banner;

        void Start()
        {
            if (banner == null)
            {
                banner = FindObjectOfType<RoutineBannerController>();
            }
            var routine = RoutineRecommender.GetTodaysRoutine();
            if (banner != null)
                banner.ShowRoutine(routine);
        }
    }
}

