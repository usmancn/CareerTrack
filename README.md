# 🎓 CareerTrack — Staj ve Mülakat Yönetim Sistemi

> Web Programlama Dersi Final Projesi  
> ASP.NET Core MVC (.NET 10) • EF Core Code-First • SQLite • Bootstrap 5

---

## 📸 Proje Hakkında

**CareerTrack**, öğrencilerin staj başvuru süreçlerini yönetmelerine yarayan kapsamlı bir web uygulamasıdır. Öğrenciler başvurularını, mülakat aşamalarını ve staj günlüklerini takip edebilir; akademisyenler ise tüm süreçleri genel istatistiklerle izleyebilir.

### Özellikler

| Rol | Özellikler |
|---|---|
| **Öğrenci** | Şirket başvurusu ekleme, mülakat aşamalarını kaydetme, staj defteri tutma, görev listesi |
| **Admin/Koordinatör** | Tüm öğrencilerin başvurularını görme, staj günlüklerini onaylama, şirket yönetimi |

---

## 🚀 Kurulum (Adım Adım)

### Gereksinimler

- .NET 10 SDK
- Git

### 1. .NET SDK Kurulumu

**macOS (Homebrew ile):**
```bash
brew install --cask dotnet-sdk
```

**Windows:**
> [dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0) adresinden yükleyiciyi indir ve çalıştır.

**Linux:**
```bash
# Ubuntu/Debian
wget https://dot.net/v1/dotnet-install.sh
bash dotnet-install.sh --channel 10.0
```

Kurulum sonrası kontrol:
```bash
dotnet --version  # 10.x.x çıkmalı
```

### 2. Projeyi Klonla

```bash
git clone https://github.com/KULLANICI_ADI/CareerTrack.git
cd CareerTrack
```

> `KULLANICI_ADI` kısmını kendi GitHub kullanıcı adınla değiştir.

### 3. Projeyi Çalıştır

```bash
# Paketleri yükle
dotnet restore

# EF Core aracını kur
dotnet tool install --global dotnet-ef

# PATH'e ekle (macOS/Linux — tek seferlik)
export PATH="$PATH:$HOME/.dotnet/tools"

# Veritabanını oluştur
dotnet ef migrations add InitialCreate
dotnet ef database update

# Çalıştır
dotnet run
```

Uygulama açılır: **http://localhost:5000**

---

## 🔐 Demo Hesapları

| Rol | E-posta | Şifre |
|---|---|---|
| **Admin / Koordinatör** | `admin@careertrack.com` | `Admin123!` |
| **Öğrenci** | `/Account/Register` ile kayıt ol | Kendin belirle |

---

## 🗃️ Veritabanı Şeması

```
ApplicationUser (ASP.NET Core Identity)
    ├── JobApplications (1-N)
    │       ├── Company (N-1)
    │       ├── Interviews (1-N)  ← Mülakat aşamaları
    │       └── Offer (1-1)       ← Teklif detayları
    ├── DailyLogs (1-N)           ← Staj defteri
    └── ToDos (1-N)               ← Görev listesi
```

---

## 📁 Klasör Yapısı

```
CareerTrack/
├── Controllers/          ← 8 controller (Account, Dashboard, Application...)
├── Models/
│   ├── Entities/         ← EF Core entity sınıfları
│   ├── Enums/            ← InternshipType, ApplicationStatus, InterviewStage...
│   └── ViewModels/       ← StudentDashboardViewModel, LoginViewModel...
├── Data/
│   ├── ApplicationDbContext.cs   ← EF Core DbContext + Fluent API
│   └── SeedData.cs               ← Rol ve admin seed
├── Views/
│   ├── Shared/_Layout.cshtml     ← Ortak sidebar + header
│   ├── Dashboard/
│   ├── Application/
│   ├── Interview/
│   ├── Offer/
│   ├── DailyLog/
│   ├── ToDo/
│   ├── Admin/
│   └── Account/
├── wwwroot/              ← CSS, JS
├── Program.cs
├── appsettings.json
└── CareerTrack.csproj
```

---

## ✅ Teknik Gereksinimler (Hoca Kriterleri)

| Kriter | Detay |
|---|---|
| **EF Core Code-First** | `ApplicationDbContext` + Fluent API + `dotnet ef migrations` |
| **Role-Based Auth** | `[Authorize(Roles="Admin")]` & `[Authorize(Roles="Student")]` |
| **_Layout.cshtml** | Sol sidebar + üst header — tüm sayfalarda ortak |
| **ViewModel** | `StudentDashboardViewModel` → 3 tablodan veri tek sınıfta |
| **ViewBag** | `ViewBag.PendingTodos`, `ViewBag.UpcomingInterviews` → header badge |
| **Data Annotations** | `[Required]`, `[StringLength]`, `[DataType]`, `[Range]` |
| **ModelState.AddModelError** | Geçmiş tarih engeli, gün tekrarı kontrolü |
| **LINQ** | `Where`, `OrderBy`, `Count`, `Take`, `Include` |
| **Enum** | InternshipType, ApplicationStatus, InterviewStage, InterviewResult |
| **1-N İlişki** | `JobApplication` → `Interview` |
| **1-1 İlişki** | `JobApplication` → `Offer` |

---

## 🛠️ Sorun Giderme

**`dotnet ef` bulunamıyor:**
```bash
dotnet tool install --global dotnet-ef
export PATH="$PATH:$HOME/.dotnet/tools"
```

**Veritabanını sıfırla:**
```bash
rm CareerTrack.db
dotnet ef database update
```

**Port zaten kullanımda:**
```bash
dotnet run --urls "http://localhost:5001"
```

---

## 👥 Ekip

> Web Programlama Dersi — 2025-2026 Bahar Dönemi
