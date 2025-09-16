#if UNITY_BARRACUDA
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

namespace BilliardMasterAi.Perception
{
    // Barracuda 기반 공 검출기 (ONNX 모델 필요). 리소스에서 NNModel 로드.
    public class MLBallDetector : System.IDisposable
    {
        private IWorker _worker;
        private Model _model;
        private string _inputName = "input";
        private string _outputName = "output";

        public MLBallDetector(NNModel modelAsset)
        {
            _model = ModelLoader.Load(modelAsset);
            _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, _model);
        }

        public List<Detection> Detect(Texture2D tex, BallDetectionConfig cfg)
        {
            var t = new Tensor(tex, channels: 3);
            _worker.Execute(new Dictionary<string, Tensor>{{_inputName, t}});
            var o = _worker.PeekOutput(_outputName);
            // NOTE: 실제 모델 포맷에 맞춰 파싱해야 함. 여기서는 스텁(빈 결과 반환) 처리.
            t.Dispose(); o.Dispose();
            return new List<Detection>();
        }

        public void Dispose(){ _worker?.Dispose(); }
    }
}
#endif

