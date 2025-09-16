using System;
using System.Collections.Generic;
using UnityEngine;

namespace BilliardMasterAi.Coach
{
    [Serializable]
    public class PatternAssignment
    {
        public string studentId;
        public string[] routineIds;
        public string dateIso; // ISO-8601
    }

    [Serializable]
    public class AssignmentHistory
    {
        public List<PatternAssignment> items = new();
    }

    [Serializable]
    public class CurrentPatterns
    {
        public string[] routineIds = Array.Empty<string>();
    }

    public static class AssignmentStore
    {
        private static string KeyCurrent(string studentId) => $"bm_ai_assign_current_{studentId}";
        private static string KeyHistory(string studentId) => $"bm_ai_assign_history_{studentId}";

        public static CurrentPatterns LoadCurrent(string studentId)
        {
            var json = PlayerPrefs.GetString(KeyCurrent(studentId), string.Empty);
            if (string.IsNullOrEmpty(json)) return new CurrentPatterns();
            try { return JsonUtility.FromJson<CurrentPatterns>(json) ?? new CurrentPatterns(); }
            catch { return new CurrentPatterns(); }
        }

        public static AssignmentHistory LoadHistory(string studentId)
        {
            var json = PlayerPrefs.GetString(KeyHistory(studentId), string.Empty);
            if (string.IsNullOrEmpty(json)) return new AssignmentHistory();
            try { return JsonUtility.FromJson<AssignmentHistory>(json) ?? new AssignmentHistory(); }
            catch { return new AssignmentHistory(); }
        }

        public static void AssignCurrent(string studentId, string[] routineIds)
        {
            // Save current
            var current = new CurrentPatterns { routineIds = routineIds };
            PlayerPrefs.SetString(KeyCurrent(studentId), JsonUtility.ToJson(current));

            // Append to history
            var history = LoadHistory(studentId);
            history.items.Add(new PatternAssignment
            {
                studentId = studentId,
                routineIds = routineIds,
                dateIso = DateTime.UtcNow.ToString("o")
            });
            PlayerPrefs.SetString(KeyHistory(studentId), JsonUtility.ToJson(history));
            PlayerPrefs.Save();
        }
    }
}

