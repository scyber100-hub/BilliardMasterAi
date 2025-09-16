using System.Collections.Generic;
using UnityEngine;

namespace BilliardMasterAi.Analysis
{
    public class TableStats
    {
        public string tableId;
        public int totalShots;
        public int successShots;
        public float rmsSum;   // meters
        public float ttiSum;   // seconds

        public float SuccessRate => totalShots > 0 ? (float)successShots / totalShots : 0f;
        public float AvgRms => totalShots > 0 ? rmsSum / totalShots : 0f;
        public float AvgTTI => totalShots > 0 ? ttiSum / totalShots : 0f;
        public float ErrorRate => AvgRms; // expose as meters (UI can convert to cm)
    }

    public static class DashboardService
    {
        private static readonly Dictionary<string, TableStats> _stats = new();
        private static readonly List<(System.DateTime ts, string tableId, bool success, float rms, float tti)> _history = new();
        private static string Dir => System.IO.Path.Combine(Application.persistentDataPath, "Dashboard");
        private static string FilePath => System.IO.Path.Combine(Dir, "history.json");

        public static void RecordShot(string tableId, bool success, float rmsMeters, float ttiSeconds)
        {
            if (string.IsNullOrEmpty(tableId)) tableId = "default";
            if (!_stats.TryGetValue(tableId, out var st))
            {
                st = new TableStats { tableId = tableId };
                _stats[tableId] = st;
            }
            st.totalShots++;
            if (success) st.successShots++;
            st.rmsSum += Mathf.Max(0f, rmsMeters);
            st.ttiSum += Mathf.Max(0f, ttiSeconds);
            _history.Add((System.DateTime.UtcNow, tableId, success, rmsMeters, ttiSeconds));
            SaveHistory();
        }

        public static List<TableStats> GetAll()
        {
            return new List<TableStats>(_stats.Values);
        }

        public static List<string> GetTableIds()
        {
            var ids = new List<string>();
            foreach (var k in _stats.Keys) ids.Add(k);
            if (ids.Count == 0) ids.Add("all");
            return ids;
        }

        public static void Clear()
        {
            _stats.Clear();
            _history.Clear();
        }

        public static (float[] successRate, float[] avgTti) Series(int days)
        {
            int n = Mathf.Max(1, days);
            var succ = new float[n]; var tti = new float[n];
            var today = System.DateTime.UtcNow.Date;
            for (int i=0;i<n;i++)
            {
                var d0 = today.AddDays(-(n-1-i)); var d1 = d0.AddDays(1);
                int tot=0, ok=0; float ttiSum=0f;
                foreach (var rec in _history)
                {
                    if (rec.ts >= d0 && rec.ts < d1) { tot++; if (rec.success) ok++; ttiSum += rec.tti; }
                }
                succ[i] = (tot>0) ? (ok/(float)tot) : 0f; tti[i] = (tot>0) ? (ttiSum/tot) : 0f;
            }
            return (succ, tti);
        }

        public static (float[] successRate, float[] avgTti) Series(int days, string tableFilter)
        {
            int n = Mathf.Max(1, days);
            var succ = new float[n]; var tti = new float[n];
            var today = System.DateTime.UtcNow.Date; bool filter = !string.IsNullOrEmpty(tableFilter) && tableFilter != "all";
            for (int i=0;i<n;i++)
            {
                var d0 = today.AddDays(-(n-1-i)); var d1 = d0.AddDays(1);
                int tot=0, ok=0; float ttiSum=0f;
                foreach (var rec in _history)
                {
                    if (rec.ts >= d0 && rec.ts < d1 && (!filter || rec.tableId == tableFilter)) { tot++; if (rec.success) ok++; ttiSum += rec.tti; }
                }
                succ[i] = (tot>0) ? (ok/(float)tot) : 0f; tti[i] = (tot>0) ? (ttiSum/tot) : 0f;
            }
            return (succ, tti);
        }

        [System.Serializable]
        private struct RecDto { public long ts; public string tableId; public bool success; public float rms; public float tti; }
        [System.Serializable]
        private struct HistoryDto { public RecDto[] items; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() { LoadHistory(); }

        private static void SaveHistory()
        {
            try
            {
                System.IO.Directory.CreateDirectory(Dir);
                var dto = new HistoryDto { items = new RecDto[_history.Count] };
                for (int i=0;i<_history.Count;i++)
                {
                    var h = _history[i];
                    dto.items[i] = new RecDto{ ts = h.ts.Ticks, tableId = h.tableId, success = h.success, rms = h.rms, tti = h.tti};
                }
                var json = JsonUtility.ToJson(dto);
                System.IO.File.WriteAllText(FilePath, json);
            }
            catch (System.Exception e) { Debug.LogWarning($"Dashboard history save failed: {e.Message}"); }
        }

        private static void LoadHistory()
        {
            try
            {
                if (!System.IO.File.Exists(FilePath)) return;
                var json = System.IO.File.ReadAllText(FilePath);
                var dto = JsonUtility.FromJson<HistoryDto>(json);
                _history.Clear();
                if (dto.items != null)
                {
                    foreach (var r in dto.items)
                        _history.Add((new System.DateTime(r.ts, System.DateTimeKind.Utc), r.tableId, r.success, r.rms, r.tti));
                }
            }
            catch (System.Exception e) { Debug.LogWarning($"Dashboard history load failed: {e.Message}"); }
        }
    }
}
