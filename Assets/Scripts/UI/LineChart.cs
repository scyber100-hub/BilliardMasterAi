using UnityEngine;
using UnityEngine.UI;

namespace BilliardMasterAi.UI
{
    public class LineChart : MonoBehaviour
    {
        public RawImage image;
        public int width = 400;
        public int height = 120;
        public Color lineColor = new Color(0.2f,0.8f,1f,1);

        public void Plot(float[] values)
        {
            if (image == null || values == null || values.Length == 0) return;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var px = new Color[width*height]; for (int i=0;i<px.Length;i++) px[i] = new Color(0,0,0,0);
            float min=float.PositiveInfinity, max=float.NegativeInfinity; foreach (var v in values){ if (v<min)min=v; if (v>max)max=v; }
            if (Mathf.Approximately(max,min)) { max=min+1; }
            for (int x=0;x<width;x++)
            {
                float t = x/(float)(width-1);
                int idx = Mathf.Clamp(Mathf.RoundToInt(t*(values.Length-1)),0,values.Length-1);
                float v = Mathf.InverseLerp(min,max,values[idx]);
                int y = Mathf.Clamp(Mathf.RoundToInt(v*(height-1)),0,height-1);
                px[y*width + x] = lineColor;
            }
            tex.SetPixels(px); tex.Apply(false);
            image.texture = tex;
        }
    }
}

