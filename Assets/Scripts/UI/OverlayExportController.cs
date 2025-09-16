using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace BilliardMasterAi.UI
{
    // 투명 배경 오버레이 PNG 시퀀스/단일 PNG 내보내기
    public class OverlayExportController : MonoBehaviour
    {
        [Header("Overlay Sources")]
        public ImagePathRenderer actualRenderer;
        public ImagePathRenderer idealRenderer;
        public Canvas overlayCanvas;      // 오버레이 전용 Canvas (Screen Space - Camera 권장)
        public Camera overlayCamera;      // 투명 배경 렌더용 카메라 (Clear: Solid Color RGBA(0,0,0,0))

        [Header("UI")] 
        public InputField outDirInput;
        public InputField widthInput;   // e.g., 1920
        public InputField heightInput;  // e.g., 1080
        public InputField durationInput; // seconds
        public InputField fpsInput;      // frames per sec
        public Toggle animateToggle;     // 진행선 애니메이션(부분 그리기)
        public Text statusText;

        private List<Vector2> _actualUV = new();
        private List<Vector2> _idealUV = new();

        public void SetUVPaths(List<Vector2> actualUV, List<Vector2> idealUV)
        {
            _actualUV = actualUV != null ? new List<Vector2>(actualUV) : new List<Vector2>();
            _idealUV = idealUV != null ? new List<Vector2>(idealUV) : new List<Vector2>();
        }

        public void ExportPngSequence()
        {
            string outDir = GetOutDir();
            (int W, int H, float dur, int fps) = GetParams();
            if (W <= 0 || H <= 0 || fps <= 0 || dur <= 0f) { SetStatus("해상도/지속시간/fps를 확인하세요."); return; }
            Directory.CreateDirectory(outDir);

            // Prepare camera
            if (overlayCamera == null) overlayCamera = Camera.main;
            if (overlayCamera == null) { SetStatus("오버레이 카메라가 필요합니다."); return; }
            var rt = new RenderTexture(W, H, 0, RenderTextureFormat.ARGB32);
            var oldTarget = overlayCamera.targetTexture;
            var oldActive = RenderTexture.active;

            try
            {
                overlayCamera.targetTexture = rt;
                RenderTexture.active = rt;

                int total = Mathf.CeilToInt(dur * fps);
                for (int i = 0; i < total; i++)
                {
                    float t = (i + 0.0001f) / Mathf.Max(1, total - 1);
                    UpdateProgress(t);
                    overlayCamera.Render();

                    var tex = new Texture2D(W, H, TextureFormat.RGBA32, false, false);
                    tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
                    tex.Apply(false);
                    byte[] png = tex.EncodeToPNG();
                    Destroy(tex);
                    string path = Path.Combine(outDir, $"overlay_{i:0000}.png");
                    File.WriteAllBytes(path, png);
                }
                SetStatus($"PNG 시퀀스 저장 완료: {outDir}");
            }
            catch (Exception e)
            {
                SetStatus($"내보내기 실패: {e.Message}");
            }
            finally
            {
                overlayCamera.targetTexture = oldTarget;
                RenderTexture.active = oldActive;
                rt.Release();
            }
        }

        public void ExportSinglePng()
        {
            string outDir = GetOutDir();
            (int W, int H, float dur, int fps) = GetParams();
            if (W <= 0 || H <= 0) { SetStatus("해상도를 확인하세요."); return; }
            Directory.CreateDirectory(outDir);
            if (overlayCamera == null) overlayCamera = Camera.main;
            if (overlayCamera == null) { SetStatus("오버레이 카메라가 필요합니다."); return; }
            var rt = new RenderTexture(W, H, 0, RenderTextureFormat.ARGB32);
            var oldTarget = overlayCamera.targetTexture;
            var oldActive = RenderTexture.active;
            try
            {
                UpdateProgress(1f); // full path
                overlayCamera.targetTexture = rt;
                RenderTexture.active = rt;
                overlayCamera.Render();
                var tex = new Texture2D(W, H, TextureFormat.RGBA32, false, false);
                tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
                tex.Apply(false);
                byte[] png = tex.EncodeToPNG();
                Destroy(tex);
                string path = Path.Combine(outDir, $"overlay.png");
                File.WriteAllBytes(path, png);
                SetStatus($"PNG 저장 완료: {path}");
            }
            catch (Exception e)
            {
                SetStatus($"내보내기 실패: {e.Message}");
            }
            finally
            {
                overlayCamera.targetTexture = oldTarget;
                RenderTexture.active = oldActive;
                rt.Release();
            }
        }

        // MP4(알파) 직접 인코딩은 기본 Unity로 불가. PNG 시퀀스 + ffmpeg 권고.
        public void CopyFfmpegCommand()
        {
            string outDir = GetOutDir();
            (int W, int H, float dur, int fps) = GetParams();
            string cmd = $"ffmpeg -framerate {fps} -i overlay_%04d.png -c:v libx264 -pix_fmt yuv420p -crf 18 overlay.mp4";
            GUIUtility.systemCopyBuffer = cmd;
            SetStatus("ffmpeg 명령이 클립보드에 복사되었습니다.");
            File.WriteAllText(Path.Combine(outDir, "README_export.txt"),
                "PNG 시퀀스를 ffmpeg로 MP4로 변환:\n" + cmd +
                "\n(투명 영상이 필요하면 알파 지원 코덱을 사용하거나, 편집 툴에서 PNG 시퀀스를 곧바로 사용하세요.)\n");
        }

        private void UpdateProgress(float t)
        {
            if (animateToggle != null && animateToggle.isOn)
            {
                // Reduce point counts according to progress
                if (actualRenderer != null) ApplyProgress(actualRenderer, _actualUV, t);
                if (idealRenderer != null) ApplyProgress(idealRenderer, _idealUV, t);
            }
        }

        private void ApplyProgress(ImagePathRenderer renderer, List<Vector2> uv, float t)
        {
            if (renderer == null) return;
            if (uv == null || uv.Count == 0) { renderer.Clear(); return; }
            int count = Mathf.Max(1, Mathf.RoundToInt(uv.Count * Mathf.Clamp01(t)));
            var tmp = uv.GetRange(0, count);
            renderer.DrawUV(tmp);
        }

        private (int,int,float,int) GetParams()
        {
            int W = TryParseInt(widthInput?.text, 1920);
            int H = TryParseInt(heightInput?.text, 1080);
            float dur = TryParseFloat(durationInput?.text, 3f);
            int fps = TryParseInt(fpsInput?.text, 30);
            return (W,H,dur,fps);
        }

        private string GetOutDir()
        {
            string dir = outDirInput != null && !string.IsNullOrEmpty(outDirInput.text)
                ? outDirInput.text
                : Path.Combine(Application.persistentDataPath, "OverlayExport");
            return dir;
        }

        private int TryParseInt(string s, int d){ return int.TryParse(s, out var v) ? v : d; }
        private float TryParseFloat(string s, float d){ return float.TryParse(s, out var v) ? v : d; }

        private void SetStatus(string msg){ if (statusText) statusText.text = msg; Debug.Log(msg); }
    }
}

