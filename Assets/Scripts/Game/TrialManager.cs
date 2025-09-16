using System;
using UnityEngine;

namespace BilliardMasterAi.Game
{
    public static class TrialManager
    {
        private const string KeyFirstSeen = "bm_first_seen_at";
        private const string KeyTrialUsed = "bm_trial_used";
        private const string KeyTrialUntil = "bm_trial_active_until"; // ISO-8601 UTC

        public static bool IsNewVisitor()
        {
            if (!PlayerPrefs.HasKey(KeyFirstSeen)) return true;
            return false;
        }

        public static void MarkFirstSeen()
        {
            if (!PlayerPrefs.HasKey(KeyFirstSeen))
            {
                PlayerPrefs.SetString(KeyFirstSeen, DateTime.UtcNow.ToString("o"));
                PlayerPrefs.Save();
            }
        }

        public static bool HasUsedTrial()
        {
            return PlayerPrefs.GetInt(KeyTrialUsed, 0) == 1;
        }

        public static bool IsTrialActive()
        {
            var iso = PlayerPrefs.GetString(KeyTrialUntil, string.Empty);
            if (string.IsNullOrEmpty(iso)) return false;
            if (!DateTime.TryParse(iso, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var until)) return false;
            return DateTime.UtcNow < until;
        }

        public static TimeSpan TrialRemaining()
        {
            var iso = PlayerPrefs.GetString(KeyTrialUntil, string.Empty);
            if (string.IsNullOrEmpty(iso)) return TimeSpan.Zero;
            if (!DateTime.TryParse(iso, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var until)) return TimeSpan.Zero;
            var rem = until - DateTime.UtcNow; if (rem.TotalSeconds < 0) rem = TimeSpan.Zero; return rem;
        }

        public static void StartTrialSeconds(int durationSeconds)
        {
            var until = DateTime.UtcNow.AddSeconds(Mathf.Max(30, durationSeconds));
            PlayerPrefs.SetString(KeyTrialUntil, until.ToString("o"));
            PlayerPrefs.SetInt(KeyTrialUsed, 1);
            PlayerPrefs.Save();
        }
    }
}

