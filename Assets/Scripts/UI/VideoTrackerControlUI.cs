using UnityEngine;
using UnityEngine.UI;

namespace BilliardMasterAi.UI
{
    // Simple UI to run VideoBallTracker.StartTrack with user-defined start/end times and options.
    public class VideoTrackerControlUI : MonoBehaviour
    {
        public BilliardMasterAi.Creator.VideoBallTracker tracker;
        public UnityEngine.Video.VideoPlayer videoPlayer;
        public InputField startTimeInput; // seconds
        public InputField endTimeInput;   // seconds
        public Toggle useKalmanToggle;
        public Button trackButton;
        public Text statusText;

        void Awake()
        {
            if (trackButton) trackButton.onClick.AddListener(OnTrackClicked);
        }

        public void OnTrackClicked()
        {
            if (tracker == null)
            {
                SetStatus("Tracker not set");
                return;
            }
            if (useKalmanToggle) tracker.useKalman = useKalmanToggle.isOn;

            double start = 0, end = (videoPlayer != null && videoPlayer.length > 0) ? videoPlayer.length : 10.0;
            if (startTimeInput && double.TryParse(startTimeInput.text, out var s)) start = Mathf.Max(0f, (float)s);
            if (endTimeInput && double.TryParse(endTimeInput.text, out var e)) end = Mathf.Max((float)start + 0.5f, (float)e);

            tracker.StartTrack(start, end);
            SetStatus($"Tracking {start:0.00}s ~ {end:0.00}s (Kalman {(tracker.useKalman?"ON":"OFF")})");
        }

        private void SetStatus(string msg)
        {
            if (statusText) statusText.text = msg;
            Debug.Log(msg);
        }
    }
}

