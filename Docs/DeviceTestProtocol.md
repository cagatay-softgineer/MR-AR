# Cihaz Test Protokolü — Görünenin Ötesinde Bir Deniz

> Gerçek cihazlarda (Android + iOS tabletler) MVP build'in uçtan uca QA testi için adım-adım protokol. Pilot test öncesi ve her release döngüsü öncesi koşturulur.

> **Sunum versiyonu**: `Docs/DeviceTestProtocol.html`.

> Bu protokol Unity Editor'da değil, **gerçek tablette basılmış AR kartlarla** çalıştırılır. Üretici (sen veya QA ekibi) cihaz, kart seti ve test çekirdek malzemesi elindeyken uygular.

---

## 1. Test öncesi hazırlık

### 1.1 Test fleet

Minimum bir cihaz, ideal olarak 4 farklı device kategorisinde test:

| Kategori | Önerilen | Hedef metrik |
|---|---|---|
| Android orta-uç | Samsung Galaxy Tab A8, Xiaomi Pad 5 | 30 FPS min |
| Android üst-uç | Samsung Galaxy Tab S8, Google Pixel Tablet | 60 FPS hedef |
| iOS orta-uç | iPad 9. nesil | 30 FPS min |
| iOS üst-uç | iPad Air (M1+), iPad Pro | 60 FPS hedef |

### 1.2 Test materyali

- [ ] APK / iOS build cihaza yüklenmiş
- [ ] Tüm 20 basılı kart hazır (production print veya en azından placeholder print-out)
- [ ] Sınıf veya benzeri ortam aydınlatması (florasan + güneş ışığı karışımı)
- [ ] HDMI / USB-C-to-HDMI kablo (projeksiyon testi için, opsiyonel)
- [ ] Stopwatch (cold start ölçümü için)
- [ ] Network monitor (privacy stance doğrulama için, opsiyonel)

### 1.3 Test öncesi cihaz state

- [ ] Cihaz tam şarjlı (%90+) — battery drain ölçümü için
- [ ] Önceki uygulamalar kapalı (clean RAM baseline için)
- [ ] Wi-Fi açık ama uygulamanın hiçbir network çağrısı yapmaması gerek (privacy doğrulama)
- [ ] Uygulamanın önceki install state'i temiz (PlayerPrefs reset için)

---

## 2. Smoke Test (5 dk)

İlk hızlı kontrol — uygulama açılıyor mu, temel akış çalışıyor mu.

| # | Adım | Beklenen | Sonuç |
|---|---|---|---|
| 1 | Uygulamayı aç | Splash → Bootstrap → Activity sahnesi yüklenir | ☐ |
| 2 | İlk açılışta kamera permission dialog | "İzin Ver" → kamera açılır | ☐ |
| 3 | Idle ekranı görünür | "Görünenin Ötesinde Bir Deniz" + bubble animasyonu | ☐ |
| 4 | Sağ üst köşe ikonuna 1 sn basılı tut | Teacher Panel açılır | ☐ |
| 5 | "Etkinliği başlat" tıkla | Scanning state, idle panel kaybolur | ☐ |
| 6 | Bir karta (örn. octopus) kameray tut | Cube/ahtapot model spawn olur, InfoPanel açılır, ses çalar | ☐ |
| 7 | Kartı kameradan çıkar | 2 sn sonra InfoPanel kapanır | ☐ |
| 8 | Teacher Panel → "Mini quiz başlat" | Quiz panel görünür, modeller kaybolur | ☐ |
| 9 | Doğru kartı tara | Correct SFX + "Doğru!" feedback bounce | ☐ |
| 10 | 3 görev tamam | Summary panel + score count-up | ☐ |
| 11 | "Yeniden başlat" | Idle state'e döner | ☐ |

**Eğer herhangi bir adım fail ederse**, devam etme. Bug detayını kaydet:
- Cihaz model + OS sürümü
- Hangi adımda fail
- Console / logcat output (varsa)
- Reproduce step'leri

---

## 3. Functional Test (15 dk)

Detaylı uçtan uca akış. 2-3 tablette koş.

### 3.1 Tracker güvenilirliği

| # | Test | Beklenen | Sonuç |
|---|---|---|---|
| 1 | Kartı kameraya 30 cm mesafede tut | Tracker 2 sn içinde kilitlenir | ☐ |
| 2 | Kartı yavaşça eğ (45° tilt) | Tracker tracking devam eder | ☐ |
| 3 | Kartı kameradan hızla çek | Tracker Gone fires, model destroy | ☐ |
| 4 | Aynı kartı tekrar göster | Spotted fresh fires, panel re-show | ☐ |
| 5 | 2 farklı kartı yan yana tut | İkisi de tracking, biri focused (tek panel) | ☐ |
| 6 | Focused kartı kaldır | Diğer kart focus alır, panel switch | ☐ |
| 7 | Düşük ışıkta dene (kapalı perde) | Tracker hâlâ çalışır, biraz yavaş | ☐ |
| 8 | Parlak ışıkta dene (pencere kenarı) | Tracker çalışır, yansıma yoksa | ☐ |

### 3.2 Tüm 20 canlı

| Kart | Tracker | Model spawn | Ses çalar | Info Panel doğru |
|---|---|---|---|---|
| octopus | ☐ | ☐ | ☐ | ☐ |
| moon-jellyfish | ☐ | ☐ | ☐ | ☐ |
| clownfish | ☐ | ☐ | ☐ | ☐ |
| great-white-shark | ☐ | ☐ | ☐ | ☐ |
| seahorse | ☐ | ☐ | ☐ | ☐ |
| moray-eel | ☐ | ☐ | ☐ | ☐ |
| stingray | ☐ | ☐ | ☐ | ☐ |
| anglerfish | ☐ | ☐ | ☐ | ☐ |
| parrotfish | ☐ | ☐ | ☐ | ☐ |
| crab | ☐ | ☐ | ☐ | ☐ |
| starfish | ☐ | ☐ | ☐ | ☐ |
| mussel | ☐ | ☐ | ☐ | ☐ |
| squid | ☐ | ☐ | ☐ | ☐ |
| sea-urchin | ☐ | ☐ | ☐ | ☐ |
| coral | ☐ | ☐ | ☐ | ☐ |
| green-algae | ☐ | ☐ | ☐ | ☐ |
| seagrass | ☐ | ☐ | ☐ | ☐ |
| plankton | ☐ | ☐ | ☐ | ☐ |
| sea-turtle | ☐ | ☐ | ☐ | ☐ |
| dolphin | ☐ | ☐ | ☐ | ☐ |

### 3.3 Quiz akışı

| # | Test | Beklenen | Sonuç |
|---|---|---|---|
| 1 | Quiz başlat | 3 random görev seçilir, ilk prompt görünür | ☐ |
| 2 | Doğru cevap kartı tara | Correct SFX + "Doğru!" + 2 sn sonra next prompt | ☐ |
| 3 | Yanlış cevap kartı tara | Incorrect SFX + "Tam değil." + next prompt | ☐ |
| 4 | 3/3 cevap tamam | Summary panel görünür | ☐ |
| 5 | Score count-up animasyonu | 0'dan final puana ~1.2 sn | ☐ |
| 6 | Kapanış sorusu görünür | "Eğer bu canlılardan biri yok olursa..." | ☐ |
| 7 | Restart tıkla | Idle state'e döner, session sıfırlanır | ☐ |

### 3.4 Teacher mode

| # | Test | Beklenen | Sonuç |
|---|---|---|---|
| 1 | Kısa tıklama (top-right icon) | Hiçbir şey olmaz (child protection) | ☐ |
| 2 | 1 sn basılı tut | Panel açılır | ☐ |
| 3 | Idle state'inde panel | Sadece "Etkinliği başlat" görünür | ☐ |
| 4 | Scanning state'inde panel | "Mini quiz başlat" + "Yeniden başlat" | ☐ |
| 5 | Quiz state'inde panel | Sadece "Yeniden başlat" | ☐ |
| 6 | "Yeniden başlat" Quiz'den | Direkt Idle'a düşer | ☐ |
| 7 | "Kapat" | Panel kaybolur, state değişmez | ☐ |

### 3.5 Projeksiyon (opsiyonel)

| # | Test | Beklenen | Sonuç |
|---|---|---|---|
| 1 | HDMI / USB-C-to-HDMI bağla (uygulama açıkken) | 2 sn içinde ProjectionCanvas display 1'e route eder | ☐ |
| 2 | Idle state | Idle splash projeksiyonda görünür | ☐ |
| 3 | Scanning + card | Büyük preview projeksiyonda görünür | ☐ |
| 4 | Quiz | Prompt + progress projeksiyonda görünür | ☐ |
| 5 | Summary | Score + scanned grid projeksiyonda görünür | ☐ |
| 6 | HDMI çek | 2 sn içinde ProjectionCanvas gizlenir | ☐ |

---

## 4. Performance Test (10 dk)

| Metrik | Hedef | Ölçüm yöntemi | Sonuç |
|---|---|---|---|
| Cold start (uygulama tıklama → Activity ready) | <5 sn | Stopwatch | _____ sn |
| FPS (Viewing state'inde, model + camera) | ≥30 (orta), ≥60 (üst) | Unity Profiler / Android GPU Inspector | _____ FPS |
| RAM peak (45 dk session sonunda) | <600 MB | Settings → Memory / Xcode Memory Graph | _____ MB |
| Battery drain (45 dk session) | <%15 | Cihaz başında %90+ → bitiş % | %_____ |
| APK / IPA size | <150 MB | Cihaz Settings → Storage | _____ MB |
| Cold camera permission dialog | <1 sn ilk açılışta | Manuel | ☐ |

---

## 5. Privacy stance doğrulama (5 dk)

Bu çok kritik. Privacy policy'nin doğru olduğunu cihaz seviyesinde teyitle.

### 5.1 Network monitor

- [ ] Bir network monitor app yükle (örn. NetGuard for Android, Charles Proxy)
- [ ] Uygulamayı 5 dk kullan
- [ ] Network log'unda **hiçbir HTTP/HTTPS isteği görünmemeli**
- [ ] DNS lookup da olmamalı

### 5.2 Android permission check

- [ ] Settings → Apps → ARFishing → Permissions
- [ ] Sadece **Camera** permission listede olmalı
- [ ] Storage permission istemiyor olmalı
- [ ] **Internet permission yok olmalı** (eğer Internet kategorisi yoksa, doğru — manifest override çalışmış)

### 5.3 iOS permission check

- [ ] Settings → ARFishing
- [ ] Sadece **Camera** access listede
- [ ] Location, Photos, Contacts vb. **gözükmemeli**

### 5.4 PlayerPrefs sınırı

- [ ] Cihazda Editor olmadan, uygulamayı kaldırıp tekrar yükle
- [ ] İlk açılışta default state'te olmalı (önceki skor silinmiş)

---

## 6. Edge case'ler (10 dk)

| # | Test | Beklenen | Sonuç |
|---|---|---|---|
| 1 | Uygulama açıkken telefon ekran kilidi → tekrar aç | State korunur, kamera resume eder | ☐ |
| 2 | Telefon yan yatır (landscape) | Portrait modunda kalır (orientation lock) | ☐ |
| 3 | Düşük battery (%15) | Uygulama çalışmaya devam eder | ☐ |
| 4 | ARCore install yok (Android) | "ARCore yükle" prompt | ☐ |
| 5 | Karta lekeli/parmak izi → tracker test | Mat laminasyon korur, çalışır | ☐ |
| 6 | Aynı kartı 10 kez peş peşe tara | Memory leak yok (RAM stabil) | ☐ |
| 7 | Quiz tamamlanmadan uygulama kapansız | Re-open → Idle state'inde başlar (Quiz state restore yok, beklenen) | ☐ |
| 8 | Cihaz sessiz modunda | SFX + narration çalmaz, görsel akış sorunsuz | ☐ |

---

## 7. Bug raporlama şablonu

Bug bulundukça doldur:

```
Cihaz: [model + OS sürümü]
Build: [APK adı + version code]
Adım numarası: [Bölüm.alt-bölüm.satır]
Beklenen davranış: [...]
Gerçekleşen davranış: [...]
Reproduce step'leri:
1. ...
2. ...
3. ...
Frekans: [Her zaman / Bazen / İlk kez gördüm]
Logcat / Console output (varsa): [...]
Ek dosya: [video / screenshot path]
```

---

## 8. Sign-off

Pilot test izni için bu testin temiz olması gerekir.

- [ ] Smoke Test ✓
- [ ] Functional Test (tüm 20 canlı) ✓
- [ ] Quiz akışı ✓
- [ ] Teacher Mode ✓
- [ ] Performance hedefleri (30 FPS min, <600 MB RAM, <%15 battery) ✓
- [ ] Privacy stance doğrulandı (network monitor temiz) ✓
- [ ] Edge case'ler ✓

**Tester adı**: ___________
**Tarih**: ___________
**Build version**: ___________
**Sonuç**: ☐ PASS · ☐ FAIL (rapor ekte)

---

## 9. Sonraki adım

PASS sonrası:
1. Build artifact'i pilot test cihazlarına yükle
2. Pilot eğitmenle koordinat (`Docs/TeacherGuide.html`)
3. 3 sınıf pilot seansı planla
4. Pilot feedback toplama protokolü (TeacherGuide §6 + §7)

FAIL sonrası:
1. Bug listesi prioritize et
2. Severity blocker olan'ları code fix
3. Re-test full protocol
