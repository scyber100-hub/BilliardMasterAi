using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Game;

namespace BilliardMasterAi.UI
{
    public class ModeSelectionScreenController : MonoBehaviour
    {
        [Header("UI")]
        public Toggle leagueModeToggle;
        public InputField timeLimitMinutesInput; // e.g., "3" for 3 minutes
        public Button applyButton;
        public Text statusText;

        void Awake()
        {
            if (applyButton != null) applyButton.onClick.AddListener(ApplySettings);
        }

        public void ApplySettings()
        {
            bool league = leagueModeToggle != null && leagueModeToggle.isOn;
            int minutes = 0;
            if (timeLimitMinutesInput != null && !string.IsNullOrEmpty(timeLimitMinutesInput.text))
            {
                int.TryParse(timeLimitMinutesInput.text, out minutes);
            }
            int limitSec = Mathf.Max(0, minutes * 60);

            GameState.SetMode(league ? GameMode.League : GameMode.Practice, limitSec);
            GameState.ResetTimer();
            if (league && limitSec > 0) GameState.StartTimer();

            if (statusText)
            {
                if (league)
                {
                    statusText.text = limitSec > 0 ? $"리그모드 ON · 제한 {minutes}분" : "리그모드 ON · 제한 시간 없음";
                }
                else
                {
                    statusText.text = "연습모드";
                }
            }
        }
    }
}

