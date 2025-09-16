using UnityEngine;
using BilliardMasterAi.Recommendation;

namespace BilliardMasterAi.UI
{
    public class RecommendationCompareScreenController : MonoBehaviour
    {
        [Header("Table + Balls (local meters)")]
        public Transform tableRoot;
        public Transform cueBall;
        public Transform targetBall;
        public Transform otherBall;

        [Header("Compare Items (3)")]
        public PathCompareItem itemA;
        public PathCompareItem itemB;
        public PathCompareItem itemC;

        [Header("Risk Profile (optional)")]
        public RiskProfileController riskProfile;

        [Header("Sorting")]
        public bool sortByExpectedValue = true;

        public void RecommendCompare()
        {
            if (cueBall == null || targetBall == null || otherBall == null)
            {
                Debug.LogWarning("RecommendationCompare: assign ball transforms.");
                return;
            }

            Vector2 cue = ToTable2D(cueBall.position);
            Vector2 tar = ToTable2D(targetBall.position);
            Vector2 oth = ToTable2D(otherBall.position);

            var top3 = ShotPlanner.PlanTopShots(cue, tar, oth, 6); // get more, then sort and pick 3
            var prof = riskProfile ? (RiskProfile?)riskProfile.GetProfile() : null;
            if (sortByExpectedValue)
            {
                top3.Sort((a,b)=>
                {
                    var ma = ShotEvaluator.Evaluate(a, cue, tar, oth, prof);
                    var mb = ShotEvaluator.Evaluate(b, cue, tar, oth, prof);
                    return ShotEvaluator.ExpectedValue(mb).CompareTo(ShotEvaluator.ExpectedValue(ma));
                });
                if (top3.Count > 3) top3 = top3.GetRange(0,3);
            }
            if (top3.Count > 0 && itemA != null)
            {
                var m = ShotEvaluator.Evaluate(top3[0], cue, tar, oth, prof);
                itemA.Bind(top3[0], m, cue, tar);
            }
            if (top3.Count > 1 && itemB != null)
            {
                var m = ShotEvaluator.Evaluate(top3[1], cue, tar, oth, prof);
                itemB.Bind(top3[1], m, cue, tar);
            }
            if (top3.Count > 2 && itemC != null)
            {
                var m = ShotEvaluator.Evaluate(top3[2], cue, tar, oth, prof);
                itemC.Bind(top3[2], m, cue, tar);
            }
        }

        private Vector2 ToTable2D(Vector3 world)
        {
            if (tableRoot == null) return new Vector2(world.x, world.z);
            var local = tableRoot.InverseTransformPoint(world);
            return new Vector2(local.x, local.z);
        }
    }
}
