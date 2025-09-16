using UnityEngine;

namespace BilliardMasterAi.Perception
{
    public static class TableCoordinateMapper
    {
        // Raycast from screen point to table plane defined by tableRoot (its up is plane normal)
        public static bool ScreenToTableLocal2D(Vector2 screenPos, Camera cam, Transform tableRoot, out Vector2 tableLocal)
        {
            tableLocal = default;
            if (cam == null || tableRoot == null) return false;
            var ray = cam.ScreenPointToRay(screenPos);
            var plane = new Plane(tableRoot.up, tableRoot.position);
            if (plane.Raycast(ray, out float dist))
            {
                var world = ray.GetPoint(dist);
                var local = tableRoot.InverseTransformPoint(world);
                tableLocal = new Vector2(local.x, local.z); // XZ plane
                return true;
            }
            return false;
        }
    }
}

