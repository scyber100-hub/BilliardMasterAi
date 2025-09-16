using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Share;
using BilliardMasterAi.Utils;

namespace BilliardMasterAi.UI
{
    // 완료 영상 공유 화면: SNS 업로드 프리셋 + 워터마크 옵션
    public class CompletedVideoShareScreenController : MonoBehaviour
    {
        [Header("Export Controller")]
        public OverlayExportController exportController;

        [Header("Preset UI")]
        public Dropdown presetDropdown;
        public InputField durationInput;

        [Header("Watermark UI")]
        public WatermarkController watermark;
        public Toggle watermarkToggle;
        public InputField watermarkTextInput;
        public Dropdown watermarkPositionDropdown; // 0 TL, 1 TR, 2 BL, 3 BR

        [Header("Share UI")]
        public InputField shareFilePathInput; // optional manual path
        public Button applyPresetButton;
        public Button exportSequenceButton;
        public Button exportSingleButton;
        public Button shareButton;
        public Text statusText;

        private readonly List<SharePreset> _presets = SharePresets.Presets;

        void Start()
        {
            PopulatePresets();
            if (applyPresetButton) applyPresetButton.onClick.AddListener(ApplyPreset);
            if (exportSequenceButton) exportSequenceButton.onClick.AddListener(()=>exportController?.ExportPngSequence());
            if (exportSingleButton) exportSingleButton.onClick.AddListener(()=>exportController?.ExportSinglePng());
            if (shareButton) shareButton.onClick.AddListener(ShareNow);
            if (watermarkToggle) watermarkToggle.onValueChanged.AddListener(OnWatermarkToggle);
            if (watermarkPositionDropdown) watermarkPositionDropdown.onValueChanged.AddListener(OnWatermarkPositionChanged);
        }

        private void PopulatePresets()
        {
            if (presetDropdown == null) return;
            var options = new List<Dropdown.OptionData>();
            foreach (var p in _presets) options.Add(new Dropdown.OptionData(p.name));
            presetDropdown.ClearOptions(); presetDropdown.AddOptions(options);
            presetDropdown.value = 0; presetDropdown.RefreshShownValue();
            ApplyPreset();
        }

        public void ApplyPreset()
        {
            if (exportController == null || presetDropdown == null) return;
            var p = _presets[Mathf.Clamp(presetDropdown.value, 0, _presets.Count-1)];
            if (exportController.widthInput) exportController.widthInput.text = p.width.ToString();
            if (exportController.heightInput) exportController.heightInput.text = p.height.ToString();
            if (exportController.fpsInput) exportController.fpsInput.text = p.fps.ToString();
            if (durationInput) durationInput.text = p.defaultDuration.ToString("0");
            if (exportController.durationInput) exportController.durationInput.text = durationInput.text;
            SetStatus($"프리셋 적용: {p.name}");
        }

        private void OnWatermarkToggle(bool on)
        {
            if (watermark != null) watermark.SetVisible(on);
            if (on) UpdateWatermark();
        }

        private void OnWatermarkPositionChanged(int idx)
        {
            UpdateWatermark();
        }

        public void UpdateWatermark()
        {
            if (watermark == null) return;
            if (watermarkTextInput) watermark.SetText(watermarkTextInput.text);
            var pos = WatermarkPosition.TopLeft;
            switch (Mathf.Clamp(watermarkPositionDropdown != null ? watermarkPositionDropdown.value : 0, 0, 3))
            {
                case 0: pos = WatermarkPosition.TopLeft; break;
                case 1: pos = WatermarkPosition.TopRight; break;
                case 2: pos = WatermarkPosition.BottomLeft; break;
                case 3: pos = WatermarkPosition.BottomRight; break;
            }
            watermark.SetPosition(pos, new Vector2(24,24));
        }

        public void ShareNow()
        {
            string path = shareFilePathInput != null ? shareFilePathInput.text : string.Empty;
            if (string.IsNullOrEmpty(path))
            {
                // Default to exported folder (open folder)
                SetStatus("공유할 파일 경로를 입력하거나, 내보내기 후 파일을 선택하세요.");
            }
            else
            {
                ShareUtility.ShareFile(path);
                SetStatus($"공유 시도: {path}");
            }
        }

        private void SetStatus(string msg){ if (statusText) statusText.text = msg; Debug.Log(msg);}        
    }
}

