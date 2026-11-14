# EZ-ITAM — Database DBA Review
## 1. Tổng quan

- **DBMS mặc định:** Microsoft SQL Server 2025 RTM (17.0.1000.7), Express Edition (64-bit), Database: EzItam, Compatibility level: 170, Recovery model: SIMPLE, User schema in use: dbo.
- **Các database:** database nghiệp vụ/Identity dùng `ConnectionStrings:Default`; Hangfire SQL Server storage dùng `ConnectionStrings:Hangfire`; HRIS nguồn dùng `ConnectionStrings:HRIS_Db`. Hangfire schema được package quản lý độc lập với EF Core migrations của ứng dụng.
- **Số bảng trong EF migrations:** 16 bảng nghiệp vụ, 7 bảng ASP.NET Core Identity và `__EFMigrationsHistory` (24 bảng). Các bảng Hangfire ở database riêng không tính vào tổng này.

Danh sách bảng:

| Nhóm | Bảng | Chức năng |
|---|---|---|
| Tài sản | `Assets` | Hồ sơ tài sản, phân loại, trạng thái, mua sắm, vị trí và người đang sử dụng |
| Tài sản | `AssetCategories` | Danh mục và thời hạn khấu hao |
| Tài sản | `AssetAssignments` | Lịch sử cấp phát/thu hồi tài sản |
| Tài sản | `MaintenanceRecords` | Lịch sử sửa chữa/bảo trì |
| Tài sản | `Locations` | Danh mục vị trí |
| Tài sản | `Vendors` | Nhà cung cấp |
| Nhân sự | `Departments` | Danh mục phòng ban |
| Nhân sự | `Employees` | Nhân viên nội bộ, có nguồn đồng bộ HRIS |
| Tích hợp HRIS | `EmployeeTemps` | Bảng staging nhận bản chụp nhân viên/phòng ban từ HRIS |
| License | `SoftwareLicenses` | Danh mục license phần mềm |
| License | `LicenseAssignments` | Lịch sử gán/thu hồi license cho nhân viên hoặc tài sản |
| Agent | `Agents` | Thiết bị đã đăng ký agent và thông tin inventory gần nhất |
| Agent | `AgentSoftware` | Phần mềm agent báo cáo trong lần inventory hiện tại |
| Kiểm kê | `Inventories` | Đợt kiểm kê và trạng thái |
| Kiểm kê | `InventoryDetails` | Kết quả kiểm kê, so sánh thông tin hệ thống với thông tin kiểm kê |
| Audit | `AuditLogs` | Nhật ký thay đổi do `AppDbContext` ghi |
| Identity | `AspNetUsers` | Tài khoản đăng nhập và thông tin xác thực ASP.NET Core Identity |
| Identity | `AspNetRoles` | Định nghĩa role |
| Identity | `AspNetUserRoles` | Liên kết user-role |
| Identity | `AspNetUserClaims` | Claims của user |
| Identity | `AspNetRoleClaims` | Claims của role |
| Identity | `AspNetUserLogins` | External login |
| Identity | `AspNetUserTokens` | Token của Identity |
| EF Core | `__EFMigrationsHistory` | Theo dõi phiên bản migration đã áp dụng |

## 2. Cấu trúc, khóa và ràng buộc

Chi tiết cột, kiểu SQL Server, độ dài, NULL, PK/FK, index, default và mô tả nghiệp vụ nằm trong [Data Dictionary (Excel)](EZITAM_Data_Dictionary.xlsx). Sheet `Hangfire Storage` kiểm kê các bảng package-owned, không thay thế dictionary cột đầy đủ của Hangfire. ERD nằm trong [ERD.md](ERD.md). DDL đầy đủ của database ứng dụng nằm trong [EZITAM.SqlServer.Migrations.sql](EZITAM.SqlServer.Migrations.sql); Hangfire dùng schema và script riêng của package.

Các enum nghiệp vụ (`AssetStatus`, `MaintenanceType`, `LicenseType`, `AuditAction`, `AgentStatus`, `AgentRing`, `InventoryStatus`) được lưu dưới dạng `int`; danh sách giá trị và ordinal được định nghĩa trong `EzItam.Contracts/Enums.cs`. Các unique index nổi bật gồm `Assets.AssetTag`, `AssetCategories.Code`, `Departments.Code`, `Employees.EmployeeNo`, `Agents.DeviceUuid`, `Inventories.Name`, `InventoryDetails(InventoryId, IdAsset)` và `EmployeeTemps(OrgCode, EmployeeNo)`.

## 3. Dữ liệu, hiệu suất và concurrency

- **Index:** đầy đủ các index EF Core đã sinh được liệt kê trong sheet `Indexes` của workbook và DDL. Không có thông tin thống kê, execution plan, kích thước bảng, selectivity hoặc workload thực tế trong repository; do đó chưa thể kết luận index nào cần thêm/xóa hay hiệu năng thực tế.
- **View/Stored Procedure/Function/Trigger:** không thấy migration hoặc định nghĩa trong mã ứng dụng. Luồng HRIS dùng câu SQL CTE trực tiếp trong `HrService`, không gọi stored procedure. Cần kiểm tra catalog của các database thực tế để phát hiện object được tạo thủ công ngoài repository.
- **INSERT/UPDATE/DELETE:** API truy cập DB qua EF Core `AppDbContext`; các thay đổi thường gọi `SaveChangesAsync`. Lịch sử cấp phát, bảo trì, license và kiểm kê được lưu trong bảng riêng. Một số delete vật lý được cung cấp qua API; hành vi cascade/restrict/set-null được thể hiện trong FK.
- **Transaction:** EF Core dùng transaction mặc định cho từng `SaveChanges`. HR sync đọc HRIS trước, rồi transaction tường minh trên database ứng dụng bao trọn thao tác thay staging snapshot, đồng bộ phòng ban/nhân viên và cập nhật trạng thái nghỉ việc; đây không phải distributed transaction giữa HRIS và EZ-ITAM. Một số thao tác inventory cũng mở transaction tường minh. Retry SQL Server transient failure được bật tối đa 3 lần. Với bản ghi mới, custom audit lưu entity trước rồi mới ghi log ở `SaveChanges` thứ hai; không thấy transaction ngoài bao trọn hai lần save này, vì vậy lỗi khi lưu log sau đó có thể để lại entity mà thiếu audit record.
- **Concurrency:** không thấy `rowversion`/optimistic concurrency token cho bảng nghiệp vụ. Identity có `ConcurrencyStamp`; các thao tác nghiệp vụ có thể theo cơ chế last-write-wins nếu không có khóa/kiểm tra bổ sung. Cần đánh giá lại cho cập nhật đồng thời (đặc biệt tồn kho/license seat và kiểm kê) theo workload thực tế.
- **Gợi ý hiệu năng/khả năng mở rộng:** thu thập query plans, Query Store/DMV và dung lượng; kiểm tra index theo workload; xem xét phân trang cho lịch sử lớn; đặt chính sách archive/retention cho `AuditLogs`, `AgentSoftware`, lịch sử assignment và staging. Tránh thêm index chỉ dựa trên tên cột khi chưa có kế hoạch truy vấn.

## 4. Bảo mật và quản trị

- **Tài khoản và phân quyền SQL Server:** `sa` là tài khoản quản trị SQL Server; `itasset` có toàn quyền truy cập schema `dbo` của database `EzItam`. Cần đối chiếu login, database user, quyền và role thực tế trong SQL Server để xác nhận phạm vi cấp quyền. Không đưa mật khẩu hoặc thông tin kết nối vào tài liệu.
- **Phân quyền trong tương lai:** có thể tạo tài khoản/role chỉ đọc dữ liệu hoặc chỉ quản trị database mà không được đọc, sửa dữ liệu nghiệp vụ; cấp quyền tối thiểu theo nhiệm vụ và kiểm tra quyền thực tế, kể cả quyền kế thừa từ role. Không mặc định dùng `sa` cho truy cập thường xuyên.
- **Audit:** bảng `dbo.AuditLogs` lưu thay đổi đối với các entity được `AppDbContext` theo dõi, gồm entity/key, action, actor, timestamp và JSON thay đổi; `Agent`, `AgentSoftware`, Identity và `EmployeeTemp` không thuộc danh sách audit tự động. Đây là audit do ứng dụng ghi, không phải SQL Server Audit và không bao phủ các thay đổi trực tiếp trên database. Có thể cân nhắc cấu hình audit ở cấp SQL Server trong tương lai.
- **Backup và recovery:** Đặt lịch full backup thông qua Maintenance Plan. Tuy nhiên, SQL Server 2025 Express không có SQL Server Agent để chạy lịch Maintenance Plan. Do đó, có thể lập lịch full backup bằng Windows Task Scheduler và lệnh `BACKUP DATABASE` thay thế.

## 5. Tích hợp

- **HRIS:** `HrService` đọc database nguồn qua SQL Server `Microsoft.Data.SqlClient`, connection key `HRIS_Db`; `@OrgCode` được truyền như SQL parameter. CTE đệ quy lấy phòng ban gốc và cấp dưới từ `D03_OrgStructure`, sau đó join `D04_Employee`, `C04_JobTitle`, `C04_Position`; lọc nhân viên đã xóa logic và ánh xạ mã/tên nhân viên, phòng ban, chức danh, vị trí và cờ nghỉ việc. Query hiện trả `NULL` cho Email và không chọn các `DateValue1..9`. Chi tiết truy vấn nằm trong [Stored-Objects.md](Stored-Objects.md).
- **Luồng đồng bộ HRIS:** Hangfire gọi `EmployeeSyncBackgroundService.SyncEmployeeDataAsync`. Job đọc nguồn HRIS trước; nếu kết quả rỗng, chỉ ghi warning và không xóa snapshot/không cập nhật master. Nếu có dữ liệu, job mở transaction trên database ứng dụng, xóa và nạp lại toàn bộ `EmployeeTemps`, đồng bộ `Departments` theo `Code`, rồi upsert `Employees` theo `EmployeeNo`: nhân viên `N` được tạo/cập nhật và bật `IsActive`; nhân viên `Y` hiện có được đánh dấu inactive, không xóa vật lý. `EmployeeTemps` là snapshot hiện tại, không phải lịch sử. Unique keys là `Departments.Code`, `Employees.EmployeeNo`, `EmployeeTemps(OrgCode, EmployeeNo)`; staging không có FK sang các mã HRIS.
- **Điểm cần xác nhận trong HRIS sync:** đường đọc thật trong `HrService` không gán `HrEmployeeDto.OrgCode`, nên property giữ giá trị mặc định rỗng khi job ghi `EmployeeTemps.OrgCode`; đường mock thì có gán. Nếu cần phân biệt nhiều tổ chức, hãy gán org code nguồn vào DTO trước khi lưu. Query cũng chiếu `NULL AS Email`, trong khi mapper chuyển giá trị đọc thành chuỗi rỗng và job ghi email đó lên `Employees`; xác nhận đây có phải quy tắc mong muốn để tránh xóa email đã có. Không thay đổi xử lý này trong deliverable tài liệu.
- **API HRIS:** `GET /api/v1/external/hr/employees?orgCode=...` đọc trực tiếp HRIS và trả kết quả; endpoint có `[AllowAnonymous]`. Đây là read endpoint riêng, không chạy transaction ghi `EmployeeTemps`/`Employees`.
- **Agent:** agent gửi thông tin inventory qua API; backend cập nhật `Agents` và thay thế danh sách phần mềm hiện tại của agent trong `AgentSoftware`. Đây không phải lịch sử snapshot phần mềm.
- **ERP:** không tìm thấy kết nối hoặc schema ERP được mô tả rõ trong repository. Chưa có contract, external ID mapping, outbox, CDC hay message broker được nhận diện. Không nên coi HRIS là ERP tích hợp sẵn.
- **API–DB:** web client gọi API; comment và cấu hình DI chỉ ra API là tiến trình truy cập DB. Controllers/services gọi EF Core. Đồng bộ HRIS dùng adapter SQL trực tiếp riêng; Hangfire lưu trạng thái job vào database SQL Server độc lập.

### Hangfire SQL Server storage

- **Package/cấu hình:** `Hangfire.SqlServer` 1.8.25, tương thích dữ liệu `Version_180`, dùng recommended serializer settings và simple assembly type names. `AddHangfireServer` chạy `Environment.ProcessorCount * 2` workers. Storage connection key là `ConnectionStrings:Hangfire`; đây không phải database `Default` của ứng dụng.
- **Schema:** package script của phiên bản hiện tại tạo schema `[HangFire]` và các bảng `Schema`, `Job`, `State`, `JobParameter`, `JobQueue`, `Server`, `Hash`, `List`, `Set`, `Value`, `Counter`, `AggregatedCounter`, cùng các index storage. Chức năng tổng quát từng bảng nằm trong sheet `Hangfire Storage` của workbook. Đây là cấu trúc nội bộ do Hangfire sở hữu: không chỉnh tay, không gộp vào migration EF nghiệp vụ và không giả định nằm cùng database với `Default`.
- **Recurring job:** `HangFireJobSetting:HrSync` chứa `JobId`, `CronExpression`, `OrgCode`; mặc định trong cấu hình repository là Chủ Nhật 03:00 (`0 3 * * 0`) theo `TimeZoneInfo.Local`. Job có tối đa 3 lần retry với khoảng chờ 60/300/600 giây; khi hết lần thử, job chuyển sang trạng thái failed. Giá trị `OrgCode` thực tế không được lặp lại trong tài liệu.
- **Dashboard:** Hangfire Dashboard được mount tại `/hangfire`; repository không đăng ký custom authorization filter cho dashboard. Xác minh cấu hình truy cập thực tế ở môi trường deploy trước khi mở endpoint ngoài phạm vi tin cậy.

##### Deliverables

1. `EZITAM.SqlServer.Migrations.sql` — SQL Server DDL/migration script từ database trống đến migration mới nhất. **Chỉ chạy trên database trống hoặc môi trường thử nghiệm sau khi DBA review**; không phải script idempotent và không trích xuất schema trực tiếp từ production.
2. `ERD.md` — Mermaid ERD của bảng nghiệp vụ và Identity.
3. `EZITAM_Data_Dictionary.xlsx` — Data Dictionary, bảng/cột/index/quan hệ.
4. `Stored-Objects.md` — trạng thái đối chiếu view, procedure, function, trigger trong source/migrations; truy vấn HRIS và Hangfire storage được mô tả riêng.
5. `Optimization-Preflight.sql` — truy vấn chỉ đọc kiểm tra dữ liệu cũ trước khi áp dụng migration tối ưu.
7. `sql_cchema.sql` - Script schema-only xuất từ SQL Server, gồm tạo database, bảng, index và ràng buộc; dùng để tham khảo, không phải script nâng cấp hoặc backup dữ liệu.
