# Build & Distribution — Görünenin Ötesinde Bir Deniz

> Production build pipeline: Android (ARCore) + iOS (ARKit), signing, store/MDM/TestFlight/enterprise dağıtım, privacy compliance, sorun giderme.

> **Sunum versiyonu**: `Docs/BuildAndDistribution.html`.

> Bu doküman dev içindir. Sınıf etkinliği için `Docs/TeacherGuide.html`, repo turu için `Docs/EngineerOnboarding.html`.

---

## 1. Bu doküman ne içerir

Production build path'leri (Android + iOS), keystore/provisioning signing, store / MDM / enterprise distribution kanalları, privacy compliance (KVKK / GDPR / COPPA) ve sorun giderme.

**Şu an ne var, ne yok**:
- ✅ Kod F1-F8 tamamlanmış, MVP code-complete
- ✅ Placeholder asset'ler tüm 20 canlı için hazır
- ❌ Production identifier'lar set değil (hâlâ template placeholder)
- ❌ Signing keystore yok
- ❌ Custom AndroidManifest yok
- ❌ Camera Usage Description set değil
- ❌ CI/CD yok

Bu dokümanın amacı bu eksikleri kapatmak ve cihaza ilk ulaşımı sağlamak.

---

## 2. Tek seferlik proje ayarları (Pre-build checklist)

### 2.1 Product Identifier'ları güncelle

`ProjectSettings/ProjectSettings.asset`'te `applicationIdentifier` hâlâ template placeholder:

```yaml
applicationIdentifier:
  Android: com.unity.template.ar_mobile
  Standalone: com.unity.template.ar-mobile
  iPhone: com.unity.template.armobile
```

Unity Editor üzerinden değiştir:
- **File → Build Profiles → Player Settings**
- Sol panelde Android sekmesi → Other Settings → "Identification" → **Package Name** =`com.your-org.arfishing`
- iOS sekmesi → Other Settings → **Bundle Identifier** = `com.your-org.arfishing`

> Production ID'yi seçtikten sonra **bir daha değiştirme**. Play Store / App Store package name'i primary key olarak kullanır; sonradan değişirse ayrı bir uygulama olarak görünür.

### 2.2 Versioning

`Bundle Version` = `1.0.0` (semantic versioning). Her build için bumplama:

| Field | Anlamı | Bumplanma |
|---|---|---|
| Bundle Version | Görünür sürüm | Major.Minor.Patch — release notes anlamlı |
| Android Version Code | Internal int, store sıralaması | Her build için +1 |
| iOS Build (CFBundleVersion) | Internal | Her build için +1 |

Player Settings'te ayrı ayrı.

### 2.3 App icon + splash screen

- **App icon**: Player Settings → Icon → 1024×1024 sahip ana ikon. Unity AR Mobile template default icon var, değiştir.
- **Splash screen**: Player Settings → Splash Image → "Show Unity Logo" = OFF (Pro license ile), brand logo ekle.

### 2.4 Color space

Player Settings → Other Settings → **Color Space = Linear** (URP gereği, default zaten Linear olmalı; gamma'ya alma).

### 2.5 Scripting backend

Player Settings → Other Settings → **Scripting Backend = IL2CPP** (Android + iOS için zorunlu — AR Foundation Mono'ya destek vermez).

---

## 3. Android (ARCore)

### 3.1 Player Settings

| Ayar | Değer |
|---|---|
| Orientation | Portrait |
| Minimum API Level | Android 7.0 (API 24) — ARCore minimum |
| Target API Level | Android 14 (API 34) veya en güncel |
| Scripting Backend | IL2CPP |
| Target Architectures | ARM64 (Play Store 64-bit requirement) |
| Internet Access | **Auto** (Require yapma — privacy stance ihlali) |
| Write Permission | Internal Only |
| Install Location | Automatic |

### 3.2 XR Plug-in Management

- Edit → Project Settings → XR Plug-in Management
- **Android sekmesi** → **AR Core Loader = enabled**
- Simulation Loader = enabled (editor için, build'de etkilenmez)

### 3.3 Keystore oluşturma + signing

Production build için signing keystore zorunlu.

**Komut satırından** (önerilen):

```bash
keytool -genkeypair -v \
  -keystore arfishing-release.keystore \
  -alias arfishing \
  -keyalg RSA -keysize 2048 -validity 10000
```

Sorulan değerleri doldur (CN, O, L, vb.). Şifreyi kaydet — **kaybedersen Play Store'a yeni sürüm yükleyemezsin**.

**Unity'de signing'i bağla**:
- Player Settings → Publishing Settings
- Use Custom Keystore = ON
- Keystore Path = `<path>/arfishing-release.keystore`
- Keystore Password
- Key Alias = `arfishing`
- Key Password

**Keystore yedekleme** (kritik):
- Şifrelenmiş offline backup (örn. Bitwarden, 1Password)
- Bu dosyayı **git'e commit etme** — `.gitignore`'a ekle

### 3.4 Custom AndroidManifest.xml

Privacy stance'ı korumak için `INTERNET` permission'ı override etmemiz gerek (AR Foundation transitive dep'i olarak gelebilir).

`Assets/Plugins/Android/AndroidManifest.xml` oluştur:

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android"
          xmlns:tools="http://schemas.android.com/tools">

  <!-- Privacy: remove INTERNET permission if any dependency declared it -->
  <uses-permission android:name="android.permission.INTERNET" tools:node="remove" />

  <!-- ARCore feature requirement: device filter to AR-capable Android phones/tablets -->
  <uses-feature android:name="android.hardware.camera.ar" android:required="true" />

  <application>
    <!-- ARCore marker: "required" means app won't install on non-AR device -->
    <meta-data android:name="com.google.ar.core" android:value="required" />
  </application>
</manifest>
```

**ARCore required vs optional**:
- `required`: Play Store sadece ARCore destekli cihazlara gösterir. Uygulama açılınca ARCore service otomatik install dialog'u tetikler. Bu projeye uygun (AR olmadan kullanışlı değil).
- `optional`: Tüm cihazlara install edilebilir, ama AR feature'lar çalışmaz. Uygun değil.

### 3.5 Build

- File → Build Profiles → Android
- **Build** (APK, test için): `arfishing-1.0.0.apk`
- **Build with Active Plug-Ins** (AAB, Play Store için): `arfishing-1.0.0.aab`
- **Build & Run**: cihaz USB ile bağlıysa direkt deploy

Build sonrası ilk test: cihaza yükle, açıldığında ARCore install prompt'u gelmeli (eğer ARCore yüklü değilse), camera permission diyalog'u gelmeli, ardından Bootstrap → Activity.

### 3.6 Android dağıtım kanalları

| Kanal | Hedef | Avantaj | Dezavantaj |
|---|---|---|---|
| **Google Play Store** | Kamu, okul/öğretmen indirebilir | Geniş erişim, otomatik güncelleme | İnceleme süreci, ratings, yaş izninin altındaki kullanıcılar için ek formlar |
| **Direct APK** | Okul IT direct yükleme | Tek tek kontrollü | Manuel her cihaz için |
| **Android Enterprise (MDM)** | Okul/kurum filo yönetimi | Merkezi yayın, otomatik install | MDM setup overhead (Microsoft Intune, VMware AirWatch, vb.) |
| **Google Play managed** | Kurum içi private listing | Hibrit yaklaşım — store + kontrol | Play Console Enterprise hesabı |

Pilot test için: önce **Direct APK**, sonra production scale için **Google Play** veya **MDM**.

---

## 4. iOS (ARKit)

### 4.1 Pre-requisites

- macOS makinası (Windows'ta iOS build edilemez)
- Xcode 15+ (Unity 6 ile uyumlu)
- Apple Developer hesabı ($99/yıl, education account önerilir)

### 4.2 Player Settings

| Ayar | Değer |
|---|---|
| Orientation | Portrait |
| Bundle Identifier | `com.your-org.arfishing` |
| Camera Usage Description | "Bu uygulama AR kartları okumak için kamerayı kullanır." |
| Minimum iOS | 13.0 (ARKit 3+ destek için) |
| Target Device Family | iPhone & iPad |
| Scripting Backend | IL2CPP |

**Camera Usage Description kritik** — eksikse uygulama App Store'da reject edilir ve cihazda camera erişimi reddedilir.

### 4.3 XR Plug-in Management

- iOS sekmesi → **AR Kit Loader = enabled**

### 4.4 Build Xcode project

- File → Build Profiles → iOS
- Build → çıktı: `arfishing.xcodeproj` (klasör)
- Xcode'da aç

### 4.5 Xcode signing & capabilities

Unity'nin Xcode proje generation'ı her seferinde overwrite edebilir. Signing'i Xcode tarafında yapacaksın:

1. Xcode → proje root → **Signing & Capabilities** sekmesi
2. **Team**: Apple Developer hesabını seç
3. **Provisioning Profile**: Automatic (development için) veya Manual (TestFlight/distribution için)
4. **Bundle Identifier** = Unity'deki ile aynı olmalı
5. ARKit capability otomatik eklenmiş olmalı (Unity Build sırasında ekler)

### 4.6 Build & Run / Archive

- Build & Run cihaza deploy (development)
- Product → Archive → Organizer → Distribute App (TestFlight, App Store, Ad Hoc, Enterprise)

### 4.7 iOS dağıtım kanalları

| Kanal | Hedef | Avantaj | Dezavantaj |
|---|---|---|---|
| **App Store** | Kamu | Geniş erişim | Stricter review (özellikle children's app + AR) |
| **TestFlight** | Beta test grubu | Hızlı iteration, 90 gün test | 90 gün limit, sadece test |
| **Ad Hoc** | Belirli UDID listesi | Manual control | 100 cihaz limit |
| **Apple Enterprise** | Org-internal | Centralized, store dışı | $299/yıl, sadece kurum içi kullanım |
| **Apple School Manager** | K-12 okul | Education-specific, Volume Purchase | Education hesabı gerekir |

Pilot test için: **TestFlight** (90 günlük internal/external tester pool). Production'da: **App Store** veya okul partneri varsa **Apple School Manager**.

---

## 5. Build size optimization

Tablet için ~100 MB ideal, ~200 MB üst limit.

| Asset türü | Ayar |
|---|---|
| Texture | ASTC (Android) / ASTC (iOS) — modern device, mobile için ideal |
| Texture max size | 1024 (placeholder), production'da 2048 kabul |
| Texture mipmap | ON (3D models için) |
| Audio | Vorbis OGG, Compressed in Memory, Force to Mono |
| Audio quality | 70 (placeholder için), production'da 100 |
| Mesh compression | Medium (LOD yok zaten) |
| Strip Unused Mesh Components | ON |
| Managed Stripping Level | Medium |
| Engine Code Stripping | ON (Player Settings → Other Settings) |

20 canlı için tahmin: 20 × 200 KB (3D model + texture) + 20 × 80 KB (audio) + 20 × 30 KB (icon) ≈ 6.2 MB content. Toplam APK ~40-60 MB beklenir.

---

## 6. Performance hedefleri

| Metrik | Hedef | Min kabul |
|---|---|---|
| FPS | 60 (yüksek-uç) | 30 (orta-uç) |
| Cold start (Bootstrap → Activity hazır) | <3 sn | <5 sn |
| RAM peak | <400 MB | <600 MB |
| Battery drain | <%10 per 45 dk seans | <%15 |
| APK size | <80 MB | <150 MB |
| AAB size | <60 MB | <120 MB |

Test cihazları (önerilen MVP test fleet):
- **Android orta-uç**: Samsung Galaxy Tab A8, Xiaomi Pad 5
- **Android üst-uç**: Samsung Galaxy Tab S8, Google Pixel Tablet
- **iOS orta-uç**: iPad 9th gen
- **iOS üst-uç**: iPad Air (M1+) veya iPad Pro

ARCore destekli cihaz listesi: https://developers.google.com/ar/devices
ARKit destekli cihaz listesi: iPhone 6s+ veya iPad 5th gen+

---

## 7. Privacy compliance (KVKK / GDPR / COPPA)

Projenin **"zero data collected"** stance'ı tüm regülasyon riskini ortadan kaldırır. Yine de store listing'de açıkça beyan edilmesi gerekir.

### Google Play Console

- **Data safety form**:
  - Data collected? **No**
  - Data shared? **No**
  - Children's content? **Yes** (7-12 yaş)
  - Designed for Families program eligible? **Yes**
- **Privacy policy URL**: zorunlu — minimal bir privacy policy host et (bir GitHub Pages sayfası yeterli):
  > "Bu uygulama hiçbir kişisel veri toplamaz, depolamaz veya paylaşmaz. İnternet bağlantısı gerektirmez."

### App Store Connect

- **App Privacy section** → "Data Not Collected" seç
- **App's Privacy Policy URL** (privacy policy zorunlu — Google Play ile aynı)
- **Age Rating**: 4+ (no objectionable content)
- **Made for Kids** programı için ek anket

### KVKK (Türkiye, 18 yaş altı)

- 6698 sayılı kanun: 18 yaş altı veri toplama açık veli rızası gerektirir
- **Toplamadığımız için bu yükümlülük yok**, ama proje broşüründe / okul protokolünde açıkça yaz: "uygulama herhangi bir kişisel veri toplamamaktadır"
- Veri kaynaklı denetim için **VERBİS kayıt yok**

---

## 8. CI/CD (henüz kurulmadı)

Şu an manual build yapıyoruz. İlerleyen aşamada CI önerileri:

| Opsiyon | Pros | Cons |
|---|---|---|
| **Unity Cloud Build** | Built-in, Unity hesap-bağlantılı, signing destekli | Free tier sınırlı, vendor lock |
| **GitHub Actions** + Unity-Activate | OSS, cross-platform, GitHub ile entegre | Self-hosted runner gerekir (macOS iOS için) |
| **Jenkins** + Unity CLI | Tam kontrol, self-hosted | Setup overhead, maintenance |
| **GitLab CI** | GitHub Actions benzeri ama GitLab'de | GitLab repo gerekir |

**Privacy stance kuralı**: CI ekleniyorsa **Unity Cloud Diagnostics, Unity Analytics, Unity Performance Reporting** entegrasyonlarını **kapatmak zorunlu**. CI build env vars'ta `UNITY_ENABLE_ANALYTICS=0` gibi flag'ler.

CI ile başlamayı erteleyebilirsin — manuel build yeterli 1-2 release için.

---

## 9. Sorun giderme

### Android

**"ARCore not available on this device"**
- Cihazın ARCore desteğini kontrol et: https://developers.google.com/ar/devices
- Eğer cihaz listede yoksa AR çalışmaz, başka cihazda test et

**"Build failed: Could not find android.support..."**
- AndroidX migration eksik olabilir → Player Settings → Other Settings → "Use Jetpack" = ON

**"Keystore was tampered with"**
- Keystore şifresi yanlış girilmiş. Şifreyi backup'tan al

**"Failed to install APK on device"**
- Cihazda USB Debug aktif değil → Settings → Developer Options → USB Debugging ON
- Bilgisayar tarafında ADB driver eksik → Android Studio yüklü mü kontrol

**APK aniden büyüdü**
- Unused asset'ler import edilmiş olabilir → Window → Analysis → Profiler → Memory
- `Resources/` klasöründe bilmediğin şey var mı kontrol

### iOS

**"App not signed for distribution"**
- Xcode → Signing & Capabilities → Team seçili mi
- Provisioning profile expired mı

**"Camera Usage Description missing"**
- Unity Player Settings → iOS → Other Settings → **Camera Usage Description** doldur ve **rebuild Xcode proje**

**TestFlight'a yüklendikten sonra "Invalid Binary"**
- IL2CPP backend seçili mi
- ARKit capability Xcode'da aktif mi
- Bitcode = NO (Unity 6 default'u doğru)

**ARKit cihazda crash**
- iOS 13 minimum mu Player Settings'te
- ARKit framework Xcode → Build Phases → Link Binary With Libraries'de var mı

---

## 10. İlk release checklist

Production release önce şunlar:

- [ ] `applicationIdentifier` template placeholder değil
- [ ] `bundleVersion` ve `versionCode` set
- [ ] App icon ve splash brand'leri yansıtıyor
- [ ] Android: Custom AndroidManifest.xml yok ya da Internet permission removed
- [ ] Android: ARCore meta-data "required"
- [ ] Android: Keystore yedeklenmiş (offline)
- [ ] iOS: Camera Usage Description doldurulmuş
- [ ] iOS: Provisioning profile geçerli
- [ ] `ARFishing → Validate All Creature Definitions` 0 hata
- [ ] 20 canlının tümü gerçek asset'lerle dolu (validator menü ile teyit)
- [ ] Privacy policy URL host edildi
- [ ] Test cihazlarında uçtan uca akış doğrulandı
- [ ] Performance metrikleri hedeflere uygun
- [ ] Release notes hazır
- [ ] Store listing screenshots + açıklama metinleri hazır

---

## 11. Sonraki adımlar

1. **İlk testflight/internal track release**: pilot test için 5-10 cihazlık dağıtım
2. **3 sınıf pilot testi**: `Docs/TeacherGuide.html` rubrik kullanılarak
3. **Pilot geri bildirimi ile iterasyon**: belki F9 polish, belki content revize
4. **Public release**: Play Store + App Store + okul partner network'ü

İterasyon süresi: pilot → public release tipik 4-8 hafta. Children's category review Apple App Store'da uzun sürebilir (2-3 hafta).
