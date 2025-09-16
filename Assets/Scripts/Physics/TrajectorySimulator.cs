using System.Collections.Generic;
using UnityEngine;

namespace BilliardMasterAi.Physics
{
    public struct TrajectoryPoint
    {
        public Vector2 Position;
        public bool IsCushion;
    }

    public struct TimedTrajectoryPoint
    {
        public Vector2 Position;
        public bool IsCushion;
        public float Time; // seconds
    }

    public static class TrajectorySimulator
    {
        public static List<TrajectoryPoint> Simulate(BallState start, float maxTime = 8f, float dt = 0.005f, int maxBounces = 10)
        {
            var path = new List<TrajectoryPoint>(1024);
            var s = start;
            float t = 0f;
            int bounces = 0;

            path.Add(new TrajectoryPoint { Position = s.Position, IsCushion = false });

            while (t < maxTime && s.Speed > 0.01f)
            {
                CaromPhysics.Integrate(ref s, dt);
                if (CaromPhysics.HandleCushion(ref s, out var hit, out var idx))
                {
                    bounces++;
                    path.Add(new TrajectoryPoint { Position = hit, IsCushion = true });
                    if (bounces >= maxBounces) break;
                }
                else
                {
                    path.Add(new TrajectoryPoint { Position = s.Position, IsCushion = false });
                }

                t += dt;
            }

            return path;
        }

        public static List<TimedTrajectoryPoint> SimulateTimed(BallState start, float maxTime = 8f, float dt = 0.005f, int maxBounces = 10)
        {
            var path = new List<TimedTrajectoryPoint>(1024);
            var s = start;
            float t = 0f;
            int bounces = 0;

            path.Add(new TimedTrajectoryPoint { Position = s.Position, IsCushion = false, Time = t });

            while (t < maxTime && s.Speed > 0.01f)
            {
                CaromPhysics.Integrate(ref s, dt);
                if (CaromPhysics.HandleCushion(ref s, out var hit, out var idx))
                {
                    bounces++;
                    path.Add(new TimedTrajectoryPoint { Position = hit, IsCushion = true, Time = t });
                    if (bounces >= maxBounces) break;
                }
                else
                {
                    path.Add(new TimedTrajectoryPoint { Position = s.Position, IsCushion = false, Time = t });
                }
                t += dt;
            }

            return path;
        }
    }
}
