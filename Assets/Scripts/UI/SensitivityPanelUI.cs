using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using BilliardMasterAi.Analytics;

namespace BilliardMasterAi.UI
{
    public class SensitivityPanelUI : MonoBehaviour
    {
        public Transform tableRoot;
        public Transform cueBall;
        public Transform targetBall;
        public Transform otherBall;
        public InputField angleMinInput, angleMaxInput, angleNInput;
        public InputField speedMinInput, speedMaxInput, speedNInput;
        public InputField spinMinInput, spinMaxInput, spinNInput;
        public Button runButton;
        public SensitivityHeatmapUI heatmap;
        public SensitivitySliceUI sliceUI;
        private System.Collections.Generic.List<BilliardMasterAi.Analytics.SensitivityMap.Cell> _lastCells;
        private int _lastAN, _lastSN, _lastSpN;
        public Text statusText;

        void Awake()
        {
            if (runButton) runButton.onClick.AddListener(Run);
        }

        private Vector2 To2D(Vector3 w){ return tableRoot? (Vector2) (tableRoot.InverseTransformPoint(w)) : new Vector2(w.x, w.z); }

        public void Run()
        {
            Vector2 cue = To2D(cueBall.position); Vector2 tar = To2D(targetBall.position); Vector2 oth = To2D(otherBall.position);
            float angMin = Parse(angleMinInput?.text, 0f), angMax = Parse(angleMaxInput?.text, 360f); int angN = (int)Parse(angleNInput?.text, 36f);
            float spMin = Parse(speedMinInput?.text, 1.5f), spMax = Parse(speedMaxInput?.text, 3.5f); int spN = (int)Parse(speedNInput?.text, 12f);
            float snMin = Parse(spinMinInput?.text, -20f), snMax = Parse(spinMaxInput?.text, 20f); int sN = (int)Parse(spinNInput?.text, 5f);
            var cells = SensitivityMap.EvaluateGrid(cue, tar, oth, angMin, angMax, angN, spMin, spMax, spN, snMin, snMax, sN);
            _lastCells = cells; _lastAN = angN; _lastSN = spN; _lastSpN = sN;
            heatmap?.Show(cells, angN, spN, sN);
            sliceUI?.BindData(cells, angN, spN, sN);
            if (statusText) statusText.text = $"cells={cells.Count} (angN={angN}, spN={spN}, spinN={sN})";
        }

        private float Parse(string s, float d){ return float.TryParse(s, out var v) ? v : d; }
    }
}
