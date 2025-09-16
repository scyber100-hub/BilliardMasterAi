using UnityEngine;

namespace BilliardMasterAi.Calibration
{
    [CreateAssetMenu(menuName = "BilliardMasterAi/Calibration Profile", fileName = "CalibrationProfile")]
    public class CalibrationProfile : ScriptableObject
    {
        public float muK = 0.20f;
        public float muR = 0.010f;
        public float muContact = 0.20f;
        public float muCushion = 0.25f;
        public float restitutionBall = 0.93f;
        public float restitutionCushion = 0.92f;

        public void Apply()
        {
            Physics.AdvancedParams.MuK = muK;
            Physics.AdvancedParams.MuR = muR;
            Physics.AdvancedParams.MuContact = muContact;
            Physics.AdvancedParams.MuCushion = muCushion;
            Physics.AdvancedParams.RestitutionBall = restitutionBall;
            Physics.AdvancedParams.RestitutionCushionBase = restitutionCushion;
        }
    }
}

