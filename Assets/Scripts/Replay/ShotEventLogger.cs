using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Physics;

namespace BilliardMasterAi.Replay
{
    public enum ShotEventType { Cushion, BallContact, Start, Stop }

    public struct ShotEvent
    {
        public ShotEventType Type;
        public float Time;
        public Vector2 Position;
        public string Info;
    }

    public class ShotEventLogger
    {
        public readonly List<ShotEvent> Events = new();
        public void Log(ShotEventType t, float time, Vector2 pos, string info="") => Events.Add(new ShotEvent{ Type=t, Time=time, Position=pos, Info=info});
    }
}

