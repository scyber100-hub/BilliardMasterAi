using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using BilliardMasterAi.Creator;

namespace BilliardMasterAi.Creator
{
    [Serializable]
    public class ClipSegment
    {
        public double start;
        public double end;
        public string label;
    }

    public class SceneSelectorController : MonoBehaviour
    {
        [Header("Refs")]
        public VideoPlayer player;
        public InputField labelInput;
        public Text inOutText;
        public Button markInButton;
        public Button markOutButton;
        public Button addClipButton;

        [Header("List UI")]
        public Transform listContent;
        public GameObject listItemPrefab; // SceneListItem

        private double _markIn = -1;
        private double _markOut = -1;
        private readonly List<ClipSegment> _clips = new();

        void Awake()
        {
            if (markInButton) markInButton.onClick.AddListener(MarkIn);
            if (markOutButton) markOutButton.onClick.AddListener(MarkOut);
            if (addClipButton) addClipButton.onClick.AddListener(AddClip);
        }

        public void MarkIn()
        {
            if (player == null || !player.isPrepared) return;
            _markIn = Math.Max(0, player.time);
            UpdateInOutText();
        }

        public void MarkOut()
        {
            if (player == null || !player.isPrepared) return;
            _markOut = Math.Max(0, player.time);
            UpdateInOutText();
        }

        public void AddClip()
        {
            if (_markIn < 0 || _markOut < 0) return;
            var a = Math.Min(_markIn, _markOut);
            var b = Math.Max(_markIn, _markOut);
            if (Math.Abs(b - a) < 0.5) return; // min 0.5s
            var clip = new ClipSegment { start = a, end = b, label = labelInput ? labelInput.text : string.Empty };
            _clips.Add(clip);
            AddListItem(clip);
            _markIn = _markOut = -1; UpdateInOutText(); if (labelInput) labelInput.text = string.Empty;
        }

        private void AddListItem(ClipSegment clip)
        {
            if (listContent == null || listItemPrefab == null) return;
            var go = Instantiate(listItemPrefab, listContent);
            var item = go.GetComponent<SceneListItem>();
            if (item != null)
            {
                item.Bind(clip, JumpToTime);
            }
        }

        private void JumpToTime(double t)
        {
            if (player == null || !player.isPrepared) return;
            player.time = Math.Max(0, Math.Min(player.length - 0.01, t));
        }

        private void UpdateInOutText()
        {
            if (inOutText == null) return;
            string i = _markIn >= 0 ? TimeUtil.FormatTime(_markIn) : "--:--";
            string o = _markOut >= 0 ? TimeUtil.FormatTime(_markOut) : "--:--";
            inOutText.text = $"IN {i}  |  OUT {o}";
        }
    }
}

