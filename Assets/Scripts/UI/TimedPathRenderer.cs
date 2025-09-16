using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Physics;

namespace BilliardMasterAi.UI
{
    [RequireComponent(typeof(LineRenderer))]
    public class TimedPathRenderer : MonoBehaviour
    {
        public LineRenderer line;
        public float zOffset = 0.013f;

        void Reset()
        {
            line = GetComponent<LineRenderer>();
            line.positionCount = 0;
            line.widthMultiplier = 0.012f;
        }

        public void Draw(List<TimedTrajectoryPoint> path)
        {
            if (line == null) line = GetComponent<LineRenderer>();
            if (path == null || path.Count == 0) { line.positionCount = 0; return; }
            line.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
            {
                var p = path[i].Position;
                line.SetPosition(i, new Vector3(p.x, zOffset, p.y));
            }
        }
    }
}

