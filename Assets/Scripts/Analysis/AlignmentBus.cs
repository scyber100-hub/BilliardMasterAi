using UnityEngine;

namespace BilliardMasterAi.Analysis
{
    // Simple singleton bus to share last alignment across components (e.g., ReplayCompareScreenController → VideoReplaySync).
    public static class AlignmentBus
    {
        private static AlignmentResult? _current;

        public static void Publish(AlignmentResult ar)
        {
            _current = ar;
        }

        public static bool TryGet(out AlignmentResult ar)
        {
            if (_current.HasValue) { ar = _current.Value; return true; }
            ar = default; return false;
        }
    }
}

