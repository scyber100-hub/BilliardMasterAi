using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Analysis;
using BilliardMasterAi.Physics;
using System;

namespace BilliardMasterAi.UI
{
    // One-click: Track -> Publish zero alignment -> Enable video-replay sync
    public class AutoTrackAlignSyncUI : MonoBehaviour
    {
        public VideoTrackerControlUI trackerUI;
        public VideoReplaySync sync;
        public BilliardMasterAi.Creator.VideoBallTracker tracker;
        public BilliardMasterAi.Replay.ShotReplayController replay;
        public BilliardMasterAi.Replay.BallTrajectoryRecorder recorder;
        public bool trackerYellowIsCue = true;
        public float alignWindow = 0.8f;
        public bool rigidOnly = false;
        [Range(0f,0.5f)] public float rejectFraction = 0.0f;
        public Button runButton;
        public Text statusText;
        [Header("Auto-Save Alignment")]
        public bool autoSave = true;
        public InputField saveNameInput; // optional
        public UnityEngine.Video.VideoPlayer videoPlayerRef; // optional (for default name)

        void Awake()
        {
            if (runButton) runButton.onClick.AddListener(Run);
        }

        public void Run()
        {
            if (trackerUI == null || tracker == null || sync == null)
            {
                SetStatus("Auto: missing references");
                return;
            }
            trackerUI.OnTrackClicked();
            StartCoroutine(WaitAndPublish());
        }

        private IEnumerator WaitAndPublish()
        {
            double waitSec = Mathf.Max(0.5f, (float)(tracker.lastEndSec - tracker.lastStartSec) + 0.3f);
            yield return new WaitForSeconds((float)waitSec);
            // Build tracked cue path
            var cueColor = trackerYellowIsCue ? BilliardMasterAi.Perception.BallColor.Yellow : BilliardMasterAi.Perception.BallColor.White;
            tracker.trajectories.TryGetValue(cueColor, out var trackedCue);
            // Build reference path: recorder actual if available; else ideal if available
            System.Collections.Generic.List<TimedTrajectoryPoint> reference = null;
            if (recorder != null) reference = recorder.StopRecording();
            if ((reference == null || reference.Count == 0) && replay != null) reference = replay.GetIdeal();
            AlignmentResult ar;
            if (trackedCue != null && trackedCue.Count > 0 && reference != null && reference.Count > 0)
            {
                var opt = new TrajectoryAligner.AlignOptions { window = alignWindow, dt = 0.02f, rigidOnly = rigidOnly, rejectFraction = rejectFraction };
                ar = TrajectoryAligner.Align(reference, trackedCue, opt);
                AlignmentBus.Publish(ar);
                SetStatus($"Auto: Align dt={ar.TimeOffset:+0.00;-0.00}s, d={ar.Offset.magnitude*100f:0.0}cm, rms={ar.RmsError*100f:0.0}cm");
                if (autoSave)
                {
                    string name = ComposeDefaultName();
                    AlignmentStore.Save(name, ar);
                    SetStatus($"Auto: Alignment saved as '{name}'");
                }
            }
            else
            {
                ar = new AlignmentResult { TimeOffset = 0f, Offset = Vector2.zero, Scale = 1f, AngleRad = 0f, RmsError = 0f, AlignedTracked = null };
                AlignmentBus.Publish(ar);
                SetStatus("Auto: Align fallback to zero");
            }
            sync.enableSync = true;
        }

        private string ComposeDefaultName()
        {
            if (saveNameInput != null && !string.IsNullOrEmpty(saveNameInput.text)) return saveNameInput.text;
            string baseName = "align_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            try
            {
                if (videoPlayerRef != null && !string.IsNullOrEmpty(videoPlayerRef.url))
                {
                    var fn = System.IO.Path.GetFileNameWithoutExtension(videoPlayerRef.url);
                    if (!string.IsNullOrEmpty(fn)) baseName = fn + "_" + baseName;
                }
            }
            catch { }
            return baseName;
        }

        private void SetStatus(string m)
        {
            if (statusText) statusText.text = m; Debug.Log(m);
        }
    }
}
