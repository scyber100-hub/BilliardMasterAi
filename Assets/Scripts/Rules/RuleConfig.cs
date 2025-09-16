using UnityEngine;

namespace BilliardMasterAi.Rules
{
    [CreateAssetMenu(menuName = "BilliardMasterAi/Rule Config", fileName = "RuleConfig")]
    public class RuleConfig : ScriptableObject
    {
        public enum GameType { Carom3Cushion, Carom4Ball }
        public GameType gameType = GameType.Carom3Cushion;
        public int requiredCushions = 3; // 3-cushion
        public float contactTolerance = Physics.CaromConstants.BallRadius * 2.05f;
        public bool foulIfEarlySecondContact = true;
        [Header("Advanced Foul/Kiss")]
        public float kissDtThreshold = 0.1f; // seconds
        public float minImpactSpeed = 0.2f;  // m/s minimum cue speed at object contact
        public float minIncidenceAngleCos = 0.05f; // allow shallow contacts (> ~87°)
    }
}
