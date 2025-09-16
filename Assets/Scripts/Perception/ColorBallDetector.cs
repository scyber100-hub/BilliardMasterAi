using System;
using System.Collections.Generic;
using UnityEngine;

namespace BilliardMasterAi.Perception
{
    public enum BallColor { Red, Yellow, White }

    [Serializable]
    public class ColorThreshold
    {
        public Vector3 hsvMin;
        public Vector3 hsvMax;
    }

    [Serializable]
    public class BallDetectionConfig
    {
        public int downscaleWidth = 320;
        public int downscaleHeight = 180;
        public int minPixels = 50;
        public bool flipY = true; // camera image to screen mapping

        public ColorThreshold red = new ColorThreshold
        {
            hsvMin = new Vector3(0f, 0.55f, 0.25f),
            hsvMax = new Vector3(0.06f, 1f, 1f)
        };

        public ColorThreshold yellow = new ColorThreshold
        {
            hsvMin = new Vector3(0.10f, 0.45f, 0.35f),
            hsvMax = new Vector3(0.18f, 1f, 1f)
        };

        public ColorThreshold white = new ColorThreshold
        {
            hsvMin = new Vector3(0f, 0f, 0.7f),
            hsvMax = new Vector3(1f, 0.25f, 1f)
        };
    }

    public struct Detection
    {
        public BallColor color;
        public Vector2 normalizedPos; // 0..1 (x,y) in image space
        public int pixelCount;
        public float confidence; // 0..1
    }

    public static class ColorBallDetector
    {
        // Very simple color segmentation + centroid per color from a Texture2D (fallback)
        public static List<Detection> Detect(Texture2D image, BallDetectionConfig cfg = null)
        {
            cfg ??= new BallDetectionConfig();
            var pixels = image.GetPixels32();
            return DetectFromBuffer(pixels, image.width, image.height, cfg);
        }

        // Faster path: process Color32 buffer with grid sampling to approximate downscale.
        public static List<Detection> DetectFromBuffer(Color32[] pixels, int srcW, int srcH, BallDetectionConfig cfg)
        {
            cfg ??= new BallDetectionConfig();
            int w = cfg.downscaleWidth;
            int h = cfg.downscaleHeight;

            int strideX = Mathf.Max(1, Mathf.FloorToInt((float)srcW / w));
            int strideY = Mathf.Max(1, Mathf.FloorToInt((float)srcH / h));

            var sumR = Vector2.zero; int cntR = 0;
            var sumY = Vector2.zero; int cntY = 0;
            var sumW = Vector2.zero; int cntW = 0;

            int sx = 0, sy = 0; // sampled coordinates in downscaled grid
            for (int y = 0; y < srcH; y += strideY)
            {
                sx = 0;
                for (int x = 0; x < srcW; x += strideX)
                {
                    var c = pixels[y * srcW + x];
                    Color.RGBToHSV(new Color32(c.r, c.g, c.b, 255), out float H, out float S, out float V);
                    var hsv = new Vector3(H, S, V);

                    if (InRange(hsv, cfg.red)) { sumR += new Vector2(sx, sy); cntR++; }
                    if (InRange(hsv, cfg.yellow)) { sumY += new Vector2(sx, sy); cntY++; }
                    if (InRange(hsv, cfg.white)) { sumW += new Vector2(sx, sy); cntW++; }
                    sx++;
                }
                sy++;
            }

            // Use downscaled width/height approximations
            int dw = Mathf.CeilToInt((float)srcW / strideX);
            int dh = Mathf.CeilToInt((float)srcH / strideY);

            var results = new List<Detection>(3);
            if (cntR >= cfg.minPixels) results.Add(MakeDet(BallColor.Red, sumR / Mathf.Max(1, cntR), cntR, dw, dh, cfg));
            if (cntY >= cfg.minPixels) results.Add(MakeDet(BallColor.Yellow, sumY / Mathf.Max(1, cntY), cntY, dw, dh, cfg));
            if (cntW >= cfg.minPixels) results.Add(MakeDet(BallColor.White, sumW / Mathf.Max(1, cntW), cntW, dw, dh, cfg));

            return results;
        }

        private static bool InRange(Vector3 hsv, ColorThreshold t)
        {
            return hsv.x >= t.hsvMin.x && hsv.x <= t.hsvMax.x &&
                   hsv.y >= t.hsvMin.y && hsv.y <= t.hsvMax.y &&
                   hsv.z >= t.hsvMin.z && hsv.z <= t.hsvMax.z;
        }

        private static Detection MakeDet(BallColor color, Vector2 sum, int count, int w, int h, BallDetectionConfig cfg)
        {
            var p = sum / Mathf.Max(1, count);
            float nx = p.x / (float)w;
            float ny = p.y / (float)h;
            if (cfg.flipY) ny = 1f - ny;
            return new Detection
            {
                color = color,
                normalizedPos = new Vector2(nx, ny),
                pixelCount = count,
                confidence = Mathf.Clamp01(count / (float)(w * h * 0.05f))
            };
        }

        // Estimate table felt hue and slightly adjust thresholds to reduce false positives.
        public static BallDetectionConfig AutoCalibrateGreenFelt(Color32[] pixels, int w, int h, BallDetectionConfig baseCfg = null)
        {
            baseCfg ??= new BallDetectionConfig();
            int stride = 16; float sumH=0, sumS=0, sumV=0; int cnt=0;
            for (int y=0; y<h; y+=stride)
            for (int x=0; x<w; x+=stride)
            {
                var c = pixels[y*w+x];
                Color.RGBToHSV(new Color32(c.r,c.g,c.b,255), out float H, out float S, out float V);
                if (S>0.2f && V>0.2f && H>0.20f && H<0.45f) { sumH+=H; sumS+=S; sumV+=V; cnt++; }
            }
            if (cnt<8) return baseCfg;

            float avgS=sumS/cnt;
            var cfg = baseCfg; // copy-by-ref ok for our use
            // Keep white low saturation upper bound based on background
            cfg.white.hsvMax.y = Mathf.Clamp01(Mathf.Min(cfg.white.hsvMax.y, avgS*0.5f));
            return cfg;
        }
    }
}
