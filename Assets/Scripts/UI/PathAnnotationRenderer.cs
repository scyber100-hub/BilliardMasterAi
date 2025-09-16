using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Physics;

namespace BilliardMasterAi.UI
{
    // 경로에 각도/거리/쿠션 지점 주석을 표시하는 간단한 렌더러 (월드 스페이스 캔버스)
    public class PathAnnotationRenderer : MonoBehaviour
    {
        public Transform tableRoot;
        public GameObject labelPrefab; // contains Text
        public float yOffset = 0.02f;

        private readonly List<GameObject> _labels = new();

        public void Show(List<TrajectoryPoint> path)
        {
            Clear();
            if (path == null || path.Count < 2) return;
            float total = 0f;
            for (int i = 1; i < path.Count; i++)
            {
                var a = path[i - 1].Position; var b = path[i].Position;
                float seg = Vector2.Distance(a, b); total += seg;
                if (path[i].IsCushion)
                {
                    var world = tableRoot ? tableRoot.TransformPoint(new Vector3(b.x, 0f, b.y)) : new Vector3(b.x, 0f, b.y);
                    var go = Instantiate(labelPrefab, world + Vector3.up * yOffset, Quaternion.identity, tableRoot);
                    var txt = go.GetComponentInChildren<Text>();
                    if (txt) txt.text = $"쿠션 {i}\n누적 {total:0.00}m";
                    _labels.Add(go);
                }
            }
        }

        public void Clear()
        {
            foreach (var go in _labels) if (go) Destroy(go);
            _labels.Clear();
        }
    }
}

