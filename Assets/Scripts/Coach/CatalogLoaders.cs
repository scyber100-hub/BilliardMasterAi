using System;
using UnityEngine;
using BilliardMasterAi.Routines;

namespace BilliardMasterAi.Coach
{
    public static class CatalogLoaders
    {
        public static StudentList LoadStudents(string resourcePath = "coach_students")
        {
            try
            {
                var ta = Resources.Load<TextAsset>(resourcePath);
                if (ta == null) return new StudentList { items = Array.Empty<Student>() };
                var data = JsonUtility.FromJson<StudentList>(ta.text);
                return data ?? new StudentList { items = Array.Empty<Student>() };
            }
            catch (Exception)
            {
                return new StudentList { items = Array.Empty<Student>() };
            }
        }

        public static TrainingRoutineList LoadTemplates(string resourcePath = "routine_templates")
        {
            try
            {
                var ta = Resources.Load<TextAsset>(resourcePath);
                if (ta == null) return new TrainingRoutineList { items = Array.Empty<TrainingRoutine>() };
                var data = JsonUtility.FromJson<TrainingRoutineList>(ta.text);
                return data ?? new TrainingRoutineList { items = Array.Empty<TrainingRoutine>() };
            }
            catch (Exception)
            {
                return new TrainingRoutineList { items = Array.Empty<TrainingRoutine>() };
            }
        }
    }
}

