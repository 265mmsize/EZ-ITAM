namespace EzItam.Api.Mapping;

/// <summary>API 요청(DTO) → 엔티티. 쓰기 가능한 필드만 복사하고 계산 필드와 참조는 무시합니다.</summary>
public static class Inbound
{
    private static string Required(string? value, string message)
        => string.IsNullOrWhiteSpace(value) ? throw new DomainException(message) : value.Trim();

    public static Asset ToAsset(AssetDto d) => new()
    {
        AssetTag = Required(d.AssetTag, "자산번호는 필수입니다.").ToUpperInvariant(), Name = Required(d.Name, "자산명은 필수입니다."), CategoryId = d.CategoryId, Status = d.Status,
        Manufacturer = d.Manufacturer, Model = d.Model, SerialNumber = d.SerialNumber, VendorId = d.VendorId,
        PurchaseDate = d.PurchaseDate, PurchasePrice = d.PurchasePrice, WarrantyExpiry = d.WarrantyExpiry,
        LocationId = d.LocationId, CurrentEmployeeId = d.CurrentEmployeeId, IpAddress = d.IpAddress, MacAddress = d.MacAddress,
        HostName = d.HostName, OperatingSystem = d.OperatingSystem, Notes = d.Notes
    };

    public static MaintenanceRecord ToMaintenance(MaintenanceDto d) => new()
    {
        Id = d.Id, AssetId = d.AssetId, Type = d.Type, Date = d.Date, Description = Required(d.Description, "내용은 필수입니다."),
        Cost = d.Cost, VendorId = d.VendorId, CompletedAt = d.CompletedAt
    };

    public static SoftwareLicense ToLicense(SoftwareLicenseDto d) => new()
    {
        Name = Required(d.Name, "소프트웨어명은 필수입니다."), Publisher = d.Publisher, Version = d.Version, LicenseType = d.LicenseType,
        LicenseKey = d.LicenseKey, TotalSeats = d.TotalSeats, PurchaseDate = d.PurchaseDate, ExpiryDate = d.ExpiryDate,
        VendorId = d.VendorId, Cost = d.Cost, Notes = d.Notes
    };

    public static Employee ToEmployee(EmployeeDto d) => new()
    {
        EmployeeNo = Required(d.EmployeeNo, "사번은 필수입니다."), Name = Required(d.Name, "이름은 필수입니다."), Email = d.Email, Title = d.Title,
        DepartmentId = d.DepartmentId, IsActive = d.IsActive
    };

    public static Department ToDepartment(DepartmentDto d) => new()
    {
        Code = Required(d.Code, "부서코드는 필수입니다.").ToUpperInvariant(), Name = Required(d.Name, "부서명은 필수입니다."), IsActive = d.IsActive
    };

    public static Vendor ToVendor(VendorDto d) => new()
    {
        Name = Required(d.Name, "공급업체명은 필수입니다."), ContactName = d.ContactName, Phone = d.Phone, Email = d.Email, Notes = d.Notes
    };

    public static Location ToLocation(LocationDto d) => new()
    {
        Name = Required(d.Name, "위치명은 필수입니다."), Building = d.Building, Floor = d.Floor, Room = d.Room, IsActive = d.IsActive
    };

    public static AssetCategory ToCategory(CategoryDto d) => new()
    {
        Code = Required(d.Code, "분류 코드는 필수입니다.").ToUpperInvariant(), Name = Required(d.Name, "분류명은 필수입니다."),
        Description = d.Description, DepreciationMonths = d.DepreciationMonths
    };
}
