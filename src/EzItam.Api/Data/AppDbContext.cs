using System.Text.Json;
using EzItam.Api.Models.Entities;
using EzItam.Api.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EzItam.Api.Data;

public class AppDbContext : IdentityDbContext
{
    private readonly ICurrentUserService? _currentUser;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetCategory> AssetCategories => Set<AssetCategory>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<AssetAssignment> AssetAssignments => Set<AssetAssignment>();
    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();
    public DbSet<SoftwareLicense> SoftwareLicenses => Set<SoftwareLicense>();
    public DbSet<LicenseAssignment> LicenseAssignments => Set<LicenseAssignment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<AgentSoftware> AgentSoftware => Set<AgentSoftware>();

    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<InventoryDetail> InventoryDetails => Set<InventoryDetail>();
    public DbSet<EmployeeTemp> EmployeeTemps => Set<EmployeeTemp>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        if (Database.IsSqlServer())
        {
            builder.Entity<Asset>().Property(a => a.PurchaseDate).HasColumnType("date");
            builder.Entity<Asset>().Property(a => a.WarrantyExpiry).HasColumnType("date");
            builder.Entity<MaintenanceRecord>().Property(m => m.Date).HasColumnType("date");
            builder.Entity<MaintenanceRecord>().Property(m => m.CompletedAt).HasColumnType("date");
            builder.Entity<SoftwareLicense>().Property(l => l.PurchaseDate).HasColumnType("date");
            builder.Entity<SoftwareLicense>().Property(l => l.ExpiryDate).HasColumnType("date");
            builder.Entity<Agent>().Property(a => a.TokenHash).HasColumnType("char(64)");
        }

        builder.Entity<Asset>(e =>
        {
            e.ToTable("Assets", table =>
            {
                table.HasCheckConstraint("CK_Assets_Status", "[Status] IN (0, 1, 2, 3, 4, 5, 6)");
                table.HasCheckConstraint("CK_Assets_PurchasePrice_NonNegative", "[PurchasePrice] IS NULL OR [PurchasePrice] BETWEEN 0 AND 999999999999");
            });
            e.HasIndex(a => a.AssetTag).IsUnique();
            e.HasIndex(a => a.SerialNumber);
            e.HasIndex(a => a.Status);
            e.Property(a => a.Status).HasDefaultValue(AssetStatus.InStock);
            e.HasOne(a => a.Category).WithMany(c => c.Assets).HasForeignKey(a => a.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Vendor).WithMany(v => v.Assets).HasForeignKey(a => a.VendorId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(a => a.Location).WithMany(l => l.Assets).HasForeignKey(a => a.LocationId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(a => a.CurrentEmployee).WithMany(emp => emp.Assets).HasForeignKey(a => a.CurrentEmployeeId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<AssetCategory>(e =>
        {
            e.ToTable("AssetCategories", table =>
                table.HasCheckConstraint("CK_AssetCategories_DepreciationMonths", "[DepreciationMonths] BETWEEN 0 AND 600"));
            e.HasIndex(c => c.Code).IsUnique();
            e.Property(c => c.DepreciationMonths).HasDefaultValue(48);
        });

        builder.Entity<Location>(e =>
        {
            e.ToTable("Locations");
            e.Property(l => l.IsActive).HasDefaultValue(true);
        });

        builder.Entity<Department>(e =>
        {
            e.ToTable("Departments");
            e.HasIndex(d => d.Code).IsUnique();
            e.Property(d => d.IsActive).HasDefaultValue(true);
        });

        builder.Entity<Employee>(e =>
        {
            e.ToTable("Employees");
            e.HasIndex(emp => emp.EmployeeNo).IsUnique();
            e.Property(emp => emp.IsActive).HasDefaultValue(true);
            e.HasOne(emp => emp.Department).WithMany(d => d.Employees).HasForeignKey(emp => emp.DepartmentId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Vendor>(e => e.ToTable("Vendors"));

        builder.Entity<AssetAssignment>(e =>
        {
            e.ToTable("AssetAssignments");
            e.HasIndex(a => new { a.AssetId, a.ReturnedAt });
            e.HasIndex(a => a.AssetId)
                .IsUnique()
                .HasDatabaseName("UX_AssetAssignments_AssetId_Open")
                .HasFilter("[ReturnedAt] IS NULL");
            e.HasOne(a => a.Asset).WithMany(a => a.Assignments).HasForeignKey(a => a.AssetId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Employee).WithMany(emp => emp.Assignments).HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<MaintenanceRecord>(e =>
        {
            e.ToTable("MaintenanceRecords", table =>
            {
                table.HasCheckConstraint("CK_MaintenanceRecords_Type", "[Type] IN (0, 1, 2, 3)");
                table.HasCheckConstraint("CK_MaintenanceRecords_Cost_NonNegative", "[Cost] IS NULL OR [Cost] BETWEEN 0 AND 999999999999");
            });
            e.HasIndex(m => new { m.Date, m.Id })
                .HasDatabaseName("IX_MaintenanceRecords_Open_Date_Id")
                .HasFilter("[CompletedAt] IS NULL");
            e.HasOne(m => m.Asset).WithMany(a => a.MaintenanceRecords).HasForeignKey(m => m.AssetId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Vendor).WithMany().HasForeignKey(m => m.VendorId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<SoftwareLicense>(e =>
        {
            e.ToTable("SoftwareLicenses", table =>
            {
                table.HasCheckConstraint("CK_SoftwareLicenses_LicenseType", "[LicenseType] IN (0, 1, 2, 3, 4)");
                table.HasCheckConstraint("CK_SoftwareLicenses_TotalSeats", "[TotalSeats] BETWEEN 0 AND 1000000");
                table.HasCheckConstraint("CK_SoftwareLicenses_Cost_NonNegative", "[Cost] IS NULL OR [Cost] BETWEEN 0 AND 999999999999");
            });
            e.HasIndex(l => l.Name);
            e.Property(l => l.LicenseType).HasDefaultValue(LicenseType.PerSeat);
            e.Property(l => l.TotalSeats).HasDefaultValue(1);
            e.HasOne(l => l.Vendor).WithMany(v => v.Licenses).HasForeignKey(l => l.VendorId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<LicenseAssignment>(e =>
        {
            e.ToTable("LicenseAssignments", table =>
                table.HasCheckConstraint(
                    "CK_LicenseAssignments_ExactlyOneAssignee",
                    "([AssetId] IS NOT NULL AND [EmployeeId] IS NULL) OR ([AssetId] IS NULL AND [EmployeeId] IS NOT NULL)"));
            e.HasIndex(a => new { a.SoftwareLicenseId, a.ReleasedAt });
            e.HasIndex(a => new { a.SoftwareLicenseId, a.EmployeeId })
                .IsUnique()
                .HasDatabaseName("UX_LicenseAssignments_Open_Employee")
                .HasFilter("[ReleasedAt] IS NULL AND [EmployeeId] IS NOT NULL");
            e.HasIndex(a => new { a.SoftwareLicenseId, a.AssetId })
                .IsUnique()
                .HasDatabaseName("UX_LicenseAssignments_Open_Asset")
                .HasFilter("[ReleasedAt] IS NULL AND [AssetId] IS NOT NULL");
            e.HasOne(a => a.SoftwareLicense).WithMany(l => l.Assignments).HasForeignKey(a => a.SoftwareLicenseId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Asset).WithMany(a => a.LicenseAssignments).HasForeignKey(a => a.AssetId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Employee).WithMany(emp => emp.LicenseAssignments).HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Agent>(e =>
        {
            e.ToTable("Agents", table =>
            {
                table.HasCheckConstraint("CK_Agents_Status", "[Status] IN (0, 1, 2)");
                table.HasCheckConstraint("CK_Agents_Ring", "[Ring] IN (0, 1)");
                table.HasCheckConstraint("CK_Agents_NonNegativeHardware", "([CpuCores] IS NULL OR [CpuCores] >= 0) AND ([TotalMemoryMb] IS NULL OR [TotalMemoryMb] >= 0) AND ([DiskTotalGb] IS NULL OR [DiskTotalGb] >= 0)");
            });
            e.HasIndex(a => a.DeviceUuid).IsUnique();
            e.HasIndex(a => a.SerialNumber);
            e.HasIndex(a => a.HostName);
            e.HasIndex(a => a.TokenHash).IsUnique();
            e.HasIndex(a => a.LastSeenAt);
            e.Property(a => a.Status).HasDefaultValue(AgentStatus.Pending);
            e.Property(a => a.Ring).HasDefaultValue(AgentRing.Stable);
            e.HasOne(a => a.Asset).WithMany(x => x.Agents).HasForeignKey(a => a.AssetId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<AgentSoftware>(e =>
        {
            e.ToTable("AgentSoftware");
            e.HasIndex(s => new { s.AgentId, s.Name });
            e.HasOne(s => s.Agent).WithMany(a => a.Software).HasForeignKey(s => s.AgentId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AuditLog>(e =>
        {
            e.ToTable("AuditLogs", table =>
                table.HasCheckConstraint("CK_AuditLogs_Action", "[Action] IN (0, 1, 2)"));
            e.HasIndex(a => new { a.EntityName, a.EntityKey, a.Timestamp })
                .HasDatabaseName("IX_AuditLogs_EntityName_EntityKey_Timestamp");
            e.HasIndex(a => a.Timestamp);
        });

        builder.Entity<Inventory>(e =>
        {
            e.ToTable("Inventories", table =>
            {
                table.HasCheckConstraint("CK_Inventories_Status", "[InventoryStatus] IN (1, 2, 3)");
                table.HasCheckConstraint("CK_Inventories_DateRange", "[DateFrom] <= [DateTo]");
            });
            e.Property(i => i.Name).IsRequired().HasMaxLength(50);
            e.Property(i => i.InventoryStatus).IsRequired().HasConversion<int>();
            e.Property(i => i.InventoryStatus)
                .HasDefaultValue(InventoryStatus.Open)
                .HasSentinel(InventoryStatus.Open);
            e.HasIndex(i => i.Name).IsUnique();
            e.Property(i => i.DateFrom).IsRequired();
            e.Property(i => i.DateTo).IsRequired();
            e.Property(i => i.Notes).IsRequired(false).HasMaxLength(200);
        });

        builder.Entity<InventoryDetail>(e =>
        {
            e.ToTable("InventoryDetails", table =>
            {
                table.HasCheckConstraint("CK_InventoryDetails_Status", "[Status] IN (0, 1, 2, 3, 4, 5, 6)");
                table.HasCheckConstraint("CK_InventoryDetails_AssetStatusIV", "[AssetStatusIV] IN (0, 1, 2, 3, 4, 5, 6)");
                table.HasCheckConstraint("CK_InventoryDetails_ScanCount", "[ScanCount] >= 0");
            });
            e.HasOne(d => d.Inventory).WithMany(i => i.InventoryDetails).HasForeignKey(d => d.InventoryId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(d => d.Asset).WithMany(a => a.InventoryDetails).HasForeignKey(d => d.IdAsset).OnDelete(DeleteBehavior.Cascade);
            e.Property(d => d.IdLocation).IsRequired(false);
            e.HasOne(d => d.Location).WithMany(l => l.InventoryDetails).HasForeignKey(d => d.IdLocation).OnDelete(DeleteBehavior.Restrict);
            e.Property(d => d.IdEmployee).IsRequired(false);
            e.HasOne(d => d.Employee).WithMany(emp => emp.InventoryDetails).HasForeignKey(d => d.IdEmployee).OnDelete(DeleteBehavior.Restrict);
            e.Property(d => d.IdLocationIV).IsRequired(false);
            e.HasOne(d => d.LocationIV).WithMany(l => l.InventoryDetailsIV).HasForeignKey(d => d.IdLocationIV).OnDelete(DeleteBehavior.Restrict);
            e.Property(d => d.IdEmployeeIV).IsRequired(false);
            e.HasOne(d => d.EmployeeIV).WithMany(emp => emp.InventoryDetailsIV).HasForeignKey(d => d.IdEmployeeIV).OnDelete(DeleteBehavior.Restrict);
            e.Property(d => d.Notes).IsRequired(false).HasMaxLength(200);
            e.Property(d => d.ScanCount).IsRequired().HasDefaultValue(0);
            e.HasIndex(d => new { d.InventoryId, d.IdAsset }).IsUnique();
            e.HasIndex(d => new { d.InventoryId, d.ScanCount });
        });

        builder.Entity<Employee>(e =>
        {
            e.Property(d => d.Source).IsRequired(false);
        });
        builder.Entity<EmployeeTemp>(e =>
        {
            e.ToTable("EmployeeTemps", table =>
                table.HasCheckConstraint("CK_EmployeeTemps_IsResigned", "[IsResigned] IN ('Y', 'N')"));
            e.Property(d => d.Id);
            e.Property(d => d.OrgCode).IsRequired(true);
            e.Property(d => d.EmployeeNo).IsRequired(true);
            e.Property(d => d.Name).IsRequired(true);
            e.Property(d => d.Email).IsRequired(false);
            e.Property(d => d.DeptCode).IsRequired(false);
            e.Property(d => d.DeptName).IsRequired(false);
            e.Property(d => d.DeptLevel).IsRequired(false);
            e.Property(d => d.TitleCode).IsRequired(false);
            e.Property(d => d.TitleName).IsRequired(false);
            e.Property(d => d.PositionCode).IsRequired(false);
            e.Property(d => d.PositionName).IsRequired(false);
            e.Property(d => d.IsResigned).IsRequired().HasDefaultValue("N");
            e.Property(d => d.DateValue1).IsRequired(false);
            e.Property(d => d.DateValue2).IsRequired(false);
            e.Property(d => d.DateValue3).IsRequired(false);
            e.Property(d => d.DateValue4).IsRequired(false);
            e.Property(d => d.DateValue5).IsRequired(false);
            e.Property(d => d.DateValue6).IsRequired(false);
            e.Property(d => d.DateValue7).IsRequired(false);
            e.Property(d => d.DateValue8).IsRequired(false);
            e.Property(d => d.DateValue9).IsRequired(false);

            e.HasIndex(d => new { d.OrgCode, d.EmployeeNo }).IsUnique();
        });


    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var added = ApplyAuditing(out var now, out var user);
        var result = base.SaveChanges(acceptAllChangesOnSuccess);
        if (added.Count > 0)
        {
            AuditLogs.AddRange(added.Select(e => BuildCreatedLog(e, now, user)));
            base.SaveChanges(acceptAllChangesOnSuccess);
        }
        return result;
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var added = ApplyAuditing(out var now, out var user);
        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        if (added.Count > 0)
        {
            AuditLogs.AddRange(added.Select(e => BuildCreatedLog(e, now, user)));
            await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        return result;
    }

    private static readonly HashSet<string> AuditedEntities = new()
    {
        nameof(Asset), nameof(AssetCategory), nameof(Location), nameof(Department), nameof(Employee),
        nameof(Vendor), nameof(AssetAssignment), nameof(MaintenanceRecord), nameof(SoftwareLicense), nameof(LicenseAssignment),
        nameof(Inventory), nameof(InventoryDetail)
    };

    /// <summary>
    /// 수정/삭제 로그는 즉시 추가하고, 생성 로그는 키(IDENTITY)가 확정되는 저장 이후에 기록하기 위해 엔트리 목록을 돌려줍니다.
    /// </summary>
    private List<EntityEntry> ApplyAuditing(out DateTime now, out string user)
    {
        now = DateTime.Now;
        user = _currentUser?.UserName ?? "system";
        var added = new List<EntityEntry>();
        var logs = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries().ToList())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)) continue;
            var typeName = entry.Metadata.ClrType.Name;
            if (!AuditedEntities.Contains(typeName)) continue;

            StampTimestamps(entry, now, user);

            if (entry.State == EntityState.Added)
            {
                added.Add(entry);
                continue;
            }

            var action = entry.State == EntityState.Deleted ? AuditAction.Deleted : AuditAction.Updated;
            var changes = BuildChanges(entry, action);
            if (action == AuditAction.Updated && changes.Count == 0) continue;

            logs.Add(new AuditLog
            {
                EntityName = typeName,
                EntityKey = BuildKey(entry),
                Action = action,
                Changes = changes.Count == 0 ? null : JsonSerializer.Serialize(changes),
                UserName = user,
                Timestamp = now
            });
        }

        if (logs.Count > 0) AuditLogs.AddRange(logs);
        return added;
    }

    private static AuditLog BuildCreatedLog(EntityEntry entry, DateTime now, string user)
    {
        var changes = BuildChanges(entry, AuditAction.Created);
        return new AuditLog
        {
            EntityName = entry.Metadata.ClrType.Name,
            EntityKey = BuildKey(entry),
            Action = AuditAction.Created,
            Changes = changes.Count == 0 ? null : JsonSerializer.Serialize(changes),
            UserName = user,
            Timestamp = now
        };
    }

    private static void StampTimestamps(EntityEntry entry, DateTime now, string user)
    {
        if (entry.State == EntityState.Added)
        {
            SetIfExists(entry, "CreatedAt", now);
            SetIfExists(entry, "CreatedBy", user);
        }
        if (entry.State is EntityState.Added or EntityState.Modified)
        {
            SetIfExists(entry, "UpdatedAt", now);
            SetIfExists(entry, "UpdatedBy", user);
        }
    }

    private static void SetIfExists(EntityEntry entry, string property, object value)
    {
        var prop = entry.Metadata.FindProperty(property);
        if (prop is null) return;
        entry.Property(property).CurrentValue = value;
    }

    private static string BuildKey(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null) return string.Empty;
        return string.Join(",", key.Properties.Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? ""));
    }

    private static Dictionary<string, object?> BuildChanges(EntityEntry entry, AuditAction action)
    {
        var result = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties)
        {
            var name = prop.Metadata.Name;
            if (name is "UpdatedAt" or "UpdatedBy" or "CreatedAt" or "CreatedBy") continue;
            switch (action)
            {
                case AuditAction.Created:
                    if (prop.CurrentValue is not null) result[name] = new { New = prop.CurrentValue };
                    break;
                case AuditAction.Deleted:
                    if (prop.OriginalValue is not null) result[name] = new { Old = prop.OriginalValue };
                    break;
                default:
                    if (prop.IsModified && !Equals(prop.OriginalValue, prop.CurrentValue))
                        result[name] = new { Old = prop.OriginalValue, New = prop.CurrentValue };
                    break;
            }
        }
        return result;
    }
}
