using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Coach;

namespace BilliardMasterAi.UI
{
    public class StudentListItem : MonoBehaviour
    {
        public Text nameText;
        public Text levelText;
        public Text noteText;

        public void Bind(Student s)
        {
            if (nameText) nameText.text = s.name;
            if (levelText) levelText.text = s.level;
            if (noteText) noteText.text = s.note;
        }
    }
}

