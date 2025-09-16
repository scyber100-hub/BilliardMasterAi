using UnityEngine;
using UnityEngine.UI;

namespace BilliardMasterAi.UI
{
    [RequireComponent(typeof(Image))]
    public class IconImage : MonoBehaviour
    {
        public IconSet iconSet;
        public IconKey key;
        public bool autoNativeSize = false;

        private Image _img;

        void Awake()
        {
            _img = GetComponent<Image>();
            Apply();
        }

        public void Apply()
        {
            if (_img == null) _img = GetComponent<Image>();
            if (iconSet == null) return;
            var sprite = iconSet.Get(key);
            if (sprite != null)
            {
                _img.sprite = sprite;
                _img.enabled = true;
                if (autoNativeSize) _img.SetNativeSize();
            }
        }
    }
}

