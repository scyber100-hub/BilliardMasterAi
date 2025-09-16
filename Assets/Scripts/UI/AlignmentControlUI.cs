using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Analysis;

namespace BilliardMasterAi.UI
{
    // Simple UI to load alignment from store and publish to bus; also display current alignment.
    public class AlignmentControlUI : MonoBehaviour
    {
        public InputField nameInput;
        public Button loadButton;
        public Button publishZeroButton; // clear alignment
        public Text statusText;

        void Awake()
        {
            if (loadButton) loadButton.onClick.AddListener(LoadAlignment);
            if (publishZeroButton) publishZeroButton.onClick.AddListener(ClearAlignment);
        }

        public void LoadAlignment()
        {
            string name = nameInput ? nameInput.text : "alignment";
            if (AlignmentStore.Load(name, out var ar))
            {
                AlignmentBus.Publish(ar);
                if (statusText) statusText.text = $"정렬 로드/게시: {name} · dt={ar.TimeOffset:+0.00;-0.00}s, rms={ar.RmsError*100f:0.0}cm";
            }
            else
            {
                if (statusText) statusText.text = "정렬 파일 없음";
            }
        }

        public void ClearAlignment()
        {
            var zero = new AlignmentResult { TimeOffset = 0f, Offset = Vector2.zero, Scale = 1f, AngleRad = 0f, RmsError = 0f, AlignedTracked = null };
            AlignmentBus.Publish(zero);
            if (statusText) statusText.text = "정렬 초기화(0) 게시";
        }
    }
}

