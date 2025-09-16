using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Routines;

namespace BilliardMasterAi.UI
{
    public class SelectableTemplateItem : MonoBehaviour
    {
        public Toggle toggle;
        public RoutineTemplateItem display;
        [HideInInspector] public string routineId;

        private System.Action<string, bool> _onToggle;

        public void Bind(TrainingRoutine r, System.Action<string, bool> onToggle)
        {
            routineId = r.id;
            _onToggle = onToggle;
            if (display != null) display.Bind(r);
            if (toggle != null)
            {
                toggle.isOn = false;
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener(OnToggled);
            }
        }

        private void OnToggled(bool isOn)
        {
            _onToggle?.Invoke(routineId, isOn);
        }
    }
}

