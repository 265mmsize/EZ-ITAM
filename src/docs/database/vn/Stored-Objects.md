# Stored Objects and Integrations

## Kết quả kiểm tra repository

| Object type | Kết quả |
|---|---|
| View | Không tìm thấy định nghĩa trong EF migrations hoặc mã nguồn ứng dụng |
| Stored procedure | Không tìm thấy định nghĩa hoặc lời gọi từ ứng dụng |
| User-defined function | Không tìm thấy định nghĩa hoặc lời gọi từ ứng dụng |
| Trigger | Không tìm thấy định nghĩa trong EF migrations hoặc mã nguồn ứng dụng |

Hangfire không dùng các object này để thay thế truy vấn nghiệp vụ trong source. `Hangfire.SqlServer` 1.8.25 quản lý schema storage riêng `[HangFire]` (các bảng và index); danh sách bảng được ghi ở sheet `Hangfire Storage` trong workbook. DDL storage không được chép vào script migration ứng dụng vì phiên bản package sở hữu vòng đời schema đó. Các database production có thể có object tạo thủ công ngoài repository; cần kiểm tra catalog riêng.

Vì không có các object này trong repository, không có definition SQL để bàn giao. Kết quả này không chứng minh rằng production database không có object được tạo thủ công; hãy kiểm kê `sys.views`, `sys.procedures`, `sys.objects`/`sys.triggers` trên từng database bằng tài khoản chỉ đọc.

## SQL xử lý HRIS trong ứng dụng

Đây là truy vấn inline trong `EzItam.Api/ExternalService/HrService.cs`, không phải stored procedure. Nó đọc HRIS; phần đồng bộ và transaction trên database ứng dụng được thực hiện trong `EmployeeSyncBackgroundService`. Nguồn là connection key `HRIS_Db`; tham số `@OrgCode` được bind bằng `SqlCommand.Parameters`.

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
    @OrgCode AS OrgCode,
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

## Luồng đồng bộ nhân viên

1. `Program.cs` đăng ký recurring job `EmployeeSyncBackgroundService.SyncEmployeeDataAsync` qua Hangfire. Job ID, lịch cron và org code đến từ `HangFireJobSetting:HrSync`; lịch mặc định của repository là Chủ Nhật 03:00 theo timezone local của server. Retry được cấu hình 3 lần với delays 60, 300 và 600 giây.
2. Job lấy danh sách từ HRIS trước khi mở transaction của database ứng dụng. Nếu danh sách rỗng, job ghi warning rồi thoát, giữ nguyên dữ liệu staging/master cũ.
3. Với dữ liệu không rỗng, transaction của database ứng dụng xóa và nạp lại `EmployeeTemps`, tạo/cập nhật `Departments`, rồi cập nhật `Employees`. Không có distributed transaction bao trọn database HRIS và database ứng dụng.
4. `EmployeeTemps` là staging snapshot bị thay thế mỗi lần chạy thành công, không lưu lịch sử. `IsResigned` chuẩn hóa về `Y`/`N`; unique key staging là `(OrgCode, EmployeeNo)`. `Departments.Code` và `Employees.EmployeeNo` cũng unique.
5. Nhân viên `N` được upsert theo `EmployeeNo`, cập nhật tên/email/chức danh/phòng ban và bật `IsActive`. Nhân viên `Y` hiện có được đánh dấu inactive; không có thao tác xóa vật lý trong luồng này. Phòng ban lấy từ nhân viên chưa nghỉ việc; mã chưa có thì tạo mới, tên thay đổi thì cập nhật.

## Các điểm dữ liệu cần xác nhận

- SQL HRIS hiện đặt `NULL AS Email`. Mapper đổi giá trị đọc thành chuỗi rỗng; job sau đó có thể ghi chuỗi rỗng đè `Employees.Email`. HRIS hiện không phải nguồn email theo query này.
- `HrController` cung cấp `GET /api/v1/external/hr/employees` với `[AllowAnonymous]`; endpoint trả dữ liệu nhân viên trực tiếp từ HRIS và không phải trigger đồng bộ staging. Xác nhận quyền truy cập theo yêu cầu triển khai.
- `EmployeeTemps.DateValue1..9` hiện không được query đồng bộ này điền; các cột chuỗi staging đó không được xem là ngày SQL đã chuẩn hóa.
