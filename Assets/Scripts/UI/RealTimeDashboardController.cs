using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Analysis;

namespace BilliardMasterAi.UI
{
    public class RealTimeDashboardController : MonoBehaviour
    {
        public Transform content;
        public GameObject itemPrefab; // contains RealTimeDashboardItem
        public float refreshInterval = 1.0f; // seconds

        private float _t;
        private readonly Dictionary<string, RealTimeDashboardItem> _items = new();

        void Update()
        {
            _t += Time.deltaTime;
            if (_t >= refreshInterval)
            {
                _t = 0f;
                Refresh();
            }
        }

        public void Refresh()
        {
            var list = DashboardService.GetAll();
            foreach (var s in list)
            {
                if (!_items.TryGetValue(s.tableId, out var item))
                {
                    if (content == null || itemPrefab == null) return;
                    var go = Instantiate(itemPrefab, content);
                    item = go.GetComponent<RealTimeDashboardItem>();
                    _items[s.tableId] = item;
                }
                item?.Bind(s);
            }
        }
    }
}

