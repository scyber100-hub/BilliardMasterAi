using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Perception;

namespace BilliardMasterAi.UI
{
    public class BallDetectionOverlay : MonoBehaviour
    {
        public Transform tableRoot;
        public GameObject markerPrefab;
        public float markerScale = 0.06f;

        private readonly List<GameObject> _pool = new();

        public void Show(List<RecognizedBall> balls)
        {
            EnsurePool(balls.Count);
            for (int i = 0; i < _pool.Count; i++) _pool[i].SetActive(false);

            for (int i = 0; i < balls.Count; i++)
            {
                var go = _pool[i];
                var local = new Vector3(balls[i].tableLocal.x, 0f, balls[i].tableLocal.y);
                go.transform.SetParent(tableRoot, false);
                go.transform.localPosition = local;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one * markerScale;
                go.SetActive(true);

                var r = go.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    var col = ColorFromBall(balls[i].color);
                    if (r.material != null && r.material.HasProperty("_Color")) r.material.color = col;
                }
            }
        }

        private void EnsurePool(int needed)
        {
            if (markerPrefab == null)
            {
                markerPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            }
            while (_pool.Count < needed)
            {
                var go = Instantiate(markerPrefab);
                _pool.Add(go);
            }
        }

        private Color ColorFromBall(BallColor c)
        {
            return c switch
            {
                BallColor.Red => new Color(0.9f, 0.2f, 0.2f),
                BallColor.Yellow => new Color(0.95f, 0.85f, 0.2f),
                BallColor.White => new Color(0.95f, 0.95f, 0.95f),
                _ => Color.magenta
            };
        }
    }
}

