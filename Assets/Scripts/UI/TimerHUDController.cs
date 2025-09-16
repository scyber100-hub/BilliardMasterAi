using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Game;

namespace BilliardMasterAi.UI
{
    public class TimerHUDController : MonoBehaviour
    {
        public Text timerText;
        public Color normalColor = Color.white;
        public Color warningColor = new Color(1f, 0.5f, 0.2f);
        public Color dangerColor = new Color(1f, 0.2f, 0.2f);

        public float warningThreshold = 30f; // seconds
        public float dangerThreshold = 10f;  // seconds

        void Update()
        {
            GameState.Tick(Time.deltaTime);
            UpdateText();
        }

        private void UpdateText()
        {
            if (timerText == null)
                return;

            if (GameState.Mode != GameMode.League || GameState.TimeLimitSec <= 0)
            {
                timerText.text = "";
                return;
            }

            int sec = Mathf.CeilToInt(GameState.RemainingSec);
            int m = Mathf.Max(0, sec / 60);
            int s = Mathf.Max(0, sec % 60);
            timerText.text = $"{m:00}:{s:00}";

            if (GameState.RemainingSec <= dangerThreshold)
                timerText.color = dangerColor;
            else if (GameState.RemainingSec <= warningThreshold)
                timerText.color = warningColor;
            else
                timerText.color = normalColor;
        }

        public void PauseTimer() => GameState.PauseTimer();
        public void StartTimer() => GameState.StartTimer();
        public void ResetTimer() => GameState.ResetTimer();
    }
}

