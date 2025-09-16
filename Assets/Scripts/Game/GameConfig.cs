using UnityEngine;

namespace BilliardMasterAi.Game
{
    public enum GameMode { Practice, League }

    public static class GameState
    {
        public static GameMode Mode { get; private set; } = GameMode.Practice;
        public static int TimeLimitSec { get; private set; } = 0; // 0 = no limit

        public static bool TimerRunning { get; private set; }
        public static float RemainingSec { get; private set; }

        public static void SetMode(GameMode mode, int timeLimitSec)
        {
            Mode = mode;
            TimeLimitSec = Mathf.Max(0, timeLimitSec);
            if (Mode == GameMode.League && TimeLimitSec > 0)
            {
                RemainingSec = TimeLimitSec;
            }
        }

        public static void StartTimer()
        {
            if (Mode != GameMode.League || TimeLimitSec <= 0) { TimerRunning = false; return; }
            RemainingSec = Mathf.Max(0f, RemainingSec > 0f ? RemainingSec : TimeLimitSec);
            TimerRunning = true;
        }

        public static void PauseTimer() => TimerRunning = false;

        public static void ResetTimer()
        {
            RemainingSec = TimeLimitSec;
            TimerRunning = false;
        }

        public static void Tick(float dt)
        {
            if (!TimerRunning) return;
            RemainingSec = Mathf.Max(0f, RemainingSec - dt);
            if (RemainingSec <= 0f) TimerRunning = false;
        }
    }
}

