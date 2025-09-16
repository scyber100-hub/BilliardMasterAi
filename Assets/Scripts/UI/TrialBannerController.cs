using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using BilliardMasterAi.Game;

namespace BilliardMasterAi.UI
{
    // 신규 방문자에게 무료 AI 가이드 체험을 제안하는 배너
    public class TrialBannerController : MonoBehaviour
    {
        [Header("UI")]
        public CanvasGroup group;
        public Text titleText;
        public Text bodyText;
        public Button startButton;
        public Button laterButton;

        [Header("Config")]
        public int trialMinutes = 10;
        public bool showOnlyIfNew = true;

        [Header("Events")]
        public UnityEvent onTrialStarted; // 예: AR 가이드 화면으로 전환 등

        private const string KeyDismissedThisSession = "bm_trial_banner_dismissed_session";

        void Awake()
        {
            TrialManager.MarkFirstSeen();
            if (startButton) startButton.onClick.AddListener(StartTrial);
            if (laterButton) laterButton.onClick.AddListener(DismissThisSession);
        }

        void Start()
        {
            ShowIfEligible();
        }

        public void ShowIfEligible()
        {
            if (PlayerPrefs.GetInt(KeyDismissedThisSession, 0) == 1) { Hide(); return; }
            if (TrialManager.IsTrialActive()) { Hide(); return; }
            if (TrialManager.HasUsedTrial())
            {
                if (showOnlyIfNew && !TrialManager.IsNewVisitor()) { Hide(); return; }
                // 이미 사용한 경우에는 숨김
                Hide();
                return;
            }
            // Eligible: 신규 방문자이거나 아직 체험 미사용
            SetTexts();
            Show();
        }

        private void SetTexts()
        {
            if (titleText) titleText.text = "무료 AI 가이드 체험";
            if (bodyText) bodyText.text = $"신규 방문자를 위한 {trialMinutes}분 무료 체험을 시작해 보세요.";
        }

        private void Show()
        {
            if (group)
            {
                group.gameObject.SetActive(true);
                group.alpha = 1f; group.interactable = true; group.blocksRaycasts = true;
            }
            else gameObject.SetActive(true);
        }

        private void Hide()
        {
            if (group)
            {
                group.alpha = 0f; group.interactable = false; group.blocksRaycasts = false;
                group.gameObject.SetActive(false);
            }
            else gameObject.SetActive(false);
        }

        public void StartTrial()
        {
            TrialManager.StartTrialSeconds(trialMinutes * 60);
            Hide();
            onTrialStarted?.Invoke();
        }

        public void DismissThisSession()
        {
            PlayerPrefs.SetInt(KeyDismissedThisSession, 1);
            PlayerPrefs.Save();
            Hide();
        }
    }
}

