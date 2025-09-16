using System;
using System.Collections.Generic;
using UnityEngine;

namespace BilliardMasterAi.Routines
{
    [Serializable]
    public class RoutineSession
    {
        public string id;           // unique id
        public string title;        // e.g., "10분 집중 루틴"
        public int durationMin;     // minutes
        public string dateIso;      // ISO-8601 UTC date/time
        public string[] tags;       // optional tags
        public int score;           // optional aggregate score
    }

    [Serializable]
    public class RoutineHistoryData
    {
        public List<RoutineSession> sessions = new();
    }

    public static class RoutineHistoryStore
    {
        private const string Key = "bm_ai_routine_history";

        public static RoutineHistoryData Load()
        {
            var json = PlayerPrefs.GetString(Key, string.Empty);
            if (string.IsNullOrEmpty(json)) return new RoutineHistoryData();
            try { return JsonUtility.FromJson<RoutineHistoryData>(json) ?? new RoutineHistoryData(); }
            catch { return new RoutineHistoryData(); }
        }

        public static void Save(RoutineHistoryData data)
        {
            try
            {
                var json = JsonUtility.ToJson(data);
                PlayerPrefs.SetString(Key, json);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"RoutineHistoryStore Save failed: {e.Message}");
            }
        }

        public static RoutineSession AddSession(string title, int durationMin, string[] tags = null, int score = 0)
        {
            var data = Load();
            var s = new RoutineSession
            {
                id = Guid.NewGuid().ToString("N"),
                title = title,
                durationMin = durationMin,
                dateIso = DateTime.UtcNow.ToString("o"),
                tags = tags ?? Array.Empty<string>(),
                score = score
            };
            data.sessions.Add(s);
            Save(data);
            return s;
        }
    }
}

