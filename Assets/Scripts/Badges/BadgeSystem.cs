using System;
using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Routines;

namespace BilliardMasterAi.Badges
{
    [Serializable]
    public class Badge
    {
        public string id;        // e.g., "focus_10min"
        public string title;     // e.g., "10분 집중 루틴 달성"
        public string desc;      // description
        public bool unlocked;
        public string unlockedAt; // ISO-8601 UTC
    }

    [Serializable]
    public class BadgeSet
    {
        public List<Badge> items = new();
    }

    public static class BadgeStore
    {
        private const string Key = "bm_ai_badges";

        public static BadgeSet Load()
        {
            var json = PlayerPrefs.GetString(Key, string.Empty);
            if (string.IsNullOrEmpty(json)) return new BadgeSet();
            try { return JsonUtility.FromJson<BadgeSet>(json) ?? new BadgeSet(); }
            catch { return new BadgeSet(); }
        }

        public static void Save(BadgeSet set)
        {
            var json = JsonUtility.ToJson(set);
            PlayerPrefs.SetString(Key, json);
            PlayerPrefs.Save();
        }
    }

    public static class BadgeEvaluator
    {
        // Returns list of newly unlocked badges.
        public static List<Badge> EvaluateAfterSession(RoutineSession session)
        {
            var unlocked = new List<Badge>();
            var set = BadgeStore.Load();

            // Ensure badge entry exists
            var focus10 = GetOrCreate(set, "focus_10min", "10분 집중 루틴 달성", "10분 이상 집중 루틴을 완료했습니다.");

            if (!focus10.unlocked && session.durationMin >= 10)
            {
                focus10.unlocked = true;
                focus10.unlockedAt = DateTime.UtcNow.ToString("o");
                unlocked.Add(focus10);
            }

            BadgeStore.Save(set);
            return unlocked;
        }

        private static Badge GetOrCreate(BadgeSet set, string id, string title, string desc)
        {
            var b = set.items.Find(x => x.id == id);
            if (b == null)
            {
                b = new Badge { id = id, title = title, desc = desc, unlocked = false, unlockedAt = null };
                set.items.Add(b);
            }
            return b;
        }
    }
}

