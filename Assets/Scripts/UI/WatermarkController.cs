using UnityEngine;
using UnityEngine.UI;

namespace BilliardMasterAi.UI
{
    public enum WatermarkPosition { TopLeft, TopRight, BottomLeft, BottomRight }

    // 오버레이 캔버스에 텍스트/이미지 워터마크를 배치/토글
    public class WatermarkController : MonoBehaviour
    {
        public CanvasGroup group;
        public Text text;
        public Image image;
        public RectTransform root; // 워터마크 루트

        public void SetVisible(bool on)
        {
            if (group == null) return;
            group.alpha = on ? 1f : 0f;
            group.interactable = on;
            group.blocksRaycasts = on;
            group.gameObject.SetActive(on);
        }

        public void SetText(string s)
        {
            if (text != null) text.text = s ?? string.Empty;
        }

        public void SetImage(Sprite sprite)
        {
            if (image != null)
            {
                image.sprite = sprite;
                image.enabled = sprite != null;
            }
        }

        public void SetPosition(WatermarkPosition pos, Vector2 margin)
        {
            if (root == null) root = GetComponent<RectTransform>();
            if (root == null) return;

            Vector2 anchorMin, anchorMax, pivot;
            switch (pos)
            {
                case WatermarkPosition.TopLeft:     anchorMin = anchorMax = new Vector2(0,1); pivot = new Vector2(0,1); break;
                case WatermarkPosition.TopRight:    anchorMin = anchorMax = new Vector2(1,1); pivot = new Vector2(1,1); break;
                case WatermarkPosition.BottomLeft:  anchorMin = anchorMax = new Vector2(0,0); pivot = new Vector2(0,0); break;
                default:                            anchorMin = anchorMax = new Vector2(1,0); pivot = new Vector2(1,0); break;
            }
            root.anchorMin = anchorMin; root.anchorMax = anchorMax; root.pivot = pivot;
            root.anchoredPosition = new Vector2((pivot.x>0.5f?-margin.x:margin.x), (pivot.y>0.5f?-margin.y:margin.y));
        }
    }
}

