using UnityEngine;
using UnityEngine.UI;
using System.Text;

namespace BilliardMasterAi.UI
{
    public class EventTimelineUI : MonoBehaviour
    {
        public Text text;

        public void Show(BilliardMasterAi.Replay.ShotEventLogger log)
        {
            if (text == null || log == null) return;
            var sb = new StringBuilder();
            foreach (var ev in log.Events)
            {
                sb.AppendFormat("{0:0.00}s  {1}  {2}\n", ev.Time, ev.Type, ev.Info);
            }
            text.text = sb.ToString();
        }
    }
}

