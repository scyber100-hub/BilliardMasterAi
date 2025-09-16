using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Creator;

namespace BilliardMasterAi.UI
{
    public class SceneListItem : MonoBehaviour
    {
        public Text labelText;
        public Text timeText;
        public Button jumpButton;

        private double _t;
        private System.Action<double> _onJump;

        public void Bind(ClipSegment clip, System.Action<double> onJump)
        {
            if (labelText) labelText.text = string.IsNullOrEmpty(clip.label) ? "장면" : clip.label;
            if (timeText) timeText.text = $"{TimeUtil.FormatTime(clip.start)} - {TimeUtil.FormatTime(clip.end)}";
            _t = clip.start;
            _onJump = onJump;
            if (jumpButton)
            {
                jumpButton.onClick.RemoveAllListeners();
                jumpButton.onClick.AddListener(() => _onJump?.Invoke(_t));
            }
        }
    }
}

