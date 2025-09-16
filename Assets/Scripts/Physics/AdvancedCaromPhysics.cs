using System.Collections.Generic;
using UnityEngine;

namespace BilliardMasterAi.Physics
{
    public static class AdvancedParams
    {
        public static float Mass = 0.17f; // kg (carom ball ~170g)
        public static float Radius = CaromConstants.BallRadius;
        public static float Inertia => 0.4f * Mass * Radius * Radius; // solid sphere I = 2/5 m r^2 ≈ 0.4 m r^2

        public static float MuK = 0.20f;        // kinetic friction (sliding)
        public static float MuR = 0.010f;       // rolling resistance (effective)
        public static float MuContact = 0.20f;  // ball-ball tangential friction
        public static float MuCushion = 0.25f;  // cushion tangential friction
        public static float RestitutionBall = 0.93f;
        public static float RestitutionCushionBase = 0.92f; // normal incidence
        public static float G = 9.81f;
    }

    public class RigidBall
    {
        public Vector2 p;   // position (m)
        public Vector2 v;   // linear vel (m/s)
        public float w;     // angular vel around Y (rad/s)

        public RigidBall(Vector2 p)
        {
            this.p = p; v = Vector2.zero; w = 0f;
        }
    }

    public static class AdvancedCaromPhysics
    {
        public static List<TrajectoryPoint> SimulateCuePath(Vector2 cuePos, Vector2 target1, Vector2 target2, Vector2 initVel, float spinY, float maxTime = 8f, float dt = 0.002f)
        {
            var cue = new RigidBall(cuePos) { v = initVel, w = spinY };
            var obj1 = new RigidBall(target1);
            var obj2 = new RigidBall(target2);

            var path = new List<TrajectoryPoint>(4096);
            path.Add(new TrajectoryPoint { Position = cue.p, IsCushion = false });

            float t = 0f; int maxIter = Mathf.CeilToInt(maxTime / dt);
            for (int it = 0; it < maxIter; it++)
            {
                // Stop condition
                if (cue.v.magnitude < 0.02f && Mathf.Abs(cue.w) < 0.5f) break;

                // Adaptive substeps based on max speed
                float vmax = Mathf.Max(cue.v.magnitude, Mathf.Max(obj1.v.magnitude, obj2.v.magnitude));
                int sub = Mathf.Clamp(Mathf.CeilToInt(vmax / 1.0f), 1, 8); // ~1 m/s per substep cap
                float h = dt / sub;
                for (int s = 0; s < sub; s++)
                {
                    IntegrateFriction(ref cue, h);
                    IntegrateFriction(ref obj1, h);
                    IntegrateFriction(ref obj2, h);
                    cue.p += cue.v * h; obj1.p += obj1.v * h; obj2.p += obj2.v * h;
                    HandleCushion(ref cue); HandleCushion(ref obj1); HandleCushion(ref obj2);
                    SweepAndResolve(ref cue, ref obj1, h); SweepAndResolve(ref cue, ref obj2, h); SweepAndResolve(ref obj1, ref obj2, h);
                    t += h;
                }
                bool cueBounced = false;
                if (HandleCushion(ref cue)) cueBounced = true;
                if (HandleCushion(ref obj1)) { }
                if (HandleCushion(ref obj2)) { }
                if (cueBounced)
                {
                    path.Add(new TrajectoryPoint { Position = cue.p, IsCushion = true });
                }
                else
                {
                    if (it % 2 == 0) // thin sampling
                        path.Add(new TrajectoryPoint { Position = cue.p, IsCushion = false });
                }
            }

            return path;
        }

        public struct SimResult
        {
            public List<TimedTrajectoryPoint> Cue;
            public List<TimedTrajectoryPoint> Obj1;
            public List<TimedTrajectoryPoint> Obj2;
        }

        public static SimResult SimulateAll(Vector2 cuePos, Vector2 target1, Vector2 target2, Vector2 initVel, float spinY, float maxTime = 8f, float dt = 0.002f)
        {
            var cue = new RigidBall(cuePos) { v = initVel, w = spinY };
            var obj1 = new RigidBall(target1);
            var obj2 = new RigidBall(target2);
            var cuePath = new List<TimedTrajectoryPoint>(4096);
            var o1Path = new List<TimedTrajectoryPoint>(1024);
            var o2Path = new List<TimedTrajectoryPoint>(1024);
            cuePath.Add(new TimedTrajectoryPoint { Position = cue.p, IsCushion = false, Time = 0f });
            o1Path.Add(new TimedTrajectoryPoint { Position = obj1.p, IsCushion = false, Time = 0f });
            o2Path.Add(new TimedTrajectoryPoint { Position = obj2.p, IsCushion = false, Time = 0f });

            float t = 0f; int maxIter = Mathf.CeilToInt(maxTime / dt);
            for (int it = 0; it < maxIter; it++)
            {
                if (cue.v.magnitude < 0.02f && Mathf.Abs(cue.w) < 0.5f && obj1.v.magnitude < 0.02f && obj2.v.magnitude < 0.02f) break;
                float vmax = Mathf.Max(cue.v.magnitude, Mathf.Max(obj1.v.magnitude, obj2.v.magnitude));
                int sub = Mathf.Clamp(Mathf.CeilToInt(vmax / 1.0f), 1, 8);
                float h = dt / sub; bool cueB=false, o1B=false, o2B=false;
                for (int s=0;s<sub;s++)
                {
                    IntegrateFriction(ref cue, h); IntegrateFriction(ref obj1, h); IntegrateFriction(ref obj2, h);
                    cue.p += cue.v * h; obj1.p += obj1.v * h; obj2.p += obj2.v * h;
                    if (HandleCushion(ref cue)) cueB=true; if (HandleCushion(ref obj1)) o1B=true; if (HandleCushion(ref obj2)) o2B=true;
                    SweepAndResolve(ref cue, ref obj1, h); SweepAndResolve(ref cue, ref obj2, h); SweepAndResolve(ref obj1, ref obj2, h);
                    t += h;
                }
                if (it % 2 == 0)
                {
                    cuePath.Add(new TimedTrajectoryPoint { Position = cue.p, IsCushion = cueB, Time = t });
                    o1Path.Add(new TimedTrajectoryPoint { Position = obj1.p, IsCushion = o1B, Time = t });
                    o2Path.Add(new TimedTrajectoryPoint { Position = obj2.p, IsCushion = o2B, Time = t });
                }
            }
            return new SimResult { Cue = cuePath, Obj1 = o1Path, Obj2 = o2Path };
        }

        private static void IntegrateFriction(ref RigidBall b, float dt)
        {
            if (b.v.sqrMagnitude < 1e-6f)
            {
                // spin damping on cloth
                b.w *= Mathf.Exp(-CaromConstants.SpinDamping * dt);
                b.v = Vector2.zero; return;
            }

            float vmag = b.v.magnitude;
            float rollingSpeed = Mathf.Abs(b.w) * AdvancedParams.Radius;
            bool sliding = Mathf.Abs(vmag - rollingSpeed) > 0.05f;
            if (sliding)
            {
                // kinetic friction opposes direction of slip at contact ≈ direction of motion for this 2D model
                Vector2 dir = b.v / Mathf.Max(1e-6f, vmag);
                Vector2 a = -AdvancedParams.MuK * AdvancedParams.G * dir;
                b.v += a * dt;
                // friction torque increases or decreases w to approach rolling
                float sign = (rollingSpeed < vmag) ? 1f : -1f;
                float alpha = sign * AdvancedParams.MuK * AdvancedParams.G * AdvancedParams.Radius / AdvancedParams.Inertia * AdvancedParams.Mass; // τ/I ≈ μ m g R / I
                b.w += alpha * dt;
            }
            else
            {
                // rolling resistance as velocity-proportional decel
                Vector2 a = -AdvancedParams.MuR * AdvancedParams.G * (b.v / Mathf.Max(1e-6f, vmag));
                b.v += a * dt;
                b.w *= Mathf.Exp(-CaromConstants.SpinDamping * dt * 0.5f);
            }
        }

        private static bool HandleCushion(ref RigidBall b)
        {
            float halfW = CaromConstants.TableWidth * 0.5f - AdvancedParams.Radius;
            float halfH = CaromConstants.TableHeight * 0.5f - AdvancedParams.Radius;
            bool bounced = false;
            Vector2 n = Vector2.zero;

            if (b.p.x < -halfW && b.v.x < 0f) { b.p.x = -halfW; n = Vector2.right; bounced = true; }
            else if (b.p.x > halfW && b.v.x > 0f) { b.p.x = halfW; n = Vector2.left; bounced = true; }
            if (b.p.y < -halfH && b.v.y < 0f) { b.p.y = -halfH; n = Vector2.up; bounced = true; }
            else if (b.p.y > halfH && b.v.y > 0f) { b.p.y = halfH; n = Vector2.down; bounced = true; }

            if (!bounced) return false;

            Vector2 t = new Vector2(-n.y, n.x);
            float vn = Vector2.Dot(b.v, n);
            float vt = Vector2.Dot(b.v, t);
            float angle = Mathf.Abs(vn) / (b.v.magnitude + 1e-6f);
            float e = SpinCushionModel.Restitution(angle, b.v.magnitude, b.w);
            vn = -e * vn;
            float tangentialDamp = SpinCushionModel.TangentialDamping(angle, b.v.magnitude, b.w);
            vt *= (1f - tangentialDamp);
            b.v = vn * n + vt * t;
            // spin-tangent coupling
            b.w += -vt * 0.1f / AdvancedParams.Radius;
            return true;
        }

        private static void HandleBallCollision(ref RigidBall a, ref RigidBall b)
        {
            Vector2 dp = b.p - a.p; float dist = dp.magnitude; float minDist = 2f * AdvancedParams.Radius;
            if (dist >= minDist || dist < 1e-6f) return;

            Vector2 n = dp / dist; Vector2 t = new Vector2(-n.y, n.x);
            // relative vel at contact
            float relVn = Vector2.Dot(b.v - a.v, n);
            float relVt = Vector2.Dot(b.v - a.v, t) + AdvancedParams.Radius * (a.w + b.w); // include spin slip approx

            if (relVn > 0f) return; // separating

            float m = AdvancedParams.Mass; float I = AdvancedParams.Inertia; float r = AdvancedParams.Radius;
            float invMassSum = 2f / m; // both balls equal mass

            // normal impulse
            float Jn = -(1f + AdvancedParams.RestitutionBall) * relVn / (invMassSum);
            Vector2 Pn = Jn * n;
            a.v -= Pn / m; b.v += Pn / m;

            // tangential impulse with friction cap
            float denomT = invMassSum + (r * r / I) + (r * r / I);
            float Jt = -relVt / denomT;
            float maxJt = AdvancedParams.MuContact * Mathf.Abs(Jn);
            Jt = Mathf.Clamp(Jt, -maxJt, maxJt);
            Vector2 Pt = Jt * t;
            a.v -= Pt / m; b.v += Pt / m;
            a.w += (Jt * r) / I; b.w -= (Jt * r) / I;

            // positional correction to resolve overlap
            float penetration = minDist - dist; float correction = penetration * 0.5f + 1e-4f;
            a.p -= n * correction; b.p += n * correction;
        }

        // Continuous sweep test (approximate): subdivide until either separating or collision occurs then resolve.
        private static void SweepAndResolve(ref RigidBall a, ref RigidBall b, float h)
        {
            Vector2 rel = b.p - a.p; float minDist = 2f * AdvancedParams.Radius;
            if (rel.sqrMagnitude <= minDist*minDist)
            {
                HandleBallCollision(ref a, ref b); return;
            }
            Vector2 rv = (b.v - a.v);
            float proj = Vector2.Dot(rel, rv);
            if (proj >= 0f) return; // moving apart
            // binary search time of impact within h
            float t0 = 0f, t1 = h; Vector2 pa0=a.p, pb0=b.p, va=a.v, vb=b.v;
            for (int i=0;i<5;i++)
            {
                float tm = 0.5f*(t0+t1);
                a.p = pa0 + va*tm; b.p = pb0 + vb*tm;
                if ((b.p - a.p).sqrMagnitude <= minDist*minDist) t1 = tm; else t0 = tm;
            }
            // advance to impact time
            a.p = pa0 + va*t1; b.p = pb0 + vb*t1;
            HandleBallCollision(ref a, ref b);
            // advance remaining time after impact
            float rem = Mathf.Max(0f, h - t1);
            a.p += a.v*rem; b.p += b.v*rem;
        }
    }
}
