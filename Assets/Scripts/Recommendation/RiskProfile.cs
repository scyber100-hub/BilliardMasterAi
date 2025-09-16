using UnityEngine;

namespace BilliardMasterAi.Recommendation
{
    [System.Serializable]
    public struct RiskProfile
    {
        [Range(0f, 2f)] public float cushionsWeight;   // prefer >=3 cushions
        [Range(0f, 2f)] public float lengthPenalty;    // penalize long paths
        [Range(0f, 2f)] public float spinPenalty;      // penalize high spin
        [Range(0f, 2f)] public float targetBonus;      // reward proximity to target
        [Range(0f, 2f)] public float otherPenalty;     // penalize proximity to other

        public static RiskProfile Conservative => new RiskProfile { cushionsWeight = 1.2f, lengthPenalty = 1.2f, spinPenalty = 1.2f, targetBonus = 0.8f, otherPenalty = 1.2f };
        public static RiskProfile Neutral => new RiskProfile { cushionsWeight = 1.0f, lengthPenalty = 1.0f, spinPenalty = 1.0f, targetBonus = 1.0f, otherPenalty = 1.0f };
        public static RiskProfile Aggressive => new RiskProfile { cushionsWeight = 1.1f, lengthPenalty = 0.8f, spinPenalty = 0.8f, targetBonus = 1.2f, otherPenalty = 0.8f };
    }
}

