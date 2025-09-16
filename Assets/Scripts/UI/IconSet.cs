using System;
using System.Collections.Generic;
using UnityEngine;

namespace BilliardMasterAi.UI
{
    [CreateAssetMenu(menuName = "BilliardMasterAi/Icon Set", fileName = "IconSet")]
    public class IconSet : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public IconKey key;
            public Sprite sprite;
        }

        public List<Entry> entries = new List<Entry>();

        private Dictionary<IconKey, Sprite> _map;

        private void OnEnable()
        {
            _map = new Dictionary<IconKey, Sprite>();
            foreach (var e in entries)
            {
                _map[e.key] = e.sprite;
            }
        }

        public Sprite Get(IconKey key)
        {
            if (_map == null) OnEnable();
            return key != IconKey.None && _map != null && _map.TryGetValue(key, out var s) ? s : null;
        }
    }
}

