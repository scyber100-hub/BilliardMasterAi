using System;
using System.Linq;
using UnityEngine;

namespace BilliardMasterAi.Routines
{
    public static class RoutineRecommender
    {
        private const string ResourcePath = "routines"; // Resources/routines.json

        public static TrainingRoutine GetTodaysRoutine()
        {
            var list = LoadRoutineList();
            if (list == null || list.items == null || list.items.Length == 0)
            {
                list = DefaultList();
            }

            var dateSeed = (int)(DateTime.UtcNow.Date - new DateTime(2000,1,1)).TotalDays;
            int idx = Mathf.Abs(dateSeed) % list.items.Length;
            return list.items[idx];
        }

        private static TrainingRoutineList LoadRoutineList()
        {
            try
            {
                var ta = Resources.Load<TextAsset>(ResourcePath);
                if (ta == null || string.IsNullOrWhiteSpace(ta.text)) return null;
                // JsonUtility requires a wrapper object
                return JsonUtility.FromJson<TrainingRoutineList>(ta.text);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"RoutineRecommender: failed to load routines.json: {e.Message}");
                return null;
            }
        }

        private static TrainingRoutineList DefaultList()
        {
            return new TrainingRoutineList
            {
                items = new[]
                {
                    new TrainingRoutine
                    {
                        id = "angles_basics",
                        title = "오늘의 루틴: 각도 기본",
                        subtitle = "단단 쿠션 3쿠션 진입각 감각 만들기",
                        tags = new []{"각도","3쿠션","입사·반사"},
                        durationMin = 20,
                        difficulty = "Easy",
                        focus = "각도 감각, 라인 읽기",
                        drills = new []{"단장단 기본 10회","반대단 진입각 10회"},
                        imageResource = null
                    },
                    new TrainingRoutine
                    {
                        id = "speed_control",
                        title = "오늘의 루틴: 스피드 컨트롤",
                        subtitle = "두께 일정, 속도로 거리 조절",
                        tags = new []{"속도","두께","컨트롤"},
                        durationMin = 25,
                        difficulty = "Normal",
                        focus = "속도 감각, 거리 예측",
                        drills = new []{"1쿠션 길이 3단계 반복","장쿠션 세이프티 10회"},
                        imageResource = null
                    },
                    new TrainingRoutine
                    {
                        id = "spin_throw",
                        title = "오늘의 루틴: 스핀과 스로우",
                        subtitle = "좌/우 회전이 라인에 미치는 영향",
                        tags = new []{"스핀","스로우","라인"},
                        durationMin = 30,
                        difficulty = "Hard",
                        focus = "스핀-라인 상관, 쿠션 반사",
                        drills = new []{"좌/우 스핀 각 10회","쿠션 전/후 라인 비교"},
                        imageResource = null
                    }
                }
            };
        }
    }
}

