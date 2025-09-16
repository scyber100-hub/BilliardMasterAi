using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Recommendation;

namespace BilliardMasterAi.UI
{
    public class RiskProfileController : MonoBehaviour
    {
        public Slider cushionsWeight;
        public Slider lengthPenalty;
        public Slider spinPenalty;
        public Slider targetBonus;
        public Slider otherPenalty;

        public RiskProfile GetProfile()
        {
            RiskProfile p = RiskProfile.Neutral;
            if (cushionsWeight) p.cushionsWeight = cushionsWeight.value;
            if (lengthPenalty)  p.lengthPenalty  = lengthPenalty.value;
            if (spinPenalty)    p.spinPenalty    = spinPenalty.value;
            if (targetBonus)    p.targetBonus    = targetBonus.value;
            if (otherPenalty)   p.otherPenalty   = otherPenalty.value;
            return p;
        }
    }
}

