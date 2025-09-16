using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Analysis;

namespace BilliardMasterAi.UI
{
    public class DashboardTrendsUI : MonoBehaviour
    {
        public InputField daysInput; // number of days window
        public LineChart successChart;
        public LineChart ttiChart;
        public Button refreshButton;
        [Header("Filter")]
        public Dropdown tableDropdown;
        public Text legendText;

        void Awake()
        {
            if (refreshButton) refreshButton.onClick.AddListener(Refresh);
            // Populate table filter
            if (tableDropdown)
            {
                tableDropdown.ClearOptions();
                var opts = new System.Collections.Generic.List<Dropdown.OptionData>();
                opts.Add(new Dropdown.OptionData("all"));
                foreach (var id in DashboardService.GetTableIds()) if (id != "all") opts.Add(new Dropdown.OptionData(id));
                tableDropdown.AddOptions(opts);
            }
        }

        public void Refresh()
        {
            int days = 7; if (daysInput && !int.TryParse(daysInput.text, out days)) days = 7;
            string tableId = tableDropdown ? tableDropdown.options[tableDropdown.value].text : "all";
            var series = DashboardSeries(days, tableId);
            successChart?.Plot(series.success);
            ttiChart?.Plot(series.tti);
            if (legendText) legendText.text = "Success / TTI trends (" + (tableDropdown? tableDropdown.options[tableDropdown.value].text : "all") + ")";
        }

        private (float[] success, float[] tti) DashboardSeries(int days, string tableId)
        {
            return DashboardService.Series(days, tableId);
        }
    }
}
