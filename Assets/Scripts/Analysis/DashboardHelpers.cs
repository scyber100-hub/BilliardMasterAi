using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Physics;

namespace BilliardMasterAi.Analysis
{
    public static class DashboardHelpers
    {
        // success: >=3 cushions and end close to target within hitThreshold meters
        // tti: time to first cushion impact (approx via IsCushion flag on timed points)
        public static void ComputeSuccessAndTTI(List<TimedTrajectoryPoint> actual, Vector2 targetLocal, out bool success, out float ttiSeconds)
        {
            success = false; ttiSeconds = 0f;
            if (actual == null || actual.Count == 0) return;

            // TTI
            ttiSeconds = FindFirstCushionTime(actual);

            // Cushion count
            int cushions = 0; foreach (var p in actual) if (p.IsCushion) cushions++;

            // Proximity at end
            Vector2 end = actual[actual.Count - 1].Position;
            float hitThreshold = CaromConstants.BallRadius * 2.2f; // a bit lenient
            float d = Vector2.Distance(end, targetLocal);
            success = cushions >= 3 && d <= hitThreshold;
        }

        private static float FindFirstCushionTime(List<TimedTrajectoryPoint> pts)
        {
            foreach (var p in pts)
                if (p.IsCushion) return Mathf.Max(0f, p.Time);
            return 0f;
        }
    }
}

