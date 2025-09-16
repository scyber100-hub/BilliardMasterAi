using UnityEngine;
using UnityEngine.UI;

namespace BilliardMasterAi.UI
{
    // RawImage 위를 탭해 영상 속 테이블 4코너(TL,TR,BR,BL)를 순서대로 찍는 보정 UI
    public class VideoTableCalibration : MonoBehaviour
    {
        public RawImage image;
        public BilliardMasterAi.Creator.VideoTableMapper mapper;
        public Text hintText;

        private int _step; // 0..3

        void OnEnable()
        {
            UpdateHint();
        }

        void Update()
        {
            if (image == null || mapper == null) return;
            if (!Input.GetMouseButtonDown(0)) return;
            var rect = image.rectTransform;
            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, null, out local)) return;
            var size = rect.rect.size; var pivot = rect.pivot;
            var pos = new Vector2(local.x + size.x * pivot.x, local.y + size.y * pivot.y);
            var uv = new Vector2(pos.x / size.x, pos.y / size.y);

            if (_step == 0) mapper.topLeft = uv;
            else if (_step == 1) mapper.topRight = uv;
            else if (_step == 2) mapper.bottomRight = uv;
            else if (_step == 3) mapper.bottomLeft = uv;

            _step = (_step + 1) % 4;
            UpdateHint();
        }

        private void UpdateHint()
        {
            if (hintText == null) return;
            string[] labels = { "좌상단", "우상단", "우하단", "좌하단" };
            hintText.text = $"테이블 {labels[_step]} 모서리를 탭하세요";
        }
    }
}

