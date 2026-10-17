using EzItam.Api.Services;

namespace EzItam.Api.Mapping;

/// <summary>엔티티 ⇄ DTO 변환. 엔티티 내부 구조가 API 응답으로 새지 않도록 여기서만 변환합니다.</summary>
public static class Map
{
    public static CategoryRef ToRef(AssetCategory c) => new() { Id = c.Id, Code = c.Code, Name = c.Name, DepreciationMonths = c.DepreciationMonths };
    public static VendorRef? ToRef(Vendor? v) => v is null ? null : new() { Id = v.Id, Name = v.Name };
    public static DepartmentRef? ToRef(Department? d) => d is null ? null : new() { Id = d.Id, Code = d.Code, Name = d.Name };
    public static LocationRef? ToRef(Location? l) => l is null ? null : new() { Id = l.Id, Name = l.Name, Building = l.Building, Floor = l.Floor, Room = l.Room, FullName = l.FullName };
    public static AssetRef? ToRef(Asset? a) => a is null ? null : new() { Id = a.Id, AssetTag = a.AssetTag, Name = a.Name };
    public static LicenseRef? ToRef(SoftwareLicense? l) => l is null ? null : new() { Id = l.Id, Name = l.Name, LicenseType = l.LicenseType };
    public static EmployeeRef? ToRef(Employee? e) => e is null ? null : new()
    {
        Id = e.Id, EmployeeNo = e.EmployeeNo, Name = e.Name, Title = e.Title, IsActive = e.IsActive, Department = ToRef(e.Department)
    };

    public static AssetDto ToDto(Asset a, bool detail = false)
    {
        var dto = new AssetDto
        {
            Id = a.Id, AssetTag = a.AssetTag, Name = a.Name, CategoryId = a.CategoryId, Category = ToRef(a.Category),
            Status = a.Status, Manufacturer = a.Manufacturer, Model = a.Model, SerialNumber = a.SerialNumber,
            VendorId = a.VendorId, Vendor = ToRef(a.Vendor), PurchaseDate = a.PurchaseDate, PurchasePrice = a.PurchasePrice,
            WarrantyExpiry = a.WarrantyExpiry, LocationId = a.LocationId, Location = ToRef(a.Location),
            CurrentEmployeeId = a.CurrentEmployeeId, CurrentEmployee = ToRef(a.CurrentEmployee),
            IpAddress = a.IpAddress, MacAddress = a.MacAddress, HostName = a.HostName, OperatingSystem = a.OperatingSystem,
            Notes = a.Notes, CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt, CreatedBy = a.CreatedBy, UpdatedBy = a.UpdatedBy,
            IsWarrantyExpired = a.IsWarrantyExpired, IsWarrantyExpiringSoon = a.IsWarrantyExpiringSoon,
            CurrentBookValue = a.CurrentBookValue()
        };
        if (detail)
        {
            dto.Assignments = [.. a.Assignments.Select(ToDto)];
            dto.MaintenanceRecords = [.. a.MaintenanceRecords.Select(ToDto)];
            dto.LicenseAssignments = [.. a.LicenseAssignments.Select(ToDto)];
        }
        return dto;
    }

    public static AssignmentDto ToDto(AssetAssignment x) => new()
    {
        Id = x.Id, AssetId = x.AssetId, Asset = ToRef(x.Asset), EmployeeId = x.EmployeeId, Employee = ToRef(x.Employee),
        AssignedAt = x.AssignedAt, ReturnedAt = x.ReturnedAt, AssignedBy = x.AssignedBy, ReturnedBy = x.ReturnedBy, Notes = x.Notes
    };

    public static MaintenanceDto ToDto(MaintenanceRecord m) => new()
    {
        Id = m.Id, AssetId = m.AssetId, Asset = ToRef(m.Asset), Type = m.Type, Date = m.Date, Description = m.Description,
        Cost = m.Cost, VendorId = m.VendorId, Vendor = ToRef(m.Vendor), CompletedAt = m.CompletedAt, CreatedBy = m.CreatedBy
    };

    public static LicenseAssignmentDto ToDto(LicenseAssignment x) => new()
    {
        Id = x.Id, SoftwareLicenseId = x.SoftwareLicenseId, SoftwareLicense = ToRef(x.SoftwareLicense), AssetId = x.AssetId,
        Asset = ToRef(x.Asset), EmployeeId = x.EmployeeId, Employee = ToRef(x.Employee), AssignedAt = x.AssignedAt,
        ReleasedAt = x.ReleasedAt, AssignedBy = x.AssignedBy, Notes = x.Notes
    };

    public static SoftwareLicenseDto ToDto(SoftwareLicense l, bool detail = false)
    {
        var dto = new SoftwareLicenseDto
        {
            Id = l.Id, Name = l.Name, Publisher = l.Publisher, Version = l.Version, LicenseType = l.LicenseType,
            LicenseKey = l.LicenseKey, TotalSeats = l.TotalSeats, PurchaseDate = l.PurchaseDate, ExpiryDate = l.ExpiryDate,
            VendorId = l.VendorId, Vendor = ToRef(l.Vendor), Cost = l.Cost, Notes = l.Notes,
            UsedSeats = l.UsedSeats, AvailableSeats = l.AvailableSeats == int.MaxValue ? -1 : l.AvailableSeats,
            IsExpired = l.IsExpired, IsExpiringSoon = l.IsExpiringSoon,
            IsUnlimited = l.LicenseType is LicenseType.Site or LicenseType.Free
        };
        if (detail) dto.Assignments = [.. l.Assignments.OrderByDescending(a => a.AssignedAt).Select(ToDto)];
        return dto;
    }

    public static EmployeeDto ToDto(Employee e, bool detail = false)
    {
        var dto = new EmployeeDto
        {
            Id = e.Id, EmployeeNo = e.EmployeeNo, Name = e.Name, Email = e.Email, Title = e.Title,
            DepartmentId = e.DepartmentId, Department = ToRef(e.Department), IsActive = e.IsActive
        };
        if (detail)
        {
            dto.Assets = [.. e.Assets.Select(a => ToDto(a))];
            dto.Assignments = [.. e.Assignments.OrderByDescending(a => a.AssignedAt).Select(ToDto)];
            dto.LicenseAssignments = [.. e.LicenseAssignments.Select(ToDto)];
        }
        return dto;
    }

    public static DepartmentDto ToDto(Department d) => new()
    {
        Id = d.Id, Code = d.Code, Name = d.Name, IsActive = d.IsActive,
        Employees = [.. d.Employees.Select(ToRef).Where(x => x is not null).Select(x => x!)]
    };

    public static VendorDto ToDto(Vendor v) => new()
    {
        Id = v.Id, Name = v.Name, ContactName = v.ContactName, Phone = v.Phone, Email = v.Email, Notes = v.Notes,
        Assets = [.. v.Assets.Select(ToRef).Where(x => x is not null).Select(x => x!)],
        Licenses = [.. v.Licenses.Select(ToRef).Where(x => x is not null).Select(x => x!)]
    };

    public static LocationDto ToDto(Location l) => new()
    {
        Id = l.Id, Name = l.Name, Building = l.Building, Floor = l.Floor, Room = l.Room, IsActive = l.IsActive, FullName = l.FullName,
        Assets = [.. l.Assets.Select(ToRef).Where(x => x is not null).Select(x => x!)]
    };

    public static CategoryDto ToDto(AssetCategory c) => new()
    {
        Id = c.Id, Code = c.Code, Name = c.Name, Description = c.Description, DepreciationMonths = c.DepreciationMonths,
        Assets = [.. c.Assets.Select(ToRef).Where(x => x is not null).Select(x => x!)]
    };

    public static AuditDto ToDto(AuditLog l) => new()
    {
        Id = l.Id, EntityName = l.EntityName, EntityKey = l.EntityKey, Action = l.Action, Changes = l.Changes,
        UserName = l.UserName, Timestamp = l.Timestamp
    };

    public static AgentDto ToDto(Agent a, bool detail = false)
    {
        var dto = new AgentDto
        {
            Id = a.Id, DeviceUuid = a.DeviceUuid, SerialNumber = a.SerialNumber, HostName = a.HostName,
            Manufacturer = a.Manufacturer, Model = a.Model, CpuName = a.CpuName, CpuCores = a.CpuCores,
            TotalMemoryMb = a.TotalMemoryMb, OsName = a.OsName, OsVersion = a.OsVersion, OsBuild = a.OsBuild,
            LoggedOnUser = a.LoggedOnUser, MacAddresses = a.MacAddresses, IpAddress = a.IpAddress,
            BitLockerOn = a.BitLockerOn, DiskTotalGb = a.DiskTotalGb, AgentVersion = a.AgentVersion, Status = a.Status,
            EnrolledAt = a.EnrolledAt, LastSeenAt = a.LastSeenAt, LastInventoryAt = a.LastInventoryAt,
            RebootPending = a.RebootPending, IsOnline = a.IsOnline, AssetId = a.AssetId, Asset = ToRef(a.Asset),
            SoftwareCount = a.Software.Count
        };
        if (detail)
        {
            dto.Software = [.. a.Software.OrderBy(s => s.Name).Select(s => new AgentSoftwareDto { Id = s.Id, AgentId = s.AgentId, Name = s.Name, Publisher = s.Publisher, Version = s.Version, InstallDate = s.InstallDate, Source = s.Source })];
        }
        return dto;
    }
}
