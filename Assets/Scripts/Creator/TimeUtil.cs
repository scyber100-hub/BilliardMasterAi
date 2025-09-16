using System;

namespace BilliardMasterAi.Creator
{
    public static class TimeUtil
    {
        public static string FormatTime(double seconds)
        {
            if (seconds < 0) seconds = 0;
            int s = (int)Math.Round(seconds);
            int h = s / 3600; s %= 3600;
            int m = s / 60; s %= 60;
            return h > 0 ? $"{h:00}:{m:00}:{s:00}" : $"{m:00}:{s:00}";
        }
    }
}

