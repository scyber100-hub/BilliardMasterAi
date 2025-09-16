using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Coach;
using BilliardMasterAi.Routines;

namespace BilliardMasterAi.UI
{
    public class PatternAssignScreenController : MonoBehaviour
    {
        [Header("UI")]
        public Dropdown studentDropdown;
        public Transform templatesContent;
        public GameObject selectableTemplateItemPrefab; // contains SelectableTemplateItem
        public Text counterText;
        public Button assignButton;
        public Text statusText;

        [Header("Resources")]
        public string studentsResource = "coach_students";
        public string templatesResource = "routine_templates";
        public int maxSelect = 5;

        private StudentList _students;
        private TrainingRoutineList _templates;
        private readonly HashSet<string> _selected = new();

        void Start()
        {
            LoadCatalogs();
            PopulateStudents();
            PopulateTemplates();
            UpdateCounter();
            if (assignButton != null) assignButton.onClick.AddListener(Assign);
        }

        private void LoadCatalogs()
        {
            _students = CatalogLoaders.LoadStudents(studentsResource);
            _templates = CatalogLoaders.LoadTemplates(templatesResource);
        }

        private void PopulateStudents()
        {
            if (studentDropdown == null) return;
            studentDropdown.ClearOptions();
            var options = new List<Dropdown.OptionData>();
            foreach (var s in _students.items)
                options.Add(new Dropdown.OptionData($"{s.name} ({s.level})"));
            studentDropdown.AddOptions(options);
        }

        private void PopulateTemplates()
        {
            if (templatesContent == null || selectableTemplateItemPrefab == null) return;
            foreach (Transform child in templatesContent) Destroy(child.gameObject);

            foreach (var r in _templates.items)
            {
                var go = Instantiate(selectableTemplateItemPrefab, templatesContent);
                var item = go.GetComponent<SelectableTemplateItem>();
                if (item != null) item.Bind(r, OnToggleTemplate);
            }
        }

        private void OnToggleTemplate(string id, bool on)
        {
            if (on)
            {
                if (_selected.Count >= maxSelect)
                {
                    if (statusText) statusText.text = $"최대 {maxSelect}개까지 선택 가능합니다.";
                    // find the toggle and revert
                    RevertToggle(id);
                    return;
                }
                _selected.Add(id);
            }
            else
            {
                _selected.Remove(id);
            }
            UpdateCounter();
        }

        private void RevertToggle(string id)
        {
            // Iterate children to find the item and turn off visually
            foreach (Transform child in templatesContent)
            {
                var item = child.GetComponent<SelectableTemplateItem>();
                if (item != null && item.routineId == id && item.toggle != null)
                {
                    item.toggle.isOn = false;
                    break;
                }
            }
        }

        private void UpdateCounter()
        {
            if (counterText) counterText.text = $"선택: {_selected.Count}/{maxSelect}";
        }

        private string CurrentStudentId()
        {
            int idx = (studentDropdown != null) ? studentDropdown.value : 0;
            if (_students.items.Length == 0) return string.Empty;
            idx = Mathf.Clamp(idx, 0, _students.items.Length - 1);
            return _students.items[idx].id;
        }

        public void Assign()
        {
            if (_selected.Count != maxSelect)
            {
                if (statusText) statusText.text = $"정확히 {maxSelect}개를 선택하세요.";
                return;
            }
            var studentId = CurrentStudentId();
            if (string.IsNullOrEmpty(studentId))
            {
                if (statusText) statusText.text = "학생을 선택하세요.";
                return;
            }
            var ids = new List<string>(_selected).ToArray();
            AssignmentStore.AssignCurrent(studentId, ids);
            if (statusText) statusText.text = $"배포 완료: {studentId}에게 {ids.Length}개 패턴 할당";
        }
    }
}

