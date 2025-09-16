using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BilliardMasterAi.EditorTools
{
    public static class SetupWizard
    {
        [MenuItem("BilliardMasterAi/Setup/Create Sample Scenes")] 
        public static void CreateSampleScenes()
        {
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            CreateHomeScene();
            CreateCameraRecognitionScene();
            CreateRecommendationScene();
            CreateReplayScene();
            CreateReplayCompareScene();
            CreateCalibrationScene();
            CreateVideoSyncScene();
            CreateDashboardTrendsScene();
            CreateSensitivityScene();
            CreateReplayEditorScene();
            CreateSuccessModelTrainerScene();
            CreateSuccessModelReportScene();
            CreateCoachDashboardScene();
            CreatePatternAssignScene();
            CreateStudentTrainingScene();
            CreateRealTimeDashboardScene();
            CreatePerformanceReportScene();
            CreateVideoImportScene();
            CreateVideoBallRecognitionScene();
            CreateReplayOverlayScene();
            CreateExportScene();
            CreateShareScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("BilliardMasterAi", "샘플 씬 생성 완료 (Assets/Scenes)", "OK");
        }

        [MenuItem("BilliardMasterAi/Open/Calibration Scene")]
        public static void OpenCalibrationScene()
        {
            var path = "Assets/Scenes/Calibration.unity";
            if (!System.IO.File.Exists(path))
            {
                if (EditorUtility.DisplayDialog("Calibration Scene", "Calibration scene not found. Create sample scenes now?", "Create", "Cancel"))
                {
                    CreateSampleScenes();
                }
            }
            else
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
            }
        }

        private static GameObject EnsureCanvas(string name, out Canvas canvas)
        {
            var go = new GameObject(name);
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 anchored, System.Action onClickAssign = null)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>(); rt.sizeDelta = new Vector2(220, 60); rt.anchoredPosition = anchored;
            var img = go.GetComponent<Image>(); img.color = new Color(0.2f,0.5f,0.9f,0.8f);
            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            txtGo.transform.SetParent(go.transform, false);
            var trt = txtGo.GetComponent<RectTransform>(); trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
            var txt = txtGo.GetComponent<Text>(); txt.text = label; txt.alignment = TextAnchor.MiddleCenter; txt.color = Color.white; txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            var btn = go.GetComponent<Button>();
            return btn;
        }

        private static Text CreateText(Transform parent, string content, Vector2 anchored)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>(); rt.sizeDelta = new Vector2(800, 60); rt.anchoredPosition = anchored;
            var txt = go.GetComponent<Text>(); txt.text = content; txt.alignment = TextAnchor.MiddleCenter; txt.color = Color.white; txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); txt.fontSize = 28;
            return txt;
        }

        private static Slider CreateSlider(Transform parent, string label, Vector2 anchored, float min, float max, float value)
        {
            var root = new GameObject(label, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rt = root.GetComponent<RectTransform>(); rt.sizeDelta = new Vector2(500, 40); rt.anchoredPosition = anchored;
            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(root.transform, false);
            var lrt = labelGo.GetComponent<RectTransform>(); lrt.anchorMin = new Vector2(0, 0); lrt.anchorMax = new Vector2(0, 1); lrt.sizeDelta = new Vector2(180, 40); lrt.anchoredPosition = new Vector2(90, 0);
            var ltxt = labelGo.GetComponent<Text>(); ltxt.text = label; ltxt.alignment = TextAnchor.MiddleRight; ltxt.color = Color.white; ltxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            var sliderGo = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            sliderGo.transform.SetParent(root.transform, false);
            var srt = sliderGo.GetComponent<RectTransform>(); srt.anchorMin = new Vector2(0, 0); srt.anchorMax = new Vector2(1, 1); srt.offsetMin = new Vector2(200, 8); srt.offsetMax = new Vector2(-20, -8);
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image)); bg.transform.SetParent(sliderGo.transform, false);
            var fillArea = new GameObject("Fill Area", typeof(RectTransform)); fillArea.transform.SetParent(sliderGo.transform, false);
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image)); fill.transform.SetParent(fillArea.transform, false);
            var handleSlideArea = new GameObject("Handle Slide Area", typeof(RectTransform)); handleSlideArea.transform.SetParent(sliderGo.transform, false);
            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image)); handle.transform.SetParent(handleSlideArea.transform, false);
            var imgBg = bg.GetComponent<Image>(); imgBg.color = new Color(1,1,1,0.2f);
            var imgFill = fill.GetComponent<Image>(); imgFill.color = new Color(0.2f,0.8f,0.9f,1);
            var imgHandle = handle.GetComponent<Image>(); imgHandle.color = Color.white;
            var s = sliderGo.GetComponent<Slider>();
            var bgRt = bg.GetComponent<RectTransform>(); bgRt.anchorMin = new Vector2(0,0.25f); bgRt.anchorMax = new Vector2(1,0.75f); bgRt.offsetMin = new Vector2(0,0); bgRt.offsetMax = new Vector2(0,0);
            var faRt = fillArea.GetComponent<RectTransform>(); faRt.anchorMin = new Vector2(0,0.25f); faRt.anchorMax = new Vector2(1,0.75f); faRt.offsetMin = new Vector2(10,0); faRt.offsetMax = new Vector2(-10,0);
            var fRt = fill.GetComponent<RectTransform>(); fRt.anchorMin = new Vector2(0,0); fRt.anchorMax = new Vector2(1,1);
            var hsaRt = handleSlideArea.GetComponent<RectTransform>(); hsaRt.anchorMin = new Vector2(0,0); hsaRt.anchorMax = new Vector2(1,1); hsaRt.offsetMin = new Vector2(10,0); hsaRt.offsetMax = new Vector2(-10,0);
            var hRt = handle.GetComponent<RectTransform>(); hRt.sizeDelta = new Vector2(20,20);
            s.fillRect = fRt; s.handleRect = hRt; s.targetGraphic = imgHandle; s.direction = Slider.Direction.LeftToRight; s.minValue = min; s.maxValue = max; s.value = value;
            return s;
        }

        private static InputField CreateInput(Transform parent, string placeholder, Vector2 anchored, string text = "")
        {
            var go = new GameObject("InputField", typeof(RectTransform), typeof(Image), typeof(InputField));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>(); rt.sizeDelta = new Vector2(220, 40); rt.anchoredPosition = anchored;
            var img = go.GetComponent<Image>(); img.color = new Color(1,1,1,0.1f);
            var phGo = new GameObject("Placeholder", typeof(RectTransform), typeof(Text)); phGo.transform.SetParent(go.transform, false);
            var txGo = new GameObject("Text", typeof(RectTransform), typeof(Text)); txGo.transform.SetParent(go.transform, false);
            var ph = phGo.GetComponent<Text>(); ph.text = placeholder; ph.color = new Color(1,1,1,0.5f); ph.alignment = TextAnchor.MiddleLeft; ph.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            var tx = txGo.GetComponent<Text>(); tx.text = text; tx.color = Color.white; tx.alignment = TextAnchor.MiddleLeft; tx.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            var ifc = go.GetComponent<InputField>(); ifc.placeholder = ph; ifc.textComponent = tx; ifc.text = text;
            var phr = phGo.GetComponent<RectTransform>(); phr.anchorMin = Vector2.zero; phr.anchorMax = Vector2.one; phr.offsetMin = new Vector2(10,0); phr.offsetMax = new Vector2(-10,0);
            var txr = txGo.GetComponent<RectTransform>(); txr.anchorMin = Vector2.zero; txr.anchorMax = Vector2.one; txr.offsetMin = new Vector2(10,0); txr.offsetMax = new Vector2(-10,0);
            return ifc;
        }

        private static Toggle CreateToggle(Transform parent, string label, Vector2 anchored, bool isOn)
        {
            var go = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle)); go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>(); rt.sizeDelta = new Vector2(240, 40); rt.anchoredPosition = anchored;
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image)); bg.transform.SetParent(go.transform, false);
            var check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image)); check.transform.SetParent(bg.transform, false);
            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text)); labelGo.transform.SetParent(go.transform, false);
            var bgImg = bg.GetComponent<Image>(); bgImg.color = new Color(1,1,1,0.2f);
            var ckImg = check.GetComponent<Image>(); ckImg.color = new Color(0.2f,0.8f,0.3f,1);
            var ltxt = labelGo.GetComponent<Text>(); ltxt.text = label; ltxt.color = Color.white; ltxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); ltxt.alignment = TextAnchor.MiddleLeft;
            var bgr = bg.GetComponent<RectTransform>(); bgr.sizeDelta = new Vector2(24,24); bgr.anchoredPosition = new Vector2(-100,0);
            var ckr = check.GetComponent<RectTransform>(); ckr.sizeDelta = new Vector2(18,18);
            var lr = labelGo.GetComponent<RectTransform>(); lr.anchorMin = new Vector2(0,0); lr.anchorMax = new Vector2(1,1); lr.offsetMin = new Vector2(-70,0); lr.offsetMax = new Vector2(0,0);
            var t = go.GetComponent<Toggle>(); t.isOn = isOn; t.graphic = ckImg; t.targetGraphic = bgImg;
            return t;
        }

        private static Dropdown CreateDropdown(Transform parent, Vector2 anchored)
        {
            var go = new GameObject("Dropdown", typeof(RectTransform), typeof(Image), typeof(Dropdown)); go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>(); rt.sizeDelta = new Vector2(240, 40); rt.anchoredPosition = anchored;
            var img = go.GetComponent<Image>(); img.color = new Color(1,1,1,0.1f);
            var label = new GameObject("Label", typeof(RectTransform), typeof(Text)); label.transform.SetParent(go.transform, false);
            var template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect)); template.transform.SetParent(go.transform, false);
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image)); viewport.transform.SetParent(template.transform, false);
            var content = new GameObject("Content", typeof(RectTransform)); content.transform.SetParent(viewport.transform, false);
            var item = new GameObject("Item", typeof(RectTransform), typeof(Toggle)); item.transform.SetParent(content.transform, false);
            var itemBg = new GameObject("Item Background", typeof(RectTransform), typeof(Image)); itemBg.transform.SetParent(item.transform, false);
            var itemLabel = new GameObject("Item Label", typeof(RectTransform), typeof(Text)); itemLabel.transform.SetParent(item.transform, false);
            var lbl = label.GetComponent<Text>(); lbl.text = "프로파일"; lbl.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); lbl.color = Color.white; lbl.alignment = TextAnchor.MiddleLeft;
            var dd = go.GetComponent<Dropdown>(); dd.captionText = lbl; dd.template = template.GetComponent<RectTransform>();
            dd.itemText = itemLabel.GetComponent<Text>(); dd.options.Add(new Dropdown.OptionData("default"));
            return dd;
        }

        private static void CreateHomeScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            EnsureCanvas("Canvas", out var canvas);
            var bannerGo = new GameObject("HomeBanner"); bannerGo.transform.SetParent(canvas.transform, false);
            var banner = bannerGo.AddComponent<BilliardMasterAi.UI.RoutineBannerController>();
            var title = CreateText(bannerGo.transform, "오늘의 루틴", new Vector2(0, 120)); banner.titleText = title;
            var subtitle = CreateText(bannerGo.transform, "각도 기본 루틴", new Vector2(0, 60)); banner.subtitleText = subtitle;
            var meta = CreateText(bannerGo.transform, "각도 · 20분 · Easy", new Vector2(0, 0)); banner.metaText = meta; 
            var start = CreateButton(bannerGo.transform, "시작하기", new Vector2(0, -100)); banner.startButton = start;
            var home = new GameObject("Home"); var homeCtrl = home.AddComponent<BilliardMasterAi.UI.HomeScreenController>(); homeCtrl.banner = banner;
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/Home.unity");
        }

        private static void CreateCameraRecognitionScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            var root = new GameObject("TableRoot");
            var recogGo = new GameObject("BallRecognition");
            var recog = recogGo.AddComponent<BilliardMasterAi.Perception.BallRecognitionController>();
            recog.tableRoot = root.transform; recog.placeTransforms = true;
            var overlayGo = new GameObject("Overlay"); var overlay = overlayGo.AddComponent<BilliardMasterAi.UI.BallDetectionOverlay>(); overlay.tableRoot = root.transform;
            EnsureCanvas("Canvas", out var canvas);
            var btn = CreateButton(canvas.transform, "촬영/인식", new Vector2(0, -200));
            var togGo = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle), typeof(Image)); togGo.transform.SetParent(canvas.transform, false);
            var toggle = togGo.GetComponent<Toggle>(); toggle.GetComponent<Image>().color = new Color(1,1,1,0.2f); togGo.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,-270);
            var ctrlGo = new GameObject("CameraRecognitionScreen");
            var ctrl = ctrlGo.AddComponent<BilliardMasterAi.UI.CameraRecognitionScreenController>(); ctrl.recognition = recog; ctrl.overlay = overlay; ctrl.captureButton = btn; ctrl.showOverlayToggle = toggle;
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/CameraRecognition.unity");
        }

        private static void CreateRecommendationScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            var table = new GameObject("TableRoot");
            var cue = GameObject.CreatePrimitive(PrimitiveType.Sphere); cue.name = "CueBall"; cue.transform.SetParent(table.transform); cue.transform.localPosition = new Vector3(-0.5f,0,0);
            var obj1 = GameObject.CreatePrimitive(PrimitiveType.Sphere); obj1.name = "TargetBall"; obj1.transform.SetParent(table.transform); obj1.transform.localPosition = new Vector3(0.3f,0,0.2f);
            var obj2 = GameObject.CreatePrimitive(PrimitiveType.Sphere); obj2.name = "OtherBall"; obj2.transform.SetParent(table.transform); obj2.transform.localPosition = new Vector3(0.4f,0,-0.1f);
            var a = new GameObject("PathA").AddComponent<BilliardMasterAi.UI.ShotPathPresenter>();
            var la = a.gameObject.AddComponent<LineRenderer>(); la.positionCount = 0; la.widthMultiplier = 0.01f;
            var b = new GameObject("PathB").AddComponent<BilliardMasterAi.UI.ShotPathPresenter>();
            var lb = b.gameObject.AddComponent<LineRenderer>(); lb.positionCount = 0; lb.widthMultiplier = 0.01f;
            var ctrl = new GameObject("Recommendation").AddComponent<BilliardMasterAi.UI.RecommendationScreenController>();
            ctrl.tableRoot = table.transform; ctrl.cueBall = cue.transform; ctrl.targetBall = obj1.transform; ctrl.otherBall = obj2.transform; ctrl.pathA = a; ctrl.pathB = b;
            EnsureCanvas("Canvas", out var canvas);
            var btn = CreateButton(canvas.transform, "추천 계산", new Vector2(0, -220));
            btn.onClick.AddListener(()=>ctrl.Recommend());
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/Recommendation.unity");
        }

        private static void CreateReplayScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            var table = new GameObject("TableRoot");
            var idealGo = new GameObject("Ideal").AddComponent<BilliardMasterAi.UI.TimedPathRenderer>(); idealGo.gameObject.AddComponent<LineRenderer>();
            var actualGo = new GameObject("Actual").AddComponent<BilliardMasterAi.UI.TimedPathRenderer>(); actualGo.gameObject.AddComponent<LineRenderer>();
            var replay = new GameObject("Replay").AddComponent<BilliardMasterAi.Replay.ShotReplayController>();
            replay.tableRoot = table.transform; replay.idealRenderer = idealGo; replay.actualRenderer = actualGo;
            EnsureCanvas("Canvas", out var canvas);
            CreateText(canvas.transform, "리플레이 샘플", new Vector2(0, 200));
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/Replay.unity");
        }

        private static void CreateReplayCompareScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            var table = new GameObject("TableRoot");
            var cue = GameObject.CreatePrimitive(PrimitiveType.Sphere); cue.name = "CueBall"; cue.transform.SetParent(table.transform); cue.transform.localPosition = new Vector3(-0.4f, 0, 0);
            var obj1 = GameObject.CreatePrimitive(PrimitiveType.Sphere); obj1.name = "Red"; obj1.transform.SetParent(table.transform); obj1.transform.localPosition = new Vector3(0.2f, 0, 0.15f);
            var obj2 = GameObject.CreatePrimitive(PrimitiveType.Sphere); obj2.name = "Other"; obj2.transform.SetParent(table.transform); obj2.transform.localPosition = new Vector3(0.3f, 0, -0.1f);

            var recGo = new GameObject("Recorder"); var recorder = recGo.AddComponent<BilliardMasterAi.Replay.BallTrajectoryRecorder>(); recorder.tableRoot = table.transform; recorder.cueBall = cue.transform;
            var replay = new GameObject("Replay").AddComponent<BilliardMasterAi.Replay.ShotReplayController>(); replay.tableRoot = table.transform;
            var ideal = new GameObject("Ideal").AddComponent<BilliardMasterAi.UI.TimedPathRenderer>(); ideal.gameObject.AddComponent<LineRenderer>(); replay.idealRenderer = ideal;
            var actual = new GameObject("Actual").AddComponent<BilliardMasterAi.UI.TimedPathRenderer>(); actual.gameObject.AddComponent<LineRenderer>(); replay.actualRenderer = actual;
            var idealMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform; idealMarker.localScale = Vector3.one * 0.03f; replay.idealMarker = idealMarker;
            var actualMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform; actualMarker.localScale = Vector3.one * 0.03f; replay.actualMarker = actualMarker;

            EnsureCanvas("Canvas", out var canvas);
            var panel = new GameObject("ReplayComparePanel", typeof(RectTransform), typeof(Image)); panel.transform.SetParent(canvas.transform, false);
            var prt = panel.GetComponent<RectTransform>(); prt.sizeDelta = new Vector2(900, 600); prt.anchoredPosition = Vector2.zero; panel.GetComponent<Image>().color = new Color(0,0,0,0.35f);

            var ctrl = panel.AddComponent<BilliardMasterAi.UI.ReplayCompareScreenController>();
            ctrl.tableRoot = table.transform; ctrl.cueBall = cue.transform; ctrl.targetBall = obj1.transform; ctrl.otherBall = obj2.transform; ctrl.recorder = recorder; ctrl.replay = replay;

            var rigid = CreateToggle(panel.transform, "Rigid Only (no scale)", new Vector2(-240, 220), false); ctrl.rigidOnlyToggle = rigid;
            var rej = CreateSlider(panel.transform, "Reject Fraction", new Vector2(0, 220), 0f, 0.5f, 0.0f); ctrl.rejectFractionSlider = rej;

            ctrl.alignStatusText = CreateText(panel.transform, "정렬 준비", new Vector2(0, 160));
            ctrl.scoringText = CreateText(panel.transform, "실제 판정", new Vector2(0, 120));
            ctrl.idealScoringText = CreateText(panel.transform, "이상 판정", new Vector2(0, 80));
            ctrl.alignNameInput = CreateInput(panel.transform, "alignment name", new Vector2(-240, 40), "default");
            ctrl.saveAlignButton = CreateButton(panel.transform, "Save Align", new Vector2(-140, -10));
            ctrl.loadAlignButton = CreateButton(panel.transform, "Load Align", new Vector2(140, -10));
            var runBtn = CreateButton(panel.transform, "Run Analysis", new Vector2(0, -60));
            runBtn.onClick.AddListener(ctrl.RunAnalysis);

            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/ReplayCompare.unity");
        }

        private static void CreateVideoSyncScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            // Table + Replay
            var table = new GameObject("TableRoot");
            var replay = new GameObject("Replay").AddComponent<BilliardMasterAi.Replay.ShotReplayController>(); replay.tableRoot = table.transform;
            var ideal = new GameObject("Ideal").AddComponent<BilliardMasterAi.UI.TimedPathRenderer>(); ideal.gameObject.AddComponent<LineRenderer>(); replay.idealRenderer = ideal;
            var actual = new GameObject("Actual").AddComponent<BilliardMasterAi.UI.TimedPathRenderer>(); actual.gameObject.AddComponent<LineRenderer>(); replay.actualRenderer = actual;

            // Video Player + RawImage
            EnsureCanvas("Canvas", out var canvas);
            var raw = new GameObject("Video", typeof(RectTransform), typeof(UnityEngine.UI.RawImage)); raw.transform.SetParent(canvas.transform, false);
            var rrt = raw.GetComponent<RectTransform>(); rrt.sizeDelta = new Vector2(960, 540); rrt.anchoredPosition = Vector2.zero;
            var vpGo = new GameObject("VideoPlayer"); var vp = vpGo.AddComponent<UnityEngine.Video.VideoPlayer>();
            vp.renderMode = UnityEngine.Video.VideoRenderMode.APIOnly; // we'll Blit to RawImage via material/texture assignment during playback

            // Tracker
            var tracker = new GameObject("VideoTracker").AddComponent<BilliardMasterAi.Creator.VideoBallTracker>();

            // Sync
            var sync = raw.AddComponent<BilliardMasterAi.UI.VideoReplaySync>();
            sync.videoPlayer = vp; sync.replay = replay; sync.tracker = tracker; sync.enableSync = true;

            // Info + Controls
            var info = CreateText(canvas.transform, "Video Sync Demo: Set url, Load, then Play. Replay follows timeline.", new Vector2(0, 300));
            var url = CreateInput(canvas.transform, "video url (file:// or http://)", new Vector2(0, 260), "");
            var loadBtn = CreateButton(canvas.transform, "Load", new Vector2(-200, 210));
            var playBtn = CreateButton(canvas.transform, "Play", new Vector2(0, 210));
            var pauseBtn = CreateButton(canvas.transform, "Pause", new Vector2(200, 210));

            var vdc = raw.AddComponent<BilliardMasterAi.UI.VideoDisplayController>();
            vdc.videoPlayer = vp; vdc.rawImage = raw.GetComponent<UnityEngine.UI.RawImage>(); vdc.urlInput = url; vdc.loadButton = loadBtn; vdc.playButton = playBtn; vdc.pauseButton = pauseBtn;

            // Alignment Control
            var alignPanel = new GameObject("AlignmentPanel", typeof(RectTransform), typeof(UnityEngine.UI.Image)); alignPanel.transform.SetParent(canvas.transform, false);
            var aprt = alignPanel.GetComponent<RectTransform>(); aprt.sizeDelta = new Vector2(600, 120); aprt.anchoredPosition = new Vector2(0, -280);
            alignPanel.GetComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0.3f);
            var nameInput = CreateInput(alignPanel.transform, "alignment name", new Vector2(-200, 20), "default");
            var loadAlignBtn = CreateButton(alignPanel.transform, "Load Align", new Vector2(0, 20));
            var clearAlignBtn = CreateButton(alignPanel.transform, "Clear Align", new Vector2(200, 20));
            var status = CreateText(alignPanel.transform, "정렬 미지정", new Vector2(0, -20));
            var acu = alignPanel.AddComponent<BilliardMasterAi.UI.AlignmentControlUI>();
            acu.nameInput = nameInput; acu.loadButton = loadAlignBtn; acu.publishZeroButton = clearAlignBtn; acu.statusText = status;

            // Tracker Control
            var trackPanel = new GameObject("TrackPanel", typeof(RectTransform), typeof(UnityEngine.UI.Image)); trackPanel.transform.SetParent(canvas.transform, false);
            var tprt = trackPanel.GetComponent<RectTransform>(); tprt.sizeDelta = new Vector2(600, 140); tprt.anchoredPosition = new Vector2(0, -420);
            trackPanel.GetComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0.3f);
            var startIn = CreateInput(trackPanel.transform, "start sec", new Vector2(-240, 30), "0");
            var endIn = CreateInput(trackPanel.transform, "end sec", new Vector2(0, 30), "5");
            var kalT = CreateToggle(trackPanel.transform, "Kalman", new Vector2(240, 30), true);
            var trackBtn = CreateButton(trackPanel.transform, "Track", new Vector2(0, -10));
            var tstat = CreateText(trackPanel.transform, "Tracking idle", new Vector2(0, -50));
            var tcu = trackPanel.AddComponent<BilliardMasterAi.UI.VideoTrackerControlUI>();
            tcu.tracker = tracker; tcu.videoPlayer = vp; tcu.startTimeInput = startIn; tcu.endTimeInput = endIn; tcu.useKalmanToggle = kalT; tcu.trackButton = trackBtn; tcu.statusText = tstat;

            // Auto Track + Align + Sync
            var autoBtn = CreateButton(canvas.transform, "Auto Track + Align + Sync", new Vector2(0, -500));
            var auto = canvas.gameObject.AddComponent<BilliardMasterAi.UI.AutoTrackAlignSyncUI>();
            auto.trackerUI = tcu; auto.sync = sync; auto.tracker = tracker; auto.runButton = autoBtn; auto.statusText = CreateText(canvas.transform, "Auto idle", new Vector2(0, -540));
            auto.replay = replay; auto.recorder = null; auto.trackerYellowIsCue = true; auto.alignWindow = 0.8f; auto.rigidOnly = false; auto.rejectFraction = 0.0f; auto.videoPlayerRef = vp; auto.autoSave = true;
            var saveNameIn = CreateInput(canvas.transform, "save name (optional)", new Vector2(0, -580), "");
            auto.saveNameInput = saveNameIn;

            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/VideoSync.unity");
        }

        private static void CreateDashboardTrendsScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            EnsureCanvas("Canvas", out var canvas);
            var days = CreateInput(canvas.transform, "days", new Vector2(-200, 200), "7");
            var refresh = CreateButton(canvas.transform, "Refresh", new Vector2(200, 200));
            var tableD = CreateDropdown(canvas.transform, new Vector2(0, 240));
            var succImg = new GameObject("SuccessChart", typeof(RectTransform), typeof(UnityEngine.UI.RawImage)); succImg.transform.SetParent(canvas.transform, false); succImg.GetComponent<RectTransform>().sizeDelta = new Vector2(400,120); succImg.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 60);
            var ttiImg = new GameObject("TTIChart", typeof(RectTransform), typeof(UnityEngine.UI.RawImage)); ttiImg.transform.SetParent(canvas.transform, false); ttiImg.GetComponent<RectTransform>().sizeDelta = new Vector2(400,120); ttiImg.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -80);
            var succChart = succImg.AddComponent<BilliardMasterAi.UI.LineChart>(); var ttiChart = ttiImg.AddComponent<BilliardMasterAi.UI.LineChart>();
            var title = CreateText(canvas.transform, "Dashboard Trends (Success / TTI)", new Vector2(0, 280));
            var trends = canvas.gameObject.AddComponent<BilliardMasterAi.UI.DashboardTrendsUI>(); trends.daysInput = days; trends.refreshButton = refresh; trends.successChart = succChart; trends.ttiChart = ttiChart; trends.tableDropdown = tableD; trends.legendText = title;
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/DashboardTrends.unity");
        }

        private static void CreateSensitivityScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            var table = new GameObject("TableRoot"); var cue = GameObject.CreatePrimitive(PrimitiveType.Sphere); cue.transform.SetParent(table.transform);
            var obj1 = GameObject.CreatePrimitive(PrimitiveType.Sphere); obj1.transform.SetParent(table.transform); obj1.transform.localPosition = new Vector3(0.3f,0,0.2f);
            var obj2 = GameObject.CreatePrimitive(PrimitiveType.Sphere); obj2.transform.SetParent(table.transform); obj2.transform.localPosition = new Vector3(0.4f,0,-0.1f);
            EnsureCanvas("Canvas", out var canvas);
            var heatImg = new GameObject("Heatmap", typeof(RectTransform), typeof(UnityEngine.UI.RawImage)).AddComponent<UnityEngine.UI.RawImage>(); heatImg.transform.SetParent(canvas.transform, false); heatImg.rectTransform.sizeDelta = new Vector2(256,256); heatImg.rectTransform.anchoredPosition = new Vector2(0,0);
            var heat = heatImg.gameObject.AddComponent<BilliardMasterAi.UI.SensitivityHeatmapUI>();
            var panel = canvas.gameObject.AddComponent<BilliardMasterAi.UI.SensitivityPanelUI>();
            panel.tableRoot = table.transform; panel.cueBall = cue.transform; panel.targetBall = obj1.transform; panel.otherBall = obj2.transform; panel.heatmap = heat;
            panel.angleMinInput = CreateInput(canvas.transform, "ang min", new Vector2(-300, 200), "0");
            panel.angleMaxInput = CreateInput(canvas.transform, "ang max", new Vector2(-100, 200), "360");
            panel.angleNInput = CreateInput(canvas.transform, "ang N", new Vector2(100, 200), "36");
            panel.speedMinInput = CreateInput(canvas.transform, "spd min", new Vector2(-300, 150), "1.5");
            panel.speedMaxInput = CreateInput(canvas.transform, "spd max", new Vector2(-100, 150), "3.5");
            panel.speedNInput = CreateInput(canvas.transform, "spd N", new Vector2(100, 150), "12");
            panel.spinMinInput = CreateInput(canvas.transform, "spin min", new Vector2(-300, 100), "-20");
            panel.spinMaxInput = CreateInput(canvas.transform, "spin max", new Vector2(-100, 100), "20");
            panel.spinNInput = CreateInput(canvas.transform, "spin N", new Vector2(100, 100), "5");
            panel.runButton = CreateButton(canvas.transform, "Run Sensitivity", new Vector2(0, 50));
            panel.statusText = CreateText(canvas.transform, "Ready", new Vector2(0, -220));
            // Spin slice controller
            var sliceSlider = CreateSlider(canvas.transform, "Spin Slice", new Vector2(0, -260), 0, 4, 0);
            var sliceLabel = CreateText(canvas.transform, "Spin slice: 0", new Vector2(0, -300));
            var sliceUI = canvas.gameObject.AddComponent<BilliardMasterAi.UI.SensitivitySliceUI>();
            sliceUI.heatmap = heat; sliceUI.spinSlider = sliceSlider; sliceUI.spinLabel = sliceLabel;
            panel.sliceUI = sliceUI;
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/Sensitivity.unity");
        }

        private static void CreateReplayEditorScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            EnsureCanvas("Canvas", out var canvas);
            var start = CreateInput(canvas.transform, "start sec", new Vector2(-240, 40), "0");
            var end = CreateInput(canvas.transform, "end sec", new Vector2(0, 40), "5");
            var label = CreateInput(canvas.transform, "label", new Vector2(240, 40), "clip");
            var speed = CreateInput(canvas.transform, "speed", new Vector2(-240, -10), "1.0");
            var caption = CreateInput(canvas.transform, "caption", new Vector2(0, -10), "");
            var add = CreateButton(canvas.transform, "Add Clip", new Vector2(0, -60));
            var export = CreateButton(canvas.transform, "Export EDL", new Vector2(0, -110));
            var status = CreateText(canvas.transform, "EDL idle", new Vector2(0, -170));
            var ui = canvas.gameObject.AddComponent<BilliardMasterAi.UI.ReplayEditorUI>(); ui.startInput = start; ui.endInput = end; ui.labelInput = label; ui.speedInput = speed; ui.captionInput = caption; ui.addClipButton = add; ui.exportEdlButton = export; ui.statusText = status;
            // Render controls
            var outDir = CreateInput(canvas.transform, "out dir", new Vector2(0, -220), System.IO.Path.Combine(Application.persistentDataPath, "Render"));
            var fps = CreateInput(canvas.transform, "fps", new Vector2(240, -220), "30");
            var renderBtn = CreateButton(canvas.transform, "Render PNGs", new Vector2(-120, -270));
            var ffmpegBtn = CreateButton(canvas.transform, "To MP4", new Vector2(120, -270));
            var rpCam = new GameObject("RenderCamera").AddComponent<UnityEngine.Camera>();
            // overlay canvas for caption
            var overlay = new GameObject("OverlayCanvas"); var oCanvas = overlay.AddComponent<UnityEngine.Canvas>(); oCanvas.renderMode = UnityEngine.RenderMode.ScreenSpaceOverlay; var captionObj = new GameObject("Caption", typeof(RectTransform), typeof(UnityEngine.UI.Text)); captionObj.transform.SetParent(overlay.transform, false); var capTxt = captionObj.GetComponent<UnityEngine.UI.Text>(); capTxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); capTxt.fontSize = 24; capTxt.alignment = TextAnchor.UpperCenter; capTxt.color = Color.white; captionObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 260);
            var pipeline = canvas.gameObject.AddComponent<BilliardMasterAi.Rendering.ReplayRenderPipeline>(); pipeline.renderCamera = rpCam; pipeline.overlayCanvas = oCanvas; pipeline.captionText = capTxt; pipeline.OnSeek = (t)=>{ var rc = canvas.gameObject.GetComponent<BilliardMasterAi.Replay.ShotReplayController>(); if (rc!=null) rc.Seek(t); };
            var rui = canvas.gameObject.AddComponent<BilliardMasterAi.UI.ReplayRenderUI>(); rui.editorUI = ui; rui.pipeline = pipeline; rui.outDirInput = outDir; rui.fpsInput = fps; rui.renderButton = renderBtn; rui.ffmpegButton = ffmpegBtn; rui.statusText = CreateText(canvas.transform, "Render idle", new Vector2(0, -320));
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/ReplayEditor.unity");
        }

        private static void CreateSuccessModelTrainerScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            EnsureCanvas("Canvas", out var canvas);
            var status = CreateText(canvas.transform, "Trainer idle", new Vector2(0, 240));
            var epochs = CreateInput(canvas.transform, "epochs", new Vector2(-200, 180), "300");
            var lr = CreateInput(canvas.transform, "lr", new Vector2(200, 180), "0.1");
            var addSample = CreateButton(canvas.transform, "Add Sample", new Vector2(-200, 120));
            var train = CreateButton(canvas.transform, "Train", new Vector2(0, 120));
            var save = CreateButton(canvas.transform, "Save Model", new Vector2(200, 120));
            var trainer = canvas.gameObject.AddComponent<BilliardMasterAi.UI.SuccessModelTrainerUI>();
            trainer.epochsInput = epochs; trainer.lrInput = lr; trainer.addSampleButton = addSample; trainer.trainButton = train; trainer.saveButton = save; trainer.statusText = status;
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/SuccessModelTrainer.unity");
        }

        private static void CreateSuccessModelReportScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            EnsureCanvas("Canvas", out var canvas);
            var rocImg = new GameObject("ROC", typeof(RectTransform), typeof(UnityEngine.UI.RawImage)).AddComponent<UnityEngine.UI.RawImage>(); rocImg.transform.SetParent(canvas.transform, false); rocImg.rectTransform.sizeDelta = new Vector2(400,120); rocImg.rectTransform.anchoredPosition = new Vector2(0, 40);
            var chart = rocImg.gameObject.AddComponent<BilliardMasterAi.UI.LineChart>();
            var aucText = CreateText(canvas.transform, "AUC: -", new Vector2(0, 120));
            var statsText = CreateText(canvas.transform, "stats", new Vector2(0, -20));
            var report = canvas.gameObject.AddComponent<BilliardMasterAi.UI.SuccessModelReportUI>(); report.rocChart = chart; report.aucText = aucText; report.statsText = statsText;
            var refresh = CreateButton(canvas.transform, "Refresh Report", new Vector2(0, -80)); refresh.onClick.AddListener(report.RefreshReport);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/SuccessModelReport.unity");
        }

        private static void CreateCalibrationScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            var table = new GameObject("TableRoot");
            var cue = GameObject.CreatePrimitive(PrimitiveType.Sphere); cue.name = "CueBall"; cue.transform.SetParent(table.transform);
            var obj1 = GameObject.CreatePrimitive(PrimitiveType.Sphere); obj1.name = "Obj1"; obj1.transform.SetParent(table.transform);
            var obj2 = GameObject.CreatePrimitive(PrimitiveType.Sphere); obj2.name = "Obj2"; obj2.transform.SetParent(table.transform);

            var recGo = new GameObject("Recorder"); var recorder = recGo.AddComponent<BilliardMasterAi.Replay.BallTrajectoryRecorder>(); recorder.tableRoot = table.transform; recorder.cueBall = cue.transform;

            EnsureCanvas("Canvas", out var canvas);
            var panel = new GameObject("CalibrationPanel", typeof(RectTransform), typeof(Image)); panel.transform.SetParent(canvas.transform, false);
            var prt = panel.GetComponent<RectTransform>(); prt.sizeDelta = new Vector2(800, 600); prt.anchoredPosition = Vector2.zero; panel.GetComponent<Image>().color = new Color(0,0,0,0.4f);
            var ctrl = panel.AddComponent<BilliardMasterAi.UI.CalibrationPanelController>();
            ctrl.recorder = recorder; ctrl.tableRoot = table.transform; ctrl.cueBall = cue.transform; ctrl.targetBall = obj1.transform; ctrl.otherBall = obj2.transform;

            // Sliders
            ctrl.muK = CreateSlider(panel.transform, "μk (sliding)", new Vector2(0, 240), 0.05f, 0.35f, 0.20f);
            ctrl.muR = CreateSlider(panel.transform, "μr (rolling)", new Vector2(0, 190), 0.002f, 0.03f, 0.010f);
            ctrl.muContact = CreateSlider(panel.transform, "μcontact", new Vector2(0, 140), 0.05f, 0.4f, 0.20f);
            ctrl.muCushion = CreateSlider(panel.transform, "μcushion", new Vector2(0, 90), 0.05f, 0.5f, 0.25f);
            ctrl.eBall = CreateSlider(panel.transform, "e(ball)", new Vector2(0, 40), 0.80f, 0.99f, 0.93f);
            ctrl.eCushion = CreateSlider(panel.transform, "e(cushion)", new Vector2(0, -10), 0.80f, 0.99f, 0.92f);

            // Inputs
            ctrl.angleInput = CreateInput(panel.transform, "angle deg", new Vector2(-240, -80), "0");
            ctrl.speedInput = CreateInput(panel.transform, "speed m/s", new Vector2(0, -80), "2.5");
            ctrl.spinInput = CreateInput(panel.transform, "spin rad/s", new Vector2(240, -80), "0");
            ctrl.iterationsInput = CreateInput(panel.transform, "iterations", new Vector2(-240, -130), "200");

            // Toggles
            ctrl.fitFrictionToggle = CreateToggle(panel.transform, "Fit Friction", new Vector2(-240, -180), true);
            ctrl.fitContactToggle = CreateToggle(panel.transform, "Fit Contact", new Vector2(0, -180), true);
            ctrl.fitRestitutionToggle = CreateToggle(panel.transform, "Fit Restitution", new Vector2(240, -180), true);

            // Profile controls
            ctrl.profileDropdown = CreateDropdown(panel.transform, new Vector2(-160, -230));
            ctrl.newProfileNameInput = CreateInput(panel.transform, "new profile name", new Vector2(140, -230), "table-A");
            ctrl.saveProfileButton = CreateButton(panel.transform, "Save Profile", new Vector2(-200, -290));
            ctrl.loadProfileButton = CreateButton(panel.transform, "Load Profile", new Vector2(0, -290));
            ctrl.deleteProfileButton = CreateButton(panel.transform, "Delete Profile", new Vector2(200, -290));

            ctrl.exportNameInput = CreateInput(panel.transform, "export file name", new Vector2(-240, -340), "calib.json");
            ctrl.importJsonInput = CreateInput(panel.transform, "paste JSON here", new Vector2(140, -340), "");
            ctrl.exportProfileButton = CreateButton(panel.transform, "Export JSON", new Vector2(-140, -390));
            ctrl.importProfileButton = CreateButton(panel.transform, "Import JSON", new Vector2(140, -390));
            ctrl.exportBrowseButton = CreateButton(panel.transform, "Browse Save", new Vector2(-140, -440));
            ctrl.importBrowseButton = CreateButton(panel.transform, "Browse Open", new Vector2(140, -440));
            ctrl.copyJsonButton = CreateButton(panel.transform, "Copy JSON", new Vector2(-140, -490));
            ctrl.pasteJsonButton = CreateButton(panel.transform, "Paste JSON", new Vector2(140, -490));

            // Status + buttons
            ctrl.statusText = CreateText(panel.transform, "Calibration Ready", new Vector2(0, -340));
            var applyBtn = CreateButton(panel.transform, "Apply", new Vector2(-200, -540));
            applyBtn.onClick.AddListener(ctrl.ApplySliders);
            var fitBtn = CreateButton(panel.transform, "AutoFit", new Vector2(200, -540));
            fitBtn.onClick.AddListener(ctrl.AutoFit);

            ctrl.useCmaEsToggle = CreateToggle(panel.transform, "Use CMA-ES", new Vector2(0, -520), true);

            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/Calibration.unity");
        }

        private static void CreateCoachDashboardScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            EnsureCanvas("Canvas", out var canvas);
            var ctrl = new GameObject("CoachDashboard").AddComponent<BilliardMasterAi.UI.CoachDashboardController>();
            CreateText(canvas.transform, "코치 대시보드 (프리팹 연결 필요)", new Vector2(0,0));
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/CoachDashboard.unity");
        }

        private static void CreatePatternAssignScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            EnsureCanvas("Canvas", out var canvas);
            new GameObject("PatternAssign").AddComponent<BilliardMasterAi.UI.PatternAssignScreenController>();
            CreateText(canvas.transform, "패턴 할당 (UI 바인딩 필요)", new Vector2(0,0));
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/PatternAssign.unity");
        }

        private static void CreateStudentTrainingScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            var table = new GameObject("TableRoot");
            var ctrl = new GameObject("StudentTraining").AddComponent<BilliardMasterAi.UI.StudentTrainingScreenController>(); ctrl.tableRoot = table.transform;
            EnsureCanvas("Canvas", out var canvas);
            var btn = CreateButton(canvas.transform, "인식+시뮬", new Vector2(0,-220));
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/StudentTraining.unity");
        }

        private static void CreateRealTimeDashboardScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            EnsureCanvas("Canvas", out var canvas);
            new GameObject("RealTimeDashboard").AddComponent<BilliardMasterAi.UI.RealTimeDashboardController>();
            CreateText(canvas.transform, "실시간 대시보드 (ScrollView 바인딩 필요)", new Vector2(0,0));
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/RealTimeDashboard.unity");
        }

        private static void CreatePerformanceReportScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            EnsureCanvas("Canvas", out var canvas);
            new GameObject("PerformanceReport").AddComponent<BilliardMasterAi.UI.PerformanceReportScreenController>();
            CreateText(canvas.transform, "성과 리포트 (버튼 연결 필요)", new Vector2(0,0));
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/PerformanceReport.unity");
        }

        private static void CreateVideoImportScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            EnsureCanvas("Canvas", out var canvas);
            new GameObject("VideoImport").AddComponent<BilliardMasterAi.Creator.VideoImportController>();
            CreateText(canvas.transform, "영상 가져오기 (UI/VideoPlayer 연결 필요)", new Vector2(0,0));
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/VideoImport.unity");
        }

        private static void CreateVideoBallRecognitionScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            EnsureCanvas("Canvas", out var canvas);
            new GameObject("VideoBallRecognition").AddComponent<BilliardMasterAi.UI.VideoBallRecognitionScreenController>();
            CreateText(canvas.transform, "영상 공 인식 (보정/트래커 연결)", new Vector2(0,0));
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/VideoBallRecognition.unity");
        }

        private static void CreateReplayOverlayScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            EnsureCanvas("Canvas", out var canvas);
            new GameObject("ReplayOverlay").AddComponent<BilliardMasterAi.UI.ReplayOverlayScreenController>();
            CreateText(canvas.transform, "리플레이 오버레이 (보정/트래커/렌더러 연결)", new Vector2(0,0));
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/ReplayOverlay.unity");
        }

        private static void CreateExportScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            EnsureCanvas("OverlayCanvas", out var overlayCanvas);
            var export = new GameObject("OverlayExport").AddComponent<BilliardMasterAi.UI.OverlayExportController>();
            CreateText(overlayCanvas.transform, "오버레이 내보내기 (카메라/캔버스 설정)", new Vector2(0,0));
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/Export.unity");
        }

        private static void CreateShareScene()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            EnsureCanvas("Canvas", out var canvas);
            new GameObject("CompletedShare").AddComponent<BilliardMasterAi.UI.CompletedVideoShareScreenController>();
            CreateText(canvas.transform, "완성 영상 공유 (프리셋/워터마크/내보내기 연결)", new Vector2(0,0));
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/CompletedShare.unity");
        }
    }
}
