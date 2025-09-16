using System.Collections.Generic;
using UnityEngine;
using BilliardMasterAi.Physics;
using BilliardMasterAi.Recommendation;

namespace BilliardMasterAi.AR
{
    [RequireComponent(typeof(LineRenderer))]
    public class ARGuideOverlay : MonoBehaviour
    {
        [Header("Anchors")]
        public Transform tableRoot;

        [Header("Line Settings")]
        public LineRenderer line;
        public Color lineColor = new Color(0.1f, 0.9f, 0.7f);
        public float lineWidth = 0.012f;
        public float zOffset = 0.014f;

        [Header("Markers")]
        public GameObject cushionMarkerPrefab;
        public float markerScale = 0.05f;
        public BilliardMasterAi.UI.PathAnnotationRenderer annotations;

        private readonly List<GameObject> _markers = new();

        void Reset()
        {
            line = GetComponent<LineRenderer>();
            if (line != null)
            {
                line.positionCount = 0;
                line.widthMultiplier = lineWidth;
            }
        }

        public void ShowPlan(ShotPlanResult plan)
        {
            if (tableRoot != null) transform.SetParent(tableRoot, worldPositionStays: true);
            DrawLine(plan.Path);
            PlaceMarkers(plan.Path);
            if (annotations != null)
            {
                annotations.tableRoot = tableRoot;
                annotations.Show(plan.Path);
            }
        }

        public void Clear()
        {
            if (line != null) line.positionCount = 0;
            foreach (var m in _markers) if (m) m.SetActive(false);
            if (annotations != null) annotations.Clear();
        }

        private void DrawLine(List<TrajectoryPoint> path)
        {
            if (line == null) line = GetComponent<LineRenderer>();
            if (line == null || path == null || path.Count == 0) return;
            line.startColor = lineColor; line.endColor = lineColor; line.widthMultiplier = lineWidth;
            line.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
            {
                var p = path[i].Position;
                var world = LocalToWorld(new Vector3(p.x, 0f, p.y));
                world.y += zOffset;
                line.SetPosition(i, world);
            }
        }

        private void PlaceMarkers(List<TrajectoryPoint> path)
        {
            int need = 0; foreach (var tp in path) if (tp.IsCushion) need++;
            EnsureMarkers(need);
            foreach (var m in _markers) m.SetActive(false);
            int idx = 0;
            foreach (var tp in path)
            {
                if (!tp.IsCushion) continue;
                var world = LocalToWorld(new Vector3(tp.Position.x, 0f, tp.Position.y));
                world.y += zOffset;
                var go = _markers[idx++];
                go.transform.position = world;
                go.transform.localScale = Vector3.one * markerScale;
                go.SetActive(true);
            }
        }

        private void EnsureMarkers(int count)
        {
            if (cushionMarkerPrefab == null)
            {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                var mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(1f, 0.5f, 0.1f);
                var rend = sphere.GetComponent<Renderer>();
                if (rend) rend.material = mat;
                cushionMarkerPrefab = sphere;
                cushionMarkerPrefab.SetActive(false);
            }
            while (_markers.Count < count)
            {
                var go = Instantiate(cushionMarkerPrefab);
                _markers.Add(go);
                if (tableRoot) go.transform.SetParent(tableRoot, true);
                go.SetActive(false);
            }
        }

        private Vector3 LocalToWorld(Vector3 local)
        {
            return tableRoot ? tableRoot.TransformPoint(local) : local;
        }
    }
}
