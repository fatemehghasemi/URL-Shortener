# URL Shortener Refactoring Plan

## وضعیت فعلی (Current State)
- پروژه با .NET 9 و Minimal APIs
- استفاده از ConcurrentDictionary برای ذخیره در memory
- فقط دو endpoint: POST /generate و GET /{shortCode}
- هیچ persistence، validation، analytics یا security نداره

## هدف نهایی (Target Architecture)

### 1. Database & Persistence
- **PostgreSQL** به عنوان primary database
- **Redis** برای caching و performance
- **Entity Framework Core** برای ORM

### 2. Simple VSA Structure
```
src/Shortener.Api/
├── Features/           # هر feature یک فایل (CreateShortUrl.cs, RedirectUrl.cs)
├── Data/              # DbContext + Entities  
├── Services/          # فقط shared services
├── Extensions/        # Service extensions
└── Program.cs         # configuration
```

### 3. Core Features
- ✅ URL shortening (موجود)
- ✅ URL redirection (موجود)
- ❌ Database persistence
- ❌ Custom short codes
- ❌ URL validation
- ❌ Click analytics & tracking
- ❌ Expiration dates
- ❌ Rate limiting
- ❌ Bulk operations
- ❌ Health checks
- ❌ Comprehensive logging

### 4. Technical Improvements
- Input validation با FluentValidation
- Response caching
- CORS configuration
- Security headers
- Request/Response logging
- Error handling middleware
- Swagger documentation enhancement
- Unit & Integration tests

## مراحل پیاده‌سازی (Implementation Phases)

### Phase 1: Database Setup & Basic Entities
- [ ] اضافه کردن PostgreSQL و EF Core packages
- [ ] ایجاد Domain entities (Link, ClickLog)
- [ ] Database context و migrations
- [ ] تست اتصال database

### Phase 2: Clean Architecture Restructure  
- [ ] جدا کردن layers (Domain, Application, Infrastructure)
- [ ] ایجاد interfaces و dependency injection
- [ ] Repository pattern implementation
- [ ] Unit of Work pattern

### Phase 3: Enhanced Business Logic
- [ ] URL validation service
- [ ] Custom short code generation
- [ ] Expiration date handling
- [ ] Click tracking و analytics

### Phase 4: Caching & Performance
- [ ] Redis integration
- [ ] Response caching
- [ ] Database query optimization
- [ ] Performance monitoring

### Phase 5: Security & Validation
- [ ] Input validation
- [ ] Rate limiting
- [ ] Security headers
- [ ] CORS configuration

### Phase 6: Advanced Features
- [ ] Bulk operations
- [ ] Analytics dashboard endpoints
- [ ] Health checks
- [ ] Comprehensive logging

### Phase 7: Testing & Documentation
- [ ] Unit tests
- [ ] Integration tests
- [ ] API documentation
- [ ] Performance tests

## شروع از Phase 1: Database Setup

### مرحله 1.1: PostgreSQL Installation
1. دانلود PostgreSQL از: https://www.postgresql.org/download/windows/
2. نصب با تنظیمات:
   - Username: postgres
   - Password: (یک پسورد قوی انتخاب کنید)
   - Port: 5432
3. ایجاد database و user:
```sql
CREATE DATABASE shortener_db;
CREATE USER shortener_user WITH PASSWORD 'shortener_pass';
GRANT ALL PRIVILEGES ON DATABASE shortener_db TO shortener_user;
```

### مرحله 1.2: Package Installation
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
```

### مرحله 1.3: Domain Entities
```csharp
// Domain/Entities/Link.cs
public class Link
{
    public Guid Id { get; set; }
    public string OriginalUrl { get; set; }
    public string ShortCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public int ClickCount { get; set; }
}

// Domain/Entities/ClickLog.cs  
public class ClickLog
{
    public Guid Id { get; set; }
    public Guid LinkId { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public DateTime ClickedAt { get; set; }
    public Link Link { get; set; }
}
```

### مرحله 1.4: DbContext
```csharp
public class ShortenerDbContext : DbContext
{
    public DbSet<Link> Links { get; set; }
    public DbSet<ClickLog> ClickLogs { get; set; }
    
    // Configuration...
}
```

## نکات مهم
- هر مرحله باید کامل تست بشه قبل از رفتن به مرحله بعد
- هر commit باید یک feature کامل باشه
- Performance و security از اولویت‌های اصلی هستند
- Code quality و best practices رعایت بشه

## فایل‌های مرجع
- این فایل: `REFACTORING_PLAN.md`
- Current code: `src/Shortener.Api/`
- README.md پروژه اصلی

---
تاریخ ایجاد: 25 دسامبر 2024
آخرین بروزرسانی: 25 دسامبر 2024