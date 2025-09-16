using System;

namespace BilliardMasterAi.Routines
{
    [Serializable]
    public class TrainingRoutine
    {
        public string id;
        public string title;
        public string subtitle;
        public string[] tags;
        public int durationMin;
        public string difficulty; // Easy/Normal/Hard
        public string focus;      // e.g., angles, speed control, spin
        public string[] drills;   // drill names or identifiers
        public string imageResource; // Resources path for banner sprite (optional)
    }

    [Serializable]
    public class TrainingRoutineList
    {
        public TrainingRoutine[] items;
    }
}

