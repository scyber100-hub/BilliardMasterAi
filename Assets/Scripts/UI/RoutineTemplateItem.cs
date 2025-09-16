using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Routines;

namespace BilliardMasterAi.UI
{
    public class RoutineTemplateItem : MonoBehaviour
    {
        public Text titleText;
        public Text subtitleText;
        public Text metaText; // duration/difficulty/tags

        public void Bind(TrainingRoutine r)
        {
            if (titleText) titleText.text = r.title;
            if (subtitleText) subtitleText.text = r.subtitle;
            if (metaText)
            {
                string tags = (r.tags != null && r.tags.Length > 0) ? string.Join(" · ", r.tags) : r.focus;
                metaText.text = $"{tags} • {r.durationMin}분 • {r.difficulty}";
            }
        }
    }
}

