# BilliardMasterAi

AI 기반 캐롬 3쿠션(3-4개 쿠션) 궤적 경로 예측 모바일 앱의 Unity 프로젝트입니다. AR을 통해 실제 당구대 위에 가상 경로를 오버레이하고, 물리/탐색 기반 추천 알고리즘으로 최적 샷(강도·방향·스핀)을 제시합니다.

## 요구사항
- Unity 2022.3 LTS 권장
- 모바일 AR: AR Foundation 5.x, ARKit(iOS), ARCore(Android)  
- 플랫폼 SDK: Xcode(iOS), Android SDK(Android)

## 폴더 구조
- `Assets/Scripts/AR/`: AR 배치·보정(테이블 캘리브레이션)
- `Assets/Scripts/Physics/`: 캐롬 물리/추론 엔진
- `Assets/Scripts/Recommendation/`: 경로 탐색/샷 추천
- `Assets/Scripts/UI/`: 경로 시각화/제어 UI
- `Docs/`: 제품/아키텍처/스토리보드

## 빠른 시작
1) Unity에서 폴더 열기 후 패키지 복원(AR Foundation 등)
2) 샘플 씬 생성 후 컴포넌트 배치
   - `ARTableCalibrator`(실제 당구대 테이블 배치)
   - `ShotOverlayController`(가이드 렌더러로 경로 표시)
3) `ShotPlanner`에서 `PlanShot(...)` 호출로 추천 경로/샷 파라미터 획득

## 카메라 인식 화면(반자동 인식)
- 구성 요소
  - `BallRecognitionController`: 카메라 프레임 캡처 후 이미지 분할(빨강/파랑/노랑) 후 화면좌표→테이블좌표 매핑
  - `BallDetectionOverlay`: 테이블 위에 인식 결과 마커 표시  
  - `CameraRecognitionScreenController`: UI 버튼(촬영/인식) 연결
- 씬 배치 예시
  - `ARSession/ARSessionOrigin/ARCamera`(AR Foundation) + `ARRaycastManager`
  - `ARTableCalibrator`로 테이블 배치 완료 후
  - 게임오브젝트에 `BallRecognitionController` 추가(`tableRoot`, `arCamera` 할당)
  - `BallDetectionOverlay` 배치 후 `tableRoot` 연결
  - 캔버스에 `Button(촬영/인식)`, `Toggle(결과보기)` 생성 후 `CameraRecognitionScreenController`에 바인딩
  - 버튼 누르면 인식 실행, 토글로 결과 확인 가능

### 즉시 자동 인식(약 1.2s 목표)
- `BallRecognitionController.autoDetect = true`, `detectInterval = 0.2f`(5Hz)로 연속 인식
- 최적화 튜닝
  - `BallDetectionConfig.downscaleWidth/Height`를 320x180(기본) 또는 256x144로 설정
  - `minConfidence`를 0.15~0.30 사이로 조절하여 정확도/속도 균형
  - ARFoundation CPU 렌더링 경로 사용, 불가능 시 WebCam 경로(에디터)
- 즉시 배치: `placeTransforms = true`로 `whiteBall/yellowBall/redBall` Transform 위치 자동 갱신

## 학생 훈련 진행 화면(카메라인식→샷추천→가이드실행)
- 구성 요소
  - `StudentTrainingScreenController`: 버튼 한 번으로 카메라 인식 실행 후 상위 1~2개 추천 경로 계산 및 AR 가이드/화면 가이드 표시
  - 의존: `BallRecognitionController`(자동 배치 사용), `ARGuideOverlay`(오버레이), `ShotPathPresenter`(가이드+파라미터)
- 씬 배치 예시
  - `BallRecognitionController`에 `placeTransforms=true`, 세 Transform(white/yellow/red) 바인딩, `yellowIsCueBall` 설정
  - `StudentTrainingScreenController`에 `tableRoot`, `recognition`, `overlay`, `pathA`/`pathB` 연결
  - 버튼 OnClick에 `StudentTrainingScreenController.DetectAndSimulate()`
  - 실행: 카메라 인식 직후 테이블 위에 최적 경로가 오버레이되고, 화면에도 1~2개 경로가 표시됨

## 경기 전 모드 선택(리그 모드 + 제한시간 표시)
- 구성 요소
  - `ModeSelectionScreenController`: 리그 모드 선택, 제한 시간(분) 입력, 적용 버튼
  - `GameState`(`GameConfig.cs`): 현재 모드/제한시간/타이머 상태 전역 관리
  - `TimerHUDController`: 상단 HUD에 mm:ss 카운트다운 표시(경고/위험 색상)
- 씬 배치 예시
  - 모드 선택 패널(Canvas): Toggle(`리그 모드`), InputField(분), Button(`적용`) 및 `ModeSelectionScreenController` 바인딩
  - HUD 시스템에 `TimerHUDController.timerText` 바인딩(상단 고정)
  - 적용 시 리그 모드 ON + 제한시간 설정 후 타이머 자동 시작. 시작/정지는 HUD의 public 메서드로 연결 가능

## 실제 규격 참고
- 캐롬대 규격: 2.84 m x 1.42 m
- 공 지름: 57.15 mm (반지름 0.028575 m)
- Unity 단위: 1 = 1 m 기준

## 빌드
- iOS: ARKit 활성화, 카메라 권한
- Android: ARCore 활성화, 카메라 권한

자세한 제품/시나리오/화면 플로우는 `Docs/PRODUCT.md` 참고.

## 추천 경로 화면(2개 경로 + 강도/스핀/난이도 표시)
- 구성 요소
  - `RecommendationScreenController`: 현재 공 좌표로 상위 2개 추천 경로 계산
  - `ShotPathPresenter` x2: 각 경로 가이드 렌더링 + 파라미터 텍스트 표시
- 씬 배치 예시
  - 테이블(= `tableRoot`) 아래에 `cueBall`, `targetBall`, `otherBall`(Transform) 배치
  - `RecommendationScreenController`에 위 3개와 `ShotPathPresenter` 2개 연결
  - 각 `ShotPathPresenter`에 LineRenderer와 UI Text(`강도`, `스핀`, `난이도`) 바인딩
  - 버튼으로 `RecommendationScreenController.Recommend()` 호출 시 각 경로와 파라미터 갱신
  - 난이도 규칙(근사)
  - 강도: 초기 진행방향 vs 목표방향 코사인 기반 비율(0~100%)
  - 스핀: 목표 Z 부호에 따른 좌우 구분(0~100%)
  - 난이도: 초기 속도(1.5~3.5 m/s)를 0~100% 선형화

## 추천 경로 비교 화면(3개 경로 + 성공확률/리스크값)
- 구성 요소
  - `RecommendationCompareScreenController`: 상위 3개 경로 추출 후 바인딩
  - `PathCompareItem` ×3: 각 경로를 LineRenderer로 표시하고 성공확률/리스크값(Image fill)과 텍스트로 나타냄
  - `ShotEvaluator`: 휴리스틱 기반 성공확률/리스크값 추출(쿠션 수, 경로 길이, 목표공 목표/방해 공 근접도)
  - `RiskProfileController`: 보수/중립/공격 프로파일로 가중치 조정(성공/리스크 비계산)
- 씬 배치 예시
  - 테이블 위에 `cueBall`, `targetBall`, `otherBall` 배치
  - `RecommendationCompareScreenController`에 `PathCompareItem` 3개 연결
  - 각 `PathCompareItem`과 내부 `ShotPathPresenter`(LineRenderer), Image 2개(prob/risk), Text 2개 연결
  - 버튼으로 `RecommendCompare()` 호출 시 3개 경로와 각 지표 갱신
- 지표 해석(0~100%)
  - 성공확률: 높을수록 성공 가능성 높음(길이/쿠션/방해 근접도에 따라 감소)
  - 리스크: 높을수록 실패/파울 가능성 높음(방해 공 근접/경로 길이/쿠션 영향)

## 샷 실행 후 리플레이(실제 궤적 vs 이상 궤적 + 오차 리포트)
- 구성 요소
  - `BallTrajectoryRecorder`: 샷 동안 수구 위치를 테이블좌표로 샘플링(기본 60Hz)
  - `ShotReplayController`: 이상 궤적(추론엔진)과 실제 궤적(기록) 표시 재생, 마커 이동
  - `TimedPathRenderer` x2: 궤적 가이드(이상/실제)
  - `ErrorReportPresenter`: RMS/최대/최종 오프셋, 쿠션 차이, 경로 길이 출력
- 사용 예시
  1) 샷 직전 `BallTrajectoryRecorder.StartRecording()` 호출
  2) 샷 종료 후 `StopRecording()`으로 리스트 확보
  3) `ShotReplayController.SetIdealFromPlan(cueState, ...)` + `SetActual(recorded)`
  4) `Play()` 실행 후 애니메이션 재생, `ComputeError()`로 리포트 생성 후 `ErrorReportPresenter.Show(...)`
  - `ReplayTimelineEditor`: 트랙바로 스텝/주석 편집(쿠션 지점·누적 거리) 지원

## 리플레이 비교 화면(실제 샷 vs 추천 경로, 선택 적절성 검토)
- 구성 요소
  - `ReplayCompareScreenController`: 선택한 파라미터(샷 강도/스핀)로 이상 경로 재현 + 실제 기록 경로 비교, 적절성 평가 실행
  - `ErrorReportPresenter`: RMS/최대/최종 오프셋, 쿠션 수, 경로 길이 출력
  - `ChoiceVerdictPresenter`: 선택 적절성(좋은 선택/보수적 추천/공격 권장)과 성공확률 및 선택 vs 최적) 표시
- 사용 예시
  - 샷 직후: `BallTrajectoryRecorder.StopRecording()` 결과를 `ReplayCompareScreenController`가 참조(하드 연결)
  - 사용자가 선택한 파라미터를 전달: `SetChosenParams(angleDeg, speed, spinZ)`
  - `RunAnalysis()` 호출 시 이상/실제 궤적 렌더링, 오차 리포트 + 적절성 verdict 추출
  - 파라미터 미전달 시 현재 배치 기준 최적(PlanShot)으로 비교 실행

## 실시간 대시보드(테이블별 성공률, 평균 TTI)
- 구성 요소
  - `DashboardService`: 샷 결과를 테이블별로 누적 집계(성공률, 평균 RMS 오차, 평균 TTI)
  - `TableTag`: 각 테이블루트에 부착하여 `tableId` 지정
  - `RealTimeDashboardController`: 주기적으로 집계값을 읽어 목록 UI 갱신
  - `RealTimeDashboardItem`: 테이블ID, 성공률(%), 오차값(cm), 평균 TTI(s) 표시
- 데이터 공급
  - 샷 분석 후 `ReplayCompareScreenController.RunAnalysis()`에서 자동 기록
    - 성공 기준: 실제 궤적의 쿠션 수와 목표공 근접(수구 반지름 1배 이내) 달성
    - TTI(Time-To-Impact): 각 쿠션 접점까지의 시간(근사)
- 씬 배치 예시
  - 각 테이블 객체에 `TableTag.tableId` 설정(예: table-1, table-2)
  - 대시보드 화면에 `ScrollView.Content`를 `RealTimeDashboardController.content` 연결, 프리팹으로 `RealTimeDashboardItem` 배치

## 성과 리포트 화면(자동 PDF 생성 + 공유)
- 구성 요소
  - `PerformanceReportScreenController`: PDF 생성 버튼/공유 버튼, 상태 표시
  - `SimplePdf`: 경량 라이브러리로 1페이지 텍스트 PDF 생성(제목/지표 목록)
  - `ShareUtility`: Android 인텐트 공유(카카오톡 포함), iOS/에디터는 파일 열기/폴더 열기 대체
- 리포트 내용
  - 생성 시각, 루틴 세션 수, 누적 샷 수, 테이블별 성공률·평균 RMS 오차(cm)·평균 TTI(s)
- 씬 배치 예시
  - 버튼 2개: `generateButton`, `shareButton`; Text: `statusText`
  - 생성 시 `Application.persistentDataPath/Reports/Report_YYYYMMDD_HHMMSS.pdf` 저장
- 주의
  - Android에서 카카오톡 공유: OS 공유 인텐트로 카카오톡에 전출하며 전송 가능(스마트폰에 설치되어 있어야 함)
  - Android 7+에서 안전한 공유를 위해 FileProvider 설정이 필요(프로젝트별 AndroidManifest/파일프로바이더 설정 권장)

## 체험판 배너 화면(신규 방문자 무료 AI 가이드 체험)
- 구성 요소
  - `TrialBannerController`: 신규 방문자이거나 체험 미사용 사용자에게 배너 표시, "무료 체험 시작"/"나중에" 버튼 제공
  - `TrialManager`: 첫 방문 시각/체험 사용 여부/체험 만료 시각 관리(PlayerPrefs), 자격 상태/남은 시간 확인
- 동작
  - 앱 시작 시 `TrialManager.MarkFirstSeen()` 후 `TrialBannerController.ShowIfEligible()`로 표시 여부 판단
  - "무료 체험 시작" 시 `TrialManager.StartTrialSeconds(N초)` 활성화 후 `onTrialStarted` 이벤트 호출(예: AR 가이드 화면으로 자동)
  - "나중에" 클릭 시 해당 세션에서만 배너 숨김
- 씬 배치 예시
  - Canvas에 배너 패널(제목/본문/버튼 2개 + CanvasGroup) 생성 후 `TrialBannerController` 바인딩
  - `trialMinutes`(기본 10분) 설정, `onTrialStarted`를 가이드 화면 전환 함수와 연결

## 동영상 가져오기 화면(경기 영상 불러오기 + 특정 구간 선택)
- 구성 요소
  - `VideoImportController`: `VideoPlayer`로 파일 경로(URL) 영상 로드, 재생/정지/탐색, 시간 슬라이더/표시
  - `SceneSelectorController`: IN/OUT 마킹, 라벨 입력, 선택 구간 리스트 관리
  - `SceneListItem`: 구간 라벨/시간 범위 표시, 삭제 버튼
- 씬 배치 예시
  - `VideoPlayer` + `RawImage`(영상 출력) 및 `VideoImportController`에 바인딩, 경로 입력 `InputField`와 버튼들 연결
  - `SceneSelectorController`에 `player`/라벨 입력/IN/OUT/추가 버튼, `listContent`와 프리팹으로 `SceneListItem`) 연결
  - 슬라이더에 BeginDrag/EndDrag 이벤트를 `VideoImportController.OnSliderBeginDrag/OnSliderEndDrag`에 연결
- 경로 입력 시
  - 에디터/데스크탑: 절대경로 또는 `StreamingAssets`/`persistentDataPath`의 파일 URL 사용(e.g., `file://...`)
  - 모바일: 사전 배치(StreamingAssets) 또는 인터넷 다운로드 후 `persistentDataPath` 경로 사용 권장

## 공 배치 인식 화면(영상 내 공 위치 자동 인식 + 궤적 추출)
- 보정(테이블 코너 지정)
  - `VideoTableCalibration`: `RawImage` 위에서 테이블 4코너를 마우스로 지정(TL→TR→BR→BL)
  - `VideoTableMapper`: 지정된 코너를 바탕으로 영상→테이블 좌표(미터) 변환 호모그래피
- 추적
  - `VideoBallTracker`: `VideoPlayer` 프레임을 샘플링(기본 10fps) 후 영상 기반 공 인식 후 테이블좌표 궤적(Time 함수) 누적
  - 출력: 영상별 `TimedTrajectoryPoint` 리스트(쿠션 근접 정보 포함)
- UI
  - `VideoBallRecognitionScreenController`: 시작/종료 시간 입력 후 추적 실행 후 영상에 `TimedPathRenderer`로 궤적 표시
- 씬 배치 예시
  - `VideoPlayer`(RenderTexture 출력) + `RawImage`(영상 표시)
  - `VideoTableCalibration`에 `RawImage`/`VideoTableMapper` 바인딩 후 화면에서 4코너 클릭
  - `VideoBallRecognitionScreenController`에 `player`/`mapper`/`tracker` 연결, `whitePath`/`yellowPath`/`redPath` 설정
  - 버튼으로 `OnTrackClicked()` 호출 시 궤적 렌더링
  - 추적 품질: `VideoBallTracker.smoothingAlpha`로 프레임간 위치 EMA 사용
  - (선택) ML 검출: Barracuda 기반 `MLBallDetector` 옵션 포함(모델 연결 시 사용)

## 리플레이 오버레이 화면(실제 궤적 vs 이상 궤적 동기 시각화)
- 구성 요소
  - `ReplayOverlayScreenController`: 영상 위에 실제(추적) 궤적과 이상(추천) 궤적을 표시하는 오버레이
  - `ImagePathRenderer`: `RawImage` 좌표계(0..1 UV)를 사용한 벡터라인 그리기(LineRenderer 기반)
  - `VideoTableMapper`: 테이블좌표(미터) → 영상 UV 변환 호모그래피 (역변환 함수)
- 동작
  - 실제 궤적: `VideoBallTracker.trajectories`의 수구 영상 궤적 사용
  - 이상 궤적: 각 프레임 기준 배치로 `ShotPlanner.PlanShot(cue, red, other)` 계산
  - 양 경로를 `ImagePathRenderer.DrawTablePoints(points, mapper)`로 영상 `RawImage` 위에 렌더링
- 씬 배치 예시
  - `RawImage`(영상), `VideoTableMapper`, `VideoBallTracker` 연결
  - `ReplayOverlayScreenController`에 위 레퍼런스와 `actualRenderer`/`idealRenderer`(각각 `ImagePathRenderer`) 바인딩
  - 버튼 시 `RenderOverlay()` 호출하면 오버레이 자동 표시
  - 주석: `PathAnnotationRenderer`로 쿠션 지점/누적 거리 라벨 표기

## 투명배경 오버레이 파일 내보내기(PNG 시퀀스)
- 구성 요소
  - `OverlayExportController`: 오버레이 전용 카메라로 투명 배경 렌더 후 PNG 시퀀스/단일 PNG 내보내기, ffmpeg 명령 복사(영상 변환)
  - 카메라 설정: Clear Flags=Solid Color, Background RGBA=(0,0,0,0), Culling Mask=오버레이 캔버스 전용
  - 캔버스: Screen Space - Camera(오버레이 카메라 할당)
- 사용 방법
  - 영상의 지정시점부터 fps 입력 후 PNG 시퀀스 내보내기를 실행 시 `persistentDataPath/OverlayExport/overlay_0000.png~`
  - 단일 PNG 내보내기로 현재 오버레이 스냅 가능
  - ffmpeg 명령 복사로 버튼으로 PNG→MP4 변환 명령을 클립보드에 복사(알파 보존을 편집앱에서 PNG 시퀀스 사용 권장)
- 애니메이션 옵션
  - 진행선 애니메이션을 켜면 경로가 시간에 따라 그려지는 시퀀스를 생성(부드러운 렌더)

## 편집 앱 연동 작업(프리미어/캡컷으로 보내기)
- 구성 요소
  - `ExportCompatibilityPopup`: "Premiere로 보내기", "CapCut으로 보내기" 버튼 및 내보내기 폴더 열기 + 가이드 텍스트/파일 생성
- 가이드 내용
  - PNG 시퀀스를 비디오로 가져와서 프레임레이트 맞추고 알파 채널/오버레이 배치/블렌딩 모드

## 완성 영상 공유 화면(SNS 프로필/프리셋 + 워터마크)
- 구성 요소
  - `CompletedVideoShareScreenController`: 프로필별 프리셋 영상(fps/기본 길이) 선택, 워터마크 텍스트/위치 설정, 내보내기/공유 버튼
  - `SharePresets`: Shorts/Reels/TikTok/Feed/Kakao 등 기본 프리셋 제공
  - `WatermarkController`: 오버레이 캔버스에 텍스트 워터마크 배치(4코너, 마진 지정)
- 사용 플로우
  1) 프리셋 선택 시 사전정의 사용자로 `OverlayExportController` 영상 fps/길이 설정
  2) 워터마크 활성화 시 텍스트/위치 선택(옵션)
  3) PNG 시퀀스 내보내기를 하거나 단일 PNG 내보내기를 실행(오버레이 전용 카메라 필요)
  4) MP4가 필요하면 ffmpeg 명령으로 변환하거나 편집앱에서 시퀀스 직접 사용)
  5) 공유: 파일 경로 입력 후 공유인텐트로 OS 공유 인텐트(안드로이드), 데스크탑/에디터는 폴더 열기
- 팁
  - 세로형(1080x1920) 프리셋은 Shorts/Reels/TikTok에 적합, 가로형(1280x720)은 카카오톡 공유에 적합

## 루틴 저장 화면(자동 저장 + 배지 알림)
- 구성 요소
  - `RoutineHistoryStore`: 로컬(PLAYER_PREFS) 세션 기록 로드
  - `RoutineSaveScreenController`: 종료 시 자동 저장(`10분 집중 루틴`), 저장 상태 표시
  - `BadgeSystem`(`BadgeEvaluator`, `BadgeStore`): 10분 이상 완료 시 배지 `focus_10min` 해제
  - `BadgeToast`: 배지 획득 토스트 UI(페이드인 레이아웃)
- 사용 예시
  - 루틴 타이머 종료 시 `RoutineSaveScreenController.AutoSaveFocusRoutine(durationMin)` 호출
  - 첫 10분 달성 시 "10분 집중 루틴 달성" 배지 토스트 출력

## 코치 대시보드(수업생 리스트 + 오늘의 훈련 패턴 템플릿)
- 구성 요소
  - `CoachDashboardController`: 리소스에서 수업생 템플릿을 로드하고 UI 목록에 바인딩
  - `StudentListItem`: 이름/레벨/메모 표시
  - `RoutineTemplateItem`: 템플릿 제목/부제목/메타(태그·시간·레벨)
  - 데이터: `Resources/coach_students.json`, `Resources/routine_templates.json`
- 씬 배치 예시
  - 좌측 패널: `ScrollView`의 `Content`를 `studentContent`에 연결, `studentItemPrefab` 프리팹에 `StudentListItem` 포함
  - 우측 패널: `ScrollView`의 `Content`를 `templateContent`에 연결, `templateItemPrefab` 프리팹에 `RoutineTemplateItem` 포함
  - 진입 시 `CoachDashboardController`의 `Start()`가 자동 `Refresh()` 실행 후 오늘 템플릿/수업생 목록 표시

## 패턴 할당 화면(5개 패턴 선택 후 학생 계정 배포)
- 구성 요소
  - `PatternAssignScreenController`: 학생 선택(드롭다운) + 템플릿 목록(선택 가능) + 배포 버튼
  - `SelectableTemplateItem`: 템플릿을 개별 선택 가능(최대 5개 제한, 실시간 카운터)
  - `AssignmentStore`: 학생별 현재 패턴 5개 저장 + 배포 이력(PlayerPrefs)
- 씬 배치 예시
  - 상단: Dropdown(`studentDropdown`) 및 `coach_students.json` 로드 목록
  - 중앙: `ScrollView.Content`를 `templatesContent` 바인딩, 프리팹으로 `selectableTemplateItemPrefab`
  - 하단: Text(`counterText`), Button(`assignButton`), Text(`statusText`)
  - 버튼 클릭 시 `Assign()` 후 정확히 5개 선택 시 `AssignmentStore.AssignCurrent(studentId, ids)` 저장
- 데이터 경로
  - 학생: `Assets/Resources/coach_students.json`
  - 패턴 템플릿: `Assets/Resources/routine_templates.json`

## AR 가이드 화면(테이블 위에 오버레이 가이드라인)
- 구성 요소
  - `ARGuideOverlay`(LineRenderer): 추천 경로를 테이블좌표(XZ)에서 월드 좌표로 변환해 가이드/쿠션 마커 렌더링
  - `ARGuideScreenController`: 현재 공 배치로 최적 경로 계산 후 `ARGuideOverlay.ShowPlan(...)` 호출
- 씬 배치 예시
  - `ARTableCalibrator`로 테이블 배치 완료 후 `tableRoot` 참조
  - `ARGuideOverlay`를 테이블 아래에 배치하고 LineRenderer 설정(가이드 색상)
  - `ARGuideScreenController`에 `tableRoot`, `cueBall`, `targetBall`, `otherBall`, `overlay` 연결
  - 버튼으로 `ShowGuide()` 호출 시 테이블 위에 가이드 라인이 즉시 오버레이됨