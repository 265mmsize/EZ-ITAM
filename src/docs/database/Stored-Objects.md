# Stored Objects

## Kết quả kiểm tra repository

| Object type | Kết quả |
|---|---|
| View | Không tìm thấy định nghĩa trong EF migrations hoặc mã nguồn ứng dụng |
| Stored procedure | Không tìm thấy định nghĩa hoặc lời gọi từ ứng dụng |
| User-defined function | Không tìm thấy định nghĩa hoặc lời gọi từ ứng dụng |
| Trigger | Không tìm thấy định nghĩa trong EF migrations hoặc mã nguồn ứng dụng |

Vì không có các object này trong repository, không có definition SQL để bàn giao. Kết quả này không chứng minh rằng production database không có object được tạo thủ công; hãy kiểm kê `sys.views`, `sys.procedures`, `sys.objects`/`sys.triggers` trên từng database bằng tài khoản chỉ đọc.

## SQL xử lý HRIS trong ứng dụng

Đây là truy vấn inline trong `EzItam.Api/ExternalService/HrService.cs`, không phải stored procedure. Nó đọc HRIS; phần đồng bộ và transaction được thực hiện trong `EmployeeSyncBackgroundService`.

```sql
WITH TreeDept AS (
    SELECT ID, ParentID, Code, OrgStructureName, OrgStructureLevel
    FROM D03_OrgStructure
    WHERE Code = @OrgCode

    UNION ALL

    SELECT d.ID, d.ParentID, d.Code, d.OrgStructureName, d.OrgStructureLevel
    FROM D03_OrgStructure d
    INNER JOIN TreeDept t ON d.ParentID = t.ID
)
SELECT
    e.Code,
    e.EmployeeName,
    null AS Email,
    td.Code AS DeptCode,
    td.OrgStructureName AS DeptName,
    td.OrgStructureLevel AS DeptLevel,
    tt.Code AS TitleCode,
    tt.JobTitleName AS TitleName,
    p.Code AS PositionCode,
    p.PositionName AS PositionName,
    CASE WHEN e.ResignNo IS NOT NULL THEN 'Y' ELSE 'N' END AS IsResigned
FROM D04_Employee e
INNER JOIN TreeDept td ON e.OrgStructureID = td.ID
LEFT JOIN C04_JobTitle tt ON e.JobTitleID = tt.ID
LEFT JOIN C04_Position p ON e.PositionID = p.ID
WHERE e.IsDelete != 1 OR e.IsDelete IS NULL
ORDER BY td.OrgStructureLevel, e.Code ASC;
```
