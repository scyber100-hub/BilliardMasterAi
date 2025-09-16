using System.Collections.Generic;
using UnityEngine;

namespace BilliardMasterAi.Replay
{
    [System.Serializable]
    public class ReplayClip
    {
        public float start;
        public float end;
        public string label;
        public float speed = 1.0f;
        public string caption;
    }

    [System.Serializable]
    public class ReplayEdl
    {
        public List<ReplayClip> clips = new();
        public string ToJson() => JsonUtility.ToJson(this);
        public static ReplayEdl FromJson(string json){ var e = new ReplayEdl(); JsonUtility.FromJsonOverwrite(json, e); return e; }
    }
}
