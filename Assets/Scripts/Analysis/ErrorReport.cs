using UnityEngine;

namespace BilliardMasterAi.Analysis
{
    public struct ErrorReport
    {
        public float RmsError;      // meters
        public float MaxError;      // meters
        public float FinalOffset;   // meters
        public int CushionDiff;     // actual - ideal
        public float PathLenIdeal;  // meters
        public float PathLenActual; // meters
    }
}

