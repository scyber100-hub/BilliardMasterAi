using UnityEngine;
using BilliardMasterAi.Recommendation;

namespace BilliardMasterAi.AR
{
    // Computes a recommended plan and renders it as AR guide overlay on the table.
    public class ARGuideScreenController : MonoBehaviour
    {
        public Transform tableRoot;
        public Transform cueBall;
        public Transform targetBall;
        public Transform otherBall;
        public ARGuideOverlay overlay;

        public void ShowGuide()
        {
            if (cueBall == null || targetBall == null || otherBall == null || overlay == null)
            {
                Debug.LogWarning("ARGuideScreen: missing refs");
                return;
            }
            Vector2 cue = ToTable2D(cueBall.position);
            Vector2 tar = ToTable2D(targetBall.position);
            Vector2 oth = ToTable2D(otherBall.position);

            var best = ShotPlanner.PlanShot(cue, tar, oth);
            overlay.tableRoot = tableRoot;
            overlay.ShowPlan(best);
        }

        private Vector2 ToTable2D(Vector3 world)
        {
            if (tableRoot == null) return new Vector2(world.x, world.z);
            var local = tableRoot.InverseTransformPoint(world);
            return new Vector2(local.x, local.z);
        }
    }
}

