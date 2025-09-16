using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Routines;
using BilliardMasterAi.Badges;

namespace BilliardMasterAi.UI
{
    public class RoutineSaveScreenController : MonoBehaviour
    {
        [Header("UI")]
        public Text statusText;
        public BadgeToast badgeToast;

        [Header("Defaults")]
        public string routineTitle = "10분 집중 루틴";

        // Call this when a routine ends; durationMin from session timer.
        public void AutoSaveFocusRoutine(int durationMin, string[] tags = null, int score = 0)
        {
            var session = RoutineHistoryStore.AddSession(routineTitle, durationMin, tags, score);
            if (statusText) statusText.text = $"자동 저장 완료 · {session.title} · {durationMin}분";

            var newly = BadgeEvaluator.EvaluateAfterSession(session);
            foreach (var b in newly)
            {
                if (badgeToast)
                    badgeToast.Show(b.title, b.desc);
            }
        }
    }
}

