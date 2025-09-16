using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Replay;
using BilliardMasterAi.Rendering;
using BilliardMasterAi.Encoding;

namespace BilliardMasterAi.UI
{
    public class ReplayRenderUI : MonoBehaviour
    {
        public ReplayEditorUI editorUI;
        public ReplayRenderPipeline pipeline;
        public InputField outDirInput; public InputField fpsInput;
        public Button renderButton; public Button ffmpegButton; public Text statusText;

        void Awake()
        {
            if (renderButton) renderButton.onClick.AddListener(Render);
            if (ffmpegButton) ffmpegButton.onClick.AddListener(RunFfmpeg);
        }

        public void Render()
        {
            var edlField = typeof(ReplayEditorUI).GetField("_edl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var edl = (ReplayEdl)edlField.GetValue(editorUI);
            if (edl == null || edl.clips.Count == 0) { SetStatus("EDL empty"); return; }
            string dir = outDirInput ? outDirInput.text : System.IO.Path.Combine(Application.persistentDataPath, "Render");
            if (fpsInput && int.TryParse(fpsInput.text, out var f)) pipeline.fps = f;
            pipeline.RenderEDL(edl, dir);
            SetStatus($"Rendered PNGs to {dir}");
        }

        public void RunFfmpeg()
        {
            string dir = outDirInput ? outDirInput.text : System.IO.Path.Combine(Application.persistentDataPath, "Render");
            FfmpegWrapper.PngToMp4(dir, "c00_f%04d.png", "overlay.mp4", pipeline.fps);
            SetStatus("ffmpeg invoked (check console)");
        }

        private void SetStatus(string s){ if (statusText) statusText.text = s; Debug.Log(s); }
    }
}

