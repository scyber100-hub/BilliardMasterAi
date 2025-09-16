using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Analysis;

namespace BilliardMasterAi.UI
{
    public class ChoiceVerdictPresenter : MonoBehaviour
    {
        public Text titleText;
        public Text detailText;
        public Image chosenProbBar;
        public Image bestProbBar;

        public void Show(ChoiceVerdict v)
        {
            if (titleText) titleText.text = v.Title;
            if (detailText) detailText.text = v.Detail;
            if (chosenProbBar) chosenProbBar.fillAmount = Mathf.Clamp01(v.ChosenProb);
            if (bestProbBar) bestProbBar.fillAmount = Mathf.Clamp01(v.BestProb);
        }
    }
}

