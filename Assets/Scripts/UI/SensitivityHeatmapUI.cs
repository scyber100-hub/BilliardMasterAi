using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using BilliardMasterAi.Analytics;

namespace BilliardMasterAi.UI
{
    public class SensitivityHeatmapUI : MonoBehaviour
    {
        public RawImage image;
        public int width = 128;
        public int height = 128;
        public Gradient gradient;

        public void Show(List<SensitivityMap.Cell> cells, int angleN, int speedN, int spinN)
        {
            if (image == null || cells == null || cells.Count == 0) return;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var px = new Color[width*height];
            // Map angle(x) and speed(y)
            for (int y=0; y<height; y++)
            {
                float fy = y/(float)(height-1); int ispd = Mathf.Clamp(Mathf.RoundToInt(fy*(speedN-1)),0,speedN-1);
                for (int x=0; x<width; x++)
                {
                    float fx = x/(float)(width-1); int ia = Mathf.Clamp(Mathf.RoundToInt(fx*(angleN-1)),0,angleN-1);
                    // take max over spin axis for this (ia,ispd)
                    float best = -999f;
                    for (int isp=0; isp<spinN; isp++)
                    {
                        int idx = (((ia*speedN)+ispd)*spinN) + isp;
                        if (idx >= 0 && idx < cells.Count) best = Mathf.Max(best, cells[idx].score);
                    }
                    float score = (best<-900f)?0f:best;
                    px[y*width+x] = gradient ? gradient.Evaluate(Mathf.InverseLerp(-1f,1f,score)) : new Color(score,0,1-score,1);
                }
            }
            tex.SetPixels(px); tex.Apply(false);
            image.texture = tex;
        }

        public void ShowSlice(List<SensitivityMap.Cell> cells, int angleN, int speedN, int spinN, int spinIndex)
        {
            if (image == null || cells == null || cells.Count == 0) return;
            spinIndex = Mathf.Clamp(spinIndex, 0, Mathf.Max(0, spinN-1));
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var px = new Color[width*height];
            for (int y=0; y<height; y++)
            {
                float fy = y/(float)(height-1); int ispd = Mathf.Clamp(Mathf.RoundToInt(fy*(speedN-1)),0,speedN-1);
                for (int x=0; x<width; x++)
                {
                    float fx = x/(float)(width-1); int ia = Mathf.Clamp(Mathf.RoundToInt(fx*(angleN-1)),0,angleN-1);
                    int idx = (((ia*speedN)+ispd)*spinN) + spinIndex;
                    float score = 0f; if (idx >= 0 && idx < cells.Count) score = cells[idx].score;
                    px[y*width+x] = gradient ? gradient.Evaluate(Mathf.InverseLerp(-1f,1f,score)) : new Color(score,0,1-score,1);
                }
            }
            tex.SetPixels(px); tex.Apply(false);
            image.texture = tex;
        }
    }
}
