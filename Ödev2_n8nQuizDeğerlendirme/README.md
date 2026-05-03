# 🎓 Autonomous Education Analysis System

> **n8n & Generative AI ile Otonom Eğitim Analiz Sistemi**
>
> Eğitim süreçlerindeki geleneksel değerlendirme yöntemlerini modernize etmek amacıyla geliştirilmiş, n8n tabanlı uçtan uca otonom bir analiz ve geri bildirim sistemi.

---

## 🚀 Öne Çıkan Özellikler

- **Tam Otonom İş Akışı** — Veri toplama, analiz ve bildirim süreçleri insan müdahalesi olmadan saniyeler içinde gerçekleşir.
- **LLM Entegrasyonu (Gemini 2 Flash)** — Öğrencilere sadece puan değil, akademik eksikliklerine yönelik mentorluk düzeyinde geri bildirim sunar.
- **JavaScript Ön İşleme** — Yapay zekâ halüsinasyonlarını önlemek için veriler LLM'e gitmeden önce mantıksal bir süzgeçten (validation) geçer.
- **Dinamik Veri Yönetimi** — Analiz sonuçları eş zamanlı olarak Notion veri tabanına işlenerek izlenebilirlik sağlar.
- **Otomatik Bildirim** — Sonuçlar SMTP protokolü üzerinden anında öğrenci e-posta adresine iletilir.

---

## 🛠️ Teknoloji Yığını (Tech Stack)

| Katman | Teknoloji |
|---|---|
| Orkestrasyon | [n8n](https://n8n.io/) |
| Yapay Zekâ | Google Gemini API (Gemini 2 Flash) |
| Programlama | JavaScript (Data Processing & Filtering) |
| Veri Tabanı & Dashboard | Notion |
| Veri Girişi | n8n Form Trigger |
| İletişim | SMTP / Gmail API |

---

## 📐 Sistem Mimarisi

Sistem dört temel katmandan oluşmaktadır:

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│  1. DATA COLLECTION                                         │
│     n8n Form Trigger → Öğrenci yanıtlarını toplar          │
│              │                                              │
│              ▼                                              │
│  2. LOGIC LAYER                                             │
│     JavaScript Code Node → Ham veriyi işler,               │
│     doğru/yanlış matrisini çıkarır                         │
│              │                                              │
│              ▼                                              │
│  3. AI AGENT LAYER                                          │
│     Google Gemini API → "Mühendislik Koçu" personası       │
│     ile kişiselleştirilmiş analiz üretir                   │
│              │                                              │
│              ▼                                              │
│  4. OUTPUT & STORAGE                                        │
│     ├── Notion DB → Sonuçları kaydeder                     │
│     └── SMTP/Gmail → Öğrenciye e-posta gönderir            │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Katman Detayları

#### 1️⃣ Data Collection
Öğrenci yanıtlarının Form aracılığıyla sisteme aktarılması. n8n'in yerleşik Form Trigger node'u ile sıfır kodla veri toplama altyapısı kurulur.

#### 2️⃣ Logic Layer
JavaScript "Code Node" ile ham verinin işlenmesi ve doğru/yanlış matrisinin çıkarılması. Bu katman, LLM'e tutarsız veya eksik veri gitmesini engelleyen bir güvenlik filtresi görevi üstlenir.

#### 3️⃣ AI Agent Layer
Yapılandırılmış verinin Gemini API'ye gönderilerek **"Mühendislik Koçu"** personası ile analiz edilmesi. Model; öğrencinin güçlü yönlerini, eksikliklerini ve önerilen çalışma rotasını içeren kişiselleştirilmiş bir geri bildirim raporu üretir.

#### 4️⃣ Output & Storage
- **Notion:** Analiz sonuçları, öğrenci kimliği ve zaman damgasıyla birlikte veri tabanına yazılır. Eğitmenler Notion dashboard'u üzerinden tüm süreci takip edebilir.
- **E-posta (SMTP/Gmail):** Geri bildirim raporu, ilgili öğrenciye otomatik olarak iletilir.

---

## ⚙️ Kurulum & Kullanım

### Gereksinimler

- [n8n](https://n8n.io/) (self-hosted veya cloud)
- Google Gemini API anahtarı
- Notion entegrasyon token'ı ve veri tabanı ID'si
- SMTP sunucu bilgileri veya Gmail OAuth2 kimlik bilgileri

### Adımlar

1. **n8n workflow'unu içe aktarın**
   ```bash
   # n8n arayüzünde: Settings > Import Workflow
   # workflow.json dosyasını seçin
   ```

2. **Credentials tanımlayın**
   - `Google Gemini API` → API key ekleyin
   - `Notion` → Integration token & Database ID girin
   - `SMTP / Gmail` → Kimlik bilgilerini yapılandırın

3. **Workflow'u aktif edin**
   - n8n dashboard'undan workflow'u **Active** konumuna getirin
   - Form URL'ini öğrencilerle paylaşın

4. **Test edin**
   - Formu doldurun
   - Notion veri tabanını ve e-posta gelen kutusunu kontrol edin

---

## 🔐 Güvenlik & Gizlilik

- Öğrenci verileri yalnızca belirlenen Notion veri tabanında depolanır.
- LLM'e gönderilen veriler JavaScript katmanında anonimleştirilebilir.
- API anahtarları n8n'in şifreli Credentials deposunda saklanır; kod içine yazılmaz.

---

<p align="center">
  <em>Built with ❤️ using n8n · Gemini · Notion · JavaScript</em>
</p>
