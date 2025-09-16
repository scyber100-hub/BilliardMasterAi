using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using BilliardMasterAi.Analytics;

namespace BilliardMasterAi.UI
{
    public class SensitivitySliceUI : MonoBehaviour
    {
        public SensitivityHeatmapUI heatmap;
        public Slider spinSlider;
        public Text spinLabel;
        private List<SensitivityMap.Cell> _cells; private int _aN, _sN, _spN;

        public void BindData(List<SensitivityMap.Cell> cells, int angleN, int speedN, int spinN)
        {
            _cells = cells; _aN = angleN; _sN = speedN; _spN = spinN;
            if (spinSlider)
            {
                spinSlider.minValue = 0; spinSlider.maxValue = Mathf.Max(0, spinN-1); spinSlider.wholeNumbers = true; spinSlider.onValueChanged.AddListener(_=>UpdateSlice());
            }
            UpdateSlice();
        }

        private void UpdateSlice()
        {
            int idx = spinSlider ? Mathf.RoundToInt(spinSlider.value) : 0;
            if (spinLabel) spinLabel.text = $"Spin slice: {idx}/{Mathf.Max(0,_spN-1)}";
            heatmap?.ShowSlice(_cells, _aN, _sN, _spN, idx);
        }
    }
}

