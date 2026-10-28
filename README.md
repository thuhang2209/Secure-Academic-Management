# Secure Academic Management

Hệ thống web ASP.NET Core MVC quản lý hồ sơ sinh viên và quá trình theo học
(module M03). Ứng dụng dùng ASP.NET Core Identity cho đăng nhập/phân quyền,
Entity Framework Core cho truy cập SQL Server và Serilog cho logging.

> **Phạm vi đánh giá:** README này mô tả những gì đang có trong source code hiện
> tại, không khẳng định các script SQL đã được triển khai trên một SQL Server
> cụ thể. Những phần phụ thuộc vào DBA, chứng chỉ, backup hoặc cấu hình
> production cần được kiểm tra riêng.

## 1. Tóm tắt trạng thái requirement

### Đã hoàn thiện ở mức ứng dụng

| Requirement | Trạng thái | Cách đã thực hiện |
|---|---|---|
| Tiếp nhận hồ sơ và cấp mã sinh viên | **Đã có** | Admin/Staff tạo hồ sơ; hệ thống sinh mã duy nhất dạng `SV{năm}{số ngẫu nhiên}` và đặt trạng thái ban đầu là `Đang học`. |
| Quản lý hồ sơ sinh viên | **Đã có** | Xem danh sách, xem chi tiết, thêm, sửa thông tin cơ bản và xóa hồ sơ. |
| Tìm kiếm, lọc và phân trang | **Đã có** | Tìm theo mã/tên, lọc theo trạng thái, phân trang; kích thước trang được giới hạn trong controller. |
| Quản lý quá trình theo học | **Đã có một phần** | Có 5 trạng thái hợp lệ: `Đang học`, `Bảo lưu`, `Tạm ngừng học`, `Đã tốt nghiệp`, `Đã thôi học`. |
| Lịch sử biến động trạng thái | **Đã có** | Mỗi lần đổi trạng thái lưu trạng thái cũ/mới, lý do, thời điểm, người thực hiện và liên kết hồ sơ. Có transaction để cập nhật trạng thái và lịch sử cùng nhau. |
| Tài liệu của lần đổi trạng thái | **Đã có** | Nhận PDF/JPG/JPEG/PNG, tối đa 5 MB, đổi tên lưu trữ ngẫu nhiên trong `App_Data`, không phục vụ trực tiếp như static file; có endpoint tải file qua controller. |
| Đăng ký/đăng nhập/đăng xuất | **Đã có** | Đăng ký tạo tài khoản role `Student`; đăng nhập dùng Identity; đăng xuất là POST có anti-forgery token. |
| Phân quyền 3 role | **Đã có** | `Admin`, `Staff`, `Student` được kiểm tra ở server bằng `[Authorize(Roles = "...")]`, không chỉ ẩn menu. |
| Đổi mật khẩu và hồ sơ tài khoản | **Đã có** | Người dùng đã đăng nhập xem profile và đổi mật khẩu; mật khẩu do Identity hash, kiểm tra mật khẩu hiện tại và chính sách độ phức tạp. |
| Khóa tài khoản/chống brute-force | **Đã có** | Khóa sau 5 lần đăng nhập sai trong 15 phút; login có rate limit theo IP. |
| API sinh viên | **Đã có một phần** | `GET /api/student/{id}` yêu cầu đăng nhập; `POST` và `DELETE` chỉ cho `Admin,Staff`; dữ liệu đầu vào có validation và truy vấn dùng EF Core/LINQ. |
| Logging và xử lý lỗi | **Đã có một phần** | Serilog ghi console/file theo ngày; các thao tác chính ghi user/IP; production dùng trang lỗi chung, không hiển thị chi tiết kỹ thuật. |
| Một số lớp bảo vệ HTTP | **Đã có** | HTTPS redirection, HSTS ngoài Development, HttpOnly/Secure/SameSite cookie, anti-forgery, `nosniff`, `X-Frame-Options`, Referrer-Policy và Permissions-Policy. |
| Giảm tải ở mức ứng dụng | **Đã có một phần** | Có global rate limit và giới hạn riêng cho login; danh sách sinh viên có `AsNoTracking`, lọc và phân trang. |

### Có mã nguồn chuẩn bị nhưng chưa thể coi là đã hoàn thiện

Thư mục [`Database/Security`](./Database/Security) có 5 script/runbook cho:

- phân quyền SQL Server theo least privilege;
- Dynamic Data Masking và Row-Level Security;
- SQL Server Audit, TDE và backup;
- view/stored procedure giới hạn cột;
- các câu lệnh kiểm tra monitoring.

Đây là **tài liệu triển khai cho DBA**, còn placeholder như database/login/path/certificate
và chưa có bằng chứng trong repository rằng chúng đã được chạy, kiểm thử và tích hợp
với ứng dụng. Đặc biệt, policy RLS cần thiết kế cơ chế truyền `SESSION_CONTEXT` từ
ứng dụng; chỉ thêm file SQL chưa đủ để hoàn thành requirement.

## 2. Ba role và những gì mỗi role làm được

### Admin

- Đăng nhập, đăng xuất, xem/cập nhật profile và đổi mật khẩu.
- Truy cập khu vực quản trị nhân sự tại `/Admin`.
- Tạo tài khoản `Staff`, sửa họ tên/email, khóa/mở khóa và xóa tài khoản Staff.
- Thực hiện toàn bộ nghiệp vụ quản lý sinh viên như Staff: danh sách, tìm kiếm,
  lọc, phân trang, xem chi tiết, tạo, sửa, đổi trạng thái, xem lịch sử, tải
  tài liệu và xóa hồ sơ.
- Gọi các API sinh viên dành cho `Admin,Staff`, gồm xem, tạo và xóa.
- Không có màn hình quản trị role/permission tổng quát; việc quản lý tài khoản
  hiện tập trung vào Staff.

### Staff

- Đăng nhập, đăng xuất, xem profile và đổi mật khẩu.
- Thực hiện toàn bộ CRUD hồ sơ sinh viên.
- Tìm kiếm/lọc/phân trang danh sách.
- Xem lịch sử trạng thái; đổi trạng thái với lý do và tài liệu đính kèm.
- Tải tài liệu lịch sử và gọi API xem/tạo/xóa sinh viên.
- **Không** truy cập được `/Admin` và không được tạo, sửa, khóa hoặc xóa tài
  khoản Staff.

### Student

- Đăng ký tài khoản (role mặc định là `Student`) và đăng nhập.
- Xem profile tài khoản.
- Xem hồ sơ sinh viên của chính mình tại `/Student/Profile` nếu hồ sơ đã được
  liên kết bằng `IdentityUserId`.
- Gọi `GET /api/student/{id}`, nhưng controller giới hạn kết quả theo cả mã sinh
  viên và `IdentityUserId` của phiên đăng nhập.
- Đổi mật khẩu và đăng xuất.
- **Không** được truy cập danh sách quản trị, CRUD sinh viên, đổi trạng thái,
  xem lịch sử của người khác, tạo/xóa qua API hoặc quản lý Staff.

## 3. Những phần requirement còn thiếu hoặc mới làm một phần

### Đã bổ sung trong phiên bản này

- Đã có màn hình/controller quản lý danh mục **học phần** tại
  `/Academic/Courses`.
- Đã có luồng nhập, cập nhật, xóa và hiển thị **kết quả học tập/điểm** theo
  sinh viên tại `/Academic/Grades`.
- Đã có `AuditLogs` ghi nhận method, URL, user, IP, User-Agent, status và thời
  lượng request; các endpoint `/health` và `/metrics` phục vụ vận hành.
- Tài liệu trạng thái được kiểm tra thêm bằng chữ ký nội dung PDF/JPEG/PNG,
  không chỉ dựa vào phần mở rộng và Content-Type.
- Cấu hình mẫu trong `appsettings.json` không còn chứa connection string hoặc
  mật khẩu seed. Giá trị thật phải dùng User Secrets/biến môi trường.

### Thiếu chức năng nghiệp vụ M03

- Đã bổ sung màn hình `/M03`: quản lý chương trình đào tạo và ghi nhận nhiều
  lần theo học cho cùng một mã sinh viên. Chính sách hiện hành là một người
  học dùng một mã duy nhất; mỗi chương trình có một bản ghi `LanTheoHoc` riêng.
- Đã bổ sung các bảng liên kết tham chiếu `LienKetTaiChinhs`,
  `LienKetHoTros` và `HoSoTotNghieps`. Các bảng chỉ lưu mã tham chiếu, hệ
  thống nguồn và mô tả, không sao chép dữ liệu tài chính/hồ sơ tốt nghiệp.
- Chưa có nghiệp vụ tiếp nhận hồ sơ trúng tuyển theo quy trình nhiều bước; hiện
  mới là form tạo hồ sơ sinh viên.
- Đã có chính sách/mô hình cho một người học tham gia nhiều chương trình đào
  tạo; cờ `LaChuongTrinhChinh` xác định chương trình chính tại một thời điểm.
- Lịch sử trạng thái có `NgayHieuLuc`; trạng thái học tập được cập nhật bởi
  Admin/Staff kèm lý do và căn cứ.
- Module Đào tạo (`AcademicController`) là đầu mối duy nhất ghi điểm; M03 chỉ
  liên kết/hiển thị kết quả, không tạo bản sao điểm.
- Chưa có báo cáo, xuất dữ liệu hoặc dashboard nghiệp vụ.

### Thiếu hoặc chưa xác nhận ở lớp bảo mật/triển khai

- Least privilege, view/procedure, RLS, masking, SQL Server Audit, TDE và backup
  mới có script hướng dẫn; cần DBA chạy trên môi trường mục tiêu và kiểm thử
  bằng các tài khoản thật.
- Chưa có cơ chế backup/restore tự động, lịch SQL Agent, lưu trữ chứng chỉ và
  kiểm thử khôi phục.
- Audit log ứng dụng đã có IP/User-Agent/request metadata; SQL Server Audit vẫn
  cần DBA triển khai theo script và xác nhận trên môi trường mục tiêu. Audit
  request không ghi password, token hoặc request body.
- Đã có health check SQL Server (`GET /health`) và metrics request tối thiểu
  (`GET /metrics`). Alerting, dashboard và kiểm thử tải production vẫn cần
  tích hợp với hệ thống vận hành thực tế.
- Chưa có kiểm thử tự động/ma trận kiểm thử bảo mật trong repository cho 30 nhóm
  yêu cầu.
- Chưa có quy trình xác nhận email; cấu hình hiện cho phép đăng nhập mà không
  cần confirmed email.
- Chưa có phân tích MIME thực sự bằng nội dung file (hiện kiểm tra extension,
  Content-Type và dung lượng); nên bổ sung nếu mức an toàn upload production cao.
- Cần rà soát và loại bỏ secret/connection string có giá trị nhạy cảm khỏi
  `appsettings.json`; dùng User Secrets ở Development và biến môi trường/secret
  manager ở Production. Không commit các giá trị đó vào repository.
- Chưa có CI/CD, quét dependency bảo mật, cấu hình production đầy đủ hoặc
  tài liệu vận hành triển khai.

## 4. Kiến trúc và dữ liệu hiện có

- **Web:** ASP.NET Core MVC, target `.NET 10`.
- **Xác thực:** ASP.NET Core Identity + role-based authorization.
- **Cơ sở dữ liệu:** SQL Server qua Entity Framework Core.
- **Bảng nghiệp vụ chính:** `SinhViens`, `HocPhans`, `Diems`,
  `LichSuTrangThais`; ngoài ra có các bảng Identity.
- **Giao diện:** Razor Views, Bootstrap/Tailwind CDN và giao diện claymorphism.
- **Lưu file:** `App_Data/status-changes`; khóa Data Protection ở
  `App_Data/keys`.
- **Log:** `Logs/app-YYYYMMDD.log` và console.

## 5. Chạy ứng dụng ở môi trường phát triển

1. Cài .NET SDK phù hợp với target framework và SQL Server/SQL Express.
2. Cấu hình `ConnectionStrings:DefaultConnection` bằng User Secrets hoặc biến
   môi trường; không đưa password vào source.
3. Cấu hình các secret seed user theo các khóa `SeedUsers:AdminPassword`,
   `SeedUsers:StaffPassword` và `SeedUsers:StudentPassword`.
4. Chạy migration/database initializer theo quy trình của môi trường.
5. Chạy:

   ```powershell
   dotnet restore
   dotnet run
   ```

6. Truy cập URL HTTPS được in ra bởi `dotnet run`, đăng nhập bằng tài khoản đã
   cấu hình secret. Không nên dùng lại mật khẩu mẫu trong môi trường thật.

### Kiểm tra vận hành

- `GET /health` trả `Healthy` khi ứng dụng kết nối được SQL Server; dùng endpoint
  này cho load balancer/readiness probe.
- `GET /metrics` trả bộ đếm request theo HTTP status để tích hợp collector đơn
  giản. Không coi endpoint này là hệ thống metrics production đầy đủ.
- Chạy `dotnet build` trước khi tạo migration. Migration audit hiện tại là
  `20261028031241_AddAuditLogs`.
- Backup/restore, SQL Agent, TDE certificate và SQL Server Audit phải chạy từ
  runbook trong [`Database/Operations`](./Database/Operations) bởi DBA có
  quyền; web process không được tự backup database.

## 6. Kết luận ngắn

Phiên bản hiện tại đã hoàn thiện tốt **phần nền tảng M03 ở cấp hồ sơ sinh viên**:
đăng nhập/phân quyền, CRUD, mã sinh viên, tìm kiếm/lọc, cập nhật trạng thái,
lịch sử và upload tài liệu. Phần còn thiếu lớn nhất là các module liên quan đến
điểm/chương trình đào tạo/tài chính/tốt nghiệp và việc triển khai, kiểm chứng các
biện pháp bảo mật SQL Server nâng cao. Vì vậy hệ thống nên được xem là **MVP của
module quản lý hồ sơ và quá trình theo học**, chưa phải hệ thống quản lý đào tạo
đầy đủ hoặc bản production đã chứng nhận bảo mật.