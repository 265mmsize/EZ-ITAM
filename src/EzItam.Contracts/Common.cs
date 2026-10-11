namespace EzItam.Contracts;

/// <summary>페이지 단위 응답. 목록 조회는 항상 이 형식으로 반환합니다.</summary>
public sealed class PageResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

/// <summary>API 오류 응답 본문. 업무 규칙 위반은 400, 인증은 401, 권한은 403 으로 구분합니다.</summary>
public sealed class ApiError
{
    public string Message { get; init; } = string.Empty;
}

public sealed class SelectOption
{
    public string Value { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
}

public sealed class AssetFilter
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public AssetStatus? Status { get; set; }
    public int? LocationId { get; set; }
    public int? DepartmentId { get; set; }
    public int? EmployeeId { get; set; }
    public bool WarrantyExpiringOnly { get; set; }
    public string Sort { get; set; } = "tag";
    public bool Desc { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
