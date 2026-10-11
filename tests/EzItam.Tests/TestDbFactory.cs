using EzItam.Web.Data;
using EzItam.Web.Models.Entities;
using EzItam.Web.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EzItam.Tests;

/// <summary>SQLite in-memory 기반 테스트용 DbContext 팩토리 (테스트마다 독립 DB)</summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;
    public ICurrentUserService CurrentUser { get; } = new StaticCurrentUserService("tester");

    public TestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        using var db = Create();
        db.Database.EnsureCreated();
        Seed(db);
    }

    public AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        return new AppDbContext(options, CurrentUser);
    }

    private static void Seed(AppDbContext db)
    {
        var cat = new AssetCategory { Code = "NB", Name = "노트북", DepreciationMonths = 36 };
        var dept = new Department { Code = "IT", Name = "정보시스템팀" };
        db.AddRange(cat, dept);
        db.SaveChanges();
        db.Employees.AddRange(
            new Employee { EmployeeNo = "E1", Name = "김철수", DepartmentId = dept.Id },
            new Employee { EmployeeNo = "E2", Name = "이영희", DepartmentId = dept.Id },
            new Employee { EmployeeNo = "E9", Name = "퇴직자", DepartmentId = dept.Id, IsActive = false });
        db.Locations.Add(new Location { Name = "창고" });
        db.Assets.Add(new Asset { AssetTag = "NB-2025-0001", Name = "테스트 노트북", CategoryId = cat.Id, PurchasePrice = 1200000, PurchaseDate = new DateTime(2024, 1, 15) });
        db.SoftwareLicenses.Add(new SoftwareLicense { Name = "Office", LicenseType = LicenseType.PerSeat, TotalSeats = 2 });
        db.SoftwareLicenses.Add(new SoftwareLicense { Name = "Expired", LicenseType = LicenseType.Subscription, TotalSeats = 5, ExpiryDate = DateTime.Today.AddDays(-1) });
        db.SoftwareLicenses.Add(new SoftwareLicense { Name = "SiteWide", LicenseType = LicenseType.Site, TotalSeats = 0 });
        db.SaveChanges();
    }

    public void Dispose() => _connection.Dispose();
}
