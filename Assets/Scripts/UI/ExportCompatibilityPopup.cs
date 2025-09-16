using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace BilliardMasterAi.UI
{
    // 프리미어/캡컷 호환 안내 팝업 및 바로가기
    public class ExportCompatibilityPopup : MonoBehaviour
    {
        public string exportDir;
        public Button sendPremiereButton;
        public Button sendCapcutButton;
        public Text statusText;

        void Awake()
        {
            if (sendPremiereButton) sendPremiereButton.onClick.AddListener(()=>CreateGuideAndOpen("premiere"));
            if (sendCapcutButton) sendCapcutButton.onClick.AddListener(()=>CreateGuideAndOpen("capcut"));
        }

        public void SetExportDir(string dir){ exportDir = dir; }

        private void CreateGuideAndOpen(string target)
        {
            if (string.IsNullOrEmpty(exportDir)) exportDir = Path.Combine(Application.persistentDataPath, "OverlayExport");
            Directory.CreateDirectory(exportDir);

            string guide = target == "premiere" ? BuildPremiereGuide() : BuildCapcutGuide();
            string file = Path.Combine(exportDir, target + "_import_guide.txt");
            File.WriteAllText(file, guide);
            Application.OpenURL(exportDir);
            SetStatus($"{target} 가이드 생성 및 폴더 열기");
        }

        private string BuildPremiereGuide()
        {
            return "Premiere Pro 가져오기 가이드\n"+
                   "1) PNG 시퀀스 파일(overlay_0000.png…)을 프로젝트로 가져옵니다.\n"+
                   "2) 가져오기 옵션에서 '이미지 시퀀스(Image Sequence)'를 체크합니다.\n"+
                   "3) 알파 채널을 유지한 상태로 타임라인의 영상 트랙 위에 얹습니다.\n"+
                   "4) 프레임 레이트가 영상과 동일한지 확인하세요(예: 30fps).\n"+
                   "5) 필요 시 혼합 모드/불투명도를 조정합니다.\n";
        }

        private string BuildCapcutGuide()
        {
            return "CapCut 가져오기 가이드\n"+
                   "1) PNG 시퀀스(overlay_0000.png…)를 이미지 시퀀스로 가져옵니다.\n"+
                   "2) 클립을 영상 상단 트랙에 배치합니다.\n"+
                   "3) 지속 시간을 시퀀스 길이에 맞춥니다(설정 fps 반영).\n"+
                   "4) 필요 시 투명도/블렌딩을 조정합니다.\n";
        }

        private void SetStatus(string msg){ if (statusText) statusText.text = msg; Debug.Log(msg);}        
    }
}

