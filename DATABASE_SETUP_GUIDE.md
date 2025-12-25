# راهنمای راه‌اندازی Database با PostgreSQL و EF Core

## مرحله 1: نصب PostgreSQL
1. دانلود از: https://www.postgresql.org/download/windows/
2. نصب با تنظیمات:
   - Username: `postgres`
   - Password: (یک پسورد قوی - مثل `postgres123`)
   - Port: `5432`

## مرحله 2: ایجاد Database در pgAdmin
1. باز کردن pgAdmin
2. اتصال به PostgreSQL server (اگه نداشتید، Register → Server)
3. روی **"Databases"** کلیک راست → **"Create" → "Database"**
4. تنظیمات:
   - Database: `your_db_name`
   - Template: `template0` (مهم برای UTF8!)
   - Encoding: `UTF8`
   - Save

## مرحله 3: اضافه کردن Packages به پروژه
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.11" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.11" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.2" />
```

## مرحله 4: Connection String در appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=your_db_name;Username=postgres;Password=your_password;Port=5432"
  }
}
```

## مرحله 5: ایجاد Entities
```csharp
// Entities/YourEntity.cs
public class YourEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

## مرحله 6: ایجاد DbContext
```csharp
// Data/YourDbContext.cs
public class YourDbContext : DbContext
{
    public YourDbContext(DbContextOptions<YourDbContext> options) : base(options) { }
    
    public DbSet<YourEntity> YourEntities { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configuration here
    }
}
```

## مرحله 7: اضافه کردن DbContext به Program.cs
```csharp
builder.Services.AddDbContext<YourDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

## مرحله 8: نصب EF Tools (یکبار)
```bash
# نصب یا بروزرسانی
dotnet tool install --global dotnet-ef --version 9.0.11

# یا اگه قبلاً نصب بود
dotnet tool update --global dotnet-ef --version 9.0.11
```

## مرحله 9: ایجاد و اعمال Migration
```bash
# رفتن به پوشه پروژه
cd src/YourProject.Api

# ایجاد migration
dotnet ef migrations add InitialCreate

# اعمال migration به database
dotnet ef database update
```

## مرحله 10: تست Connection (اختیاری)
```csharp
// اضافه کردن health endpoint
app.MapGet("/health", async (YourDbContext db) =>
{
    try
    {
        await db.Database.CanConnectAsync();
        return Results.Ok(new { Status = "Healthy", Database = "Connected" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
});
```

## دستورات مفید EF Core

### Migration Commands:
```bash
# ایجاد migration جدید
dotnet ef migrations add MigrationName

# اعمال migrations
dotnet ef database update

# حذف آخرین migration (اگه هنوز apply نشده)
dotnet ef migrations remove

# بازگشت به migration خاص
dotnet ef database update PreviousMigrationName

# مشاهده لیست migrations
dotnet ef migrations list

# تولید SQL script
dotnet ef migrations script
```

### Database Commands:
```bash
# حذف database
dotnet ef database drop

# بررسی وضعیت database
dotnet ef database update --dry-run
```

## نکات مهم:
- ✅ همیشه `template0` استفاده کنید برای UTF8
- ✅ Version سازگاری EF tools با پروژه
- ✅ Connection string رو در appsettings.Development.json هم بذارید
- ✅ قبل از migration، build کنید: `dotnet build`
- ✅ Backup بگیرید قبل از migration در production

## مثال کامل Connection String:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=myapp_db;Username=postgres;Password=postgres123;Port=5432;Pooling=true;Connection Lifetime=0;"
  }
}
```

---
تاریخ ایجاد: 25 دسامبر 2024