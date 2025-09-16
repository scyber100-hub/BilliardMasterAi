using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Physics;
using BilliardMasterAi.Recommendation;

namespace BilliardMasterAi.UI
{
    public class PathCompareItem : MonoBehaviour
    {
        public ShotPathPresenter presenter;
        public Image successBar; // fillAmount 0..1
        public Image riskBar;    // fillAmount 0..1
        public Text successText; // e.g., "성공확률 72%"
        public Text riskText;    // e.g., "리스크 28%"
        public Text evText;      // optional, e.g., "EV +0.42"

        public void Bind(ShotPlanResult plan, ShotMetrics metrics, Vector2 cue, Vector2 target)
        {
            if (presenter != null) presenter.Present(plan, cue, target);
            if (successBar) successBar.fillAmount = Mathf.Clamp01(metrics.SuccessProb);
            if (riskBar) riskBar.fillAmount = Mathf.Clamp01(metrics.Risk);
            if (successText) successText.text = $"성공확률 {Mathf.RoundToInt(metrics.SuccessProb * 100f)}%";
            if (riskText) riskText.text = $"리스크 {Mathf.RoundToInt(metrics.Risk * 100f)}%";
            if (evText) evText.text = $"EV {ShotEvaluator.ExpectedValue(metrics):+0.00;-0.00;0.00}";
        }
    }
}
