using System;
using System.Collections.Generic;
using UnityEngine;

namespace BilliardMasterAi.Calibration
{
    // Simple PlayerPrefs-based store for multiple calibration profiles keyed by name.
    public static class CalibrationProfileStore
    {
        private const string IndexKey = "bm_calib_profiles_index"; // comma-separated names

        public static void Save(string name, CalibrationProfile profile)
        {
            if (string.IsNullOrEmpty(name) || profile == null) return;
            var json = JsonUtility.ToJson(profile);
            PlayerPrefs.SetString(Key(name), json);
            var list = new List<string>(ListNames());
            if (!list.Contains(name)) list.Add(name);
            PlayerPrefs.SetString(IndexKey, string.Join(",", list));
            PlayerPrefs.Save();
        }

        public static CalibrationProfile Load(string name)
        {
            var json = PlayerPrefs.GetString(Key(name), string.Empty);
            if (string.IsNullOrEmpty(json)) return null;
            var tmp = ScriptableObject.CreateInstance<CalibrationProfile>();
            JsonUtility.FromJsonOverwrite(json, tmp);
            return tmp;
        }

        public static void Delete(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            PlayerPrefs.DeleteKey(Key(name));
            var list = new List<string>(ListNames());
            if (list.Remove(name)) PlayerPrefs.SetString(IndexKey, string.Join(",", list));
            PlayerPrefs.Save();
        }

        public static string ExportJson(string name)
        {
            var json = PlayerPrefs.GetString(Key(name), string.Empty);
            return json;
        }

        public static bool ImportJson(string name, string json)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(json)) return false;
            try
            {
                // validate
                var tmp = ScriptableObject.CreateInstance<CalibrationProfile>();
                JsonUtility.FromJsonOverwrite(json, tmp);
                Save(name, tmp);
                return true;
            }
            catch { return false; }
        }

        public static IEnumerable<string> ListNames()
        {
            var s = PlayerPrefs.GetString(IndexKey, string.Empty);
            if (string.IsNullOrEmpty(s)) yield break;
            foreach (var n in s.Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries)) yield return n;
        }

        public static void Apply(string name)
        {
            var p = Load(name);
            if (p != null) p.Apply();
        }

        private static string Key(string name) => $"bm_calib_profile_{name}";
    }
}
