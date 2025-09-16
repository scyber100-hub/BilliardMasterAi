using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BilliardMasterAi.UI
{
    // Draws a polyline over a RawImage using UV(0..1) points or table-local points with a mapper.
    [RequireComponent(typeof(LineRenderer))]
    public class ImagePathRenderer : MonoBehaviour
    {
        public RawImage targetImage;
        public LineRenderer line;
        public float width = 3f; // pixels, approximated via world units below
        public Color color = new Color(1f, 0.3f, 0.2f, 0.9f);

        void Reset()
        {
            line = GetComponent<LineRenderer>();
            if (line)
            {
                line.useWorldSpace = false;
                line.positionCount = 0;
                line.widthMultiplier = 0.003f; // tuned for 1080p canvas scale; adjust via Canvas scaler
                line.startColor = color; line.endColor = color;
            }
        }

        public void DrawUV(List<Vector2> uvPoints)
        {
            if (targetImage == null || line == null || uvPoints == null || uvPoints.Count == 0) { Clear(); return; }
            var rt = targetImage.rectTransform;
            var size = rt.rect.size; var pivot = rt.pivot;
            line.positionCount = uvPoints.Count;
            for (int i = 0; i < uvPoints.Count; i++)
            {
                var uv = uvPoints[i];
                float x = uv.x * size.x - pivot.x * size.x;
                float y = uv.y * size.y - pivot.y * size.y;
                line.SetPosition(i, new Vector3(x, y, 0f));
            }
        }

        public void DrawTablePoints(List<Vector2> tablePoints, BilliardMasterAi.Creator.VideoTableMapper mapper)
        {
            if (tablePoints == null || tablePoints.Count == 0 || mapper == null)
            { Clear(); return; }
            var uv = new List<Vector2>(tablePoints.Count);
            foreach (var p in tablePoints) uv.Add(mapper.TableToImage(p));
            DrawUV(uv);
        }

        public void Clear()
        {
            if (line) line.positionCount = 0;
        }
    }
}

