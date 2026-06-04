# 🎓 CareerTrack — Staj ve Kariyer Takip Sistemi

> Web Programlama Dersi Final Projesi  
> ASP.NET Core MVC (.NET 10) • EF Core Code-First • SQLite • Bootstrap 5

---

## 📸 Proje Hakkında

**CareerTrack**, öğrencilerin staj başvuru süreçlerini yönetmelerine yarayan kapsamlı bir web uygulamasıdır. Öğrenciler ilanlara başvurabilir, işveren değerlendirme aşamalarını takip edebilir, okul onayı sonrasında staj günlüklerini tutabilir ve görevlerini yönetebilir.

### Özellikler

| Rol | Özellikler |
|---|---|
| **Öğrenci** | İlan veya şirket başvurusu oluşturma, süreç aşamalarını takip etme, staj defteri tutma, görev listesi |
| **İşveren** | İlan yayınlama, başvuruları değerlendirme, staj görevleri atama, günlükleri onaylama |
| **Okul** | Öğrencileri takip etme, işveren kabulü sonrası stajı onaylama, günlükleri onaylama veya revize isteme |
| **Admin** | Kullanıcı ve rol yönetimi, şirket yönetimi, tüm başvuru ve günlükleri izleme |

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

# Mevcut migration'ları veritabanına uygula
dotnet ef database update

# Çalıştır
dotnet run
```

Uygulama açılır: **http://localhost:5000**

---

## 🔐 Demo Hesapları

| Rol | E-posta | Şifre |
|---|---|---|
| **Admin** | `admin@careertrack.com` | `Admin123!` |
| **Okul** | `okul@careertrack.com` | `Okul123!` |
| **İşveren** | `isveren@careertrack.com` | `Isveren123!` |
| **Öğrenci** | `ogrenci@careertrack.com` | `Ogrenci123!` |

Yeni kayıt olan kullanıcılar otomatik olarak **Öğrenci** rolü alır. İşveren, Okul ve Admin rolleri Admin panelinden atanır.

---

## 🗃️ Veritabanı Şeması

```
ApplicationUser (ASP.NET Core Identity)
    ├── JobApplications (1-N)     ← Öğrenci başvuruları
    ├── DailyLogs (1-N)           ← Staj defteri
    ├── ToDos (1-N)               ← Bireysel ve süreç görevleri
    └── JobPostings (1-N)         ← İşveren ilanları

Company
    ├── JobApplications (1-N)
    └── JobPostings (1-N)

JobApplication
    ├── DailyLogs (1-N)
    ├── ToDos (1-N)
    └── StudentTasks (1-N)
```

---

## 📁 Klasör Yapısı

```
CareerTrack/
├── Controllers/          ← 9 controller (Account, Admin, School, Employer...)
├── Models/
│   ├── Entities/         ← EF Core entity sınıfları
│   ├── Enums/            ← InternshipType, ApplicationStatus, DailyLogStatus
│   └── ViewModels/       ← StudentDashboardViewModel, LoginViewModel...
├── Data/
│   ├── ApplicationDbContext.cs   ← EF Core DbContext + Fluent API
│   └── SeedData.cs               ← Rol ve admin seed
├── Views/
│   ├── Shared/_Layout.cshtml     ← Ortak sidebar + header + footer
│   ├── Dashboard/
│   ├── Application/
│   ├── DailyLog/
│   ├── ToDo/
│   ├── Admin/
│   ├── School/
│   ├── Employer/
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
| **Role-Based Auth** | Admin, School, Employer ve Student rollerine özel controller yetkilendirmesi |
| **_Layout.cshtml** | Sol sidebar + üst header + footer — tüm sayfalarda ortak |
| **ViewModel** | `StudentDashboardViewModel` → 3 tablodan veri tek sınıfta |
| **ViewBag / ViewData** | `ViewBag.StudentTasks`, `ViewBag.ApprovedApplications`, `ViewData["Title"]` |
| **Data Annotations** | `[Required]`, `[StringLength]`, `[DataType]`, `[Range]` |
| **ModelState.AddModelError** | Geçmiş tarih engeli, gün tekrarı kontrolü |
| **LINQ** | `Where`, `OrderBy`, `Count`, `Take`, `Include` |
| **Enum** | `InternshipType`, `ApplicationStatus`, `DailyLogStatus` |
| **1-N İlişki** | `JobApplication` → `DailyLog`, `ToDo`, `StudentTask` |

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
