# Pilot Feedback Protokolü — Görünenin Ötesinde Bir Deniz

> Sınıf pilot testlerinden toplanan geri bildirimin nasıl agregat edileceği, kategorize edileceği ve hangi dokümanların/dosyaların güncelleneceğine dair karar matrisi. **Pilot test sonrası iterasyon döngüsü** için tek kaynak.

> **Sunum versiyonu**: `Docs/PilotFeedbackProtocol.html`.

> Bu doküman pilot test sırasında değil, **pilot test sonrası iterasyon kararı verirken** açılır. Etkinlik yönetimi için `Docs/TeacherGuide.html`, cihaz testi için `Docs/DeviceTestProtocol.html`.

---

## 1. Feedback kaynakları

Her pilot seans sonrası 3 kaynaktan veri gelir:

| Kaynak | Format | Doldurur |
|---|---|---|
| **Eğitmen gözlem rubriği** | 7 madde × 4 puan + serbest not | TeacherGuide §6 doldurulmuş kopyası |
| **Çocuk geri bildirimi** | 4 açık uçlu soru (sözlü veya yazılı) | TeacherGuide §7 |
| **QA gözlem notları** | Edge case'ler, teknik sorunlar, performance | Eğitmen veya gözlemci |

**Önerilen toplama**: pilot bittikten 24 saat içinde tek bir Google Form / Notion / Linear ticket olarak yansıt. Geç yazılan not unutulur.

---

## 2. Kategorize sistemi

Her feedback öğesini 4 kategoriye ayır:

| Kategori | Ne kapsar | Tipik aksiyon |
|---|---|---|
| **UX** | Akış kafası karıştırıcı, button bulamadı, panel kayboldu | UI/UX revize, controller davranışı değişikliği |
| **Content** | Bilgi eksik, yanlış, yaşa uygun değil, anlatım uzun | CreatureDefinition/TaskDefinition asset güncelleme |
| **Technical** | Crash, tracker fail, ses yok, FPS düşük | Code fix, build settings, asset optimization |
| **Pedagogical** | Çocuk anlamadı, eğitmen müdahalesi gerekti, kavram net değil | Doc revize, narration script revize, eğitmen kılavuzu netleştirme |

Bir feedback öğesi birden fazla kategoriye düşebilir — etiketle, ayrı ayrı işle.

---

## 3. Severity

Her item'a severity ver:

| Seviye | Tanım | Aksiyon |
|---|---|---|
| **Blocker** | Pilot tamamlanamadı / app çöktü / çocuk ağladı | Sonraki pilot ÖNCESİ fix zorunlu |
| **Major** | Akış aksadı ama tamamlandı / etkinliğin değeri azaldı | Sonraki release ÖNCESİ fix |
| **Minor** | Polish opportunity, bir grup deneyimi etkilendi | Backlog'a, müsait olunca |
| **Note** | Gözlem, değişiklik gerektirmez | Trend analizi için sakla |

---

## 4. Karar matrisi — Hangi dosya güncellenecek?

Kategoriye göre hangi artifact'in değişeceği:

| Feedback türü | Etkilenen dosya(lar) | Doc güncellemesi |
|---|---|---|
| Çocuk anlatımı uzun bulduğunu söyledi | `CreateMvpContentMenu.cs` (InterestingTrait kısalt), `narration_<id>.wav` (rerecord) | `ArtistBrief §7.<id>` script güncellenir |
| Card tracker takılmadı (tek bir kart için) | `card_<id>_tracker.png` (yeniden tasarım) | `ArtistBrief §5` kart tasarım kuralı | 
| Tracker tüm kartlarda yavaş | Aydınlatma rehberi (TeacherGuide), kart laminasyon (ArtistBrief) | `TeacherGuide §8`, `ArtistBrief §5` |
| Eğitmen "Quiz başlat"ı bulamadı | `TeacherPanelController` UI revize | `TeacherGuide §5.4` adımlar netleştir |
| Yanlış cevapta SFX rahatsız edici | `SfxPlayer` volume ya da yeni SFX | `ArtistBrief §11` ton spec revize |
| Çocuklar Idle splash'taki bubble'lara takılıp etkinliği geç başlattı | `IdleBackgroundAnimator` bubble count azalt, alpha düşür | — |
| Quiz çok kolay / çok zor | `CreateMvpContentMenu.cs` task seti revize, `m_TasksPerSession` ayarla | `TeacherGuide §5.4` opsiyonel zorluk |
| Narration tonu çok ciddi | Voice talent revize, yeniden kayıt | `ArtistBrief §4` ton spec güncelle |
| Bilimsel yanlışlık | `CreateMvpContentMenu.cs` metadata + narration script | `ArtistBrief §7.<id>`, eğitmen review zorunlu |
| Crash / freeze | Code fix, `DeviceTestProtocol` edge case ekle | — |
| FPS düşük | Asset optimization (texture, mesh comp), `BuildAndDistribution §5` revize | — |
| 30 dk yeterli olmadı | Etkinlik akışı revize, kart başına süre kısalt | `TeacherGuide §5` süre revize |

---

## 5. İterasyon döngüsü

Pilot → feedback → iterasyon → tekrar pilot süreci:

### Pilot N+1 öncesi (önerilen sequence)

1. **Toplama** (1 gün): tüm 3 grup pilot eğitmeninden geri bildirim agregat
2. **Triyaj** (1 gün): kategorize + severity
3. **Plan** (1 gün): blocker + major'leri sonraki release'e plan
4. **Fix** (~1 hafta): code/content/doc güncelle
5. **Validation** (1 gün): `DeviceTestProtocol` rerun, validator menüsü temizliği teyit
6. **Pilot N+1**

### Minor + Note işleme

Blocker/Major sıkışıkken Minor/Note backlog'a düşer. Backlog'u 3 pilotta bir gözden geçir, batch'le.

---

## 6. Doc-update playbook

Hangi dokümanın hangi durumda güncelleneceği:

### TeacherGuide
- Etkinlik akışı timing değişti → §5
- Yeni sorun türü → §8 troubleshooting Q&A
- Gözlem rubriği eksik / yanlış maddesi → §6
- Hızlı referans kart eksik bilgi → §10

### ArtistBrief
- Canlı metadata değişti (kategori, habitat, vb.) → §7.<id> + `CreateMvpContentMenu.cs`
- Narration script revize → §7.<id> script bölümü + yeni voice recording
- Kart tasarım kuralı eklendi → §5
- SFX ton revize → §11
- İllüstrasyon spec değişti → §12

### EngineerOnboarding
- Yeni faz eklendi → §3 faz tarihi
- Yeni dev görev pattern'i → §9
- Yeni convention veya gotcha → §11

### BuildAndDistribution
- Performance hedefi revize → §6
- Yeni cihaz desteklenmedi → §6 ARCore/ARKit cihaz listesi
- Store form cevabı değişti → §7

### DeviceTestProtocol
- Yeni edge case bulundu → §6
- Yeni test case → §3.x
- Performance ölçüm yöntemi değişti → §4

### PrivacyPolicy
- Mevzuat güncellemesi → ilgili tablo satırı
- Veri toplama politikası **asla** değişmesin — değişiyorsa tüm proje revize

### CLAUDE.md
- Asmdef değişikliği → "Code layout"
- Yeni convention → "Conventions" or "Content authoring conventions"
- Yeni doc eklendi → "Docs"

---

## 7. Feedback log şablonu

Pilot bittikten sonra doldurulacak tek-sayfa şablon:

```
PİLOT FEEDBACK LOG

Pilot tarih: __________
Sınıf / okul: __________
Eğitmen: __________
Gözlemci: __________
Build version: __________

ÖZET (3-5 cümle):
[Pilot'un genel akışı, dikkat çeken durumlar]

GÖZLEM RUBRİĞİ TOPLAMI:
1. Kartı rahat okutma: __ / 4
2. Sesli anlatım: __ / 4
3. Form yazımı: __ / 4
4. Rol rotasyonu: __ / 4
5. Birbirine soru: __ / 4
6. Eğitmen sorusu derinleşmesi: __ / 4
7. Quiz işbirliği: __ / 4

ÇOCUKLARDAN ALINAN GERİ BİLDİRİM:
[Şaşırtıcı bulduğu canlılar / en sevdiği aşama / "tekrar isterim" cevabı]

EĞİTMEN NOTLARI:
[Beklediğinden farklı giden, hangi soru cevap aldı, teknik sorun]

FEEDBACK ITEMS:
| # | Kategori | Severity | Aksiyon önerisi |
|---|---|---|---|
| 1 | __ | __ | __ |
| 2 | __ | __ | __ |
...

KARAR:
[Sonraki pilot için plan, blocker'lar, deadline]
```

---

## 8. Versioning ve değişim takibi

- Her doc'un sonunda **v1**, **v2** sürüm etiketi tut (zaten footer'larda var)
- Major content değişikliğinde sürüm bump
- CHANGELOG.md tutulabilir ama zorunlu değil — git log yeterli
- HTML versiyonu MD ile aynı sürümde tut

---

## 9. Pilot başarı kriterleri

Pilot programı bitirme kriterleri (3 pilot sonra değerlendir):

- [ ] Blocker severity ile karşılaşılmadı (son 3 pilotta)
- [ ] Çocukların %80+'i pilot sonunda "tekrar etmek isterim" dedi
- [ ] Eğitmenler "rehber yeterliydi" geri bildirimi verdi
- [ ] Gözlem rubriği ortalaması ≥3 (her madde)
- [ ] DeviceTestProtocol her cihazda PASS

Pilot başarı kriterleri sağlanırsa public release planı başlar.

---

## 10. Sonraki adımlar

1. Pilot N+1'i planla (eğitmen, sınıf, tarih)
2. Bu protokole göre veri topla
3. Triyaj + iterasyon
4. Repeat

Pilot başarı kriterleri sağlanırsa: `Docs/BuildAndDistribution.html` §10 release checklist.
