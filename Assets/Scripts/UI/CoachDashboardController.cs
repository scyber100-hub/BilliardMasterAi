using UnityEngine;
using BilliardMasterAi.Coach;
using BilliardMasterAi.Routines;

namespace BilliardMasterAi.UI
{
    public class CoachDashboardController : MonoBehaviour
    {
        [Header("Student List")]
        public Transform studentContent;      // parent for student items
        public GameObject studentItemPrefab;  // prefab with StudentListItem

        [Header("Today Templates")]
        public Transform templateContent;      // parent for template items
        public GameObject templateItemPrefab;  // prefab with RoutineTemplateItem

        [Header("Options")]
        public string studentsResource = "coach_students";
        public string templatesResource = "routine_templates";

        public void Refresh()
        {
            PopulateStudents();
            PopulateTemplates();
        }

        void Start()
        {
            Refresh();
        }

        private void PopulateStudents()
        {
            if (studentContent == null || studentItemPrefab == null) return;
            foreach (Transform child in studentContent) Destroy(child.gameObject);
            var data = CatalogLoaders.LoadStudents(studentsResource);
            foreach (var s in data.items)
            {
                var go = Instantiate(studentItemPrefab, studentContent);
                var item = go.GetComponent<StudentListItem>();
                if (item != null) item.Bind(s);
            }
        }

        private void PopulateTemplates()
        {
            if (templateContent == null || templateItemPrefab == null) return;
            foreach (Transform child in templateContent) Destroy(child.gameObject);
            var list = CatalogLoaders.LoadTemplates(templatesResource);
            foreach (var r in list.items)
            {
                var go = Instantiate(templateItemPrefab, templateContent);
                var item = go.GetComponent<RoutineTemplateItem>();
                if (item != null) item.Bind(r);
            }
        }
    }
}

