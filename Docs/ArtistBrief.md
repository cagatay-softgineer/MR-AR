# Artist Brief — Görünenin Ötesinde Bir Deniz

> AR destekli deniz ekosistemi eğitimi (Unity 6 / AR Foundation 6.4).
> 7–12 yaş çocuk dostu içerik üretim brief'i — 20 deniz canlısı, 4 üretim hattı.

> **Sunum versiyonu**: `Docs/ArtistBrief.html` (self-contained, browser'da aç ya da PDF olarak yazdır). Sanatçı/tedarikçilere paylaşırken HTML tercih edilir; markdown bu repo içinde git diff için tutuluyor.

---

## 1. Proje Özeti

7–12 yaş çocuklar için sınıf etkinliği. Her grup (5 çocuk + 1 tablet) AR kartları tarar, canlının 3D modelini AR'da kartın üzerinde görür, sesli anlatımı dinler, ekosistem rolünü öğrenir. Etkinlik ~35–45 dakikadır.

Bu brief **4 ayrı üretim hattının** eşgüdümlü teslimini hedefler:

| Hat | Adet | Format |
|---|---|---|
| 3D model + idle animasyon | 20 | Unity Prefab (`.prefab`) — FBX'ten wrap edilmiş |
| Türkçe seslendirme anlatımı | 20 | WAV (mono, 22 kHz) |
| Basılı AR kart (tracker + tasarım) | 20 | A6 print PDF + 1024×1024 tracker PNG |
| UI ikonu | 20 | 256×256 transparan PNG |

**Toplam 80 asset.** Pilot fazda 3 canlı tamamlanır; geri kalan 17 batch'ler hâlinde teslim edilir (Bölüm 8).

---

## 2. Hedef Kitle ve Genel Sanat Yönü

**Yaş**: 7–12. Çocuklar her canlıyı **bakar bakmaz tanıyabilmeli** — bilimsel doğruluk + tanınabilirlik dengesi.

**Stil DNA'sı**:
- Sevimli ama bilimsel olarak tanınabilir. Sticker / illüstrasyon hissi — fotoğraf gerçekçiliği değil.
- Hafif antropomorfik dokunuş (canlı bir karakter hissi). Aşırı kawaii, aşırı büyük göz değil.
- **Korkutucu değil**. Köpekbalığı bile "vahşi yaşam" hissi vermeli, tehdit değil.
- Stilize düşük poly, temiz silüet, anlaşılır pose.

**Renk paleti** (genel öneri — habitat'a göre değişir):
- Resif canlıları: turuncu / sarı / kırmızı, doygun ama yumuşak
- Açık deniz: mavi-gri tonları, soft fade
- Derin deniz: koyu mor + selektif emissive vurgu (sadece fener balığı)
- Üreticiler (alg, çayır, plankton): pastel emerald yeşil

**Genel kaçınılacaklar**:
- Neon parlaklığı, aşırı saturasyon
- Photogrammetry / sub-surface scattering / gerçekçi shader
- Korkutucu pose veya ifade
- Cinsiyetli karakterleştirme (her canlı nötr cins)

---

## 3. Teknik Spec — 3D Modeller

| Alan | Spec |
|---|---|
| Teslim format | FBX dosyası + texture PNG (Unity dev prefab'a sarar) |
| Polygon bütçesi | **≤ 3,000 tris** (mobil AR performansı) |
| Texture | 512×512 albedo (PNG), opsiyonel 512×512 emissive |
| Material | URP/Lit shader, **tek material per model** |
| Pivot | Modelin transform pivot = (0, 0, 0) → kartın merkezinde |
| Yön | Z+ ekseni kamera-yönü; Y+ kart-dik |
| Doğal ölçek | scale = 1 iken model genişliği ≈ 1 unit (≈ 10 cm — kart genişliği) |
| Animasyon | Tek **Idle** loop, 1–2 saniye, seamless döngü |
| Animator Controller | Prefab'a embedded, tek state "Idle" |
| Collider | YOK |
| LOD | YOK |
| Klasör (Unity tarafı) | `Assets/ARFishing/Content/Models/` |
| Dosya adı | `creature_<creatureId>.prefab` (örn. `creature_octopus.prefab`) |

### Idle animation rehberi

| Tür | Animasyon karakteri |
|---|---|
| Balıklar (palyaço, vatoz, müren, vb.) | Dorsal/caudal fin dalgalanması + omurga hafif sinüs eğrisi |
| Ahtapot / mürekkep balığı | Kollar yumuşak dalgalanır, mantle nazikçe şişer-iner |
| Mercan / anemon | Polipler uzar-kısalır, kolonu hafif sallanır |
| Denizatı | Dorsal fin titreşim (5–10 Hz), kuyruk hafif kıvrım |
| Köpekbalığı / yunus | Tek-yönlü caudal propulsion (sallanma değil ileri yüzme hissi) |
| Plankton / alg | Nazik akıntı dalgalanması, asenkronize parçacık hareketi |
| Yengeç | Kıskaçların yumuşak açılma-kapanma, bacak hafif değişim |
| Deniz yıldızı / kestanesi | Çok hafif "nefes" + tüpe ayaklar opsiyonel hareket |

### Reject kriterleri
- 3,500+ tris
- Birden fazla material
- Animation loop'unda görünür pop / kesinti
- Pivot model dışında / kenarda
- Naked Standard shader (URP/Lit'e dönüştürülecek)

---

## 4. Teknik Spec — Ses Anlatımı

| Alan | Spec |
|---|---|
| Dil | Türkçe |
| Format | WAV (16-bit PCM) **veya** OGG, **mono**, **22 kHz** |
| Süre | İdeal **15–18 sn**, **maksimum 25 sn** (validator hard cap) |
| Voice talent | Profesyonel TR seslendirme — **proje genelinde tek ses** (tutarlılık için) |
| Ton | Anlatıcı, meraklı, sıcak — "hikaye anlat" hissi, "ders ver" değil |
| Hız | Çocuklar için ortalama hızdan biraz yavaş (~180–200 hece/dakika) |
| Background | Sessiz stüdyo kaydı, hum/hiss yok |
| Müzik / SFX | **YOK** — sadece kuru anlatım |
| Normalize | Peak −3 dB, ortalama −18 LUFS |
| Teslim format | WAV (lossless) — Unity dev OGG'ye gerekirse convert eder |
| Klasör (Unity) | `Assets/ARFishing/Content/Audio/` |
| Dosya adı | `narration_<creatureId>.wav` |

### İçerik kalıbı (her anlatım için akış)

1. **Tanıtım** (1–2 sn): "Bu canlı bir [isim]."
2. **Yaşam alanı** (3–4 sn): nerede yaşar
3. **Beslenme** (2–3 sn): ne yer
4. **İlginç özellik** (5–7 sn): "wow" anı
5. **Tehdit + koruma** (2–3 sn): kapanış mesajı

### Yasaklı dil
- "Ölüm", "yok olmak" → "azalmak", "tehlikede"
- Latin nomenklatür ("Octopus vulgaris" vb.)
- Soyut bilimsel terimler ("bioluminans" → "ışık saçar")
- Korkutucu ton ("yırtıcı", "tehlikeli avcı" yumuşatılmalı)

### Reject kriterleri
- 25+ sn süre
- Stereo dosya (mono'ya dönüştürülmesi gerekir)
- Background ortam sesi >−60 dB
- Müzik veya SFX yatağı
- Birden fazla voice talent (tutarlılık bozulur)

---

## 5. Teknik Spec — Basılı AR Kart

İki çıktı: (a) **basılı kart** (fiziksel ürün), (b) **tracker PNG** (Unity image library için).

### Basılı kart (fiziksel)

| Alan | Spec |
|---|---|
| Boyut | A6 (105 × 148 mm) |
| Stok | **350 gsm** karton |
| Laminasyon | **MAT** (parlak / yarı-parlak yasak — tracker bozulur) |
| Köşe | 1.5–2 mm radius yuvarlanmış |
| Kesim payı | 3 mm bleed her kenarda |
| Renk modu | CMYK |
| Çözünürlük | 300 DPI (1240 × 1748 px) |
| Ön yüz | Tracker pattern + canlı adı (alt bantta, Türkçe) |
| Arka yüz | Sade beyaz veya minimal bilgi metni (opsiyonel) |
| Teslim format | Print-ready PDF, 3 mm bleed + crop marks |

### Tracker source PNG (Unity image library)

| Alan | Spec |
|---|---|
| Boyut | 1024 × 1024 px (kare; orijinal kart aspect kırpılıp doldurulabilir) |
| Format | PNG, RGB (CMYK değil) |
| Klasör (Unity) | `Assets/ARFishing/Content/CardImages/` |
| Dosya adı | `card_<creatureId>.png` |

### Tracker pattern kuralları (zorunlu)

Bu kuralları ihlal eden kart **AR Foundation'da çalışmaz**:

- **Yüksek kontrast** — siyah-beyaz veya yüksek-saturasyon renkler arasında
- **Asimetrik kompozisyon** — rotational symmetry **yasak** (yıldız, eşit-dörtlü, kar tanesi vb.)
- **Repeating pattern yasak** — texture / wallpaper / mozaik desen yok
- **Geniş tek-renk alanı yasak** — >%30 solid background tracker'a feature vermez
- **Orta detay densitesi** — fazla detay yavaşlatır, az detay confidence düşürür
- **Köşelerde özgün marker** — köşelerdeki kontrast tracker'a oryantasyon verir

### İdeal kart kompozisyonu

```
┌─────────────────────────┐
│                         │ ← Üst %70:
│   [Stilize canlı       │   Stilize canlı çizimi
│    çizimi, asimetrik]   │   Çevresel detay (mercan, kum vb.) opsiyonel
│                         │   Asimetrik kompozisyon
│                         │
├─────────────────────────┤
│   AHTAPOT               │ ← Alt %30:
│   Octopus               │   TR adı büyük, bilimsel ad küçük (opsiyonel)
└─────────────────────────┘
```

### Reject kriterleri
- Parlak / yarı-parlak laminasyon
- Solid renk arka plan
- Rotational simetri
- 20 kartın hepsi aynı kompozisyon şablonunda (tracker karıştırır — her kart silüet bakımından özgün olmalı)

---

## 6. Teknik Spec — UI İkonları

| Alan | Spec |
|---|---|
| Boyut | 256 × 256 px |
| Format | PNG, transparan background |
| Stil | Kart çizimin **simplified versiyonu** — sticker hissi |
| Renk paleti | Kart imajıyla tutarlı |
| Klasör (Unity) | `Assets/ARFishing/Content/Icons/` |
| Dosya adı | `icon_<creatureId>.png` |
| Unity import | Sprite (2D and UI), Pixels Per Unit = 256, Mesh Type = Tight |

Kullanım: InfoPanel'in canlı ikonu, ProjectionPanel preview, Summary scanned grid chip'leri.

İdeal ikon: **silüet net**, arka plan transparan, gölge yok, 64×64'e küçültüldüğünde hâlâ tanınabilir.

---

## 7. 20 Canlı — Sanat Yönü ve TR Anlatım Scripti

Her canlı için: ID + display name + sanat yönü ipuçları + TR anlatım scripti (voice talent'a verilecek metin).

Anlatım scriptleri ~40–50 TR kelime, ~15–18 sn hedeflenmiştir. Voice talent / copywriter ton uyumu için hafifçe revize edebilir; **maksimum 25 sn aşılmamalıdır**.

---

### 1. `octopus` — Ahtapot

**Sanat yönü**: Yumuşak kırmızı-kahverengi tonlar. 8 kol zarif şekilde yayılır, mantle nefes alıyor gibi hafifçe şişer-iner. Hafif kamuflaj gradient'i (renk geçişi). Kompozisyon: spiral, kollar dışa açık.

**Narration**:
> Bu canlı bir ahtapot. Mercan resiflerinde, kayalıkların arasında yaşar. Yengeçler ve küçük balıklarla beslenir. En şaşırtıcı özelliği renk değiştirerek kamufle olabilmesi - hatta dar yarıklara bile sığar! Avlanma ve habitat kaybı onu tehdit ediyor.

---

### 2. `moon-jellyfish` — Ay Denizanası

**Sanat yönü**: Yarı şeffaf mavi-beyaz, kubbeli üst, dokunaçlar uzun ince dalgalanır. Hafif iç ışıltı (subtle emissive). Açık deniz boşluğunda süzülme hissi.

**Narration**:
> Bu canlı bir ay denizanası. Açık denizlerde, suyun içinde nazikçe süzülür. Süzerek beslenir - plankton ve küçük canlıları yakalar. Bazı denizanaları ışık saçabilir! Deniz kirliliği ve iklim değişikliği onları tehdit ediyor.

---

### 3. `clownfish` — Palyaço Balığı

**Sanat yönü**: Parlak turuncu + beyaz dikey şeritler + siyah kontur. Sevimli ifade, küçük yuvarlak göz. Arka planda anemon dokunaçları ipucu opsiyonel.

**Narration**:
> Bu canlı bir palyaço balığı. Mercan resiflerinde, anemonların arasında yaşar. Anemonun zehirli dokunaçlarından etkilenmez - ikisi birlikte yaşar! Plankton ve küçük canlıları yer. Mercan beyazlaması onların evini tehdit ediyor.

---

### 4. `great-white-shark` — Beyaz Köpekbalığı

**Sanat yönü**: Gri-mavi sırt, beyaz alt. Sleek torpedo silüet. Dişler hafifçe görünür ama **korkutucu değil** (ağzı kapalı veya hafif aralık). Güçlü ama dostça bir varlık.

**Narration**:
> Bu canlı bir beyaz köpekbalığı. Açık denizlerde dolaşır. Bir damla kanı kilometrelerce uzaktan algılayabilir! Çok güçlü bir yüzücüdür ve küçük balıklarla beslenir. Avlanma ve plastik kirliliği onları azaltıyor. Onları korumalıyız.

---

### 5. `seahorse` — Denizatı

**Sanat yönü**: Pastel sarı veya turkuaz. Karakteristik kıvrık kuyruk + at-başı silüeti. Küçük dorsal fin titreşim animasyonu.

**Narration**:
> Bu canlı bir denizatı. Mercan resiflerinde, deniz çayırlarına tutunarak yaşar. Süzerek küçük canlıları yer. En özel yanı: erkekler yumurtaları kendi kesesinde taşır ve yavruları dünyaya getirir! Avlanma ve habitat kaybı onları azaltıyor.

---

### 6. `moray-eel` — Müren

**Sanat yönü**: Yeşil-sarı gradient gövde, uzun ince silüet. Başı kayalıktan çıkmış pozisyon (opsiyonel arka plan ipucu). Hafif yılansı dalgalanma.

**Narration**:
> Bu canlı bir müren. Mercan resiflerinde kayalıkların arasında saklanır. Geceleri avlanır - balıkları ve ahtapotları yer. İki tane çenesi vardır - birini ileri uzatıp avını yakalar! Avlanma ve habitat kaybı onu tehdit ediyor.

---

### 7. `stingray` — Vatoz

**Sanat yönü**: Yumuşak gri-mavi sırt, beyaz alt. Geniş diamond silüet. Kuyrukta belirgin ama abartılı olmayan diken. Hafif kanat çırpma hareketi.

**Narration**:
> Bu canlı bir vatoz. Deniz tabanında, kumun altına saklanarak yaşar. Küçük balıkları ve kabukluları yer. Kuyruğundaki dikende zehir bulunur - bu onun savunmasıdır! Avlanma ve ağlara takılma onu tehdit ediyor.

---

### 8. `anglerfish` — Fener Balığı

**Sanat yönü**: Koyu mor / koyu mavi gövde. **Başındaki ışık emissive material** ile parlaklayan sarı (~1.5x base intensity). Geniş ağız belli ama abartılı değil — bilim olduğu kadar çocuk dostu. Karanlık derinlik hissi.

**Narration**:
> Bu canlı bir fener balığı. Güneş ışığının ulaşamadığı çok derin denizlerde yaşar. Başındaki ışıkla avlarını çeker - karanlıkta yanan küçük bir lamba gibi! Etle beslenir. Derin deniz balıkçılığı onları tehdit ediyor.

---

### 9. `parrotfish` — Papağan Balığı

**Sanat yönü**: Çok renkli (mavi-yeşil-turuncu degradesi). Papağan gibi karakteristik **gaga vurgusu**. Pulları belirgin.

**Narration**:
> Bu canlı bir papağan balığı. Mercan resiflerinde yaşar ve mercanları gagası gibi sert ağzıyla kemirir. Sindirdiği taşları kum hâline getirir - plajların kumu kısmen ondandır! Mercan beyazlaması habitatını tehdit ediyor.

---

### 10. `crab` — Yengeç

**Sanat yönü**: Kırmızı-turuncu kabuk, geniş kıskaçlar belli. Kabukta hafif doku detayı. Karakter pose'u: hafif yan duruş.

**Narration**:
> Bu canlı bir yengeç. Deniz tabanında, kayalıklar arasında yaşar. Yanlamasına yürür! Yiyebileceği her şeyle beslenir - ölü canlıları temizler. Sert kabuğu zırh gibi onu korur. Avlanma ve habitat kaybı onu tehdit ediyor.

---

### 11. `starfish` — Deniz Yıldızı

**Sanat yönü**: Parlak turuncu veya pembe-mor. 5 kol simetrik (rotational simetri **modelde OK** ama kart tasarımında değil — kart asimetrik çevre öğeleriyle dengelenmeli). Yüzeyde küçük doku detayı.

**Narration**:
> Bu canlı bir deniz yıldızı. Deniz tabanında, mercanların ve kayaların üzerinde yaşar. Midyeleri açıp içlerini yiyebilir! En şaşırtıcı özelliği: kopan bir kolunu yeniden büyütebilir. Kirlilik ve hastalıklar onları tehdit ediyor.

---

### 12. `mussel` — Midye

**Sanat yönü**: Koyu mavi-siyah dış kabuk, hafif aralık (içi görünür). İnci-beyaz iç. Suyu süzdüğünü gösteren çok hafif partikel akışı opsiyonel.

**Narration**:
> Bu canlı bir midye. Deniz tabanında, kayalara yapışarak yaşar. Suyu süzerek beslenir. Bir midye saatte litrelerce suyu temizleyebilir - denizin doğal filtresidir! Kirlilik ve aşırı toplama onları azaltıyor.

---

### 13. `squid` — Mürekkep Balığı

**Sanat yönü**: Pastel pembe-mor gradient. 10 dokunaç (8 kol + 2 uzun tentacle). Sleek torpido vücut. Yan fin'ler hafif dalgalanır.

**Narration**:
> Bu canlı bir mürekkep balığı. Açık denizlerde yüzer ve küçük balıklarla beslenir. Tehlike anında siyah mürekkep bulutu salar ve hızla kaçar - sihirli bir kaçış! Avlanma ve plastik kirliliği onları tehdit ediyor.

---

### 14. `sea-urchin` — Deniz Kestanesi

**Sanat yönü**: Koyu mor küre + uzun siyah dikenler. Geometrik, neredeyse minimalist. Dikenler hafif radyal hareket.

**Narration**:
> Bu canlı bir deniz kestanesi. Deniz tabanında, kayalıkların üzerinde yaşar. Algleri yer ve resifi temiz tutar - küçük ama önemli bir bahçıvan! Dikenleri onu korur. Asit kirliliği ve habitat kaybı onları tehdit ediyor.

---

### 15. `coral` — Mercan

**Sanat yönü**: Çok renkli koloni (sarı, pembe, turuncu kombinasyonu). Branching coral şekli + polip detayı. Hafif polip animasyonu (uzar-kısalır). Bu **resifin temel taşı**, görsel olarak güçlü bir varlık olmalı.

**Narration**:
> Bu canlı bir mercan. Sıcak ve sığ denizlerde, koloniler hâlinde yaşar. Süzerek beslenir. Aslında binlerce canlıya ev sahipliği yapan canlı kayalıklardır! Mercan beyazlaması ve iklim değişikliği mercanları azaltıyor.

---

### 16. `green-algae` — Yeşil Alg

**Sanat yönü**: Parlak emerald yeşil. Dalgalanan thallus / yaprak benzeri yapı. Su altı şaft hissi. Sudan ışıklar geçer (subtle emissive caustic dokunuş opsiyonel).

**Narration**:
> Bu canlı bir yeşil alg. Denizin yüzeyine yakın, güneş ışığının ulaştığı yerlerde yaşar. Güneş ışığıyla kendi besinini üretir ve denize oksijen verir! Tüm deniz canlılarının havasını sağlar. Kirlilik onları tehdit ediyor.

---

### 17. `seagrass` — Deniz Çayırı

**Sanat yönü**: Yumuşak yeşil bıçak yapraklar, hafif akıntı dalgalanması. Taban çayırı kompozisyonu. Aralarında küçük balıklar (opsiyonel art direction çevre öğesi).

**Narration**:
> Bu canlı bir deniz çayırı. Deniz tabanında sualtı çayırları oluşturur. Güneşten besin üretir. Bu çayırlar küçük balıklara saklanacak yer verir - sualtı parkı gibi! Gemi çapaları ve kirlilik onları azaltıyor.

---

### 18. `plankton` — Plankton

**Sanat yönü**: Stilize tek karakter (mikroskobik görünüm büyütülmüş) **veya** abstract küme (çok sayıda partikel). Açık mavi-yeşil. Her iki yön de kabul.

**Narration**:
> Bu canlı plankton. Gözle göremeyecek kadar küçüktür ama her yerdedir - milyarlarcası denizde süzülür! Güneşten besin üretir. Tüm deniz canlılarının yiyeceğinin başlangıcıdır. Deniz sıcaklığı artışı planktonu tehdit ediyor.

---

### 19. `sea-turtle` — Deniz Kaplumbağası

**Sanat yönü**: Yumuşak yeşil-kahve kabuk pattern. Geniş yüzgeçler. Sevimli yüz ifadesi (büyük ama doğal göz). Yüzme pozu: yüzgeçler yana açık.

**Narration**:
> Bu canlı bir deniz kaplumbağası. Açık denizlerde binlerce kilometre yüzer! Hem otçul hem etçildir. Doğduğu kıyıya yumurtlamak için geri döner - inanılmaz bir yolculuk! Plastik kirliliği ve avlanma onları tehlikeye atıyor.

---

### 20. `dolphin` — Yunus

**Sanat yönü**: Gümüş gri sırt + beyaz alt. Sleek torpedo silüet. **Gülümseyen ağız ifadesi** (yunus karakteristiği). Sıçrama veya yüzme pozu.

**Narration**:
> Bu canlı bir yunus. Açık denizlerde gruplar hâlinde yaşar. Bir memelidir - akciğerle nefes alır! Sesle birbiriyle konuşur ve birlikte avlanır. Plastik kirliliği ve gürültü onları tehdit ediyor. Denizleri sessiz tutalım.

---

## 8. Üretim Akışı

### Faz 1 — Pilot (3 canlı, ~2 hafta)

Stil rehberini valide etmek için 3 farklı **ekosistem rolü**nden örnek:
- `octopus` (Predator + Invertebrate + Reef)
- `coral` (ShelterProvider + Coral + Reef)
- `plankton` (Producer + Producer + Surface)

Tüm 4 üretim hattı bu 3 için tamamlanır. **Eğitmen + Unity dev review**. Stil rehberi finalize.

### Faz 2 — Geri kalan 17, batch'ler hâlinde

Tahmini 3–4 batch:

| Batch | Canlılar | Tema |
|---|---|---|
| 1 | clownfish, parrotfish, seahorse, moray-eel | Resif balıkları |
| 2 | great-white-shark, dolphin, moon-jellyfish, squid, sea-turtle | Açık deniz |
| 3 | crab, starfish, mussel, sea-urchin, stingray, seagrass | Deniz tabanı + üreticiler |
| 4 | anglerfish, green-algae | Derin deniz + son üretici |

Her batch teslimi sonrası **review checkpoint** (1–2 gün): Unity entegrasyonu + cihaz testi + revizyon listesi.

### Pilot testi

20 canlı tamamlandıktan sonra **3 sınıf pilot seansı**:
- 5 öğrenci + 1 tablet, 35–45 dk
- Eğitmen ve çocuk geri bildirimi toplanır
- Voice talent ton uyumu, kart tracking güvenilirliği, model net görünürlüğü değerlendirilir

---

## 9. Teslim Formatı

Artist'lerin shared cloud folder'a teslim ettiği yapı:

```
deliveries/
  3d-models/
    octopus/
      creature_octopus.fbx           # rigged + animated
      creature_octopus_albedo.png    # 512x512
      creature_octopus_emissive.png  # 512x512, opsiyonel
      preview.gif                    # 1-2 sn idle turntable, review için
  audio/
    octopus/
      narration_octopus.wav          # mono, 22 kHz, ≤25 sn
      transcript_octopus.txt         # okunan TR metin, kayıt sonrası finalize
  cards/
    octopus/
      card_octopus_print.pdf         # CMYK, A6 + 3mm bleed, crop marks
      card_octopus_tracker.png       # RGB, 1024x1024, Unity image library için
  icons/
    octopus/
      icon_octopus.png               # 256x256 transparent PNG
```

**Unity dev'in entegrasyon adımları** (sanatçının yapması gerekmez):
1. FBX'i Unity'e import → URP/Lit material assign → Animator wrap → prefab kaydet
2. Prefab'ı `CreatureDefinition.ModelPrefab` field'ına drag
3. WAV'ı Unity'e import → `CreatureDefinition.NarrationClip` field'ına drag
4. Icon PNG'yi import (Texture Type = Sprite) → `CreatureDefinition.Icon` field'ına drag
5. Tracker PNG'yi `CardLibrary.asset` entry'sine drag (entry name = `creatureId`)
6. `ARFishing → Validate All Creature Definitions` → temiz olmalı

---

## 10. Kabul Kriterleri

Bir canlının "tamamlandı" sayılması için **tüm** aşağıdaki kontrollerin geçmesi gerekir:

### Otomatik kontroller
- [ ] `ARFishing → Validate All Creature Definitions` → 0 hata
- [ ] `CreatureDefinition` asset'inde 4 referans dolu (ModelPrefab, NarrationClip, Icon, ReferenceImageName)
- [ ] Narration clip süresi ≤ 25 sn
- [ ] Model prefab tek material, ≤3,000 tris

### Manuel kontroller
- [ ] Editor'da XR Simulation environment'a tracker PNG eklenmiş, **2 farklı açıdan başarıyla algılanıyor**
- [ ] Gerçek Android tablet'te basılı kart **2 farklı ışıkta** (sınıf, hafif gölgeli) tracker'a takılıyor (>2 sn hold tracking)
- [ ] 3D model kart üzerinde **doğru ölçekte** görünüyor (~10 cm), pivot kart merkezinde
- [ ] Idle animation seamless döngü (pop yok)
- [ ] Anlatım sesi tabletin hoparlöründen **sınıf gürültüsünde anlaşılır**
- [ ] Anlatım scripti eğitmen tarafından bilimsel doğruluk açısından onaylanmış

### Pilot test kapısı
- [ ] 3 sınıf pilot testi tamamlandı
- [ ] Çocukların %80+'i kartı 5 sn içinde tarayabildi
- [ ] Eğitmen geri bildirimi entegre edildi

---

## 11. SFX Spec (Polish ses efektleri)

F9.1 fazında `SfxPlayer` service + 3 placeholder SFX scaffold'u kod tarafında hazır. Production'a geçişte gerçek SFX seti üretilmeli. Tek bir designer / composer üretmeli (brand consistency için).

### Genel teknik spec

| Alan | Spec |
|---|---|
| Format | WAV (16-bit PCM) **veya** OGG, **mono**, **22 kHz** |
| Süre | 0.05–1.0 sn (tek SFX) |
| Loop (ambient için) | OGG, seamless döngü |
| Normalize | Peak −6 dB (PlayOneShot ile mixed sound üst üste binebilir) |
| Klasör | `Assets/ARFishing/Content/Audio/Sfx/` |
| Dosya adı | `sfx_<id>.wav` |

### Genel ses yönü

- **Mood**: gentle, sea-themed, calming. Cartoonish overload yok (boing, slide whistle).
- **Çocuk dostu**: korkutucu / sert tonlar yok. Negatif feedback bile yumuşak (cezalandırıcı değil).
- **Tablet hoparlörü dostu**: heavy bass distort eder. Mid-range odaklı.
- **Sustained tone yok** (>1s): narration kesintisi yaratır.

### SFX listesi (öncelik sırasıyla)

| ID | Kullanım | Süre | Ton karakteri |
|---|---|---|---|
| `sfx_correct` | Quiz doğru cevap | ~0.4 s | Yükselen majör arpej, başarı hissi (placeholder C5-E5-G5) |
| `sfx_incorrect` | Quiz yanlış cevap | ~0.3 s | Alçalan, yumuşak (placeholder A4-F4 — dramatik minor değil) |
| `sfx_scan` | Kart algılandı | ~0.08 s | Kısa "ping", scan onayı (placeholder D6 high blip) |
| `sfx_tap` | UI button tap | ~0.05 s | Subtle click |
| `sfx_panel_open` | Panel slide-in (PanelTween) | ~0.2 s | Yumuşak swoosh |
| `sfx_panel_close` | Panel slide-out | ~0.2 s | Reverse swoosh |
| `sfx_quiz_start` | Quiz state'ine giriş | ~0.6 s | Drum hit + chime, "şimdi quiz" |
| `sfx_summary` | Summary'ye giriş | ~0.8 s | Triumphant chord, etkinlik sonu |
| `sfx_score_tick` | ScoreTextAnimator her sayım | ~0.04 s | Subtle tick (silent if loop irritating) |
| `sfx_creature_appear` | 3D model spawn | ~0.4 s | Magic shimmer, model belirme |
| `sfx_teacher_open` | Teacher panel açma (long-press tamamlandı) | ~0.15 s | Authority/professional tone |
| `sfx_long_press_charge` | Toggle long-press tick (~0.2s aralıklarla) | ~0.05 s | Build-up tick (3-4 kez tekrar 1 sn boyunca) |
| `sfx_idle_ambient` | Idle state background loop | ≤30 s loop | Sea ambient, **ultra subtle** (volume 0.15 max) |

### Reject kriterleri

- 1+ sn sustained tone
- Stereo dosya
- Heavy bass / sub-bass
- Cartoon-style FX (boing, slide whistle)
- Loud / piercing high frequencies (>3 kHz peak)
- Vocal samples (TR sözcükler narration'la çakışır)

### Teslim ve entegrasyon

Üretici WAV dosyalarını shared folder'a koyar. Unity dev:
1. WAV'ları `Assets/ARFishing/Content/Audio/Sfx/` altına import eder
2. `SfxPlayer` (Activity sahnesindeki Services GO altında) inspector'ından clip referanslarını gerçek dosyalarla replace eder (placeholder otomatik override edilmez — `Create Placeholder SFX` menüsü "skip existing" davranışında)
3. Volume slider'larından balans

---

## 12. 2D İllüstrasyon Spec

Idle splash + UI decoration için 2D illustration. F9'da `IdleBackgroundAnimator` procedural bubble particle üretir; production'da illustrated background + sahil dekorasyonu gerekir.

### Genel teknik spec

| Alan | Spec |
|---|---|
| Format | PNG (transparan veya solid), 24-bit |
| Renk modu | sRGB |
| Stil | Aynı palet ailesi (canlı kartlarıyla tutarlı line weight + saturation) |
| Klasör | `Assets/ARFishing/Content/UI/Illustrations/` |

### İhtiyaç listesi (öncelik sırasıyla)

| Asset | Kullanım | Boyut | Format | Öncelik |
|---|---|---|---|---|
| `illustration_idle_seascape` | Idle panel background | 2160×3840 (portrait tablet) | PNG | **P0** |
| `illustration_waves_top` | Top header decoration | 2160×400, tileable horizontal | PNG transparent | **P0** |
| `illustration_waves_bottom` | Bottom footer decoration | 2160×300 | PNG transparent | P1 |
| `illustration_quiz_thinking` | Quiz wait state visual | 512×512 | PNG transparent | P1 |
| `illustration_summary_celebration` | Summary screen art | 1024×1024 | PNG transparent | P1 |
| `illustration_mascot` | Proje mascot (opsiyonel) | 1024×1024 | PNG transparent | P2 |
| `illustration_parallax_far` | Idle background derin su layer | 2160×3840 | PNG | P2 |
| `illustration_parallax_mid` | Idle background orta layer | 2160×3840 PNG transparent | PNG transparent | P2 |
| `illustration_parallax_near` | Idle background ön layer | 2160×3840 PNG transparent | PNG transparent | P2 |

### Sanat yönü

- Aynı stil ailesi (canlı kartları + UI ikonlarıyla tutarlı palet + line weight)
- Pastel / yumuşak tonlar — overstimulating değil
- Türkiye sahil estetiği opsiyonel (kıyı şeridi, Akdeniz tonları, sıcak coral + cool blue)
- Bir bakışta tanınabilir (idle splash 3 sn'de "deniz keşfi" hissi vermeli)

### Idle seascape kompozisyonu

3-layer parallax önerisi (sahne wiring'inde kullanılabilir):

```
illustration_parallax_far       → derin mavi, uzak su, hafif silüetler
illustration_parallax_mid       → mid-water, geçen balıklar (silüet)
illustration_parallax_near      → ön plan dalgalar, su yüzeyi
illustration_idle_seascape      → tüm 3 layer flat tek PNG (parallax istenmezse)
```

P0 fazda tek PNG (`illustration_idle_seascape`) yeterli. P2 fazda 3 layer açıp `IdleBackgroundAnimator`'ı disable ederek statik parallax sahnesi.

### Reject kriterleri

- Photorealistic render
- Aşırı saturasyon / neon
- Korkutucu deniz canavarları (ahtapot bile sevimli)
- Çocuk olmayan figürler (sadece deniz canlıları)
- Marka/brand referansları (üçüncü taraf logo, karakter)

### Teslim ve entegrasyon

Üretici PNG dosyalarını shared folder'a koyar. Unity dev:
1. PNG'leri `Assets/ARFishing/Content/UI/Illustrations/` altına import
2. Sprite import settings: Sprite (2D and UI), Mesh Type = Full Rect, sRGB color space
3. IdlePanel'in arka planına Image component olarak ekle
4. P0 senaryoda `IdleBackgroundAnimator`'ı ya disable et ya da bubble alpha'sını düşür (illüstrasyonu kapatmasın)
5. P2 senaryoda 3 layer'ı ayrı child Image olarak ekle, basit `Update` animation ile parallax tetikle

---

## 13. Referanslar

Kod tarafı referansı:
- Veri kontratı: `Assets/ARFishing/Scripts/Creatures/CreatureDefinition.cs`
- Doğruluk validator: `Assets/ARFishing/Scripts/Editor/CreatureValidationTool.cs`
- MVP içerik scaffold: `Assets/ARFishing/Scripts/Editor/CreateMvpContentMenu.cs`
- SFX player: `Assets/ARFishing/Scripts/Narration/SfxPlayer.cs`
- Idle background: `Assets/ARFishing/Scripts/UI/IdleBackgroundAnimator.cs`
- Proje genel rehber: `CLAUDE.md` (kök) — "MVP creature content set (20)" ve "Physical card specs" bölümleri

**Sorular için iletişim**: Unity geliştirme tarafı asset entegrasyonunu yönetir; sanat yönü / kopya soruları için eğitsel danışman + ürün sahibi.
