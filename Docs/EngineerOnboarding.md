# Engineer Onboarding — Görünenin Ötesinde Bir Deniz

> Projeye yeni katılan Unity dev için repo turu. Unity 6 / AR Foundation 6.4 / URP / Input System / XRI 3.4 zemininde, 8 fazla inşa edilmiş bir codebase'i hızlıca anlamak ve üretken olmak için.

> **Sunum versiyonu**: `Docs/EngineerOnboarding.html` (browser'da aç, sticky TOC ile gezin). Bu markdown git diff için.

> **Kural kitabı**: `CLAUDE.md` (root). Bu doküman walkthrough; CLAUDE.md tüm zorunlu kuralları içerir. Bu dokümanı bir kere oku, CLAUDE.md'yi her seans yanında aç.

---

## 1. Ürün özeti

Ürün adı `ARFishing` ama **bu bir balıkçılık oyunu değildir**. Gerçek konsept: **AR destekli deniz ekosistemi eğitim uygulaması** ("Görünenin Ötesinde Bir Deniz"). Hedef kitle 7–12 yaş, sınıf etkinliği, 5 öğrenci başına 1 tablet, ~35–45 dk.

- Çocuklar fiziksel AR kartları tabletin kamerasına gösterir.
- AR Foundation `ARTrackedImageManager` kartı tanır → ilgili `CreatureDefinition`'a map'ler.
- 3D model kartın üzerinde belirir + Türkçe seslendirme çalar + bilgi paneli açılır.
- Etkinlik sonunda öğretmen quiz başlatır, gruplar cevap kartı tarayarak yarışır.

Detaylı tasarım için `CLAUDE.md` → "What this project actually is" ve `Docs/TeacherGuide.md`.

---

## 2. Day-one Checklist

| # | Adım | Notlar |
|---|---|---|
| 1 | Unity Hub yükle | https://unity.com/download |
| 2 | Unity **6000.4.9f1** yükle (tam sürüm) | Farklı patch silently re-import yapar |
| 3 | Modules ekle: Android Build Support + iOS Build Support | iOS sadece macOS'ta build edilir |
| 4 | Repo'yu klonla / `D:\Projects\Unity\ARFishing` dizinini aç | Path Windows için `D:\` örnek |
| 5 | Unity Hub → "Add" → projeyi seç → "Open" | İlk import ~5–10 dk |
| 6 | TMP Essentials dialog'u çıkarsa → "Import TMP Essentials" | Bir kerelik, sonra kapanır |
| 7 | `Assets/ARFishing/Scenes/Bootstrap.unity`'i aç | İlk açılışta sahne boş görünebilir, normal |
| 8 | Console temiz olmalı | Derleme hatası varsa Bölüm 11'e bak |
| 9 | `ARFishing → Create MVP Content (20 creatures + 8 tasks)` çalıştır | 20 canlı + 8 görev asset'i oluşur |
| 10 | `ARFishing → Create Placeholder Models/Audio/Icons (skip existing)` çalıştır | Test edilebilir uygulama için placeholder asset'ler |
| 11 | `ARFishing → Validate All Creature Definitions` çalıştır | 0 hata bekleniyor |
| 12 | Bootstrap'a tekrar dön, Play tuşuna bas | İlk seans için sahne wiring gerekir — Bölüm 7 |

---

## 3. Proje şekli ve 8 faz tarihi

Codebase 8 fazla katmanlı şekilde inşa edildi. Her faz kendinden öncekinin üzerine bina:

| Faz | İçerik | Sonuç |
|---|---|---|
| **F1** İskelet | asmdef + folder yapısı + boş sahneler + `ActivityController` FSM + `ServiceLocator` | Çalışan ama içeriksiz uygulama |
| **F2** Veri | `CreatureDefinition` + `CreatureDatabase` + `TaskDefinition` ScriptableObject'ler + validator | Asset üretim altyapısı hazır |
| **F3** Marker + Viewer | AR Foundation 6.x `ARTrackedImageManager` wrap + `FocusResolver` + `MarkerStateBridge` + `CreatureViewer` + `ContentBootstrapper` | Kart tarama → model spawn çalışıyor |
| **F4** Narration + Info Panel | `NarrationPlayer` + `InfoPanelController` | Tam Viewing state — model + ses + bilgi paneli |
| **F5** Quiz + Summary | `QuizController` + `QuizPanelController` + `SummaryPanelController` + 8 placeholder görev | Uçtan uca gameplay loop |
| **F6** Teacher Mode | `TeacherToggleButton` (long-press) + `TeacherPanelController` + `SessionScannedTracker` + `ProjectionCanvasRouter` + `ProjectionPanelController` | Sınıf demo + projeksiyon route'u |
| **F7** İçerik dolumu | 20 canlı + 8 görev + `Mammal`/`Reptile`/`Photosynthesizer` enum eklemeleri + `TaskPresented` refactor | Presentable MVP içerik seti |
| **F8** Polish | `PanelTween` slide+fade utility + InfoPanel 2-sn passive close + `IdlePanelController` + placeholder icon menu + runtime display detection | Smooth UX, tüm placeholder slot'lar dolu |

Bu sıra önemli: F3 yapacaksan F2 anlamış olmalısın, F6 yapacaksan F3-F5 anlamış olmalısın. Her fazın detayı CLAUDE.md'de + git history'de.

---

## 4. asmdef yapısı

Sadece **iki asmdef** kullanıyoruz, hepsi `Assets/ARFishing/` altında:

```
Assets/ARFishing/Scripts/
  ARFishing.asmdef           # runtime — tüm gameplay kodu
  Editor/
    ARFishing.Editor.asmdef  # editor-only — validator + content menü'leri
```

**`ARFishing.asmdef` references**:
- `Unity.XR.ARFoundation` + `Unity.XR.ARSubsystems` — AR tracking API
- `Unity.XR.CoreUtils` — XR utility types
- `Unity.XR.Interaction.Toolkit` — XRI runtime
- `Unity.InputSystem` — Input System
- `Unity.TextMeshPro` — TMP (henüz UI'da kullanılmıyor ama hazır)
- `Unity.XR.Interaction.Toolkit.Samples.StarterAssets` — **zorunlu**, XR Origin prefab buna bağımlı
- `Unity.XR.Interaction.Toolkit.Samples.ARStarterAssets` — **zorunlu**, AR Rig prefab buna bağımlı

`UnityEngine.UI` auto-referenced (asmdef'te belirtmek gerekmiyor).

**Kural**: `Assembly-CSharp`'a hiçbir şey yazma. Yeni gameplay kodu `ARFishing.asmdef` altında olur. `Assets/Samples/XR Interaction Toolkit/3.3.0/` altındaki sample asset'ler kendi asmdef'leriyle var olur ve **silinmez** — `ARFishing.asmdef` onlara dependent.

---

## 5. Module haritası

`Assets/ARFishing/Scripts/` altındaki klasörler ve sorumlulukları:

```
Core/        → ActivityState (enum FSM), ServiceLocator (type→instance registry),
               ActivityController (FSM, StateChanged event, IsLegal guard)

Creatures/   → CreatureCategory/Habitat/DietType/EcosystemRole (enums),
               CreatureLabels (TR display labels via extension methods),
               CreatureDefinition (SO), CreatureDatabase (SO + lookup)

Marker/      → MarkerTracker (ARTrackedImageManager wrap, Spotted/Updated/Gone events),
               FocusResolver (tek-odak heuristic, FocusChanged event),
               MarkerStateBridge (FocusChanged → ActivityController state transition)

Viewer/      → CreatureViewer (instantiates ModelPrefab per tracked image,
               ShowModels flag for Quiz mode)

Narration/   → NarrationPlayer (single AudioSource,
               plays on FocusChanged, NarrationStarted/Finished events)

UI/          → InfoPanelController (creature info, 2-sn passive close),
               QuizPanelController, SummaryPanelController, IdlePanelController,
               PanelTween (slide+fade utility)

Quiz/        → MatchRule (enum), TaskDefinition (SO), TaskDatabase (SO),
               QuizController (drives Quiz state, picks tasks, evaluates answers)

Teacher/     → TeacherToggleButton (long-press gate), TeacherPanelController,
               SessionScannedTracker (dedupe by CreatureId),
               ProjectionCanvasRouter (display 1 routing),
               ProjectionPanelController (class-facing content)

Content/     → ContentBootstrapper (lives in Bootstrap.unity, loads DBs +
               Activity scene + transitions FSM to Idle)

Editor/      → ARFishing.Editor.asmdef, all editor menu scripts
```

---

## 6. Activity FSM

`ActivityController` enum'lu basit bir state machine:

```
                ┌────────────┐
                │ Bootstrap  │ (ContentBootstrapper.Start sırasında)
                └─────┬──────┘
                      │ DB load done
                      ▼
                ┌────────────┐ ◄────────────┐
        ┌────►  │   Idle     │ Restart      │
        │       └─────┬──────┘ from anywhere│
        │             │ Teacher → "Başla"   │
        │             ▼                     │
        │       ┌────────────┐              │
   Summary─►Idle│  Scanning  │ ◄──────┐     │
        │       └─────┬──────┘        │     │
        │             │ card tracked  │     │
        │             ▼               │     │
        │       ┌────────────┐  card  │     │
        │       │  Viewing   │ ────── ┘     │
        │       └─────┬──────┘ removed      │
        │             │ Teacher → "Quiz"    │
        │             ▼                     │
        │       ┌────────────┐              │
        │       │   Quiz     │ ─────────────┘
        │       └─────┬──────┘ Restart
        │             │ 3/3 tasks
        │             ▼
        │       ┌────────────┐
        └───────│  Summary   │
                └────────────┘
```

`ActivityController.IsLegal(from, to)` switch'inde tüm legal geçişler explicit. Restart escapes (`Scanning/Viewing/Quiz → Idle`) F6'da eklendi.

**Kim hangi geçişi tetikler**:
- `Bootstrap → Idle`: `ContentBootstrapper` (DB load complete)
- `Idle → Scanning`: `TeacherPanelController.OnStartActivity` button click
- `Scanning ↔ Viewing`: `MarkerStateBridge` (FocusResolver.FocusChanged ile)
- `Scanning/Viewing → Quiz`: `TeacherPanelController.OnStartQuiz` button click
- `Quiz → Summary`: `QuizController.AdvanceAfterDelay` (3/3 tasks done)
- `Summary → Idle`: `SummaryPanelController.OnRestartClicked` button click
- `*/Idle`: `TeacherPanelController.OnRestart` button click (F6 escape)

---

## 7. Sahne wiring (en zor kısım)

**Önemli**: Sahne dosyaları (`.unity`) repo'da bilinçli olarak minimal — sadece settings block'ları. Script GUID'leri Unity-assigned, YAML'da elle component referansı yazmak fragile. Bu yüzden ilk seferinde sahne kurulumu Editor'da elle yapılır, sonra commit edilir.

CLAUDE.md'de **çok detaylı adım adım** wiring procedure var ("Scene wiring" başlığı). Tekrar etmiyorum — sıkıştırılmış özet:

### Bootstrap.unity

1 GameObject: `ContentBootstrapper`
- Script: `ContentBootstrapper`
- `m_CreatureDatabase` ← `Assets/ARFishing/Content/CreatureDatabase.asset`
- `m_TaskDatabase` ← `Assets/ARFishing/Content/TaskDatabase.asset`

### Activity.unity

**XR Origin (AR Rig)** prefab'ı `Assets/Samples/XR Interaction Toolkit/3.3.0/AR Starter Assets/Prefabs/` altından sürükle. Root GameObject'e:
- `ARTrackedImageManager` component → `CardLibrary.asset` referansı
- `MarkerTracker` script → `m_Manager` + `m_Database` referansları

**Services** boş GameObject + child'lar:
- `ActivityController` (script)
- `FocusResolver` (script + MarkerTracker ref)
- `MarkerStateBridge` (script + FocusResolver + ActivityController refs)
- `CreatureViewer` (script + MarkerTracker ref)
- `NarrationPlayer` (script + AudioSource + FocusResolver ref)
- `QuizController` (script + 6 reference)
- `SessionScannedTracker` (script + Tracker + Controller refs)

**Canvas (tablet, display 0)** + child UI:
- `InfoPanel` + `InfoPanelController` script (13 reference)
- `QuizPanel` + `QuizPanelController` (6 reference)
- `SummaryPanel` + `SummaryPanelController` (4 reference)
- `IdlePanel` + `IdlePanelController` (3 reference) — F8
- `TeacherToggleButton` (Image, raycastTarget=true) + `TeacherToggleButton` script + TeacherPanel ref
- `TeacherPanel` + `TeacherPanelController` (5 reference)

Her panel root'una opsiyonel `PanelTween` ekle (F8) → controller'ın `m_Tween` field'ına sürükle. Atlanmasa panel'ler instant snap modunda çalışır.

**ProjectionCanvas (display 1, opsiyonel)** + `ProjectionCanvasRouter` + `ProjectionPanelController` + 4 section (Idle/Scanning/Quiz/Summary) + ScannedGridParent + ScannedChipPrefab.

**XRReferenceImageLibrary** — `Assets/ARFishing/Content/` altında `Create → XR → Reference Image Library` ile `CardLibrary.asset` oluştur. 20 entry ekle (CreatureDefinition.ReferenceImageName ile aynı isim — kebab-case), her birinde Specify Size = ON, width = 0.105 m, placeholder texture sürükle (XR Simulation için).

Tam adım adım için CLAUDE.md → "Scene wiring".

---

## 8. Editor menü referansı

Tüm menüler `ARFishing` menübar başlığı altında:

| Menü | Ne yapar | Ne zaman çalıştır |
|---|---|---|
| `Create Example Content` | 2 canlı + database (octopus + jellyfish) | Veri modeli iterate ederken |
| `Create MVP Content (20 creatures + 8 tasks)` | Tam MVP content set | Asset'leri bootstrap'larken |
| `Create Placeholder Models (skip existing)` | URP/Lit-tinted primitive prefab per canlı, deterministic shape+color | Test için görsel asset gerektiğinde |
| `Create Placeholder Audio (skip existing)` | 1.5s sine WAV per canlı, deterministic frequency | Test için ses gerektiğinde |
| `Create Placeholder Icons (skip existing)` | 256×256 PNG per canlı, kategori bg + HSV center | InfoPanel/Summary için ikon gerektiğinde |
| `Create Example Tasks` | 5-task standalone scaffold | Quiz iterate ederken, MVP menu superseded |
| `Validate All Creature Definitions` | kebab-case ID + uniqueness + dolu field + ≤25s ses cap kontrolü | Asset değişikliği sonrası, build öncesi |

**Skip existing semantiği**: 3 placeholder menüsü mevcut asset'i atlar. Gerçek artist'ler 3D model/ses/ikon teslim ettiğinde placeholder'ları silmek gerekmez — re-run güvenle skip eder.

---

## 9. Yaygın dev görevleri

### Yeni canlı ekleme (MVP set genişletme)

`CreateMvpContentMenu.cs` aç → `creatures.Add(MakeCreature(...))` listesinin sonuna yeni satır ekle:

```csharp
creatures.Add(MakeCreature("lobster", "Istakoz",
    CreatureCategory.Invertebrate, Habitat.Seabed, DietType.Omnivore, EcosystemRole.Cleaner,
    new[] { "avlanma", "habitat kaybı" },
    "Sert kabuğu ve büyük kıskaçlarıyla kendini savunur."));
```

Sonra Unity'de:
1. `ARFishing → Create MVP Content` → yeni `lobster.asset` üretilir + CreatureDatabase güncellenir
2. `ARFishing → Create Placeholder Models/Audio/Icons` → placeholder asset'ler atanır
3. `CardLibrary.asset`'a yeni entry: name = `lobster`, Specify Size + width = 0.105, placeholder texture
4. `ARFishing → Validate All Creature Definitions` → temiz olmalı

### Yeni quiz görevi ekleme

Aynı dosyada `tasks.Add(MakeTask(...))` listesine ekle:

```csharp
tasks.Add(MakeTask("task-prey",
    "Başka canlılara av olan bir canlı bul.",
    MatchRule.EcosystemRoleEquals, "Prey", 1));
```

`MatchValue` enum **adı** olmalı (`Prey`, not `"Av"`). Detay için CLAUDE.md → "Content authoring conventions".

### Narration cap'ini değiştirme

`Editor/CreatureValidationTool.cs` → `const float MaxNarrationSeconds = 25f;` → istediğin değere ayarla. Tasarımla teyitle.

### Quiz görev sayısını değiştirme

`QuizController` GameObject inspector → `m_TasksPerSession` field. Default 3.

### Yeni panel ekleme (örnek: HintPanel)

1. `Scripts/UI/HintPanelController.cs` yaz:
   ```csharp
   public class HintPanelController : MonoBehaviour
   {
       [SerializeField] ActivityController m_Controller;
       [SerializeField] GameObject m_PanelRoot;
       [SerializeField] CanvasGroup m_CanvasGroup;
       [SerializeField] PanelTween m_Tween;  // optional
       
       void Awake() { if (m_Controller == null) ServiceLocator.TryGet(out m_Controller); Hide(); }
       void OnEnable() { if (m_Controller != null) m_Controller.StateChanged += HandleStateChanged; }
       void OnDisable() { if (m_Controller != null) m_Controller.StateChanged -= HandleStateChanged; }
       
       void HandleStateChanged(ActivityState p, ActivityState n) { /* show/hide logic */ }
       void SetVisible(bool v) { /* PanelTween or fallback */ }
   }
   ```
2. Activity sahnesinde Canvas altına HintPanel GameObject + UI children oluştur
3. HintPanelController ekle, reference'ları wire et
4. Opsiyonel: PanelTween ekle

Pattern aynı: `[SerializeField]` reference, ServiceLocator fallback Awake'de, StateChanged subscribe.

---

## 10. Run / build / test

### Editor (XR Simulation)

1. Bootstrap.unity aç
2. Play
3. Bootstrap → Activity sahnesine geçer → Idle state
4. Test için `ActivityController` inspector → right-click → `Force: Idle → Scanning` (ContextMenu affordance)
5. XR Simulation environment'a kart texture'ı eklenmişse, model spawn olur

### Android (ARCore)

1. File → Build Profiles → Android'e geç
2. XR Plug-in Management → Android sekmesi → AR Core Loader enabled
3. Build veya Build & Run

### iOS (ARKit)

1. macOS gereklidir
2. File → Build Profiles → iOS'a geç
3. XR Plug-in Management → iOS sekmesi → AR Kit Loader enabled
4. Xcode project build
5. Xcode'dan archive + provisioning

### Test framework

**Yok.** Unity Test Framework asmdef'leri kurulmadı. Manuel testing yapıyoruz. Eğer test eklemek istersen Unity Package Manager'dan Test Framework'u ekle + yeni `ARFishing.Tests.asmdef` oluştur, referansları wire et.

---

## 11. Conventions ve gotcha'lar

### Conventions

- **English code, Turkish content**. Identifier'lar (class, method, field, enum member) hep İngilizce. UI metni, log mesajı, narration script TR.
- **kebab-case CreatureId**. `octopus`, `moon-jellyfish` — `XRReferenceImageLibrary` entry name ile **aynı** olmalı.
- **`m_` prefix serialized field'larda**. `[SerializeField] string m_DisplayName`. Unity convention.
- **`Unknown = 0` her enum'da**. Default değer "yet to fill" sinyali, validator hata olarak işaretler.
- **ServiceLocator pattern**. MonoBehaviour'lar `ServiceLocator.Register(this)` yapar Awake'de, `Unregister(this)` OnDestroy'da. Diğer modüller `ServiceLocator.TryGet<T>(out var x)` ile bulur. Inspector reference fallback olarak da var.
- **Tüm UI panel controller'lar aynı pattern**: `[SerializeField] PanelTween m_Tween` opsiyonel, `SetVisible(bool)` tween varsa onu kullanır yoksa fallback.

### Privacy stance (sert kural)

- **Hiçbir network çağrısı yok**. Firebase, Sentry, GameAnalytics, vb. eklenmez.
- **Hiçbir analytics**. Unity Analytics modülü `manifest.json`'dan çıkarıldı.
- **Android Internet permission yok**. AR Foundation Internet gerektirmez; merged manifest'te varsa override ile kaldır.
- Yeni feature network gerektirse → user'a sor, otomatik ekleme.

### AR Foundation 6.x API

5.x API'leri **gitti**, kullanma:
- `ARSessionOrigin` → yerine `XROrigin`
- Eski `trackedImagesChanged` event imzası → yerine `trackablesChanged` UnityEvent
- `ARTrackedImageManager.trackablesChanged.AddListener(...)` (UnityEvent, C# event değil)
- `ARTrackedImage.referenceImage.name` → kart name'i
- `ARTrackedImage.size.x` → kart genişliği (metre, image library'den gelir)

### Placeholder menüleri "skip existing"

3 placeholder menüsü mevcut asset'i overwrite **etmez**. Gerçek 3D model/ses/ikon teslim alınınca placeholder'ları silmek gerekmez. Re-run güvenli.

### Sahne file değişikliği commit'i

F4-F8 boyunca Activity.unity ve Bootstrap.unity Editor'da elle modifiye edildi. Wiring değişiklik yaparken `.unity` dosyaları + `.meta` dosyaları commit edilmeli. Sahne YAML diff'i okunabilir ama büyük olabilir.

---

## 12. Sonraki adımlar / backlog

F1-F8 ile MVP code-complete. Açık iş başlıkları:

### Polish (F9 — başlanmamış)
- Idle splash illüstrasyonu (`IdlePanelController` placeholder text yerine)
- Score animations, transition SFX
- `PanelTween` initialize edge case'leri
- Multi-task scoring tuning (`task-camouflage` çok dar)

### İçerik üretim bekliyor
- 20 canlının gerçek 3D modeli (artist iş) → `Docs/ArtistBrief.html`
- 20 canlının TR seslendirmesi (voice talent) → `Docs/ArtistBrief.html` script'ler
- 20 basılı kart (illüstratör + print evi)
- 20 UI ikonu (illüstratör)

### Pilot test logistics
- 3 sınıf pilot seansı planı → `Docs/TeacherGuide.html`
- Eğitmen partner
- Feedback toplama (form, dijital olabilir)

### Build & distribution
- Android signing keystore
- iOS provisioning + TestFlight veya enterprise dağıtım
- Internet permission override (Android merged manifest)

---

## 13. Nerede ne var

| Dosya | İçerik |
|---|---|
| `CLAUDE.md` (root) | **Kural kitabı**. Her seans yanında aç. Asmdef, sahne wiring, conventions, MVP content set tablosu. |
| `Docs/EngineerOnboarding.{md,html}` | **Bu doküman**. Bir kez oku, sonra reference. |
| `Docs/ArtistBrief.{md,html}` | Vendor brief (3D + ses + kart + ikon spec'leri + 20 canlının sanat yönü ve TR anlatım scripti). |
| `Docs/TeacherGuide.{md,html}` | Educator brief (sınıf etkinlik akışı + sorun giderme + hızlı referans kart). |
| `Assets/ARFishing/Scripts/` | Tüm gameplay kodu, asmdef'le birlikte. |
| `Assets/ARFishing/Content/` | Asset'ler — creatures, tasks, models, materials, audio, icons, image library. |
| `Assets/ARFishing/Scenes/` | Bootstrap.unity + Activity.unity. |
| `Assets/Samples/XR Interaction Toolkit/3.3.0/` | XRI sample asset'leri — **silinmez**, `ARFishing.asmdef` bunlara bağımlı. |
| `Assets/MobileARTemplateAssets/Scripts/` | Unity AR Mobile template scripts. **Referans only**, education app kullanmaz. |
| `Packages/manifest.json` | Unity packages. `com.unity.modules.unityanalytics` çıkarılmış (privacy stance). |
| `ProjectSettings/` | XR loaders, URP settings, build settings, Unity Connect (analytics disable). |

---

## 14. İlk gün üretim önerisi

İlk gününde küçük bir dokunuş yap ki codebase'i tanı:

**Hafif görev örnekleri**:
- `CreatureLabels.cs`'e bir TR label değiştir (örn. "Etçil" → "Etobur"), Play et, InfoPanel'in güncellendiğini gör.
- `CreateMvpContentMenu.cs`'de bir canlının trait metnine küçük bir kelime ekle, re-run et, yeni asset'te değişikliği gör.
- `ActivityController.cs`'e yeni bir `[ContextMenu]` ekle (örn. `Force: Quiz → Idle` direct restart için).
- `QuizController.AnswerEvaluated` event'ine subscribe olan basit bir log component yaz, console'da quiz akışını izle.

Daha derin görev örnekleri (sonra):
- Yeni bir paneli wire et (HintPanel) — Bölüm 9'a göre.
- Validation tool'a yeni bir kural ekle (örn. "InterestingTrait min 10 karakter").
- Slide tween'a damping/bounce variation ekle.

---

> Sorular için: `CLAUDE.md` her zaman ilk durak. Sonra git log'a bak — son 8 fazın hepsi commit history'sinde (eğer commit edilmişse). Sonra bana sor 🐙
