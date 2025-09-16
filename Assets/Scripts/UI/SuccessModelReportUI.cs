using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Analytics;

namespace BilliardMasterAi.UI
{
    public class SuccessModelReportUI : MonoBehaviour
    {
        public LineChart rocChart;
        public Text aucText;
        public Text statsText;

        public void RefreshReport()
        {
            var roc = SuccessModel.RocCurve(50);
            if (rocChart != null && roc.fpr.Length>0)
            {
                // plot TPR vs FPR; reuse LineChart by packing points in order of fpr index
                rocChart.Plot(roc.tpr); // x-axis will be index; conceptual
            }
            if (aucText) aucText.text = $"AUC: {roc.auc:0.000}";
            if (statsText)
            {
                var (mC, sC) = SuccessModel.FeatureStats(s=>s.cushions);
                var (mL, sL) = SuccessModel.FeatureStats(s=>s.length);
                var (mPT, sPT) = SuccessModel.FeatureStats(s=>s.proxTarget);
                statsText.text = $"cushions μ={mC:0.00} σ={sC:0.00}\nlength μ={mL:0.00} σ={sL:0.00}\nproxT μ={mPT:0.00} σ={sPT:0.00}";
            }
        }
    }
}

