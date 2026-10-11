using EzItam.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EzItam.Web.Data;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Viewer = "Viewer";
    public static readonly string[] All = { Admin, Manager, Viewer };
}

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config, ILogger logger)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var adminUser = config["Seed:AdminUser"] ?? "admin";
        var adminPassword = config["Seed:AdminPassword"] ?? "Admin!2345";
        var adminEmail = config["Seed:AdminEmail"] ?? "admin@localhost";

        if (await userManager.FindByNameAsync(adminUser) is null)
        {
            var user = new IdentityUser { UserName = adminUser, Email = adminEmail, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, AppRoles.Admin);
                logger.LogInformation("기본 관리자 계정 생성: {User}", adminUser);
            }
            else
            {
                logger.LogWarning("관리자 계정 생성 실패: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }

        if (!await db.AssetCategories.AnyAsync())
        {
            db.AssetCategories.AddRange(
                new AssetCategory { Code = "PC", Name = "데스크탑 PC", DepreciationMonths = 48 },
                new AssetCategory { Code = "NB", Name = "노트북", DepreciationMonths = 36 },
                new AssetCategory { Code = "MON", Name = "모니터", DepreciationMonths = 60 },
                new AssetCategory { Code = "SRV", Name = "서버", DepreciationMonths = 60 },
                new AssetCategory { Code = "NET", Name = "네트워크 장비", DepreciationMonths = 60 },
                new AssetCategory { Code = "PRN", Name = "프린터/복합기", DepreciationMonths = 60 },
                new AssetCategory { Code = "MOB", Name = "모바일 기기", DepreciationMonths = 24 },
                new AssetCategory { Code = "ETC", Name = "기타 주변기기", DepreciationMonths = 36 });
        }

        if (!await db.Locations.AnyAsync())
        {
            db.Locations.AddRange(
                new Location { Name = "본사 1층 사무실", Building = "본사", Floor = "1F" },
                new Location { Name = "본사 2층 사무실", Building = "본사", Floor = "2F" },
                new Location { Name = "전산실", Building = "본사", Floor = "B1" },
                new Location { Name = "자재창고", Building = "본사", Floor = "1F" });
        }

        if (!await db.Departments.AnyAsync())
        {
            db.Departments.AddRange(
                new Department { Code = "IT", Name = "정보시스템팀" },
                new Department { Code = "HR", Name = "인사총무팀" },
                new Department { Code = "FIN", Name = "재무팀" },
                new Department { Code = "SAL", Name = "영업팀" },
                new Department { Code = "PRD", Name = "생산팀" });
        }

        await db.SaveChangesAsync();

        if (config.GetValue<bool>("Seed:SampleData") && !await db.Assets.AnyAsync())
        {
            await SeedSampleDataAsync(db);
        }
    }

    private static async Task SeedSampleDataAsync(AppDbContext db)
    {
        var it = await db.Departments.FirstAsync(d => d.Code == "IT");
        var hr = await db.Departments.FirstAsync(d => d.Code == "HR");
        var sal = await db.Departments.FirstAsync(d => d.Code == "SAL");

        var emps = new[]
        {
            new Employee { EmployeeNo = "E1001", Name = "김철수", Email = "cskim@example.com", Title = "과장", DepartmentId = it.Id },
            new Employee { EmployeeNo = "E1002", Name = "이영희", Email = "yhlee@example.com", Title = "대리", DepartmentId = hr.Id },
            new Employee { EmployeeNo = "E1003", Name = "박민수", Email = "mspark@example.com", Title = "사원", DepartmentId = sal.Id },
            new Employee { EmployeeNo = "E1004", Name = "최지은", Email = "jechoi@example.com", Title = "차장", DepartmentId = sal.Id }
        };
        db.Employees.AddRange(emps);

        var vendor = new Vendor { Name = "(주)한국컴퓨터", ContactName = "홍길동", Phone = "02-1234-5678", Email = "sales@hkcomputer.example" };
        var vendor2 = new Vendor { Name = "델테크놀로지스", ContactName = "Dell Korea", Phone = "080-200-3800" };
        db.Vendors.AddRange(vendor, vendor2);
        await db.SaveChangesAsync();

        var cats = await db.AssetCategories.ToDictionaryAsync(c => c.Code);
        var locs = await db.Locations.ToListAsync();

        var assets = new List<Asset>
        {
            new() { AssetTag = "NB-2024-0001", Name = "Dell Latitude 5540", CategoryId = cats["NB"].Id, Manufacturer = "Dell", Model = "Latitude 5540", SerialNumber = "DL5540-0001", VendorId = vendor2.Id, PurchaseDate = DateTime.Today.AddMonths(-10), PurchasePrice = 1850000, WarrantyExpiry = DateTime.Today.AddMonths(26), LocationId = locs[0].Id, Status = AssetStatus.Assigned, CurrentEmployeeId = emps[0].Id, HostName = "NB-CSKIM", OperatingSystem = "Windows 11 Pro" },
            new() { AssetTag = "NB-2024-0002", Name = "Dell Latitude 5540", CategoryId = cats["NB"].Id, Manufacturer = "Dell", Model = "Latitude 5540", SerialNumber = "DL5540-0002", VendorId = vendor2.Id, PurchaseDate = DateTime.Today.AddMonths(-10), PurchasePrice = 1850000, WarrantyExpiry = DateTime.Today.AddMonths(26), LocationId = locs[1].Id, Status = AssetStatus.Assigned, CurrentEmployeeId = emps[1].Id, HostName = "NB-YHLEE", OperatingSystem = "Windows 11 Pro" },
            new() { AssetTag = "PC-2023-0001", Name = "HP ProDesk 400 G9", CategoryId = cats["PC"].Id, Manufacturer = "HP", Model = "ProDesk 400 G9", SerialNumber = "HP400-0001", VendorId = vendor.Id, PurchaseDate = DateTime.Today.AddMonths(-20), PurchasePrice = 1200000, WarrantyExpiry = DateTime.Today.AddDays(12), LocationId = locs[1].Id, Status = AssetStatus.Assigned, CurrentEmployeeId = emps[2].Id, HostName = "PC-MSPARK", OperatingSystem = "Windows 10 Pro" },
            new() { AssetTag = "PC-2023-0002", Name = "HP ProDesk 400 G9", CategoryId = cats["PC"].Id, Manufacturer = "HP", Model = "ProDesk 400 G9", SerialNumber = "HP400-0002", VendorId = vendor.Id, PurchaseDate = DateTime.Today.AddMonths(-20), PurchasePrice = 1200000, WarrantyExpiry = DateTime.Today.AddDays(-30), LocationId = locs[3].Id, Status = AssetStatus.InStock },
            new() { AssetTag = "MON-2023-0001", Name = "LG 27UL550", CategoryId = cats["MON"].Id, Manufacturer = "LG", Model = "27UL550", SerialNumber = "LG27-0001", VendorId = vendor.Id, PurchaseDate = DateTime.Today.AddMonths(-20), PurchasePrice = 350000, LocationId = locs[1].Id, Status = AssetStatus.Assigned, CurrentEmployeeId = emps[2].Id },
            new() { AssetTag = "SRV-2022-0001", Name = "Dell PowerEdge R750", CategoryId = cats["SRV"].Id, Manufacturer = "Dell", Model = "PowerEdge R750", SerialNumber = "R750-0001", VendorId = vendor2.Id, PurchaseDate = DateTime.Today.AddMonths(-30), PurchasePrice = 12000000, WarrantyExpiry = DateTime.Today.AddMonths(6), LocationId = locs[2].Id, Status = AssetStatus.InStock, HostName = "DB-01", IpAddress = "10.0.1.11", OperatingSystem = "Oracle Linux 8" },
            new() { AssetTag = "NET-2022-0001", Name = "Cisco Catalyst 9200L", CategoryId = cats["NET"].Id, Manufacturer = "Cisco", Model = "C9200L-48P", SerialNumber = "C9200-0001", PurchaseDate = DateTime.Today.AddMonths(-30), PurchasePrice = 4500000, LocationId = locs[2].Id, Status = AssetStatus.InStock, IpAddress = "10.0.0.2" },
            new() { AssetTag = "PRN-2021-0001", Name = "Canon iR-ADV C3530", CategoryId = cats["PRN"].Id, Manufacturer = "Canon", Model = "iR-ADV C3530", SerialNumber = "CN3530-0001", PurchaseDate = DateTime.Today.AddMonths(-50), PurchasePrice = 3200000, LocationId = locs[0].Id, Status = AssetStatus.InRepair },
            new() { AssetTag = "MOB-2024-0001", Name = "Galaxy S24", CategoryId = cats["MOB"].Id, Manufacturer = "Samsung", Model = "SM-S921N", SerialNumber = "SMS921-0001", PurchaseDate = DateTime.Today.AddMonths(-6), PurchasePrice = 1150000, WarrantyExpiry = DateTime.Today.AddMonths(18), Status = AssetStatus.Assigned, CurrentEmployeeId = emps[3].Id },
            new() { AssetTag = "NB-2020-0001", Name = "LG gram 15", CategoryId = cats["NB"].Id, Manufacturer = "LG", Model = "15Z90N", SerialNumber = "LG15-0001", PurchaseDate = DateTime.Today.AddMonths(-60), PurchasePrice = 1600000, WarrantyExpiry = DateTime.Today.AddMonths(-36), Status = AssetStatus.Retired, LocationId = locs[3].Id }
        };
        db.Assets.AddRange(assets);
        await db.SaveChangesAsync();

        foreach (var a in assets.Where(a => a.CurrentEmployeeId.HasValue))
        {
            db.AssetAssignments.Add(new AssetAssignment { AssetId = a.Id, EmployeeId = a.CurrentEmployeeId!.Value, AssignedAt = a.PurchaseDate?.AddDays(3) ?? DateTime.Now, AssignedBy = "system" });
        }

        db.MaintenanceRecords.Add(new MaintenanceRecord { AssetId = assets[7].Id, Type = MaintenanceType.Repair, Date = DateTime.Today.AddDays(-2), Description = "급지 오류로 롤러 교체 요청", VendorId = vendor.Id, Cost = 120000 });

        var lic1 = new SoftwareLicense { Name = "Microsoft 365 Business Standard", Publisher = "Microsoft", LicenseType = LicenseType.Subscription, TotalSeats = 50, PurchaseDate = DateTime.Today.AddMonths(-8), ExpiryDate = DateTime.Today.AddMonths(4), Cost = 9000000 };
        var lic2 = new SoftwareLicense { Name = "Adobe Acrobat Pro", Publisher = "Adobe", LicenseType = LicenseType.PerSeat, TotalSeats = 5, PurchaseDate = DateTime.Today.AddMonths(-11), ExpiryDate = DateTime.Today.AddDays(20), Cost = 1500000 };
        var lic3 = new SoftwareLicense { Name = "Windows Server 2022 Standard", Publisher = "Microsoft", LicenseType = LicenseType.PerDevice, TotalSeats = 2, PurchaseDate = DateTime.Today.AddMonths(-30), Cost = 2400000 };
        db.SoftwareLicenses.AddRange(lic1, lic2, lic3);
        await db.SaveChangesAsync();

        db.LicenseAssignments.AddRange(
            new LicenseAssignment { SoftwareLicenseId = lic1.Id, EmployeeId = emps[0].Id, AssignedAt = DateTime.Now.AddMonths(-8), AssignedBy = "system" },
            new LicenseAssignment { SoftwareLicenseId = lic1.Id, EmployeeId = emps[1].Id, AssignedAt = DateTime.Now.AddMonths(-8), AssignedBy = "system" },
            new LicenseAssignment { SoftwareLicenseId = lic1.Id, EmployeeId = emps[2].Id, AssignedAt = DateTime.Now.AddMonths(-8), AssignedBy = "system" },
            new LicenseAssignment { SoftwareLicenseId = lic2.Id, EmployeeId = emps[3].Id, AssignedAt = DateTime.Now.AddMonths(-5), AssignedBy = "system" },
            new LicenseAssignment { SoftwareLicenseId = lic3.Id, AssetId = assets[5].Id, AssignedAt = DateTime.Now.AddMonths(-30), AssignedBy = "system" });
        await db.SaveChangesAsync();
    }
}
