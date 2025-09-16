using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

#if TMP_PRESENT
using TMPro;
#endif

namespace BilliardMasterAi.EditorTools
{
    public class EmojiScanner : EditorWindow
    {
        [MenuItem("BilliardMasterAi/Tools/Scan Emojis in UI Text")] 
        public static void ShowWindow()
        {
            GetWindow<EmojiScanner>("Emoji Scanner");
        }

        private Vector2 _scroll;
        private List<string> _results = new List<string>();

        void OnGUI()
        {
            if (GUILayout.Button("Scan Open Scenes"))
            {
                ScanOpenScenes();
            }
            _scroll = GUILayout.BeginScrollView(_scroll);
            foreach (var r in _results)
                GUILayout.Label(r);
            GUILayout.EndScrollView();
        }

        private void ScanOpenScenes()
        {
            _results.Clear();
            var scenes = UnityEditor.SceneManagement.EditorSceneManager.sceneCount;
            for (int i=0;i<scenes;i++)
            {
                var s = UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i);
                if (!s.isLoaded) continue;
                foreach (var root in s.GetRootGameObjects())
                {
                    ScanGO(root, s.path);
                }
            }
            if (_results.Count == 0) _results.Add("No emojis detected in open scenes.");
        }

        private void ScanGO(GameObject go, string scenePath)
        {
            foreach (var text in go.GetComponentsInChildren<Text>(true))
            {
                if (ContainsEmoji(text.text))
                    _results.Add($"{Path.GetFileName(scenePath)} > {GetPath(text.gameObject)} : '{text.text}'");
            }
#if TMP_PRESENT
            foreach (var tmp in go.GetComponentsInChildren<TMP_Text>(true))
            {
                if (ContainsEmoji(tmp.text))
                    _results.Add($"{Path.GetFileName(scenePath)} > {GetPath(tmp.gameObject)} : '{tmp.text}'");
            }
#endif
        }

        private static string GetPath(GameObject go)
        {
            var stack = new Stack<string>();
            var cur = go.transform;
            while (cur != null) { stack.Push(cur.name); cur = cur.parent; }
            return string.Join("/", stack.ToArray());
        }

        // Simple heuristic: characters outside common ASCII + Hangul range or surrogate pairs
        private static bool ContainsEmoji(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (var ch in s)
            {
                if (char.IsSurrogate(ch)) return true; // many emojis are surrogate pairs
                int code = ch;
                // filter Hangul(AC00-D7AF), CJK, Latin, punctuation; flag pictographs/arrows/misc symbols
                if (code >= 0x2190 && code <= 0x2BFF) return true; // arrows/misc symbols
                if (code >= 0x1F300) return true; // emojis and symbols (approx)
            }
            return false;
        }
    }
}

