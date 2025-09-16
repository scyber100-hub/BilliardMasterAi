using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Analysis;

namespace BilliardMasterAi.UI
{
    public class RealTimeDashboardItem : MonoBehaviour
    {
        public Text tableIdText;
        public Text successRateText;
        public Text errorRateText;
        public Text avgTtiText;

        public void Bind(TableStats s)
        {
            if (tableIdText) tableIdText.text = s.tableId;
            if (successRateText) successRateText.text = $"성공률 {s.SuccessRate * 100f:0.#}% ({s.successShots}/{s.totalShots})";
            if (errorRateText) errorRateText.text = $"오차율 {s.AvgRms * 100f:0.0} cm";
            if (avgTtiText) avgTtiText.text = $"평균 TTI {s.AvgTTI:0.00} s";
        }
    }
}

