using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Analytics;
using BilliardMasterAi.Physics;
using BilliardMasterAi.Replay;
using BilliardMasterAi.Recommendation;

namespace BilliardMasterAi.UI
{
    public class SuccessModelTrainerUI : MonoBehaviour
    {
        public Replay.ShotReplayController replay;
        public Replay.BallTrajectoryRecorder recorder;
        public Transform tableRoot;
        public Transform targetBall;
        public Transform otherBall;
        public InputField epochsInput; public InputField lrInput;
        public Button addSampleButton; public Button trainButton; public Button saveButton; public Text statusText;

        private LogisticModel _model;

        void Awake()
        {
            if (addSampleButton) addSampleButton.onClick.AddListener(AddSample);
            if (trainButton) trainButton.onClick.AddListener(Train);
            if (saveButton) saveButton.onClick.AddListener(Save);
        }

        private Vector2 To2D(Transform t){ var v = t.position; return tableRoot? (Vector2)tableRoot.InverseTransformPoint(v) : new Vector2(v.x, v.z); }

        public void AddSample()
        {
            var actual = recorder != null ? recorder.StopRecording() : null; if (actual == null || actual.Count == 0) { SetStatus("No actual path"); return; }
            var tar = To2D(targetBall); var oth = To2D(otherBall);
            // Success via simple rule (>=3 cushions before second contact using static targets)
            var verdict = CaromScorer.EvaluateThreeCushion(actual, tar, oth);
            SuccessModel.AddSample(actual, tar, oth, verdict.Success);
            SetStatus($"Sample added (cushion>=3? {verdict.Success})");
        }

        public void Train()
        {
            int epochs = 300; float lr = 0.1f;
            if (epochsInput && int.TryParse(epochsInput.text, out var e)) epochs = e;
            if (lrInput && float.TryParse(lrInput.text, out var l)) lr = l;
            _model = SuccessModel.TrainLogistic(epochs, lr);
            ShotEvaluator.SetSuccessModel(_model);
            SetStatus($"Model trained (dim={_model.Dim}) and applied");
        }

        public void Save()
        {
            if (_model == null) { SetStatus("No model"); return; }
            var json = _model.ToJson();
            var path = System.IO.Path.Combine(Application.persistentDataPath, "success_model.json");
            System.IO.File.WriteAllText(path, json);
            SetStatus($"Model saved: {path}");
        }

        private void SetStatus(string m){ if (statusText) statusText.text = m; Debug.Log(m); }
    }
}

