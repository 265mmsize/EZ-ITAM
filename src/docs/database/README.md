# EZ-ITAM — Database DBA Review

**Phạm vi:** tài liệu này được lập từ mã nguồn, EF Core model và các migration có trong repository. Nó mô tả thiết kế trong repository, không phải bản kiểm kê trực tiếp từ một máy chủ production. Không có dữ liệu nghiệp vụ, bản ghi tài khoản hay thông tin kết nối nào được đưa vào các deliverable.

## 1. Tổng quan

- **DBMS mặc định:** Microsoft SQL Server, chọn bằng `Database:Provider` (fallback là `SqlServer`). Mã nguồn không khóa phiên bản SQL Server; vì vậy không thể xác nhận phiên bản engine đang chạy.
- **Provider:** Entity Framework Core SQL Server 8.0.11; EF Core SQLite 8.0.11 cũng được tham chiếu. `EZITAM_PROFILE=Sqlite` là profile demo; profile này gọi `EnsureCreated`, còn SQL Server chạy migrations. SQLite không được xem là môi trường production thay thế tương đương nếu chưa kiểm thử riêng.
- **Các database:** database nghiệp vụ/Identity dùng connection `Default`; Hangfire được cấu hình với connection riêng và SQL Server storage. Hangfire schema do package tạo/quản lý, không thuộc EF Core migrations trong repository và không nằm trong DDL/ERD nghiệp vụ đính kèm.
- **Số bảng trong EF migrations:** 16 bảng nghiệp vụ, 7 bảng ASP.NET Core Identity và `__EFMigrationsHistory` (24 bảng). Các bảng Hangfire ở database riêng không tính vào tổng này.
- **Migrations hiện có:** `InitialCreate`, `AddAgentRing`, `addInventory`, `fix_inventory_detail_table`, `addTableEmployeeTemp`, `OptimizeDataIntegrityAndQueryIndexes`, `OptimizeDatabaseColumnTypesAndLengths`; script đính kèm là SQL Server script từ database trống đến migration mới nhất.

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

Chi tiết cột, kiểu SQL Server, độ dài, NULL, PK/FK, index, default và mô tả nghiệp vụ nằm trong [Data Dictionary (Excel)](EZITAM_Data_Dictionary.xlsx). ERD nằm trong [ERD.md](ERD.md). DDL đầy đủ nằm trong [EZITAM.SqlServer.Migrations.sql](EZITAM.SqlServer.Migrations.sql).

Các enum nghiệp vụ (`AssetStatus`, `MaintenanceType`, `LicenseType`, `AuditAction`, `AgentStatus`, `AgentRing`, `InventoryStatus`) được lưu dưới dạng `int`; danh sách giá trị và ordinal được định nghĩa trong `EzItam.Contracts/Enums.cs`. Các unique index nổi bật gồm `Assets.AssetTag`, `AssetCategories.Code`, `Departments.Code`, `Employees.EmployeeNo`, `Agents.DeviceUuid`, `Inventories.Name`, `InventoryDetails(InventoryId, IdAsset)` và `EmployeeTemps(OrgCode, EmployeeNo)`.

## Schema integrity and query optimization (`OptimizeDataIntegrityAndQueryIndexes`)

Migration `OptimizeDataIntegrityAndQueryIndexes` thực hiện các thay đổi sau:

- Đặt `EmployeeTemps.IsResigned` thành `NOT NULL`, mặc định `N`, kiểm tra chỉ nhận `Y`/`N`. Các giá trị NULL hiện có được migration chuyển thành `N`, phù hợp với fallback đang dùng trong HR sync.
- Không siết các trường nullable khác chỉ dựa trên cảm tính: email, vendor, vị trí/người dùng và hardware metadata có thể thực sự chưa biết hoặc chưa gán; chỉ đổi chúng khi nghiệp vụ xác nhận bắt buộc.
- Đồng bộ default ở DB với default của model cho trạng thái tài sản/agent/inventory, trạng thái active, license type/seats và thời hạn khấu hao.
- Thêm `CHECK` cho miền enum và các invariant hiển nhiên: giá trị tiền, số seat/khấu hao, số đo phần cứng, số lượt quét, ngày bắt đầu/kết thúc kiểm kê, và mỗi license assignment phải có đúng một đối tượng nhận.
- Thêm filtered unique indexes bảo đảm mỗi tài sản chỉ có một assignment đang mở, cùng một license không bị gán đang mở trùng cho cùng nhân viên/tài sản; `Agents.TokenHash` duy nhất khi có giá trị.
- Thêm index phục vụ màn hình maintenance chưa hoàn thành và truy vấn audit theo entity/key/time; thêm index thời gian liên lạc gần nhất của agent. Index audit cũ `(EntityName, EntityKey)` được thay bằng index có thêm `Timestamp`; index đơn `Timestamp` vẫn giữ cho truy vấn toàn cục.
- `LicenseService` từ chối yêu cầu gán cả employee lẫn asset hoặc không gán đối tượng nào, thống nhất với check constraint.

Các constraint/index có thể từ chối dữ liệu production đã vi phạm invariant. Trước khi áp dụng, chạy [Optimization-Preflight.sql](Optimization-Preflight.sql) trên database đích, xử lý các nhóm vi phạm có chủ sở hữu nghiệp vụ xác nhận, và thử migration trên bản sao đã restore. Không tự xóa/sửa lịch sử gán để làm migration qua được. Bản script preflight chỉ trả về số lượng, không trả dữ liệu nghiệp vụ.

Các index mới bám theo truy vấn hiện có trong ứng dụng; chưa có workload, Query Store hay execution plan production để chứng minh mức tăng tốc. Tìm kiếm `Contains` thường tạo điều kiện dạng `%term%`, không tận dụng tốt B-tree index; không thêm index thường chỉ để tối ưu kiểu tìm kiếm đó, cân nhắc full-text hoặc đổi sang prefix search nếu nghiệp vụ cho phép. Sau triển khai, theo dõi thời gian truy vấn, logical reads, write overhead và mức sử dụng index.

## Tối ưu kiểu dữ liệu và độ dài

Migration `OptimizeDatabaseColumnTypesAndLengths` đưa các trường ngày nghiệp vụ (`PurchaseDate`, `WarrantyExpiry`, ngày bảo trì/hoàn thành và ngày mua/hết hạn license) về SQL Server `date`, phù hợp với form và quy tắc ngày hiện có; các `DateTime` trong API vẫn được giữ nguyên để tránh đổi JSON contract. Trước khi áp dụng, chạy các kiểm tra hậu tố `_time_component` trong preflight. Nếu có bản ghi khác 00:00:00, migration sẽ bỏ phần giờ; cần xác nhận với chủ sở hữu nghiệp vụ trước khi tiếp tục. Cũng xử lý trước mọi giá trị IP trên 64 ký tự hoặc token hash không đúng 64 ký tự; script chỉ trả số lượng vi phạm.

Các trường tiền dùng `decimal(14,2)`, tương ứng mức tối đa đang được kiểm tra là 999,999,999,999 với hai chữ số thập phân. `Agent.IpAddress` và `Asset.IpAddress` giới hạn 64 ký tự, đủ cho biểu diễn IPv4/IPv6 có khoảng dự phòng cho scope ID; agent giữ giới hạn đồng bộ ở đầu vào. `Agents.TokenHash` dùng `char(64)` vì SHA-256 hex luôn đủ 64 ký tự; preflight kiểm tra các giá trị hiện có có đúng độ dài trước khi đổi. Email nhân viên, staging HRIS và nhà cung cấp tăng lên 254 ký tự để phù hợp giới hạn địa chỉ email phổ biến. SQLite không nhận các kiểu SQL Server-specific `date`/`char`; model của SQLite giữ ánh xạ provider mặc định.

Không thu hẹp độ dài của mã nhân viên, mã staging, serial, UUID, hostname, tên phần mềm hoặc giá trị HRIS tùy biến khi chưa có hợp đồng dữ liệu/đo đạc production. Thay đổi precision của decimal diễn đạt giới hạn nghiệp vụ hiện có, không giảm dung lượng lưu trữ SQL Server so với `decimal(18,2)` vì cả hai precision đều nằm trong cùng nhóm lưu trữ.

Việc đổi kiểu cột cần schema-modification lock và có thể ảnh hưởng ghi/đọc trong lúc migration; lập lịch trong maintenance window phù hợp và xác nhận backup/restore trước rollout. Chưa áp dụng migration nào lên production trong phạm vi công việc này.

## 3. Dữ liệu, hiệu suất và concurrency

- **Index:** đầy đủ các index EF Core đã sinh được liệt kê trong sheet `Indexes` của workbook và DDL. Không có thông tin thống kê, execution plan, kích thước bảng, selectivity hoặc workload thực tế trong repository; do đó chưa thể kết luận index nào cần thêm/xóa hay hiệu năng thực tế.
- **View/Stored Procedure/Function/Trigger:** không thấy migration hoặc định nghĩa trong mã ứng dụng. Luồng HRIS dùng câu SQL CTE trực tiếp trong `HrService`, không gọi stored procedure. Cần kiểm tra catalog của các database thực tế để phát hiện object được tạo thủ công ngoài repository.
- **INSERT/UPDATE/DELETE:** API truy cập DB qua EF Core `AppDbContext`; các thay đổi thường gọi `SaveChangesAsync`. Lịch sử cấp phát, bảo trì, license và kiểm kê được lưu trong bảng riêng. Một số delete vật lý được cung cấp qua API; hành vi cascade/restrict/set-null được thể hiện trong FK.
- **Transaction:** EF Core dùng transaction mặc định cho từng `SaveChanges`. HR sync và một số thao tác inventory mở transaction tường minh. Retry SQL Server transient failure được bật tối đa 3 lần. Với bản ghi mới, custom audit lưu entity trước rồi mới ghi log ở `SaveChanges` thứ hai; không thấy transaction ngoài bao trọn hai lần save này, vì vậy lỗi khi lưu log sau đó có thể để lại entity mà thiếu audit record.
- **Concurrency:** không thấy `rowversion`/optimistic concurrency token cho bảng nghiệp vụ. Identity có `ConcurrencyStamp`; các thao tác nghiệp vụ có thể theo cơ chế last-write-wins nếu không có khóa/kiểm tra bổ sung. Cần đánh giá lại cho cập nhật đồng thời (đặc biệt tồn kho/license seat và kiểm kê) theo workload thực tế.
- **Gợi ý hiệu năng/khả năng mở rộng:** thu thập query plans, Query Store/DMV và dung lượng; kiểm tra index theo workload; xem xét phân trang cho lịch sử lớn; đặt chính sách archive/retention cho `AuditLogs`, `AgentSoftware`, lịch sử assignment và staging. Tránh thêm index chỉ dựa trên tên cột khi chưa có kế hoạch truy vấn.

## 4. Bảo mật và quản trị

- Role ứng dụng được seed gồm `Admin`, `Manager`, `Viewer`; policy `CanEdit` dành cho Admin/Manager và `AdminOnly` cho Admin. Đây là authorization tầng API/Identity, không phải database role SQL Server. Repository chỉ cho thấy ứng dụng sử dụng connection string của service; không đủ căn cứ xác định user/role, least privilege hay tách quyền đọc/ghi trên SQL Server.
- Audit do `AppDbContext` xử lý cho các entity nghiệp vụ được liệt kê trong code; log chứa tên entity/key, action, actor, timestamp và JSON thay đổi. `Agent`, `AgentSoftware`, Identity và `EmployeeTemp` không nằm trong danh sách audited entities. Không có FK từ log đến entity; dữ liệu cũ không tự mất theo cascade.
- Không tìm thấy chính sách backup/restore, RPO/RTO, mã hóa-at-rest, retention hoặc data purge trong repository. Các chính sách này cần được xác nhận ở vận hành/DBA và hạ tầng.
- **Lưu ý bảo mật cấu hình:** có cấu hình kết nối và thông tin xác thực dạng literal trong file cấu hình được theo dõi trong repository. Giá trị đó không được chép vào deliverable này. Cần loại secret khỏi Git, dùng secret manager/environment injection, và luân chuyển các secret đã từng được commit; xác thực lịch sử Git và quyền truy cập repository theo quy trình nội bộ.

## 5. Tích hợp

- **HRIS:** `HrService` đọc dữ liệu HRIS qua SQL Server provider `Microsoft.Data.SqlClient`; truy vấn dùng tham số hóa `@OrgCode`. Hangfire schedule đồng bộ nhân viên định kỳ; job xóa và nạp lại `EmployeeTemps`, đồng bộ `Departments` và upsert/đánh dấu nghỉ việc trong `Employees` trong một transaction. Unique key nghiệp vụ hiện có là `Departments.Code`, `Employees.EmployeeNo`, `EmployeeTemps(OrgCode, EmployeeNo)`. `EmployeeTemps` có các cột date dạng chuỗi và một cột trạng thái nghỉ việc có thể NULL trong migration cuối dù entity khai báo `[Required]`; cần đối chiếu DB production và tạo migration sửa nếu đó không phải chủ ý.
- **Agent:** agent gửi thông tin inventory qua API; backend cập nhật `Agents` và thay thế danh sách phần mềm hiện tại của agent trong `AgentSoftware`. Đây không phải lịch sử snapshot phần mềm.
- **ERP:** không tìm thấy kết nối hoặc schema ERP được mô tả rõ trong repository. Chưa có contract, external ID mapping, outbox, CDC hay message broker được nhận diện. Không nên coi HRIS là ERP tích hợp sẵn.
- **API–DB:** web client gọi API; comment và cấu hình DI chỉ ra API là tiến trình truy cập DB. Controllers/services gọi EF Core. Đồng bộ HRIS dùng adapter SQL trực tiếp riêng, còn Hangfire dùng database SQL Server độc lập.

## 6. Hạng mục cần xác nhận với DBA/hạ tầng

Không cần thông tin phần cứng để lập tài liệu schema. Để hoàn tất đánh giá production, cần bản export metadata đã loại thông tin nhạy cảm hoặc quyền truy vấn chỉ đọc để xác nhận phiên bản/edition SQL Server, compatibility level, collation, cấu hình HA, CPU/RAM/storage/IOPS, DB size/growth, recovery model, backup lịch gần nhất và lần restore test, DB principals/roles, Query Store/slow-query metrics, và object ngoài migrations. Không gửi dữ liệu thực tế hay secret.

## Deliverables

1. `EZITAM.SqlServer.Migrations.sql` — SQL Server DDL/migration script từ database trống đến migration mới nhất. **Chỉ chạy trên database trống hoặc môi trường thử nghiệm sau khi DBA review**; không phải script idempotent và không trích xuất schema trực tiếp từ production.
2. `ERD.md` — Mermaid ERD của bảng nghiệp vụ và Identity.
3. `EZITAM_Data_Dictionary.xlsx` — Data Dictionary, bảng/cột/index/quan hệ.
4. `Stored-Objects.md` — trạng thái đối chiếu view, procedure, function, trigger trong source/migrations.
5. `Optimization-Preflight.sql` — truy vấn chỉ đọc kiểm tra dữ liệu cũ trước khi áp dụng migration tối ưu.
