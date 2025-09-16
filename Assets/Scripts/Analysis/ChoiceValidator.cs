using UnityEngine;

namespace BilliardMasterAi.Analysis
{
    public struct ChoiceVerdict
    {
        public string Title;      // e.g., "좋은 선택", "대안 권장"
        public string Detail;     // short explanation
        public float ChosenProb;  // 0..1
        public float BestProb;    // 0..1
        public float RmsErrorCm;  // centimeters
        public bool SuggestAlternative;
    }

    public static class ChoiceValidator
    {
        // Heuristic validation of the chosen path vs the best alternative and realized execution error.
        public static ChoiceVerdict Evaluate(float chosenProb, float chosenRisk, float bestProb, float bestRisk, ErrorReport err)
        {
            float diff = Mathf.Clamp01(bestProb - chosenProb);
            float rmsCm = err.RmsError * 100f;

            // If chosen is within 5% of best, count as valid.
            if (diff <= 0.05f)
            {
                string exec = rmsCm <= 3f ? "실행 정확도 우수" : "실행 보정 필요";
                return new ChoiceVerdict
                {
                    Title = "좋은 선택",
                    Detail = $"최적 대비 확률 차 {diff * 100f:0.#}% · {exec}",
                    ChosenProb = chosenProb,
                    BestProb = bestProb,
                    RmsErrorCm = rmsCm,
                    SuggestAlternative = false
                };
            }

            // If chosen is safer (lower risk) and probability penalty is small (<8%), accept as conservative valid choice.
            if (diff <= 0.08f && chosenRisk + 0.05f < bestRisk)
            {
                return new ChoiceVerdict
                {
                    Title = "보수적이지만 타당",
                    Detail = $"성공확률 -{diff * 100f:0.#}% 대신 리스크 ↓",
                    ChosenProb = chosenProb,
                    BestProb = bestProb,
                    RmsErrorCm = rmsCm,
                    SuggestAlternative = false
                };
            }

            // Otherwise recommend considering the best alternative if margin >=15% or execution error small.
            if (diff >= 0.15f)
            {
                return new ChoiceVerdict
                {
                    Title = "대안 권장",
                    Detail = $"최적 대비 성공확률 -{diff * 100f:0.#}% (대안 검토)",
                    ChosenProb = chosenProb,
                    BestProb = bestProb,
                    RmsErrorCm = rmsCm,
                    SuggestAlternative = true
                };
            }

            // Default: acceptable but consider refinement
            return new ChoiceVerdict
            {
                Title = "타당성 보통",
                Detail = $"확률 격차 {diff * 100f:0.#}% · 보완 여지",
                ChosenProb = chosenProb,
                BestProb = bestProb,
                RmsErrorCm = rmsCm,
                SuggestAlternative = diff > 0.1f
            };
        }
    }
}

