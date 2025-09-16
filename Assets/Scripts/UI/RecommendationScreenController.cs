using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Recommendation;

namespace BilliardMasterAi.UI
{
    public class RecommendationScreenController : MonoBehaviour
    {
        [Header("Table + Balls (local meters)")]
        public Transform tableRoot;
        public Transform cueBall;
        public Transform targetBall;
        public Transform otherBall;

        [Header("Presenters")]
        public ShotPathPresenter pathA;
        public ShotPathPresenter pathB;

        [Header("Path Colors")]
        public Color colorA = new Color(0.2f, 0.7f, 1f);
        public Color colorB = new Color(1f, 0.5f, 0.2f);

        public void Recommend()
        {
            if (cueBall == null || targetBall == null || otherBall == null)
            {
                Debug.LogWarning("RecommendationScreen: assign ball transforms.");
                return;
            }

            Vector2 cue = ToTable2D(cueBall.position);
            Vector2 tar = ToTable2D(targetBall.position);
            Vector2 oth = ToTable2D(otherBall.position);

            var top2 = ShotPlanner.PlanTopShots(cue, tar, oth, 2);
            if (top2.Count > 0 && pathA != null)
            {
                pathA.pathColor = colorA;
                pathA.Present(top2[0], cue, tar);
            }
            if (top2.Count > 1 && pathB != null)
            {
                pathB.pathColor = colorB;
                pathB.Present(top2[1], cue, tar);
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

