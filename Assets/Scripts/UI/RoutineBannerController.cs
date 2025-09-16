using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Routines;

namespace BilliardMasterAi.UI
{
    public class RoutineBannerController : MonoBehaviour
    {
        [Header("UI Bindings")]
        public Text titleText;
        public Text subtitleText;
        public Text metaText; // e.g., tags + duration
        public Image bannerImage;
        public Button startButton;

        private TrainingRoutine _routine;

        void Awake()
        {
            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);
        }

        public void ShowRoutine(TrainingRoutine routine)
        {
            _routine = routine;
            if (titleText) titleText.text = routine.title;
            if (subtitleText) subtitleText.text = routine.subtitle;
            if (metaText) metaText.text = ComposeMeta(routine);

            if (bannerImage)
            {
                bannerImage.enabled = false;
                if (!string.IsNullOrEmpty(routine.imageResource))
                {
                    var sprite = Resources.Load<Sprite>(routine.imageResource);
                    if (sprite != null)
                    {
                        bannerImage.sprite = sprite;
                        bannerImage.enabled = true;
                    }
                }
            }
        }

        private string ComposeMeta(TrainingRoutine r)
        {
            string tags = (r.tags != null && r.tags.Length > 0) ? string.Join(" · ", r.tags) : r.focus;
            return $"{tags}  •  {r.durationMin}분  •  {r.difficulty}";
        }

        private void OnStartClicked()
        {
            // TODO: navigate to routine detail / practice scene.
            Debug.Log($"Start routine: {_routine?.id}");
        }
    }
}

