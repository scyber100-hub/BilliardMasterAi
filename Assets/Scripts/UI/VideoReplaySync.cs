using UnityEngine;

namespace BilliardMasterAi.UI
{
    // Synchronize ShotReplayController playback position to a VideoPlayer time with optional offsets.
    public class VideoReplaySync : MonoBehaviour
    {
        public UnityEngine.Video.VideoPlayer videoPlayer;
        public BilliardMasterAi.Replay.ShotReplayController replay;
        public BilliardMasterAi.Creator.VideoBallTracker tracker; // for lastStartSec window
        public float alignTimeOffsetSec = 0f; // from alignment (overrides bus when non-zero)
        public bool enableSync = true;

        void Update()
        {
            if (!enableSync || videoPlayer == null || replay == null) return;
            // Pull from bus if offset not set
            float busOffset = 0f;
            if (Mathf.Approximately(alignTimeOffsetSec, 0f))
            {
                if (BilliardMasterAi.Analysis.AlignmentBus.TryGet(out var ar)) busOffset = ar.TimeOffset; else busOffset = 0f;
            }
            else busOffset = alignTimeOffsetSec;

            double baseStart = (tracker != null) ? tracker.lastStartSec : 0.0;
            float t = Mathf.Max(0f, (float)(videoPlayer.time - baseStart) + busOffset);
            t = Mathf.Min(t, replay.Duration);
            replay.Seek(t);
        }
    }
}
