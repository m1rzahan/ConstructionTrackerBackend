# Construction Tracker Backend

Bu ABP Core backend'i, Android tabanlı İnşaat Takip uygulaması için geliştirilmiştir.

## Özellikler

### 🏗️ Proje Yönetimi
- Proje oluşturma, güncelleme ve takip
- Proje ilerlemesi ve durum yönetimi
- Personel atama ve rol yönetimi
- Bütçe ve maliyet takibi

### 👥 Kullanıcı Yönetimi
- 4 farklı kullanıcı rolü:
  - **Admin**: Sistem yöneticisi
  - **Office Staff**: Ofis personeli
  - **Site Staff**: Saha personeli
  - **Subcontractor**: Alt yüklenici
- Şirket bazlı kullanıcı organizasyonu
- Authentication ve authorization

### 📱 QR Kod Sistemi
- QR kod tarama ve doğrulama
- Check-in/Check-out işlemleri
- Konum bazlı taramalar
- Malzeme ve ekipman takibi

### 📊 Dashboard ve Raporlama
- Gerçek zamanlı istatistikler
- Proje durumu özeti
- Aktivite geçmişi
- Haftalık performans raporları

### 📝 Aktivite Takibi
- Kullanıcı aktivitelerinin loglanması
- Sistem olaylarının kaydı
- Öncelik seviyelerine göre sınıflandırma

## Teknolojiler

- **ASP.NET Core** - Web API Framework
- **ABP Framework** - Application Framework
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **AutoMapper** - Object Mapping
- **Swagger** - API Documentation

## Database Schema

### Ana Tablolar

#### Users (Kullanıcılar)
- ABP'nin User tablosuna ek alanlar:
  - `FirstName`, `LastName`
  - `Role` (UserRoleType enum)
  - `CompanyId`, `LastLoginAt`

#### Companies (Şirketler)
- `Name`, `Description`, `Address`
- `Phone`, `Email`, `TaxNumber`
- İletişim ve vergi bilgileri

#### Projects (Projeler)
- `Name`, `Description`, `Address`
- `StartDate`, `EndDate`, `PlannedEndDate`
- `Status`, `Progress`, `Budget`, `SpentAmount`

#### UserProjects (Kullanıcı-Proje İlişkisi)
- Many-to-many relationship
- `Role` (projede sahip olunan rol)
- `AssignedDate`, `UnassignedDate`

#### ActivityLogs (Aktivite Logları)
- `Title`, `Description`, `ActivityType`
- `ActivityDate`, `Priority`
- Kullanıcı, proje ve şirket referansları

#### QrCodeScans (QR Kod Taramaları)
- `QrCodeData`, `ScanDate`, `ScanType`
- `Location`, `Latitude`, `Longitude`
- GPS koordinatları ile konum bilgisi

#### ProjectMaterials (Proje Malzemeleri)
- `Name`, `Description`, `Unit`
- `RequiredQuantity`, `UsedQuantity`
- `UnitPrice`, `Supplier`, `Brand`

## API Endpoints

### Authentication
```
POST /api/ConstructionTracker/login
POST /api/ConstructionTracker/logout/{userId}
```

### Dashboard
```
GET /api/ConstructionTracker/dashboard
GET /api/ConstructionTracker/weekly-stats
GET /api/ConstructionTracker/current-user
PUT /api/ConstructionTracker/current-user
```

### Projects
```
GET /api/Projects/{id}
GET /api/Projects
GET /api/Projects/user/{userId}
POST /api/Projects
PUT /api/Projects
DELETE /api/Projects/{id}
GET /api/Projects/{projectId}/users
POST /api/Projects/{projectId}/assign-user
DELETE /api/Projects/{projectId}/users/{userId}
PUT /api/Projects/{projectId}/progress
POST /api/Projects/{projectId}/complete
GET /api/Projects/stats
GET /api/Projects/active
GET /api/Projects/delayed
```

### QR Code Operations
```
POST /api/QrCode/scan
POST /api/QrCode/validate
GET /api/QrCode/user/{userId}/scans
GET /api/QrCode/project/{projectId}/scans
GET /api/QrCode/today
POST /api/QrCode/checkin
POST /api/QrCode/checkout
GET /api/QrCode/user/{userId}/checked-in
GET /api/QrCode/stats
GET /api/QrCode/recent
```

## Demo Veriler

Sistem otomatik olarak aşağıdaki demo verileri ile gelir:

### Demo Kullanıcılar
- **admin@insaat.com** (Şifre: 123456) - Ahmet Yılmaz (Admin)
- **ofis@insaat.com** (Şifre: 123456) - Ayşe Demir (Ofis Personeli)
- **saha@insaat.com** (Şifre: 123456) - Mehmet Kaya (Saha Personeli)
- **yuklenci@insaat.com** (Şifre: 123456) - Fatma Özkan (Alt Yüklenici)

### Demo Projeler
- **Ataşehir Rezidans** - %75 tamamlanmış
- **Pendik AVM** - %45 tamamlanmış
- **Bakırköy Ofis** - %90 tamamlanmış (Gecikme)

### Demo Şirketler
- **Acil Yapı İnşaat** - Ana şirket
- **Güven İnşaat Ltd.** - Alt yüklenici şirket

## Kurulum

1. **Gereksinimler**
   - .NET 6.0 veya üzeri
   - SQL Server
   - Visual Studio 2022 veya VS Code

2. **Database Migration**
   ```bash
   cd backend/aspnet-core/src/ConstructionTracker.EntityFrameworkCore
   dotnet ef database update
   ```

3. **Projeyi Çalıştırma**
   ```bash
   cd backend/aspnet-core/src/ConstructionTracker.Web.Host
   dotnet run
   ```

4. **Swagger UI**
   - http://localhost:21021/swagger (Development)
   - Tüm API endpoint'leri test edilebilir

## Android App Integration

Android uygulaması aşağıdaki şekilde backend ile entegre olur:

### Login Flow
1. Android app `/api/ConstructionTracker/login` endpoint'ine email/password gönderir
2. Backend UserLoginResultDto döner (user bilgileri + token)
3. Token sonraki isteklerde Authorization header'ında kullanılır

### Dashboard Data
1. Android app `/api/ConstructionTracker/dashboard` endpoint'inden ana ekran verilerini alır
2. İstatistikler, son projeler ve aktiviteler gelir
3. Kullanıcı rolüne göre filtrelenmiş veriler sunulur

### QR Code Scanning
1. Android app QR kod tarar
2. `/api/QrCode/scan` endpoint'ine GPS koordinatları ile birlikte gönderir
3. Backend taramayı kaydeder ve aktivite loglar
4. Check-in/Check-out durumuna göre işlem tamamlanır

## Güvenlik

- ABP Framework'ün built-in authentication/authorization sistemi
- Role-based access control
- Şirket bazlı data isolation
- Input validation ve sanitization
- SQL injection koruması (EF Core)

## Performance

- Entity Framework Core ile optimized queries
- Include/ThenInclude ile N+1 probleminin önlenmesi
- Database indexleri performans için
- Lazy loading devre dışı (explicit loading)
- Paginated results büyük veri setleri için

## Monitoring ve Logging

- ABP'nin built-in logging sistemi
- Activity logs ile business event tracking
- Error handling ve exception logging
- Performance monitoring için hazır altyapı

---

**Not**: Bu backend Android uygulamasının mock data'sını gerçek API çağrıları ile değiştirmek için tasarlanmıştır. Tüm endpoint'ler Android app'in mevcut UI/UX yapısına uygun şekilde geliştirilmiştir. 