using System.Collections.Generic;
using UnityEngine;

namespace BilliardMasterAi.Physics
{
    public static class PhysicsFacade
    {
        public static List<TrajectoryPoint> SimulateCue(Vector2 cue, Vector2 obj1, Vector2 obj2, float angleDeg, float speed, float spinY)
        {
            Vector2 dir = new Vector2(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad)).normalized;
            if (PhysicsConfig.UseAdvanced)
            {
                return AdvancedCaromPhysics.SimulateCuePath(cue, obj1, obj2, dir * speed, spinY, 8f, 0.002f);
            }
            else
            {
                var s = new BallState { Position = cue, Velocity = dir * speed, SpinZ = spinY };
                return TrajectorySimulator.Simulate(s, maxTime: 7f, dt: 0.006f, maxBounces: 12);
            }
        }

        public static AdvancedCaromPhysics.SimResult SimulateCueDetailed(Vector2 cue, Vector2 obj1, Vector2 obj2, float angleDeg, float speed, float spinY)
        {
            Vector2 dir = new Vector2(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad)).normalized;
            if (PhysicsConfig.UseAdvanced)
            {
                return AdvancedCaromPhysics.SimulateAll(cue, obj1, obj2, dir * speed, spinY, 8f, 0.002f);
            }
            else
            {
                // Fallback: build only cue path
                var s = new BallState { Position = cue, Velocity = dir * speed, SpinZ = spinY };
                var timed = new List<TimedTrajectoryPoint>();
                float t = 0f; var simple = TrajectorySimulator.SimulateTimed(s, 7f, 0.006f, 12);
                return new AdvancedCaromPhysics.SimResult { Cue = simple, Obj1 = new List<TimedTrajectoryPoint>(), Obj2 = new List<TimedTrajectoryPoint>() };
            }
        }
    }
}
