using UnityEngine;

namespace BilliardMasterAi.Physics
{
    public static class CaromConstants
    {
        // Inner cushion-to-cushion size (meters) for carom table
        public const float TableWidth = 2.84f;
        public const float TableHeight = 1.42f;
        public const float BallRadius = 0.028575f; // 57.15mm diameter

        // Simplified surface/friction parameters
        public const float MuRolling = 0.015f;
        public const float MuSliding = 0.10f;
        public const float RestitutionCushion = 0.92f;
        public const float SpinDamping = 0.2f; // per second
        public const float G = 9.81f;
    }

    public struct BallState
    {
        public Vector2 Position; // meters
        public Vector2 Velocity; // m/s
        public float SpinZ;      // rad/s (+ top, - backspin). Simplified scalar about Z.

        public float Speed => Velocity.magnitude;
    }

    public static class CaromPhysics
    {
        // Integrate motion with very simplified rolling/sliding + spin coupling.
        public static void Integrate(ref BallState s, float dt)
        {
            if (s.Speed <= 1e-3f) {
                s.Velocity = Vector2.zero;
                s.SpinZ *= Mathf.Exp(-CaromConstants.SpinDamping * dt);
                return;
            }

            // Friction deceleration opposite to velocity
            float mu = Mathf.Lerp(CaromConstants.MuSliding, CaromConstants.MuRolling, Mathf.Clamp01(Mathf.Abs(s.SpinZ) / 30f));
            Vector2 aFric = -mu * CaromConstants.G * s.Velocity.normalized;

            // Very rough spin-induced lateral acceleration (sidespin/throw approx)
            Vector2 lat = new Vector2(-s.Velocity.y, s.Velocity.x).normalized; // left normal
            Vector2 aSpin = lat * (s.SpinZ * 0.02f);

            Vector2 a = aFric + aSpin;
            s.Velocity += a * dt;

            // Clamp if reversed by friction
            if (Vector2.Dot(s.Velocity, s.Velocity + a * dt) < 0f) s.Velocity = Vector2.zero;

            s.Position += s.Velocity * dt;
            s.SpinZ *= Mathf.Exp(-CaromConstants.SpinDamping * dt);
        }

        // Reflect off cushions, return true if a bounce occurred.
        public static bool HandleCushion(ref BallState s, out Vector2 hitPoint, out int cushionIndex)
        {
            hitPoint = s.Position; cushionIndex = -1;
            float halfW = CaromConstants.TableWidth * 0.5f - CaromConstants.BallRadius;
            float halfH = CaromConstants.TableHeight * 0.5f - CaromConstants.BallRadius;

            bool bounced = false;
            Vector2 pos = s.Position;
            Vector2 vel = s.Velocity;

            if (pos.x < -halfW && vel.x < 0f) { // left
                pos.x = -halfW; vel.x = -vel.x * CaromConstants.RestitutionCushion; cushionIndex = 0; bounced = true;
            }
            else if (pos.x > halfW && vel.x > 0f) { // right
                pos.x = halfW; vel.x = -vel.x * CaromConstants.RestitutionCushion; cushionIndex = 1; bounced = true;
            }

            if (pos.y < -halfH && vel.y < 0f) { // bottom
                pos.y = -halfH; vel.y = -vel.y * CaromConstants.RestitutionCushion; cushionIndex = 2; bounced = true;
            }
            else if (pos.y > halfH && vel.y > 0f) { // top
                pos.y = halfH; vel.y = -vel.y * CaromConstants.RestitutionCushion; cushionIndex = 3; bounced = true;
            }

            if (bounced)
            {
                // Very rough spin-wall interaction: tweak tangent component
                Vector2 n = Vector2.zero;
                if (cushionIndex == 0) n = Vector2.right; // left wall normal
                else if (cushionIndex == 1) n = Vector2.left;
                else if (cushionIndex == 2) n = Vector2.up;
                else if (cushionIndex == 3) n = Vector2.down;

                Vector2 t = new Vector2(-n.y, n.x);
                float vt = Vector2.Dot(vel, t);
                vt += s.SpinZ * 0.05f; // spin adds/subtracts tangential component
                vel = Vector2.Dot(vel, n) * n + vt * t;

                s.Position = pos;
                s.Velocity = vel;
                hitPoint = pos;
            }

            return bounced;
        }
    }
}

