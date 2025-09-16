using System;

namespace BilliardMasterAi.Coach
{
    [Serializable]
    public class Student
    {
        public string id;
        public string name;
        public string level; // Beginner/Intermediate/Advanced or custom
        public string note;  // short memo
    }

    [Serializable]
    public class StudentList
    {
        public Student[] items;
    }
}

