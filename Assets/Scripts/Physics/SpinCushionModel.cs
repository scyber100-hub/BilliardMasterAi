using UnityEngine;

namespace BilliardMasterAi.Physics
{
    // Empirical model for cushion restitution and tangential damping as a function of incidence and spin.
    public static class SpinCushionModel
    {
        // angleInc: |vn|/|v| in [0,1]; speed m/s; spin rad/s
        public static float Restitution(float angleInc, float speed, float spin)
        {
            float e0 = AdvancedParams.RestitutionCushionBase;
            float shallow = 1f - angleInc; // shallow when angleInc small
            float speedLoss = Mathf.Clamp01(0.03f * Mathf.Max(0, speed - 2.0f));
            float spinLoss = Mathf.Clamp01(Mathf.Abs(spin) * 0.0025f);
            return Mathf.Clamp01(e0 - 0.08f * shallow - speedLoss - 0.03f * spinLoss);
        }

        // Tangential velocity damping coefficient [0..1]
        public static float TangentialDamping(float angleInc, float speed, float spin)
        {
            float baseDamp = 0.1f + 0.2f * (1f - angleInc);
            float speedTerm = Mathf.Clamp01(0.02f * speed);
            float spinTerm = Mathf.Clamp01(0.03f * Mathf.Abs(spin));
            return Mathf.Clamp01(baseDamp + speedTerm + spinTerm);
        }
    }
}

