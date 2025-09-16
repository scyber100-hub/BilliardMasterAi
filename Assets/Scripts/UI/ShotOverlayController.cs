using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Physics;

namespace BilliardMasterAi.UI
{
    [RequireComponent(typeof(LineRenderer))]
    public class ShotOverlayController : MonoBehaviour
    {
        public LineRenderer line;
        public float zOffset = 0.01f; // slightly above table

        void Reset()
        {
            line = GetComponent<LineRenderer>();
            if (line != null)
            {
                line.positionCount = 0;
                line.widthMultiplier = 0.01f;
            }
        }

        public void DrawPath(List<TrajectoryPoint> path)
        {
            if (line == null) line = GetComponent<LineRenderer>();
            if (line == null || path == null || path.Count == 0) return;

            line.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
            {
                var p = path[i].Position;
                line.SetPosition(i, new Vector3(p.x, zOffset, p.y));
            }
        }
    }
}

