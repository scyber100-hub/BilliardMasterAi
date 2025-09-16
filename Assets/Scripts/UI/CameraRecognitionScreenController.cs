using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Perception;

namespace BilliardMasterAi.UI
{
    public class CameraRecognitionScreenController : MonoBehaviour
    {
        public BallRecognitionController recognition;
        public BallDetectionOverlay overlay;
        public Button captureButton;
        public Toggle showOverlayToggle;

        void Awake()
        {
            if (captureButton != null) captureButton.onClick.AddListener(OnCaptureClicked);
            if (showOverlayToggle != null) showOverlayToggle.onValueChanged.AddListener(OnToggleChanged);
        }

        private void OnCaptureClicked()
        {
            recognition?.CaptureAndDetect();
            if (overlay != null && recognition != null)
            {
                overlay.tableRoot = recognition.tableRoot;
                overlay.Show(recognition.lastResults);
            }
        }

        private void OnToggleChanged(bool on)
        {
            if (overlay != null) overlay.gameObject.SetActive(on);
        }
    }
}

