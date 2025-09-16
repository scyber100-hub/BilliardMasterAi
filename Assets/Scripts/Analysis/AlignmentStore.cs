using System;
using System.IO;
using UnityEngine;

namespace BilliardMasterAi.Analysis
{
    [Serializable]
    public class AlignmentDto
    {
        public float timeOffset;
        public float scale;
        public float angleRad;
        public float offsetX;
        public float offsetY;
        public float rms;
    }

    public static class AlignmentStore
    {
        public static string Dir => Path.Combine(Application.persistentDataPath, "Alignments");

        public static void Save(string name, AlignmentResult ar)
        {
            Directory.CreateDirectory(Dir);
            var dto = new AlignmentDto { timeOffset = ar.TimeOffset, scale = ar.Scale, angleRad = ar.AngleRad, offsetX = ar.Offset.x, offsetY = ar.Offset.y, rms = ar.RmsError };
            var json = JsonUtility.ToJson(dto);
            File.WriteAllText(Path.Combine(Dir, Sanitize(name) + ".json"), json);
        }

        public static bool Load(string name, out AlignmentResult ar)
        {
            ar = default;
            var path = Path.Combine(Dir, Sanitize(name) + ".json");
            if (!File.Exists(path)) return false;
            var json = File.ReadAllText(path);
            var dto = JsonUtility.FromJson<AlignmentDto>(json);
            ar = new AlignmentResult { TimeOffset = dto.timeOffset, Scale = dto.scale, AngleRad = dto.angleRad, Offset = new Vector2(dto.offsetX, dto.offsetY), RmsError = dto.rms, AlignedTracked = null };
            return true;
        }

        private static string Sanitize(string s)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            return string.IsNullOrEmpty(s) ? "alignment" : s;
        }
    }
}

