using UnityEngine;
using UnityEngine.UI;
using BilliardMasterAi.Calibration;
using BilliardMasterAi.Physics;
using BilliardMasterAi.Replay;

namespace BilliardMasterAi.UI
{
    public class CalibrationPanelController : MonoBehaviour
    {
        [Header("Params")]
        public Slider muK;
        public Slider muR;
        public Slider muContact;
        public Slider muCushion;
        public Slider eBall;
        public Slider eCushion;

        [Header("References")]
        public BallTrajectoryRecorder recorder;
        public Transform tableRoot;
        public Transform cueBall;
        public Transform targetBall;
        public Transform otherBall;
        public InputField angleInput; // deg
        public InputField speedInput; // m/s
        public InputField spinInput;  // rad/s
        public Text statusText;

        [Header("Profile Management")]
        public Dropdown profileDropdown;
        public InputField newProfileNameInput;
        public Button saveProfileButton;
        public Button loadProfileButton;
        public Button deleteProfileButton;
        public Button exportProfileButton;
        public Button importProfileButton;
        public InputField exportNameInput;
        public InputField importJsonInput;
        public Button exportBrowseButton;
        public Button importBrowseButton;
        public Button copyJsonButton;
        public Button pasteJsonButton;

        [Header("Fitter Options")]
        public InputField iterationsInput; // default 200
        public Toggle fitFrictionToggle;   // μk, μr
        public Toggle fitContactToggle;    // μcontact, μcushion
        public Toggle fitRestitutionToggle;// e_ball, e_cushion
        public Toggle useCmaEsToggle;      // choose CMA-ES

        void Start()
        {
            RefreshSlidersFromCurrent();
            RefreshProfileList();
            if (saveProfileButton) saveProfileButton.onClick.AddListener(SaveProfile);
            if (loadProfileButton) loadProfileButton.onClick.AddListener(LoadSelectedProfile);
            if (deleteProfileButton) deleteProfileButton.onClick.AddListener(DeleteSelectedProfile);
            if (exportProfileButton) exportProfileButton.onClick.AddListener(ExportSelectedProfile);
            if (importProfileButton) importProfileButton.onClick.AddListener(ImportProfileFromJson);
            if (exportBrowseButton) exportBrowseButton.onClick.AddListener(ExportBrowse);
            if (importBrowseButton) importBrowseButton.onClick.AddListener(ImportBrowse);
            if (copyJsonButton) copyJsonButton.onClick.AddListener(CopyExportJson);
            if (pasteJsonButton) pasteJsonButton.onClick.AddListener(PasteImportJson);
        }

        public void RefreshSlidersFromCurrent()
        {
            if (muK) muK.value = AdvancedParams.MuK;
            if (muR) muR.value = AdvancedParams.MuR;
            if (muContact) muContact.value = AdvancedParams.MuContact;
            if (muCushion) muCushion.value = AdvancedParams.MuCushion;
            if (eBall) eBall.value = AdvancedParams.RestitutionBall;
            if (eCushion) eCushion.value = AdvancedParams.RestitutionCushionBase;
        }

        public void ApplySliders()
        {
            AdvancedParams.MuK = muK ? muK.value : AdvancedParams.MuK;
            AdvancedParams.MuR = muR ? muR.value : AdvancedParams.MuR;
            AdvancedParams.MuContact = muContact ? muContact.value : AdvancedParams.MuContact;
            AdvancedParams.MuCushion = muCushion ? muCushion.value : AdvancedParams.MuCushion;
            AdvancedParams.RestitutionBall = eBall ? eBall.value : AdvancedParams.RestitutionBall;
            AdvancedParams.RestitutionCushionBase = eCushion ? eCushion.value : AdvancedParams.RestitutionCushionBase;
            SetStatus("Parameters applied.");
        }

        public void AutoFit()
        {
            if (recorder == null || cueBall == null || targetBall == null || otherBall == null) { SetStatus("Missing refs"); return; }
            var actual = recorder.StopRecording();
            Vector2 cue = ToLocal2D(cueBall.position);
            Vector2 obj1 = ToLocal2D(targetBall.position);
            Vector2 obj2 = ToLocal2D(otherBall.position);
            float angle = Parse(angleInput?.text, 0f);
            float speed = Parse(speedInput?.text, 2.5f);
            float spin = Parse(spinInput?.text, 0f);
            int iters = (int)Parse(iterationsInput?.text, 200f);
            // lock parameters by temporarily keeping current values and disabling perturbation via toggles is a larger change;
            // here we run the fitter normally, then re-apply locked groups from sliders to respect user intent.
            CalibrationFitter.FitResult res = useCmaEsToggle && useCmaEsToggle.isOn
                ? CalibrationFitter.FitCmaEs(cue, obj1, obj2, angle, speed, spin, actual, iters)
                : CalibrationFitter.Fit(cue, obj1, obj2, angle, speed, spin, actual, iters);
            // Re-apply locked groups
            if (fitFrictionToggle && !fitFrictionToggle.isOn)
            { AdvancedParams.MuK = muK ? muK.value : AdvancedParams.MuK; AdvancedParams.MuR = muR ? muR.value : AdvancedParams.MuR; }
            if (fitContactToggle && !fitContactToggle.isOn)
            { AdvancedParams.MuContact = muContact ? muContact.value : AdvancedParams.MuContact; AdvancedParams.MuCushion = muCushion ? muCushion.value : AdvancedParams.MuCushion; }
            if (fitRestitutionToggle && !fitRestitutionToggle.isOn)
            { AdvancedParams.RestitutionBall = eBall ? eBall.value : AdvancedParams.RestitutionBall; AdvancedParams.RestitutionCushionBase = eCushion ? eCushion.value : AdvancedParams.RestitutionCushionBase; }
            RefreshSlidersFromCurrent();
            SetStatus($"Fit done: RMS {res.rms*100f:0.0} cm / {res.iterations} it");
        }

        private Vector2 ToLocal2D(Vector3 world)
        {
            if (tableRoot == null) return new Vector2(world.x, world.z);
            var local = tableRoot.InverseTransformPoint(world); return new Vector2(local.x, local.z);
        }

        private float Parse(string s, float d){ return float.TryParse(s, out var v) ? v : d; }
        private void SetStatus(string m){ if (statusText) statusText.text = m; Debug.Log(m); }

        public void RefreshProfileList()
        {
            if (profileDropdown == null) return;
            profileDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<Dropdown.OptionData>();
            foreach (var n in CalibrationProfileStore.ListNames()) options.Add(new Dropdown.OptionData(n));
            if (options.Count == 0) options.Add(new Dropdown.OptionData("default"));
            profileDropdown.AddOptions(options);
        }

        public void SaveProfile()
        {
            string name = newProfileNameInput != null ? newProfileNameInput.text : null;
            if (string.IsNullOrEmpty(name)) { SetStatus("프로파일 이름을 입력하세요."); return; }
            var p = ScriptableObject.CreateInstance<CalibrationProfile>();
            p.muK = AdvancedParams.MuK; p.muR = AdvancedParams.MuR; p.muContact = AdvancedParams.MuContact; p.muCushion = AdvancedParams.MuCushion; p.restitutionBall = AdvancedParams.RestitutionBall; p.restitutionCushion = AdvancedParams.RestitutionCushionBase;
            CalibrationProfileStore.Save(name, p);
            RefreshProfileList();
            SetStatus($"프로파일 저장: {name}");
        }

        public void LoadSelectedProfile()
        {
            if (profileDropdown == null) return;
            string name = profileDropdown.options[profileDropdown.value].text;
            CalibrationProfileStore.Apply(name);
            RefreshSlidersFromCurrent();
            SetStatus($"프로파일 적용: {name}");
        }

        public void DeleteSelectedProfile()
        {
            if (profileDropdown == null) return;
            string name = profileDropdown.options[profileDropdown.value].text;
            CalibrationProfileStore.Delete(name);
            RefreshProfileList();
            SetStatus($"프로파일 삭제: {name}");
        }

        public void ExportSelectedProfile()
        {
            if (profileDropdown == null) return;
            string name = profileDropdown.options[profileDropdown.value].text;
            string json = CalibrationProfileStore.ExportJson(name);
            if (string.IsNullOrEmpty(json)) { SetStatus("내보낼 프로파일이 없습니다."); return; }
            string fname = exportNameInput != null && !string.IsNullOrEmpty(exportNameInput.text) ? exportNameInput.text : $"{name}.json";
            var path = System.IO.Path.Combine(Application.persistentDataPath, fname);
            try { System.IO.File.WriteAllText(path, json); SetStatus($"내보내기 완료: {path}"); }
            catch (System.Exception e) { SetStatus($"내보내기 실패: {e.Message}"); }
        }

        public void ImportProfileFromJson()
        {
            string name = newProfileNameInput != null ? newProfileNameInput.text : null;
            if (string.IsNullOrEmpty(name)) { SetStatus("가져올 프로파일 이름을 입력하세요."); return; }
            string json = importJsonInput != null ? importJsonInput.text : null;
            if (string.IsNullOrEmpty(json)) { SetStatus("가져올 JSON을 입력하세요."); return; }
            bool ok = CalibrationProfileStore.ImportJson(name, json);
            if (ok) { RefreshProfileList(); SetStatus($"가져오기 완료: {name}"); }
            else SetStatus("가져오기 실패: JSON 확인");
        }

        public void ExportBrowse()
        {
#if UNITY_EDITOR
            string path = UnityEditor.EditorUtility.SaveFilePanel("Export Calibration JSON", Application.persistentDataPath, "calib.json", "json");
            if (!string.IsNullOrEmpty(path))
            {
                if (profileDropdown == null) return; string name = profileDropdown.options[profileDropdown.value].text;
                string json = CalibrationProfileStore.ExportJson(name);
                try { System.IO.File.WriteAllText(path, json); SetStatus($"내보내기 완료: {path}"); } catch (System.Exception e) { SetStatus($"실패: {e.Message}"); }
            }
#else
            Application.OpenURL(Application.persistentDataPath);
            SetStatus("런타임에 파일 선택은 지원되지 않습니다. 폴더가 열렸습니다.");
#endif
        }

        public void ImportBrowse()
        {
#if UNITY_EDITOR
            string path = UnityEditor.EditorUtility.OpenFilePanel("Import Calibration JSON", Application.persistentDataPath, "json");
            if (!string.IsNullOrEmpty(path))
            {
                try { importJsonInput.text = System.IO.File.ReadAllText(path); SetStatus("JSON 로드 완료. 이름 입력 후 Import"); } catch (System.Exception e) { SetStatus($"실패: {e.Message}"); }
            }
#else
            Application.OpenURL(Application.persistentDataPath);
            SetStatus("런타임에 파일 선택은 지원되지 않습니다. 폴더가 열렸습니다.");
#endif
        }

        public void CopyExportJson()
        {
            if (profileDropdown == null) { SetStatus("프로파일을 선택하세요."); return; }
            string name = profileDropdown.options[profileDropdown.value].text;
            string json = CalibrationProfileStore.ExportJson(name);
            if (string.IsNullOrEmpty(json)) { SetStatus("복사할 JSON이 없습니다."); return; }
            GUIUtility.systemCopyBuffer = json;
            SetStatus("JSON이 클립보드에 복사되었습니다.");
        }

        public void PasteImportJson()
        {
            string json = GUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(json)) { SetStatus("클립보드가 비어 있습니다."); return; }
            if (importJsonInput) importJsonInput.text = json;
            SetStatus("클립보드 JSON을 붙여넣었습니다. 이름 입력 후 Import 하세요.");
        }
    }
}
