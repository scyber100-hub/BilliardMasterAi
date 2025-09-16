using UnityEngine;
using UnityEngine.UI;

namespace BilliardMasterAi.UI
{
    public class BadgeToast : MonoBehaviour
    {
        public CanvasGroup group;
        public Text titleText;
        public Text descText;
        public float showTime = 2.0f;
        public float fadeTime = 0.25f;

        private float _t;
        private int _state; // 0 idle, 1 fade in, 2 hold, 3 fade out

        void Awake()
        {
            if (group == null) group = GetComponent<CanvasGroup>();
            if (group != null) group.alpha = 0f;
        }

        public void Show(string title, string desc)
        {
            if (titleText) titleText.text = title;
            if (descText) descText.text = desc;
            _t = 0f; _state = 1;
            if (group) group.gameObject.SetActive(true);
        }

        void Update()
        {
            if (group == null || _state == 0) return;
            _t += Time.deltaTime;

            if (_state == 1) // fade in
            {
                group.alpha = Mathf.Clamp01(_t / fadeTime);
                if (_t >= fadeTime) { _t = 0f; _state = 2; }
            }
            else if (_state == 2) // hold
            {
                group.alpha = 1f;
                if (_t >= showTime) { _t = 0f; _state = 3; }
            }
            else if (_state == 3) // fade out
            {
                group.alpha = 1f - Mathf.Clamp01(_t / fadeTime);
                if (_t >= fadeTime) { _state = 0; group.alpha = 0f; group.gameObject.SetActive(false); }
            }
        }
    }
}

