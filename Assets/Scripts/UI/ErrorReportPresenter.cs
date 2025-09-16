using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Analysis;

namespace BilliardMasterAi.UI
{
    public class ErrorReportPresenter : MonoBehaviour
    {
        public Text rmsText;
        public Text maxText;
        public Text finalText;
        public Text cushionText;
        public Text lengthText;

        public void Show(ErrorReport r)
        {
            if (rmsText) rmsText.text = $"RMS: {r.RmsError * 100f:0.0} cm";
            if (maxText) maxText.text = $"Max: {r.MaxError * 100f:0.0} cm";
            if (finalText) finalText.text = $"End Offset: {r.FinalOffset * 100f:0.0} cm";
            if (cushionText) cushionText.text = $"Cushion Δ: {r.CushionDiff:+#;-#;0}";
            if (lengthText) lengthText.text = $"Len ideal/actual: {r.PathLenIdeal:0.00} / {r.PathLenActual:0.00} m";
        }
    }
}

