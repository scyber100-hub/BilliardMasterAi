using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BilliardMasterAi.Replay;

namespace BilliardMasterAi.Rendering
{
    public class ReplayRenderPipeline : MonoBehaviour
    {
        public Camera renderCamera; // set to overlay/scene camera
        public Canvas overlayCanvas; // optional
        public int width = 1920; public int height = 1080; public int fps = 30;
        public System.Action<float> OnSeek; // external seek callback (time seconds)
        public UnityEngine.UI.Text captionText; // optional overlay caption

        public void RenderEDL(ReplayEdl edl, string outDir)
        {
            if (renderCamera == null || edl == null) { Debug.LogWarning("RenderEDL: missing camera or EDL"); return; }
            Directory.CreateDirectory(outDir);
            var rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            var old = renderCamera.targetTexture; renderCamera.targetTexture = rt; var oldActive = RenderTexture.active;
            try
            {
                int clipIndex = 0; foreach (var clip in edl.clips)
                {
                    float duration = Mathf.Max(0.001f, clip.end - clip.start);
                    float spd = Mathf.Max(0.01f, clip.speed);
                    int frames = Mathf.CeilToInt(duration * fps);
                    if (captionText) captionText.text = clip.caption;
                    for (int i=0;i<frames;i++)
                    {
                        // Advance scene via seek callback: time mapping considers speed
                        float t = clip.start + (i/(float)fps) / spd;
                        OnSeek?.Invoke(t);
                        renderCamera.Render();
                        RenderTexture.active = rt; var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                        tex.ReadPixels(new Rect(0,0,width,height),0,0); tex.Apply(false);
                        var bytes = tex.EncodeToPNG(); Destroy(tex);
                        string path = Path.Combine(outDir, $"c{clipIndex:00}_f{i:0000}.png"); File.WriteAllBytes(path, bytes);
                    }
                    clipIndex++;
                }
            }
            finally
            {
                renderCamera.targetTexture = old; RenderTexture.active = oldActive; rt.Release();
            }
        }
    }
}
