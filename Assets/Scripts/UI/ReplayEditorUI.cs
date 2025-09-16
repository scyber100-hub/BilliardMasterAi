using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Replay;

namespace BilliardMasterAi.UI
{
    public class ReplayEditorUI : MonoBehaviour
    {
        public InputField startInput;
        public InputField endInput;
        public InputField labelInput;
        public InputField speedInput;
        public InputField captionInput;
        public Button addClipButton;
        public Button exportEdlButton;
        public Text statusText;

        private ReplayEdl _edl = new ReplayEdl();

        void Awake()
        {
            if (addClipButton) addClipButton.onClick.AddListener(AddClip);
            if (exportEdlButton) exportEdlButton.onClick.AddListener(Export);
        }

        public void AddClip()
        {
            if (!float.TryParse(startInput?.text, out var s)) s = 0f;
            if (!float.TryParse(endInput?.text, out var e)) e = s + 1f;
            e = Mathf.Max(e, s + 0.5f);
            var clip = new ReplayClip { start = s, end = e, label = labelInput ? labelInput.text : string.Empty };
            _edl.clips.Add(clip);
            SetStatus($"Clip added: {s:0.00}-{e:0.00} {clip.label}");
        }

        public void Export()
        {
            var json = _edl.ToJson();
#if UNITY_EDITOR
            string path = UnityEditor.EditorUtility.SaveFilePanel("Export EDL", Application.persistentDataPath, "replay_edl.json", "json");
            if (!string.IsNullOrEmpty(path)) { System.IO.File.WriteAllText(path, json); SetStatus($"EDL saved: {path}"); }
#else
            var path = System.IO.Path.Combine(Application.persistentDataPath, "replay_edl.json"); System.IO.File.WriteAllText(path, json); SetStatus($"EDL saved: {path}");
#endif
        }

        private void SetStatus(string s){ if (statusText) statusText.text = s; Debug.Log(s); }
    }
}
