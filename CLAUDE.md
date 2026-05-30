# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this project actually is

Despite the repo/product name `ARFishing`, this is **not a fishing game**. The product is an AR-based marine-ecosystem education app for children aged **7–12** — working title **"Görünenin Ötesinde Bir Deniz" / "Denizin Gizli Yaşamı"**. The `ARFishing` name is legacy; do not infer fishing mechanics, fishing assets, or fishing UX from it.

The intended UX (per the design brief, not yet implemented):

- Classroom / workshop activity, ~35–45 minutes, 5 students per tablet.
- Each group gets a random subset of physical **AR marker cards** (denizanası, mercan, algler, ahtapot, köpekbalığı, etc.).
- Tablet camera scans a card → tracked image triggers a 3D creature model, short audio narration, and an info panel (habitat, diet, threats, adaptation, role in food web).
- End of activity: groups assemble a shared ecosystem map / mini quiz, optionally projected from the teacher's tablet.
- Must work **offline** (no network in many classrooms). Optional Wi-Fi mode adds a teacher projection / shared map.

The user communicates in **Turkish** for design and planning. Match Turkish in design discussions; keep code, identifiers, comments, and commit messages in English.

## Project shape

Unity **6000.4.9f1** project scaffolded from Unity's **AR Mobile** template. As of this writing, the codebase is the unmodified template — no education-specific gameplay, scripts, creature data, or AR image-tracking setup exists yet. Treat current contents as scaffold to build *on top of*, not as architecture.

Default application identifiers are still the template's placeholders (`com.unity.template.ar_mobile` / `.ar-mobile` / `.armobile` in `ProjectSettings/ProjectSettings.asset`). Replace before any real build/distribution.

## Build, run, test

No shell-driven build/lint/test setup exists — no `package.json`, no test asmdefs, no CI. Everything runs through the Unity Editor.

- **Open the project**: open `D:\Projects\Unity\ARFishing` with Unity **6000.4.9f1** exactly. A different patch will silently re-import and may shift package versions.
- **Run in editor**: open `Assets/ARFishing/Scenes/Bootstrap.unity` (scene 0 in `EditorBuildSettings`) and press Play. Bootstrap loads `Activity.unity` (scene 1) after init. The legacy `Assets/Scenes/SampleScene.unity` is kept disabled in build settings for reference only — open it separately if you need to inspect template UI. The editor uses the **XR Simulation** loader (`Assets/XR/Loaders/SimulationLoader.asset`). To test image tracking in editor, configure XR Simulation environment with a printed-card image — there is no fallback for marker scanning without it.
- **Build for device**:
  - **Android (ARCore)**: switch platform to Android, ensure `AR Core Loader` is enabled in XR Plug-in Management, then File → Build Profiles → Build.
  - **iOS (ARKit)**: switch to iOS, ensure `AR Kit Loader` is enabled, build Xcode project, archive from Xcode.
  - No headless / CLI build pipeline is configured.
- **Tests**: no Unity Test Framework asmdefs are present.

## Rendering / input pipelines

- **URP 17.4.0** is the active render pipeline. Runtime asset is `Assets/Settings/URP-Performant.asset` — mobile-tuned. Keep new shaders/materials URP-compatible.
- **Input System 1.19.0** is the active input backend (legacy `UnityEngine.Input` is off).
- **XR Interaction Toolkit 3.4.1**. The version-3.3.0 sample assets under `Assets/Samples/XR Interaction Toolkit/3.3.0/...` ship the prefabs and input actions the scene references — **don't delete them** even though they look like example content.

## Code layout that matters today

Three first-party folders; everything else under `Assets/` is package samples or generated settings.

- `Assets/ARFishing/` — **the education app code, owned by us**. Two asmdefs: `ARFishing.asmdef` (runtime, root namespace `ARFishing`) and `ARFishing.Editor.asmdef` (Editor-only, references `ARFishing`). Subfolders:
  - `Scripts/Core/` — `ActivityState` (FSM enum), `ServiceLocator` (Type→instance registry), `ActivityController` (FSM, emits `StateChanged(prev, next)` and guards illegal transitions in `IsLegal`), `AccessibilitySettings` (**F10**: ScriptableObject with `ReducedMotion` / `NarrationCaptions` / `UiScaleMultiplier`), `AccessibilityState` (**F10**: static accessor + `Changed` event, populated by ContentBootstrapper). All under `ARFishing.Core`.
  - `Scripts/Creatures/` — `CreatureCategory`, `Habitat`, `DietType`, `EcosystemRole` enums; `CreatureLabels` (extension methods `ToTurkish` / `ToEnglish` / `ToArabic` per enum + locale-aware `Localize(this enum, Locale)` and `Localize(this enum)` that reads `Localizer.Active` — **F10**: AR stubs currently return TR fallback, replace with native-reviewed translations before AR release); `CreatureDefinition` (ScriptableObject with `[CreateAssetMenu]` at `ARFishing/Creature Definition`); `CreatureDatabase` (SO with `[CreateAssetMenu]` at `ARFishing/Creature Database`, lazy `Dictionary<string, CreatureDefinition>` lookup, `TryGet` / `GetOrNull` / `All`). All under `ARFishing.Creatures`.
  - `Scripts/Localization/` (**F10**) — `Locale` enum (Turkish default, English, Arabic), `LocalizationTable` ScriptableObject (key→[TR, EN, AR] entry list with lazy `Dictionary<string, Entry>` lookup, `TryGet` / `Get` with TR fallback), `Localizer` static (active locale + table accessors, `Get(key, fallback)`, `LocaleChanged` event). Wire via `ContentBootstrapper` inspector references. UI code should call `Localizer.Get("ui.button.start_activity")` or `creature.Habitat.Localize()` rather than hardcoding TR strings — current panels still use direct TR for F1–F9.1 simplicity; i18n migration is incremental. Under `ARFishing.Localization`.
  - `Scripts/Quiz/` — `MatchRule` enum + `TaskDefinition` SO (`[CreateAssetMenu]` at `ARFishing/Task Definition`) with `Matches(CreatureDefinition)` evaluator, `TaskDatabase` SO (mirror of `CreatureDatabase`), and `QuizController` (drives the Quiz state: picks N random tasks from `TaskDatabase`, subscribes to `MarkerTracker.Spotted` for answer scans, evaluates via `task.Matches(scanned)`, accumulates score, transitions to `Summary` when done; toggles `CreatureViewer.ShowModels = false` during Quiz so the answer prefab doesn't reveal itself). Under `ARFishing.Quiz`.
  - `Scripts/Marker/` — `MarkerTracker` (wraps `ARTrackedImageManager.trackablesChanged` UnityEvent, resolves `referenceImage.name → CreatureDefinition` via `CreatureDatabase`, emits typed `Spotted/Updated/Gone` events), `FocusResolver` (tracks every spotted image but exposes a single `Focused` creature + `FocusChanged(prev, next)` event; new tracking claims focus, gone-focus hands off to next tracking image), `MarkerStateBridge` (translates `FocusChanged` to `ActivityController.TryTransition(Scanning ↔ Viewing)`). Under `ARFishing.Marker`.
  - `Scripts/Viewer/` — `CreatureViewer`: subscribes to `MarkerTracker`, instantiates `CreatureDefinition.ModelPrefab` as child of `ARTrackedImage.transform`, scales to `ARTrackedImage.size.x` (fallback `m_FallbackCardWidth = 0.105m`), hides during `TrackingState.Limited`, destroys on `Gone`. Has `ShowModels` boolean — Quiz mode (F5) sets `false` so answer model doesn't reveal itself before scoring. Under `ARFishing.Viewer`.
  - `Scripts/Content/` — `ContentBootstrapper`: lives in `Bootstrap.unity`, takes `[SerializeField] CreatureDatabase` (required), `[SerializeField] TaskDatabase` (optional but needed for Quiz), `[SerializeField] AccessibilitySettings` (**F10** optional — if null, defaults to no reduced motion / no captions), `[SerializeField] LocalizationTable` (**F10** optional — if null, UI strings are hardcoded TR), `[SerializeField] Locale m_DefaultLocale = Locale.Turkish` (**F10**). Registers DBs in `ServiceLocator`, applies `AccessibilityState.Current` + `Localizer.Table` + `Localizer.Active`, async-loads Activity scene (`LoadSceneMode.Single`), finds `ActivityController`, calls `TryTransition(Idle)`, then `Destroy(gameObject)`. Marked `DontDestroyOnLoad` so it survives the scene swap. Under `ARFishing.Content`.
  - `Scripts/Narration/` — `NarrationPlayer`: `[RequireComponent(AudioSource)]`, subscribes to `FocusResolver.FocusChanged`, plays `CreatureDefinition.NarrationClip` on new focus, interrupts on focus change (does NOT fire `NarrationFinished` — only natural-end does), exposes `Play(def)` / `Replay()` / `Stop()` + `NarrationStarted` / `NarrationFinished` events. `SfxPlayer` (**F9.1**): separate `[RequireComponent(AudioSource)]` service playing short feedback clips via `PlayOneShot`. Subscribes to `QuizController.AnswerEvaluated` (correct → `m_CorrectAnswerClip`, !correct → `m_IncorrectAnswerClip`) and `MarkerTracker.Spotted` (→ `m_CardSpottedClip`). Per-clip volume sliders; default scan clip volume lowered to 0.55 so it doesn't drown out narration that fires immediately after on the same Spotted. Both under `ARFishing.Narration`. Quiz controller listens to `NarrationFinished` for "is it OK to advance to next task" cues.
  - `Scripts/UI/` — `InfoPanelController` (creature info; subscribes to `FocusResolver.FocusChanged` AND `ActivityController.StateChanged` so it only shows in `Scanning`/`Viewing` states; **F8**: on focus→null, defers hide by `m_PassiveCloseSeconds = 2s` so brief tracking interruptions don't flicker the panel — coroutine cancelled if new focus arrives during the wait or if state changes mid-defer), `QuizPanelController` (Quiz state UI: `ShowPrompt(task, index, total)` + `ShowFeedback(correct, task, scanned)` API, **F9**: optional `[SerializeField] TextPopAnimator m_FeedbackPop` pops the feedback text on reveal), `SummaryPanelController` (Summary state UI: subscribes to `StateChanged`, reads `QuizController.RunningScore`, has Restart button → `Summary → Idle`; **F9**: optional `[SerializeField] ScoreTextAnimator m_ScoreAnimator` count-up animates score from 0 to final on ShowSummary, falls back to instant text if null), `IdlePanelController` (**F8**: tablet welcome screen shown only in Idle state — children see "öğretmenin etkinliği başlatmasını bekle" instead of blank camera; auto-fills title + subtitle if Text fields are left empty in Inspector), `PanelTween` (**F8**: optional utility for slide+fade in/out via coroutine + EaseOutCubic; each panel controller has an optional `[SerializeField] PanelTween m_Tween` — if assigned, replaces snap show/hide; if null, panels fall back to instant SetActive + CanvasGroup alpha; **F10**: respects `AccessibilityState.ReducedMotion` — if true, Show/Hide both apply instantly without animation), `IdleBackgroundAnimator` (**F9**: spawns N decorative bubble GameObjects inside the IdlePanel — Image + CanvasGroup children animated drifting upward with sin-curve lateral drift, fade by vertical position, wrap at top; idle screen turns from blank text into an ambient seascape; **F10**: respects `AccessibilityState.ReducedMotion` — if true, skips both spawn and Update tick so the panel stays static), `CaptionOverlayController` (**F10**: subscribes to `NarrationPlayer.NarrationStarted/Finished` and shows the focused creature's `InterestingTrait` as a text overlay during narration — only when `AccessibilityState.NarrationCaptions` is true; useful for hearing-impaired children, noisy classrooms, or supplementing the audio with on-screen text), `ScoreTextAnimator` (**F9**: `Animate(from, to)` lerps integer score with EaseOutCubic over `m_Duration` and writes via format string — only re-applies text on integer change to avoid flicker), `TextPopAnimator` (**F9**: `Pop()` scales the target RectTransform `m_StartScale → m_OvershootScale → 1.0` over `m_Duration` for bounce-in effect on quiz feedback). Under `ARFishing.UI`. **Note on Close button**: `InfoPanelController.Close` transitions `Viewing → Scanning` and hides panel, but does NOT clear `FocusResolver.Focused`. The card is still tracked; user has to physically lift it for focus to drop. If user puts the card back down, `Spotted` fires fresh and the panel re-shows.
  - `Scripts/Teacher/` — `TeacherToggleButton` (IPointerDownHandler / Up / Exit + Update tick; long-press ≥1s triggers `TeacherPanel.Toggle()`; deliberate gate so children don't open it accidentally — sits on the main tablet Canvas at a corner with an Image graphic for raycast hits), `TeacherPanelController` (state-aware: `StartActivityButton` visible only in Idle, `StartQuizButton` only in Scanning/Viewing, `RestartButton` in any non-Idle non-Bootstrap; Close button hides panel without changing state; also accepts optional `PanelTween`), `SessionScannedTracker` (subscribes to `MarkerTracker.Spotted` + `ActivityController.StateChanged`; dedupes by `CreatureDefinition.CreatureId`, emits `ScannedAdded`/`SessionReset`; auto-resets on entry to Idle from a non-Bootstrap state — i.e. restart, not initial bootstrap), `ProjectionCanvasRouter` (checks `Display.displays.Length`; if external display present, calls `Display.displays[1].Activate()` and sets `Canvas.targetDisplay = 1`; if absent, hides the canvas; **F8**: re-checks every `m_RecheckInterval = 2s` in `Update()` so mid-session HDMI plug/unplug re-routes without app restart — set interval to 0 to disable), `ProjectionPanelController` (class-facing content: Scanning/Viewing → large focused-creature icon + name + category; Quiz → mirrors `QuizPanelController` prompt + progress via `QuizController.TaskPresented` event; Summary → score + grid of scanned creatures from `SessionScannedTracker`; Idle → optional idle splash). Under `ARFishing.Teacher`.
  - `Scripts/Editor/` — `CreatureValidationTool` (menu `ARFishing → Validate All Creature Definitions`: kebab-case ID, uniqueness, required enum/prefab/clip fields, ≤25s narration cap), `CreateExampleContentMenu` (menu `ARFishing → Create Example Content`: minimal 2-creature scaffold), `CreateMvpContentMenu` (menu `ARFishing → Create MVP Content (20 creatures + 8 tasks)`: full MVP content set — 20 creatures + 8 tasks + both databases, drives F7), `CreatePlaceholderModelsMenu` (menu `ARFishing → Create Placeholder Models (skip existing)`: iterates every `CreatureDefinition` in the project, generates a category-shaped URP/Lit-tinted primitive prefab per creature with deterministic HSV color from CreatureId hash, **skips creatures that already have a ModelPrefab** so real art is never overwritten), `CreatePlaceholderAudioMenu` (menu `ARFishing → Create Placeholder Audio (skip existing)`: iterates every `CreatureDefinition`, generates a 1.5s sine tone WAV with frequency picked deterministically from CreatureId hash across 320–880 Hz, **skips creatures that already have a NarrationClip**), `CreatePlaceholderIconsMenu` (**F8**: menu `ARFishing → Create Placeholder Icons (skip existing)`: iterates every `CreatureDefinition`, writes a 256×256 transparent PNG with category-tinted rounded-square + HSV-hashed center color per creature, sets Sprite import settings (PPU=256, alphaIsTransparency), **skips creatures that already have an Icon**), `CreateUISpritesMenu` (**F9**: menu `ARFishing → Create UI Sprites`: writes a 128×128 soft-edged white circle PNG at `Content/UI/Sprites/bubble.png` for `IdleBackgroundAnimator` to use as the bubble sprite; idempotent — re-runs only re-apply Sprite import settings), `CreatePlaceholderSfxMenu` (**F9.1**: menu `ARFishing → Create Placeholder SFX (skip existing)`: writes three sine-arpeggio WAVs in `Content/Audio/Sfx/` — `sfx_correct.wav` C5-E5-G5 ascending ~0.36s, `sfx_incorrect.wav` A4-F4 descending ~0.3s, `sfx_scan.wav` D6 single tone ~0.08s; skips files that already exist), and `CreateExampleTasksMenu` (menu `ARFishing → Create Example Tasks`: standalone 5-task scaffold, superseded by `Create MVP Content` for the full 8-task set).
  - `Content/` — created on demand by editor menus. Holds creature `.asset` files, `CreatureDatabase.asset`, placeholder material/prefab subfolders, and (added manually) the `XRReferenceImageLibrary` + audio clips + real models.
  - `Scenes/Bootstrap.unity` — scene 0. Needs one GameObject with `ContentBootstrapper`; assign `CreatureDatabase` in its inspector. Not auto-populated (script GUIDs are Unity-assigned, so wiring happens in Editor — see "Scene wiring" below).
  - `Scenes/Activity.unity` — scene 1. Needs XR Origin (AR Rig) prefab, `ActivityController` + `MarkerTracker` + `FocusResolver` + `MarkerStateBridge` + `CreatureViewer` + `NarrationPlayer` GameObjects, an `XRReferenceImageLibrary` referenced by the `ARTrackedImageManager`, and a UI Canvas containing the InfoPanel hierarchy. See "Scene wiring" below.
- `Assets/MobileARTemplateAssets/Scripts/` — namespace `UnityEngine.XR.Templates.AR`. **No asmdef**, so these compile into the default `Assembly-CSharp`. Three files:
  - `ARTemplateMenuManager.cs` — UI menu for spawning/deleting generic shape prefabs (cube, pyramid, torus...). **Reference only — not part of the education app.**
  - `GoalManager.cs` — onboarding state machine for the *plane-detection / object-placement* flow. **Reference only.** The education app uses a card-scanning flow, not plane placement, so this won't be reused as-is.
  - `ARPlaneMeshVisualizerFader.cs` — fades AR plane mesh alpha via XRI tweenable primitives. May be reusable.
- `Assets/Samples/XR Interaction Toolkit/3.3.0/` — XRI sample assets with their own asmdefs (`StarterAssets`, `ARStarterAssets`, `*.Editor` variants). Both the template scripts and `ARFishing.asmdef` depend on types from `StarterAssets` / `ARStarterAssets`.

## Where new code should go

The design brief defines seven modules: **AR Marker System, Creature Database, Audio Narration, Creature Viewer, Quiz/Task System, Teacher Mode, Offline Content Manager**. Target layout (subfolders are added as each faz lands; `Scripts/Core/` exists today):

```
Assets/ARFishing/
  Scripts/
    ARFishing.asmdef           # references Unity.XR.ARFoundation/ARSubsystems/CoreUtils,
                               #            Unity.XR.Interaction.Toolkit, StarterAssets,
                               #            ARStarterAssets, Unity.InputSystem, Unity.TextMeshPro
    Core/                      # ActivityState, ServiceLocator, ActivityController, AccessibilitySettings, AccessibilityState  (F1+F10, done)
    Localization/              # Locale enum, LocalizationTable SO, Localizer static  (F10, done)
    Creatures/                 # enums, CreatureLabels, CreatureDefinition, CreatureDatabase  (F2, done)
    Quiz/                      # MatchRule, TaskDefinition, TaskDatabase, QuizController (F2/F5, done)
    Editor/                    # ARFishing.Editor.asmdef, validator, example menu  (F2/F3, done)
    Marker/                    # MarkerTracker, FocusResolver, MarkerStateBridge    (F3, done)
    Viewer/                    # CreatureViewer                                     (F3, done)
    Content/                   # ContentBootstrapper                                (F3, done)
    Narration/                 # NarrationPlayer (single AudioSource)               (F4, done)
    UI/                        # InfoPanel, QuizPanel, SummaryPanel                 (F4/F5, done)
    Teacher/                   # TeacherToggle/Panel, SessionTracker, Projection*   (F6, done)
  Content/                     # CreatureDatabase + CreatureDefinition assets       (F2, scaffolded)
                               # Models/ and Materials/ (F3 placeholder, scaffolded)
                               # Audio/ (F4 placeholder, scaffolded)
                               # Tasks/ + TaskDatabase.asset (F5, scaffolded)
  Scenes/
    Bootstrap.unity            # scene 0  (F1, done)
    Activity.unity             # scene 1  (F1, done — XR rig added in F3)
```

Guidelines:
- **Use `ARFishing.asmdef`** for all education-app code. Do not add new code to `Assembly-CSharp`. The asmdef already references `StarterAssets` and `ARStarterAssets` because the XR rig prefabs need those types.
- **AR marker scanning** uses AR Foundation 6.x `ARTrackedImageManager` + an `XRReferenceImageLibrary`. Each card becomes a reference image; each reference image's `name` maps to a `CreatureDefinition` (ScriptableObject) by stable ID. Do not use QR codes or Vuforia.
- **Concurrent cards**: tracker supports multiple tracked images simultaneously (each gets its 3D model), but only **one creature is "focused"** at a time — focus owns the info panel and narration. `FocusResolver` (F3) picks the focused image (last-tracked or nearest-to-center heuristic).
- **Offline-first**: ship all 3D models, audio clips, and creature metadata inside the build (direct SO references; no Addressables in MVP, no StreamingAssets streaming). No runtime downloads anywhere in MVP.
- The AR rig, AR Session, and plane/anchor managers come from the `ARStarterAssets` sample prefabs. The education `Activity.unity` scene will host its own copy of the XR Origin (AR Rig) prefab — don't reinvent it.
- Use AR Foundation **6.x** APIs (`ARRaycastManager.Raycast`, `ARAnchorManager.TryAddAnchorAsync`, `ARTrackedImageManager.trackablesChanged`). The 5.x surface (`ARSessionOrigin`, the old `trackedImagesChanged` event signature) is gone.

## Privacy stance — zero data collection

This product targets children aged 7–12 in classrooms. The stance is **collect nothing, send nothing**:

- **No analytics, no telemetry, no crash reporting.** `com.unity.modules.unityanalytics` removed from `Packages/manifest.json`; `UnityConnectSettings.asset` has Analytics/Ads/Purchasing/PerformanceReporting/CrashReporting all disabled (`m_Enabled: 0`) and `InsightsSettings.m_EngineDiagnosticsEnabled: 0`.
- **No outbound HTTP** anywhere in the runtime. Do not add Firebase, Sentry, GameAnalytics, AppsFlyer, or any third-party SDK that phones home.
- **Local-only state**: `PlayerPrefs` is allowed for trivial local prefs (e.g. last group name, last score). Nothing leaves the device.
- **Android manifest**: when configuring Android Player Settings, set **Internet Access = Auto** (not Require). AR Foundation does not need `INTERNET` permission. If the merged manifest still requests it from a transitive dependency, override it with `<uses-permission android:name="android.permission.INTERNET" tools:node="remove" />` in a custom `AndroidManifest.xml` so the store listing can honestly say "no network".
- **No Unity Cloud / DevOps integration.** Do not connect the project to a Unity organization or enable Cloud Build's data collection paths.

If you add a feature that needs the network, surface that as a blocking decision — the privacy stance is part of the product, not a default that can be silently relaxed.

## Content authoring conventions

Decisions baked into the data layer — change these here AND in code if you ever revisit them:

- **English enum identifiers, Turkish display labels.** Enums (`CreatureCategory`, `Habitat`, `DietType`, `EcosystemRole`, `MatchRule`) use English PascalCase members. UI never shows the enum name directly — it goes through `CreatureLabels.ToTurkish(this Enum)` extension methods. When adding a new enum member, add its Turkish label in `CreatureLabels.cs` in the same commit.
  - `CreatureCategory`: Fish, Invertebrate, Producer, Coral, DeepSea, Endangered, Mammal, Reptile. The last two were added in F7 (dolphin = Mammal, sea-turtle = Reptile) to avoid forcing them into Fish.
  - `DietType`: Carnivore, Herbivore, Omnivore, FilterFeeder, Photosynthesizer. Photosynthesizer added in F7 for algae / seagrass / plankton — design brief lists them as producers and no prior diet value fit.
- **CreatureId is kebab-case.** Stable, ASCII, used as both the dictionary key in `CreatureDatabase` and the `XRReferenceImageLibrary` entry `name`. Validator enforces `^[a-z0-9]+(-[a-z0-9]+)*$`. Do not rename a `CreatureId` once content references it — treat it like a primary key.
- **ReferenceImageName defaults to CreatureId.** `CreatureDefinition.OnValidate` auto-fills it when blank. Only override if a card image is shared across multiple creatures (rare; F2 design assumes 1:1).
- **All `Unknown` enum values are validation failures.** They exist so newly-created assets surface as "not yet filled in" rather than silently defaulting to (say) `Fish`. Validator flags `Unknown` as an error.
- **Narration clips ≤ 25 seconds.** Hard cap in `CreatureValidationTool`. Children lose focus past that.
- **Example content workflow.** Two scaffolds exist:
  - `ARFishing → Create Example Content` — minimal 2-creature scaffold (`octopus`, `moon-jellyfish`) + matching `CreatureDatabase`. Useful for iterating on the data model itself.
  - `ARFishing → Create MVP Content (20 creatures + 8 tasks)` — full MVP content set. Creates / refreshes all 20 `CreatureDefinition` assets, the `CreatureDatabase` referencing all 20, and 8 `TaskDefinition` assets + `TaskDatabase`. Re-running refills metadata from code, so manual edits to creature metadata in those assets WILL be overwritten — author overrides in code or fork a new asset under a different path.
- **Adding a creature to the database is manual.** `CreateExampleContentMenu` only handles the two examples. New `CreatureDefinition` assets must be dragged into `CreatureDatabase.m_Creatures` (or its inspector list) by hand. Run `ARFishing → Validate All Creature Definitions` after to confirm IDs are unique and fields are populated.
- **TaskDefinition `MatchValue` is the enum name, not the label.** A `HabitatEquals` task with `MatchValue = "Reef"` matches the `Habitat.Reef` enum — case-insensitive. The Turkish label "Mercan Resifi" will NOT match. Author the value field with the English enum name.

## Scene wiring (F3, manual one-time setup in Editor)

The scene files in `Assets/ARFishing/Scenes/` are intentionally minimal — script GUIDs are Unity-assigned at import, so we can't author component references in the YAML by hand. After the project opens in Unity 6000.4.9f1 once and scripts compile, do this manual wiring **once**, then commit the saved scene files.

### Bootstrap.unity

1. Open `Assets/ARFishing/Scenes/Bootstrap.unity`.
2. Create an empty GameObject named `ContentBootstrapper`.
3. Add the `ContentBootstrapper` component to it.
4. Drag `Assets/ARFishing/Content/CreatureDatabase.asset` into its `Database` field. (Run `ARFishing → Create Example Content` first if it doesn't exist.)
5. Leave `Activity Scene Name` = `Activity`.
6. Save the scene.

### Activity.unity

1. Open `Assets/ARFishing/Scenes/Activity.unity`.
2. Delete any default Main Camera (the XR Origin brings its own).
3. From `Assets/Samples/XR Interaction Toolkit/3.3.0/AR Starter Assets/Prefabs/`, drag the **XR Origin (AR Rig)** prefab into the scene. This brings AR Session, AR Camera, Input Action references, etc.
4. From the same Prefabs folder (or via GameObject menu), add an **AR Session** GameObject if the rig prefab doesn't include one. (Most ARStarterAssets rig prefabs include the session as a sibling — check first.)
5. On the **XR Origin (AR Rig)** root, add an `ARTrackedImageManager` component. Assign:
   - `Serialized Library` = the project's `XRReferenceImageLibrary` asset (see "Reference image library" below).
   - `Max Number Of Moving Images` = `4` (supports several cards on the table at once).
   - `Tracked Image Prefab` = leave **empty**. `CreatureViewer` spawns models manually so we control parenting and scale.
6. Create an empty GameObject named `Services` at the scene root, then create child GameObjects:
   - `ActivityController` → add `ActivityController` script.
   - `MarkerTracker` → add `MarkerTracker` script + an `ARTrackedImageManager` reference is required by `[RequireComponent]`; instead, put `MarkerTracker` on the **same GameObject as the existing `ARTrackedImageManager`** (the XR Origin root). Drag the XR Origin's `ARTrackedImageManager` into `MarkerTracker.m_Manager`, and drag `CreatureDatabase.asset` into `MarkerTracker.m_Database`.
   - `FocusResolver` → add `FocusResolver` script. Drag the `MarkerTracker` reference into its `m_Tracker` field (or rely on `ServiceLocator` resolution at `Awake`).
   - `MarkerStateBridge` → add the script. Drag in `FocusResolver` + `ActivityController` references.
   - `CreatureViewer` → add the script. Drag in the `MarkerTracker` reference.
7. Save the scene.

`Services` GameObject is just for hierarchy organization — none of these scripts need to share a Transform with the XR rig.

### Reference image library

1. Right-click in `Assets/ARFishing/Content/` → `Create → XR → Reference Image Library`. Name it `CardLibrary.asset`.
2. Add two entries: `octopus` and `moon-jellyfish`. Names **must match** the `ReferenceImageName` on the corresponding `CreatureDefinition` (which defaults from `CreatureId`).
3. For each entry, drag in a placeholder image texture (any high-contrast asymmetric image works for editor XR Simulation testing). For production, these are scans of the actual A6 card fronts.
4. For each entry, check **Specify Size** and set physical width to **0.105 m** (A6 short edge — see "Physical card specs" below).
5. Drag this `CardLibrary.asset` into the `ARTrackedImageManager.Serialized Library` field.

### Bootstrap.unity — F5 additions

In the `ContentBootstrapper` inspector, drag `Assets/ARFishing/Content/TaskDatabase.asset` into the new `Task Database` field. The CreatureDatabase field remains required; TaskDatabase is optional (Quiz state will warn and skip straight to Summary if missing).

### Activity.unity — F4 additions

After completing the F3 scene wiring above, add these to the same scene:

1. Under the `Services` GameObject, create a child `NarrationPlayer`:
   - Add an `AudioSource` component (the `NarrationPlayer` script's `[RequireComponent]` will add it automatically when you attach the script).
   - Add the `NarrationPlayer` script. Its `m_FocusResolver` and `m_AudioSource` will auto-wire via `Awake` (`ServiceLocator` + `GetComponent`), but for clarity drag the `FocusResolver` reference in too.
2. Create a UI Canvas at the scene root:
   - GameObject menu → `UI → Canvas`. Set render mode = `Screen Space - Overlay`, CanvasScaler `UI Scale Mode = Scale With Screen Size`, reference resolution = `1080 × 1920` (portrait), match mode = 0.5.
   - This also adds an `EventSystem` GameObject if none exists. Without an EventSystem, Button clicks won't register.
3. Inside the Canvas, build the InfoPanel hierarchy:
   ```
   Canvas
     InfoPanel                     (RectTransform, Image bg, CanvasGroup)
       ├── Icon                    (Image)
       ├── DisplayName             (Text)
       ├── Category                (Text)
       ├── Habitat                 (Text)
       ├── Diet                    (Text)
       ├── EcosystemRole           (Text)
       ├── InterestingTrait        (Text)
       ├── Threats                 (Text)
       ├── ReplayButton            (Button + child Text "Sesi tekrar dinle")
       └── CloseButton             (Button + child Text "Kapat")
   ```
   Layout details (margins, fonts, colors) are placeholder for F4 — anything readable works.
4. On the InfoPanel root, add an `InfoPanelController` script. Drag in:
   - `m_FocusResolver` ← FocusResolver GameObject
   - `m_NarrationPlayer` ← NarrationPlayer GameObject
   - `m_ActivityController` ← ActivityController GameObject
   - `m_PanelRoot` ← the InfoPanel GameObject itself (so the script can `SetActive(false)`)
   - `m_PanelCanvasGroup` ← the CanvasGroup component on the InfoPanel
   - Each text/image/button field ← the corresponding child component
5. Save the scene.

After saving, the panel should start hidden on Play and slide in (well, snap in — F4 has no animation yet) whenever `FocusResolver.Focused` becomes non-null **and** the state is Scanning/Viewing (F5 made this state-aware).

### Activity.unity — F5 additions

Add these to the same Canvas built in the F4 step:

1. **QuizPanel** hierarchy (state: Quiz):
   ```
   Canvas
     QuizPanel                       (RectTransform, Image bg, CanvasGroup)
       ├── PromptText                (Text, large body)
       ├── ProgressText              (Text, e.g. "1 / 3")
       └── FeedbackRoot              (GameObject; starts disabled)
            └── FeedbackText         (Text)
   ```
   Add `QuizPanelController` to the QuizPanel root. Wire `m_PanelRoot` to the QuizPanel itself, `m_PanelCanvasGroup` to its CanvasGroup, `m_PromptText`, `m_ProgressText`, `m_FeedbackRoot`, `m_FeedbackText` to the corresponding children.

2. **SummaryPanel** hierarchy (state: Summary):
   ```
   Canvas
     SummaryPanel                    (RectTransform, Image bg, CanvasGroup)
       ├── ScoreText                 (Text, e.g. "Toplam puan: 5")
       ├── ClosingPromptText         (Text — leave empty; controller fills with default closing question)
       └── RestartButton             (Button + child Text "Yeniden başlat")
   ```
   Add `SummaryPanelController` to the SummaryPanel root. Wire `m_Controller` ← ActivityController, `m_Quiz` ← QuizController (added in step 3), plus the panel/text/button references.

3. **QuizController service GameObject** (under `Services`):
   - Create `Services/QuizController` and add the `QuizController` script.
   - Wire: `m_Controller` ← ActivityController; `m_Tracker` ← MarkerTracker; `m_Viewer` ← CreatureViewer; `m_Narration` ← NarrationPlayer; `m_Panel` ← QuizPanel's QuizPanelController; `m_TaskDatabase` ← `Assets/ARFishing/Content/TaskDatabase.asset`.

4. Save the scene.

> If you previously wired the F5 `StartQuizButton`, delete it — it was a temporary child-facing button and the F6 Teacher Panel replaces it. The `StartQuizButton.cs` script has been removed from the codebase.

### Activity.unity — F6 additions

Two new UI areas: the tablet-side Teacher Panel (inputs) and the optional ProjectionCanvas (display-only, second display only).

**1. SessionScannedTracker service** (under `Services`):
- Create `Services/SessionScannedTracker` and add the script. Wire `m_Tracker` ← MarkerTracker and `m_Controller` ← ActivityController.

**2. Teacher Panel on the tablet Canvas**:
   ```
   Canvas (existing, display 0)
     TeacherToggleButton             (Image, anchored top-right corner, ~64px square,
                                      semi-transparent or barely visible — must have Image
                                      with raycastTarget=true for pointer events to fire;
                                      do NOT make it a Button)
     TeacherPanel                    (RectTransform, Image bg, CanvasGroup; starts hidden)
       ├── StartActivityButton       (Button + child Text "Etkinliği başlat")
       ├── StartQuizButton           (Button + child Text "Mini quiz başlat")
       ├── RestartButton             (Button + child Text "Yeniden başlat")
       └── CloseButton               (Button + child Text "Kapat")
   ```
   - Add `TeacherToggleButton` script to the TeacherToggleButton GameObject. Drag the TeacherPanel's `TeacherPanelController` into `m_Panel`. Leave `m_HoldDuration = 1`.
   - Add `TeacherPanelController` to the TeacherPanel root. Wire `m_Controller` ← ActivityController; `m_PanelRoot` ← TeacherPanel itself; `m_CanvasGroup` ← TeacherPanel's CanvasGroup; the four buttons ← corresponding children.

**3. Optional ProjectionCanvas on display 1**:
   ```
   ProjectionCanvas                  (root Canvas, render mode = Screen Space - Overlay,
                                      separate from the tablet Canvas; CanvasScaler same
                                      reference resolution; targetDisplay irrelevant —
                                      ProjectionCanvasRouter sets it at runtime)
     IdleSection                     (GameObject; e.g. "Etkinliği başlatmak için tableti hazırla")
     ScanningSection                 (GameObject)
       ├── LargeIcon                 (Image)
       ├── LargeName                 (Text, big)
       └── LargeCategory             (Text)
     QuizSection                     (GameObject)
       ├── QuizPromptText            (Text, big)
       └── QuizProgressText          (Text)
     SummarySection                  (GameObject)
       ├── SummaryScoreText          (Text)
       └── ScannedGridParent         (GridLayoutGroup parent for instantiated chips)
   ```
   - Add `ProjectionCanvasRouter` to the ProjectionCanvas root. It auto-detects `Display.displays.Length > 1`; if no external display, the canvas hides entirely. Leave `m_PreferredDisplay = 1`.
   - Add `ProjectionPanelController` to the ProjectionCanvas root. Wire all section references + per-section text/image fields. Wire `m_QuizPanelSource` ← QuizPanelController (used to mirror the prompt text). Wire `m_SessionTracker` ← SessionScannedTracker.
   - Create a small **ScannedChipPrefab** (Image + Text under a horizontal layout, ~80×80 px) anywhere under `Assets/ARFishing/Content/UI/`; drag it into `m_ScannedChipPrefab`. The controller instantiates one per scanned creature in Summary.

**4. Save the scene.**

### Activity.unity — F8 additions

**Panel slide animations (optional)**:
For each of the four panels (`InfoPanel`, `QuizPanel`, `SummaryPanel`, `TeacherPanel`):
1. Add a `PanelTween` component on the panel root GameObject (same GO as the existing `CanvasGroup`).
2. In the panel's controller (e.g. `InfoPanelController`), drag the `PanelTween` into its new `m_Tween` field.
3. Adjust `m_Duration` (~0.28s default, EaseOutCubic) and `m_SlideOffset` (px applied to anchored Y while hidden; negative slides down off-screen). The script captures the initial RectTransform position as the "visible" position, so design the panel for its on-screen layout and let `SlideOffset` define the hidden state.

Without `PanelTween` assigned, panels fall back to instant `SetActive` + `CanvasGroup` snap (existing behavior). Animations are pure visual polish and never block state logic.

**IdlePanel on the tablet Canvas**:
```
Canvas (display 0, tablet)
  IdlePanel                       (RectTransform, Image bg, CanvasGroup; starts hidden)
    ├── TitleText                 (Text, large — leave empty for default "Görünenin Ötesinde Bir Deniz")
    └── SubtitleText              (Text — leave empty for default "öğretmenin etkinliği başlatmasını bekle")
```
Add `IdlePanelController` to the IdlePanel root. Wire `m_Controller` ← ActivityController, plus panel/text references. Optional `m_Tween` ← PanelTween on the same root.

This way the tablet shows clear feedback during Idle state instead of just the camera view + invisible teacher toggle.

### Activity.unity — F9 additions

**Bubble animation on IdlePanel** (visual polish):
1. Run `ARFishing → Create UI Sprites` to generate `bubble.png` (one-time).
2. On the `IdlePanel` GameObject (or a child of it), add the `IdleBackgroundAnimator` component.
3. Wire `m_Container` ← the IdlePanel's RectTransform (or leave null — auto-uses own transform), `m_BubbleSprite` ← `bubble.png`.
4. Tune `m_BubbleCount` (default 14), `m_SizeRange`, `m_SpeedRange`, `m_DriftAmplitudeRange`, and `m_BubbleColor` in the Inspector. Bubbles spawn at random positions on Start and drift upward indefinitely with sin-curve lateral motion, wrapping when they pass the top. Children see an ambient seascape instead of a blank welcome screen.

**Score count-up on SummaryPanel** (optional):
1. On the SummaryPanel's `ScoreText` GameObject (or any GO under SummaryPanel), add `ScoreTextAnimator`.
2. Wire `m_Target` ← the Text component, leave `m_Format` as `"Toplam puan: {0}"` or customize.
3. Drag this animator into `SummaryPanelController.m_ScoreAnimator`. Without this reference, score still shows but as an instant string.

**Quiz feedback bounce on QuizPanel** (optional):
1. On the QuizPanel's `FeedbackText` GameObject, add `TextPopAnimator`.
2. Wire `m_Target` ← the FeedbackText's RectTransform (or leave null — auto-uses own).
3. Drag this animator into `QuizPanelController.m_FeedbackPop`. Without this reference, feedback text appears instantly with no bounce.

All F9 polish is opt-in via inspector wiring — existing scenes without these components work exactly as F8 (snap, instant text).

### Bootstrap.unity — F10 additions

In the `ContentBootstrapper` inspector:
1. Right-click `Assets/ARFishing/Content/` → `Create → ARFishing → Accessibility Settings`. Drag the new asset into `m_AccessibilitySettings`. Default flags (`ReducedMotion = false`, `NarrationCaptions = false`, `UiScaleMultiplier = 1`) preserve F9 behavior — opt in per child / classroom need.
2. Right-click → `Create → ARFishing → Localization Table`. Drag into `m_LocalizationTable`. Add entries as `Key` + `Turkish` + `English` + `Arabic` triples. UI panels still hardcode TR strings as of F10, so an empty table is OK; the framework is wired for incremental migration.
3. `m_DefaultLocale` defaults to `Turkish` — leave it for the TR-first MVP, change when AR / EN content is ready.

If `AccessibilitySettings` is left unassigned, `AccessibilityState.Current` stays null and the bool accessors return false — exactly the F9 behavior. If `LocalizationTable` is unassigned, `Localizer.Get(key)` returns the key as fallback.

### Activity.unity — F10 additions

**Caption overlay** (optional, for `NarrationCaptions` accessibility):
1. Under the existing Canvas, create a `CaptionOverlay` GameObject + child `Text`:
   ```
   Canvas
     CaptionOverlay              (RectTransform, anchored bottom, Image bg, CanvasGroup)
       └── CaptionText           (Text, large, readable)
   ```
2. Add `CaptionOverlayController` to the CaptionOverlay root. Wire `m_Narration` ← NarrationPlayer (or rely on ServiceLocator), `m_PanelRoot` ← CaptionOverlay itself, `m_CanvasGroup` ← the CanvasGroup, `m_CaptionText` ← the child Text.
3. The overlay is **hidden by default** and only appears when `AccessibilitySettings.NarrationCaptions` is true AND narration is playing. Useful for hearing-impaired children, noisy classrooms, or supplementing the audio with on-screen text.

**Runtime toggle caveat** (`IdleBackgroundAnimator`): if `AccessibilityState.ReducedMotion` is toggled true *after* `Start()` has spawned bubbles, the existing bubbles freeze in place instead of being destroyed (Update skips the tick). Visual artifact, not a crash. F10 assumes settings are set at Bootstrap time and not toggled mid-session — which matches the classroom-deploy use case. If runtime toggle is needed later, subscribe to `AccessibilityState.Changed` and `Destroy(bubble.gameObject)` in the handler.

**SfxPlayer service** (F9.1, optional but recommended):
1. Run `ARFishing → Create Placeholder SFX (skip existing)` to generate `sfx_correct.wav`, `sfx_incorrect.wav`, `sfx_scan.wav` under `Content/Audio/Sfx/`.
2. Under the `Services` GameObject, create a child `SfxPlayer`. Add the `SfxPlayer` script — `[RequireComponent(AudioSource)]` auto-adds the audio source.
3. Drag the three WAV clips into `m_CorrectAnswerClip` / `m_IncorrectAnswerClip` / `m_CardSpottedClip`.
4. Optionally drag `QuizController` into `m_Quiz` and `MarkerTracker` into `m_Tracker` (both auto-resolve via `ServiceLocator` at `Awake` if left empty).
5. Tune per-clip volume sliders — default scan volume `0.55` so it doesn't drown out the narration that fires immediately after on the same `Spotted` event.

Audio overlap notes:
- Scan SFX and creature narration share the same `Spotted` event. They play in registration order — usually scan SFX first (~80ms) overlaps with narration intro. Lowering scan SFX volume in the Inspector is the recommended balance.
- Correct/incorrect feedback SFX fire during Quiz state when `NarrationPlayer` is already stopped (BeginQuiz called Stop), so no overlap with narration.

### Testing the F3/F4/F5 flow

- **In editor with XR Simulation**: open `Bootstrap.unity`, press Play. ContentBootstrapper loads CreatureDatabase + TaskDatabase, loads Activity scene, transitions `ActivityController` to `Idle`. Use `ActivityController`'s `Force: Idle → Scanning` context menu to advance the FSM. Hold a printed card in front of the simulated camera (configure XR Simulation environment with the placeholder image).
  - **F3 expectation**: `CreatureViewer` spawns the placeholder primitive on top of the card, and `MarkerStateBridge` transitions the FSM to `Viewing`.
  - **F4 expectation**: InfoPanel populates with the creature's TR-labeled fields and snaps in, and a tone plays from `NarrationPlayer`. Card kameradan çıkınca panel kaybolur. Sesi tekrar dinle → tone restarts. Kapat → panel hides + FSM goes back to Scanning.
  - **F5 expectation**: After the F6 wiring lands, the StartQuiz path moves into the Teacher Panel (see F6 expectation below). The Quiz state behavior itself is unchanged from F5: QuizPanel shows random prompt, CreatureViewer hides models (`ShowModels=false`), InfoPanel hides too, scan answer card → "Doğru!" or "Tam değil." for 2 seconds → next prompt → after 3 prompts → SummaryPanel → "Yeniden başlat" → Idle. CreatureViewer models reappear after Quiz ends (`ShowModels=true`).
  - **F6 expectation**: A small ~64px toggle button is visible at a tablet corner during all states. **Briefly tap it → nothing happens** (deliberate; protects against accidental child taps). **Long-press ≥1s → TeacherPanel opens.** Panel shows only the buttons applicable to the current state: in Idle, only `Etkinliği başlat`; in Scanning/Viewing, `Mini quiz başlat` + `Yeniden başlat`; in Quiz/Summary, `Yeniden başlat`. `Etkinliği başlat` → Scanning. `Mini quiz başlat` → Quiz. `Yeniden başlat` → drops back to Idle from anywhere (extends `IsLegal` with restart escapes). `Kapat` hides the panel without changing state. If you plug in an external display before Play (HDMI / USB-C / Miracast), `ProjectionCanvasRouter` activates display 1 and routes `ProjectionCanvas` to it — the class sees the focused creature large during Scanning/Viewing, the current prompt during Quiz, and the score grid during Summary. Without an external display, ProjectionCanvas is hidden entirely (do not fall back to overlaying it on the tablet — tablet has its own UI).
- **Opening Activity directly** for testing without Bootstrap: the FSM stays in `Bootstrap` state until you right-click `ActivityController` → `Force: Bootstrap → Idle`. Databases won't be registered in `ServiceLocator`, so `MarkerTracker`/`QuizController` fall back to their inspector-assigned database fields. As long as those references are set in the scene, marker → creature resolution and task lookup still work.

## MVP creature content set (20)

The 20-creature MVP, as scaffolded by `ARFishing → Create MVP Content`. All `CreatureId` values are kebab-case and stable — do not rename once an `XRReferenceImageLibrary` references them.

| CreatureId | DisplayName | Category | Habitat | Diet | Ecosystem Role |
|---|---|---|---|---|---|
| `octopus` | Ahtapot | Invertebrate | Reef | Carnivore | Predator |
| `moon-jellyfish` | Ay Denizanası | Invertebrate | OpenSea | FilterFeeder | Predator |
| `clownfish` | Palyaço Balığı | Fish | Reef | Omnivore | Prey |
| `great-white-shark` | Beyaz Köpekbalığı | Fish | OpenSea | Carnivore | Predator |
| `seahorse` | Denizatı | Fish | Reef | FilterFeeder | Prey |
| `moray-eel` | Müren | Fish | Reef | Carnivore | Predator |
| `stingray` | Vatoz | Fish | Seabed | Carnivore | Predator |
| `anglerfish` | Fener Balığı | DeepSea | DeepSea | Carnivore | Predator |
| `parrotfish` | Papağan Balığı | Fish | Reef | Herbivore | Cleaner |
| `crab` | Yengeç | Invertebrate | Seabed | Omnivore | Cleaner |
| `starfish` | Deniz Yıldızı | Invertebrate | Seabed | Carnivore | Predator |
| `mussel` | Midye | Invertebrate | Seabed | FilterFeeder | Cleaner |
| `squid` | Mürekkep Balığı | Invertebrate | OpenSea | Carnivore | Predator |
| `sea-urchin` | Deniz Kestanesi | Invertebrate | Seabed | Herbivore | Cleaner |
| `coral` | Mercan | Coral | Reef | FilterFeeder | ShelterProvider |
| `green-algae` | Yeşil Alg | Producer | Surface | Photosynthesizer | Producer |
| `seagrass` | Deniz Çayırı | Producer | Seabed | Photosynthesizer | ShelterProvider |
| `plankton` | Plankton | Producer | Surface | Photosynthesizer | Producer |
| `sea-turtle` | Deniz Kaplumbağası | Reptile | OpenSea | Omnivore | Prey |
| `dolphin` | Yunus | Mammal | OpenSea | Carnivore | Predator |

The TaskDatabase covers 8 rules so that any random 3-task quiz draws from this pool:

| TaskId | Prompt | Rule | MatchValue | Points |
|---|---|---|---|---|
| `task-reef-creature` | Mercan resifinde yaşayan canlıyı bul. | HabitatEquals | Reef | 1 |
| `task-filter-feeder` | Süzerek beslenen canlıyı bul. | DietEquals | FilterFeeder | 1 |
| `task-camouflage` | Kamuflaj yapan canlıyı bul. | TraitContains | kamufle | 2 |
| `task-invertebrate` | Bir omurgasız canlı bul. | CategoryEquals | Invertebrate | 1 |
| `task-predator` | Bir avcı canlı bul. | EcosystemRoleEquals | Predator | 1 |
| `task-producer` | Üretici bir canlı bul. | CategoryEquals | Producer | 1 |
| `task-deep-sea` | Derin denizde yaşayan canlıyı bul. | HabitatEquals | DeepSea | 2 |
| `task-shelter-provider` | Diğer canlılara ev sahipliği yapan bir canlı bul. | EcosystemRoleEquals | ShelterProvider | 2 |

Marine biology nuance: dolphins are mammals (Mammal), sea turtles are reptiles (Reptile), and "Producer" is used as a child-friendly grouping for algae / seagrass / plankton (the brief's "Bitkisel / Üretici Canlılar"). The pedagogical framing is "this card teaches X role in the ecosystem", not strict scientific taxonomy. An educator should sanity-check the metadata in code before printing 20 cards.

## Docs (Turkish, stakeholder-facing)

Seven production documents live in `Docs/`, plus a navigation landing page and a root README. Each doc has a markdown source-of-truth + a hand-styled HTML version (shared CSS theme — coral accent, ocean hero, sticky TOC, category-color components, print-optimized stylesheet).

**Entry points**:
- `README.md` (root) — repo-level pointer for GitHub viewers; quick links into Docs/index.html.
- `Docs/index.html` — landing page with role-based navigation ("Sen kimsin?" chips for Artist / Teacher / Onboarding Dev / Shipping Dev / QA / Privacy), 2×3 doc card grid (audience-color-coded), CLAUDE.md rulebook callout, "Karar matrisi" task→reference lookup. Open this when handing the repo to anyone — it routes them to the right document.

The six docs:

- `Docs/ArtistBrief.{md,html}` — **vendor-facing**. Full spec for the production tracks (3D model, TR audio narration, A6 print card, UI icon, **§11 SFX**, **§12 2D illustration**) for the 20-creature MVP set. Per-creature art direction notes + drafted TR narration scripts (~15–18 sec each). §11 added F9.1 (SFX set spec for the SfxPlayer that lands as placeholder in code); §12 added F9.1 (illustrated background spec to replace IdleBackgroundAnimator's procedural bubbles in production).
- `Docs/TeacherGuide.{md,html}` — **educator-facing**. Session script for the 35–45 min classroom activity: prep checklist, day-of setup, group roles, 5-phase timeline, observation rubric, post-session feedback, troubleshooting Q&A, KVKK privacy stance, and a tear-off Hızlı Referans Kartı.
- `Docs/EngineerOnboarding.{md,html}` — **dev-facing (walkthrough)**. Repo tour for new Unity engineers: 8-faz history (F1 skeleton → F8 polish), asmdef rules, module map, ASCII Activity FSM diagram, scene wiring summary, editor menu reference, common dev tasks (add creature/task/panel with code snippets), conventions + gotchas, backlog. Companion to CLAUDE.md (CLAUDE.md = rulebook; this = walkthrough).
- `Docs/BuildAndDistribution.{md,html}` — **dev-facing (production pipeline)**. Android (ARCore) + iOS (ARKit) build path: identifier setup, signing keystore, Custom AndroidManifest with INTERNET permission removal, Xcode signing, distribution channels (Play Store / App Store / TestFlight / MDM / Enterprise / Apple School Manager), build size + performance targets, KVKK/GDPR/COPPA compliance forms, CI/CD options, troubleshooting, release checklist.
- `Docs/DeviceTestProtocol.{md,html}` — **QA-facing**. Step-by-step protocol for testing the MVP build on real Android/iOS tablets. Smoke test (5 min) + functional test (15 min) + per-creature checklist (20 cards × 4 verifications) + performance targets + privacy stance device-level verification (network monitor) + edge cases + bug report template + sign-off page. Required before pilot test handoff.
- `Docs/PrivacyPolicy.{md,html}` — **store-listing-facing**. Minimal privacy policy declaring "zero data collected". KVKK/GDPR/COPPA reference table + ready-to-paste Google Play Data Safety + App Store Connect Privacy form answers. Host the HTML on any static host (GitHub Pages, Netlify) — the URL is required by both stores.
- `Docs/PilotFeedbackProtocol.{md,html}` — **post-pilot iteration-facing**. Aggregation + categorization + severity + decision matrix for translating pilot session feedback into doc/code/content updates. 4-category system (UX / Content / Technical / Pedagogical), 4-severity ladder (Blocker / Major / Minor / Note), per-doc playbook (which Doc gets updated for which feedback type), feedback log template, success criteria. Open after each pilot session to drive the next iteration.

Conventions for all six:
- **Hand to stakeholders as HTML** — self-contained, no external deps, opens in any browser, prints to PDF cleanly.
- **Edit the .md, regenerate the .html** — content lives in the markdown for git diffability. The HTML is hand-styled (not generated from the .md), so structural changes in the .md need a matching update to the .html. For text-only fixes (typo, narration tweak), edit both files in the same commit.
- **Shared CSS theme** — same color palette / typography / hero treatment. When introducing a new doc, copy the inline `<style>` block from one of these for visual consistency. The technical docs add specialized components: `.faz-grid` cards (EngineerOnboarding), `pre.code` syntax-tinted blocks (Onboarding + Build), `pre.diagram` ASCII boxes (Onboarding), `pre.term` `$`-prefixed terminal commands (Build), `.status` done/missing badges (Build), `.signoff` field block (DeviceTestProtocol), checklist `☐` prefix lists. Reuse them for any new engineering-flavored doc.

## CI/CD

`.github/workflows/build-android.yml` — GitHub Actions workflow for Android (ARCore) build via `game-ci/unity-builder@v4`. Triggered on push to `main` touching `Assets/` `Packages/` or `ProjectSettings/`, plus `workflow_dispatch` with APK / AAB choice. Caches `Library/` per-source-hash. Privacy stance enforced: `UNITY_ENABLE_ANALYTICS=0` and `UNITY_DISABLE_DIAGNOSTICS=1` env vars set on the runner.

Required GitHub secrets (Settings → Secrets and variables → Actions):
- `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD` — Unity activation
- `ANDROID_KEYSTORE_BASE64`, `ANDROID_KEYSTORE_PASS`, `ANDROID_KEYALIAS_PASS` — signing

iOS build job is commented out in the YAML — uncomment when an Apple Developer account + macOS runner secrets are configured. See `Docs/BuildAndDistribution.html` §3.3 for keystore generation detail.

## Content production checklist

When real assets (3D models, audio narration, card images, icons) arrive, replace placeholders per-creature without changing code:

1. **3D model** — drop the prefab into `Assets/ARFishing/Content/Models/creature_<id>.prefab`. Make sure the prefab pivot is at the card's center and the natural-scale-1 size visually matches a ~10 cm card width (`CreatureViewer` rescales to `ARTrackedImage.size.x`). Drag it into the `CreatureDefinition.ModelPrefab` field. `ARFishing → Create Placeholder Models (skip existing)` will leave it alone on subsequent runs.
2. **Audio narration** — TR voiceover, mono, 22 kHz, **≤25 seconds** (validator enforces). Drop into `Assets/ARFishing/Content/Audio/narration_<id>.wav` (or `.ogg`). Drag into `CreatureDefinition.NarrationClip`. `Create Placeholder Audio (skip existing)` skips it on subsequent runs.
3. **Card image** — A6 (105 × 148 mm), matte laminated, 350 gsm — see "Physical card specs" below. Add to `CardLibrary.asset` (XRReferenceImageLibrary) with `name = CreatureId`, `Specify Size = true`, `physicalSize.x = 0.105`.
4. **Icon** — UI sprite for `CreatureDefinition.Icon`. Used by InfoPanel, ProjectionPanel preview, and the Summary scanned grid. Square ~256×256, transparent PNG.
5. **Validate** — run `ARFishing → Validate All Creature Definitions`. The report should drop to zero problems once all four asset slots are filled.

For new creatures beyond the 20 (e.g. regional content packs):
- Add a new `MakeCreature(...)` call to `CreateMvpContentMenu` so the scaffold stays single-source.
- Or for one-off content, use `Create → ARFishing → Creature Definition` right-click menu and edit fields in the Inspector. Then drag the new asset into `CreatureDatabase.m_Creatures` manually.

## Physical card specs

Locked product decision — referenced by image library setup and the print supplier brief:

- **Size**: A6 (105 × 148 mm).
- **Stock**: 350 gsm cardstock.
- **Finish**: **matte** lamination (glossy finish creates specular highlights that destroy image tracking under classroom lights).
- **Corners**: rounded (durability + safety).
- **Front**: tracker pattern (high contrast, asymmetric, no large solid-color regions, no repeating motifs) + creature name at bottom.
- **Back**: blank or minimal info text.
- **Image library setting**: in `XRReferenceImageLibrary`, every reference image must have **Specify Size = ON, physical width = 0.105 m**. The viewer scales the 3D model from `ARTrackedImage.size`, so this number is load-bearing — if the print size ever changes, only this number updates, not code.

## MVP scope reminder

Build these; defer everything else until the user explicitly asks:

- **In MVP**: 20 marker cards, 20 creature 3D models with idle animation, per-creature audio narration (Turkish), card-scan → model + info panel flow, offline operation, group activity flow, teacher projection screen, end-of-activity matching or mini quiz.
- **Out of MVP**: multiplayer sync across tablets, online accounts, score history, complex food-web simulation, realtime backend, cloud content delivery.
