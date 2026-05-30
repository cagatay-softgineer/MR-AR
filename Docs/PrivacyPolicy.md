# Gizlilik Politikası — Görünenin Ötesinde Bir Deniz

**Son güncelleme**: 2026-05-29

> **Sunum versiyonu**: `Docs/PrivacyPolicy.html` — store listing'inde URL olarak referans edilecek versiyon (örn. GitHub Pages üzerinde host).

---

## Özet

Bu uygulama **hiçbir kişisel veri toplamaz, depolamaz veya paylaşmaz**. İnternet bağlantısı gerektirmez.

---

## Detay

### Toplanan veriler

**Hiçbiri.** Uygulama:
- Kullanıcı kimliği (ad, e-mail, ID) toplamaz
- Konum bilgisi toplamaz
- Demografik bilgi toplamaz
- Davranış metriği toplamaz
- Kullanım istatistiği toplamaz
- Çökme raporu göndermez
- Cihaz bilgisi (model, OS sürümü, IP) toplamaz
- Herhangi bir tanımlayıcı (advertising ID, instance ID) toplamaz

### Üçüncü taraf SDK'lar

**Yok.** Uygulamada Firebase, Google Analytics, Facebook SDK, Adjust, AppsFlyer, Unity Analytics veya benzeri telemetri / analytics / advertising SDK'sı bulunmaz.

### Network kullanımı

**Yok.** Uygulama çalışırken hiçbir HTTP / HTTPS isteği yapmaz. Android için `INTERNET` permission `AndroidManifest.xml`'den kaldırılmıştır. Uygulama tamamen offline çalışır.

### Yerel veri saklama

Cihazda yerel olarak saklanabilen tek veri: opsiyonel grup adı ve son etkinlik puanı (`PlayerPrefs` üzerinden). Bu veriler:
- Cihazdan çıkmaz
- Ağ üzerinden iletilmez
- Başka bir uygulamayla paylaşılmaz
- Uygulama kaldırıldığında silinir

### Çocuk kullanıcılar

Uygulama 7–12 yaş hedef kitleli bir eğitim aracıdır. Çocuklardan herhangi bir kişisel veri toplanmadığı için **açık veli rızası gerekmez**.

### Mevzuat uyumu

| Mevzuat | Durum |
|---|---|
| **KVKK** (6698 sayılı kanun, Türkiye) | Veri toplama yok → VERBİS kayıt yükümlülüğü yok |
| **GDPR** (EU 2016/679) | Veri toplama yok → veri sahibi hakları konu olmaz |
| **COPPA** (US, 16 CFR Part 312) | Under-13 verisi toplama yok → onay gerekmez |
| **Apple App Store** | "Data Not Collected" deklarasyonu |
| **Google Play Data Safety** | "No data collected, no data shared" |

### Veri talepleri

Hiçbir veri toplanmadığı için erişim, silme, düzeltme veya aktarım talebi konusu oluşmaz.

### Politika değişiklikleri

Politika güncellenirse bu sayfanın "Son güncelleme" tarihi yenilenir. Materyal değişiklikler store listing'lerinde de güncellenir.

### İletişim

Bu politikaya dair sorular için: **[iletişim e-postası — store submission öncesi doldurulacak]**

---

## Store Listing Cevap Şablonu

### Google Play Console — Data safety form

- Data collected? → **No**
- Data shared with third parties? → **No**
- Designed for Families program eligible? → **Yes**
- Children's content (Ages 5–12)? → **Yes**

### App Store Connect — App Privacy

- Data Used to Track You → **None**
- Data Linked to You → **None**
- Data Not Linked to You → **None**

→ Tüm sorulara "Data Not Collected" cevabı.

### Privacy Policy URL

Bu sayfanın hosted URL'sini (örn. `https://example.org/arfishing/privacy`) Apple + Google formlarında zorunlu URL alanına yapıştır.
