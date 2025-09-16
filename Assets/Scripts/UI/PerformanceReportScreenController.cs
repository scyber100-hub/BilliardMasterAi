using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Analysis;
using BilliardMasterAi.Routines;
using BilliardMasterAi.Reports;
using BilliardMasterAi.Utils;

namespace BilliardMasterAi.UI
{
    public class PerformanceReportScreenController : MonoBehaviour
    {
        public Text statusText;
        public Button generateButton;
        public Button shareButton;

        private string _lastReportPath;

        void Awake()
        {
            if (generateButton != null) generateButton.onClick.AddListener(GenerateReport);
            if (shareButton != null) shareButton.onClick.AddListener(ShareLastReport);
        }

        public void GenerateReport()
        {
            try
            {
                var lines = new List<string>();
                lines.Add($"생성 시각: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                // Sessions summary
                var hist = RoutineHistoryStore.Load();
                lines.Add($"총 루틴 세션 수: {hist.sessions.Count}");
                int totalMinutes = 0; foreach (var s in hist.sessions) totalMinutes += s.durationMin;
                lines.Add($"누적 연습 시간: {totalMinutes}분");

                // Table stats
                var stats = DashboardService.GetAll();
                if (stats.Count == 0) lines.Add("대시보드 데이터 없음");
                else
                {
                    lines.Add("테이블별 지표:");
                    foreach (var st in stats)
                    {
                        lines.Add($"- {st.tableId}: 성공률 {st.SuccessRate * 100f:0.#}% ({st.successShots}/{st.totalShots}), 오차 {st.AvgRms * 100f:0.0} cm, 평균 TTI {st.AvgTTI:0.00}s");
                    }
                }

                string dir = Path.Combine(Application.persistentDataPath, "Reports");
                Directory.CreateDirectory(dir);
                string filename = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string path = Path.Combine(dir, filename);
                SimplePdf.Generate(path, "BilliardMasterAi 성과 리포트", lines);
                _lastReportPath = path;

                SetStatus($"PDF 생성 완료: {_lastReportPath}");
            }
            catch (Exception e)
            {
                SetStatus($"리포트 생성 실패: {e.Message}");
            }
        }

        public void ShareLastReport()
        {
            if (string.IsNullOrEmpty(_lastReportPath))
            {
                SetStatus("먼저 리포트를 생성하세요.");
                return;
            }
            ShareUtility.ShareFile(_lastReportPath, "application/pdf", "성과 리포트 공유");
        }

        private void SetStatus(string msg)
        {
            if (statusText) statusText.text = msg;
            Debug.Log(msg);
        }
    }
}

