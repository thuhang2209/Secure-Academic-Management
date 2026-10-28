# Tài Liệu Yêu Cầu Hệ Thống & Ma Trận Kiểm Thử Bảo Mật

---

## 1. Module M03: Sinh Viên Và Quá Trình Theo Học

### 1.1. Mục Đích

Quản lý hồ sơ người học và lịch sử theo học bằng mã chuẩn.

### 1.2. Chức Năng

* **Tiếp nhận hồ sơ trúng tuyển:** Ghi nhận và xử lý hồ sơ trúng tuyển của người học.
* **Cấp mã sinh viên:** Khởi tạo và cấp định danh (mã sinh viên) duy nhất theo quy chuẩn.
* **Quản lý hồ sơ:** Quản lý thông tin cá nhân, chương trình đào tạo, khóa học và trạng thái học tập.
* **Quản lý kết quả học tập:** Quản lý hoặc hiển thị kết quả học tập theo phân công liên kết với module đào tạo.
* **Liên kết nghiệp vụ:** Liên kết thông tin với nghĩa vụ tài chính, các chương trình hỗ trợ và hồ sơ tốt nghiệp.

### 1.3. Luồng Hoạt Động Dự Kiến

```
Nhận bàn giao ➔ Kiểm tra hồ sơ ➔ Cấp mã ➔ Ghi nhận quá trình theo học ➔ Cập nhật biến động ➔ Hoàn tất hoặc kết thúc quá trình học

```

### 1.4. Dữ Liệu Quản Lý

* Hồ sơ người học và mã sinh viên.
* Chương trình đào tạo, khóa học, các lần theo học.
* Trạng thái học tập, thời gian hiệu lực và căn cứ thay đổi trạng thái.
* Dữ liệu liên kết kết quả học tập và nghĩa vụ tài chính.

### 1.5. Quy Tắc Nghiệp Vụ

* Cần quyết định chính sách mã đối với người học tham gia nhiều chương trình đào tạo.
* Không sao chép toàn bộ dữ liệu tài chính hoặc luận văn vào hồ sơ sinh viên (chỉ lưu liên kết/mã tham chiếu).
* Xác định rõ hệ thống đầu mối quản lý điểm chính thức để tránh tình trạng dữ liệu bị chỉnh sửa ở hai nơi khác nhau.

---

## 2. Danh Mục Yêu Cầu & Kịch Bản Kiểm Thử Bảo Mật

### A. Bảo Mật Cơ Sở Dữ Liệu SQL Server

#### 1. Phân Quyền Theo Đặc Quyền Tối Thiểu

* **Nhóm:** A. Bảo mật cơ sở dữ liệu SQL Server
* **Danh Sách Ví Dụ Mẫu:**
* Tài khoản quản trị cơ sở dữ liệu, nhân viên nhập liệu và người chỉ xem báo cáo.
* Login `store_reader`, `store_sales`, `store_auditor` gắn với role `Store_ReadOnly`, `Store_Sales`, `Store_Auditor`.


* **Gợi Ý Cách Thực Hiện:**
* Tạo login, user và role theo nhóm nghiệp vụ; gán user vào role thay vì cấp quyền trực tiếp.
* `GRANT` chỉ quyền cần dùng; `DENY` các thao tác vượt quyền như xóa danh mục hoặc sửa bảng nhạy cảm.
* Không dùng tài khoản `sa` cho ứng dụng và cho tài khoản thực hành thường.


* **Gợi Ý Cách Kiểm Tra:**
* Đăng nhập từng tài khoản rồi thử thực hiện các câu lệnh `SELECT`, `INSERT`, `UPDATE`, `DELETE`.
* Thao tác vượt quyền phải bị hệ thống từ chối.
* Đối chiếu quyền bằng `sys.database_permissions` và `sys.database_role_members`.



#### 2. Quyền Trên Bảng, View Và Stored Procedure

* **Nhóm:** A. Bảo mật cơ sở dữ liệu SQL Server
* **Danh Sách Ví Dụ Mẫu:**
* Bảng khách hàng chứa dữ liệu nhạy cảm.
* View chỉ hiện các cột được phép xem.
* Stored procedure cập nhật dữ liệu có kiểm soát.


* **Gợi Ý Cách Thực Hiện:**
* Thu hồi quyền truy cập trực tiếp trên bảng gốc đối với người dùng nghiệp vụ.
* `GRANT SELECT` trên view và `GRANT EXECUTE` trên procedure.
* Đặt logic kiểm tra trong procedure trước khi ghi dữ liệu.


* **Gợi Ý Cách Kiểm Tra:**
* So sánh truy cập thẳng vào bảng với truy cập qua view hoặc procedure.
* Tài khoản chỉ có quyền `EXECUTE` không đọc được bảng gốc bằng lệnh `SELECT`.
* Ghi nhận cách làm nào giảm được bề mặt lộ dữ liệu.



#### 3. Kiểm Soát Truy Cập Theo Dòng (Row-Level Security)

* **Nhóm:** A. Bảo mật cơ sở dữ liệu SQL Server
* **Danh Sách Ví Dụ Mẫu:**
* Bảng nhân sự hoặc bảng lương; mỗi người chỉ thấy bản ghi của phòng mình.
* Tài khoản thuộc các phòng ban khác nhau cùng truy vấn một bảng.


* **Gợi Ý Cách Thực Hiện:**
* Tạo hàm predicate lọc theo người dùng hoặc mã phòng ban.
* Gắn `FILTER PREDICATE` bằng `CREATE SECURITY POLICY`.
* Không dựa vào điều kiện lọc chỉ viết ở ứng dụng.


* **Gợi Ý Cách Kiểm Tra:**
* Đăng nhập lần lượt các tài khoản phòng ban.
* Mỗi tài khoản chỉ nhận đúng các dòng thuộc phạm vi của mình.
* Thử bỏ điều kiện `WHERE` ở câu `SELECT`; policy vẫn phải lọc dòng.



#### 4. Che Giấu Dữ Liệu Nhạy Cảm (Dynamic Data Masking)

* **Nhóm:** A. Bảo mật cơ sở dữ liệu SQL Server
* **Danh Sách Ví Dụ Mẫu:**
* Che số điện thoại, email và số định danh trên bảng khách hàng hoặc nhân viên.
* Một vai trò được xem đầy đủ, một vai trò chỉ xem dữ liệu đã che.


* **Gợi Ý Cách Thực Hiện:**
* sử dụng `ADD MASKED WITH` cho từng cột (email, partial hoặc default).
* Chỉ `GRANT UNMASK` cho vai trò được xem bản rõ.
* Giải thích masking là che khi hiển thị, không thay cho mã hóa hay phân quyền.


* **Gợi Ý Cách Kiểm Tra:**
* So sánh kết quả `SELECT` giữa người dùng thường và người có quyền `UNMASK`.
* Người không có quyền `UNMASK` không thấy dữ liệu gốc.
* Nêu giới hạn: người có quyền truy vấn đủ vẫn có thể suy luận hoặc xuất dữ liệu nếu được cấp quyền quá rộng.



#### 5. Kiểm Toán SQL Server Audit

* **Nhóm:** A. Bảo mật cơ sở dữ liệu SQL Server
* **Danh Sách Ví Dụ Mẫu:**
* Ghi nhận đăng nhập thất bại.
* Ghi nhận đọc bảng nhạy cảm.
* Ghi nhận xóa dữ liệu hoặc đổi cấu trúc bảng.


* **Gợi Ý Cách Thực Hiện:**
* Tạo Server Audit ghi ra file, bật audit.
* Tạo Database Audit Specification cho các hành động cần theo dõi.
* Phân quyền thư mục chứa file audit, tránh để người dùng thường xóa log.


* **Gợi Ý Cách Kiểm Tra:**
* Mô phỏng truy cập trái phép và một lệnh DDL.
* Đọc log bằng `sys.fn_get_audit_file`.
* Log phải có người thực hiện, thời điểm và đối tượng bị tác động.



#### 6. Mã Hóa Cơ Sở Dữ Liệu (Transparent Data Encryption)

* **Nhóm:** A. Bảo mật cơ sở dữ liệu SQL Server
* **Danh Sách Ví Dụ Mẫu:**
* Mã hóa database lưu trên đĩa, ví dụ database `store`.
* Bản sao lưu đi kèm yêu cầu chứng chỉ khi khôi phục.


* **Gợi Ý Cách Thực Hiện:**
* Tạo master key và certificate trong `master`; sao lưu certificate cùng khóa riêng.
* Tạo database encryption key và bật `ENCRYPTION`.
* Lưu chứng chỉ ở nơi tách khỏi file dữ liệu và file backup.


* **Gợi Ý Cách Kiểm Tra:**
* Xem `sys.dm_database_encryption_keys` trước và sau khi bật.
* Khôi phục sang instance khác khi thiếu certificate phải thất bại.
* Khôi phục khi có đủ certificate phải thành công.



#### 7. Bảo Mật Sao Lưu Và Khôi Phục

* **Nhóm:** A. Bảo mật cơ sở dữ liệu SQL Server
* **Danh Sách Ví Dụ Mẫu:**
* Backup full, differential và transaction log của database nghiệp vụ.
* File backup được mã hóa; thư mục backup không để tài khoản ứng dụng đọc.


* **Gợi Ý Cách Thực Hiện:**
* `BACKUP WITH ENCRYPTION` hoặc bảo vệ bằng certificate.
* Phân quyền NTFS cho thư mục backup; hạn chế tài khoản được restore.
* Lưu bản sao chứng chỉ độc lập với file `.bak`.


* **Gợi Ý Cách Kiểm Tra:**
* Tài khoản không được cấp quyền không đọc hoặc copy được file backup.
* Restore sai mật khẩu hoặc thiếu certificate bị từ chối.
* Restore hợp lệ trên môi trường thử nghiệm cho dữ liệu dùng lại được.



#### 8. Giám Sát Tài Khoản Người Dùng Và Tài Khoản Hệ Thống

* **Nhóm:** A. Bảo mật cơ sở dữ liệu SQL Server
* **Danh Sách Ví Dụ Mẫu:**
* Đăng nhập sai nhiều lần, đăng nhập ngoài giờ, đăng nhập từ máy lạ.
* Rà tài khoản `sa`, tài khoản dịch vụ và login đang bị tắt.


* **Gợi Ý Cách Thực Hiện:**
* Kiểm tra `sys.sql_logins`: chính sách mật khẩu, hạn dùng, trạng thái disable.
* Bật audit nhóm `FAILED_LOGIN_GROUP` và theo dõi SQL Server error log.
* Quy ước cảnh báo cho đăng nhập bất thường.


* **Gợi Ý Cách Kiểm Tra:**
* Cố tình đăng nhập sai và đăng nhập ngoài khung giờ cho phép.
* Nhật ký hoặc audit phải ghi được sự kiện.
* Tài khoản không còn dùng thì disable, không xóa vội nếu còn cần truy vết.



---

### B. Bảo Mật Lập Trình Phần Mềm ASP.NET

#### 9. Xác Thực Và Phân Quyền Với ASP.NET Identity

* **Nhóm:** B. Bảo mật lập trình phần mềm ASP.NET
* **Danh Sách Ví Dụ Mẫu:**
* Đăng ký, đăng nhập; các vai trò Administrator, Staff và User.
* Khu vực quản trị chỉ dành cho Administrator.


* **Gợi Ý Cách Thực Hiện:**
* Dùng ASP.NET Core Identity; gắn `[Authorize(Roles = "...")]` cho từng nhóm chức năng.
* Bật khóa tài khoản sau nhiều lần đăng nhập sai.
* Không chỉ ẩn menu; phải chặn ở action và ở server.


* **Gợi Ý Cách Kiểm Tra:**
* Tài khoản User mở trực tiếp URL quản trị và bị từ chối.
* Đăng nhập sai liên tiếp thì tài khoản bị khóa.
* Staff không thực hiện được chức năng chỉ dành cho Administrator.



#### 10. Phòng Chống SQL Injection

* **Nhóm:** B. Bảo mật lập trình phần mềm ASP.NET
* **Danh Sách Ví Dụ Mẫu:**
* Ô tìm kiếm viết bằng nối chuỗi SQL.
* Cùng chức năng viết lại bằng tham số hoặc Entity Framework Core.


* **Gợi Ý Cách Thực Hiện:**
* Không ghép dữ liệu người dùng vào câu lệnh.
* Dùng `SqlParameter`, truy vấn tham số hoặc LINQ.
* Quyền tài khoản ứng dụng ở SQL Server cũng theo đặc quyền tối thiểu.


* **Gợi Ý Cách Kiểm Tra:**
* Trong môi trường lab, nhập chuỗi phá truy vấn vào bản nối chuỗi và bản tham số.
* Bản nối chuỗi đổi kết quả hoặc báo lỗi SQL; bản tham số vẫn an toàn.
* Không thử trên hệ thống thật hoặc dữ liệu thật.



#### 11. Phòng Chống Cross-Site Scripting (XSS)

* **Nhóm:** B. Bảo mật lập trình phần mềm ASP.NET
* **Danh Sách Ví Dụ Mẫu:**
* Form bình luận, phản hồi hoặc hồ sơ cá nhân, sau đó hiển thị lại trên trang.
* Chuỗi có thẻ script nhập trong môi trường kiểm thử.


* **Gợi Ý Cách Thực Hiện:**
* Kiểm tra dữ liệu đầu vào theo kiểu và độ dài.
* Mã hóa đầu ra; với Razor không dùng `Html.Raw` cho dữ liệu người dùng.
* Làm sạch nội dung nếu nghiệp vụ bắt buộc cho phép một phần HTML.


* **Gợi Ý Cách Kiểm Tra:**
* Nhập script mẫu trước và sau khi mã hóa đầu ra.
* Trước khi xử lý, script chạy trên trình duyệt; sau khi xử lý, chuỗi hiện dạng văn bản.
* Xem mã nguồn trang để chắc dữ liệu đã được escape.



#### 12. Phòng Chống CSRF Bằng Anti-Forgery Token

* **Nhóm:** B. Bảo mật lập trình phần mềm ASP.NET
* **Danh Sách Ví Dụ Mẫu:**
* Form cập nhật hồ sơ hoặc đổi mật khẩu.
* Yêu cầu POST gửi từ trang khác, không kèm token của phiên hiện tại.


* **Gợi Ý Cách Thực Hiện:**
* Bật `ValidateAntiForgeryToken` cho các action thay đổi dữ liệu.
* Form dùng tag helper để sinh token.
* Cookie phiên đặt HttpOnly, Secure và SameSite phù hợp.


* **Gợi Ý Cách Kiểm Tra:**
* Gửi POST không có token hoặc token sai; hệ thống trả lỗi và không ghi dữ liệu.
* Form hợp lệ trên chính ứng dụng vẫn thực hiện được.
* Đối chiếu request hợp lệ và request bị từ chối trong log.



#### 13. Bảo Mật Connection String, Mật Khẩu Và Secret

* **Nhóm:** B. Bảo mật lập trình phần mềm ASP.NET
* **Danh Sách Ví Dụ Mẫu:**
* Ứng dụng kết nối SQL Server.
* So sánh cách ghi mật khẩu ngay trong mã với User Secrets, biến môi trường và appsettings theo môi trường.


* **Gợi Ý Cách Thực Hiện:**
* Tách cấu hình Development và Production.
* Mật khẩu để trong User Secrets hoặc biến môi trường, không commit vào mã nguồn.
* Tài khoản SQL của ứng dụng không phải `sa`.


* **Gợi Ý Cách Kiểm Tra:**
* Tìm trong mã và trong appsettings xem còn mật khẩu dạng rõ hay không.
* Ứng dụng vẫn kết nối được khi secret nằm ngoài mã nguồn.
* Đưa nhầm file secret vào gói triển khai thì coi là chưa đạt.



#### 14. Bảo Mật Chức Năng Tải Tệp Lên

* **Nhóm:** B. Bảo mật lập trình phần mềm ASP.NET
* **Danh Sách Ví Dụ Mẫu:**
* Người dùng tải tài liệu hoặc ảnh đính kèm.
* Tệp sai phần mở rộng, quá dung lượng hoặc mang tên bất thường.


* **Gợi Ý Cách Thực Hiện:**
* Chỉ cho phép danh sách phần mở rộng; kiểm tra kích thước và tên tệp.
* Đổi tên ngẫu nhiên trước khi lưu; để file ngoài thư mục chạy được của web.
* Không tin tên file gốc do trình duyệt gửi lên.


* **Gợi Ý Cách Kiểm Tra:**
* Tải tệp không hợp lệ thì bị từ chối và có dòng log.
* Tệp hợp lệ lưu được, không ghi đè tệp đã có.
* Không mở trực tiếp được tệp thực thi từ URL ứng dụng.



#### 15. Ghi Log, Xử Lý Lỗi Và Giám Sát Ứng Dụng

* **Nhóm:** B. Bảo mật lập trình phần mềm ASP.NET
* **Danh Sách Ví Dụ Mẫu:**
* Ghi đăng nhập thành công hoặc thất bại, truy cập trái phép và lỗi truy xuất dữ liệu.
* Trang lỗi thân thiện khi có exception.


* **Gợi Ý Cách Thực Hiện:**
* Dùng `ILogger`; không ghi mật khẩu, token hay connection string vào log.
* Bật trang xử lý lỗi chung; tắt chi tiết lỗi kỹ thuật ở môi trường triển khai.
* Log đủ người dùng, thời điểm, đường dẫn và mã lỗi để điều tra.


* **Gợi Ý Cách Kiểm Tra:**
* Gây lỗi đăng nhập, lỗi nhập liệu và lỗi truy cập.
* Log có sự kiện tương ứng.
* Màn hình người dùng không hiện stack trace, tên server hay câu lệnh SQL.



---

### C. Thực Hành Nâng Cao

#### 16. Đăng Nhập Nhiều Lớp, Kiểm Tra Quyền Từ Cơ Sở Dữ Liệu

* **Nhóm:** C. Thực hành nâng cao
* **Danh Sách Ví Dụ Mẫu:**
* Bảng người dùng, vai trò và quyền trong SQL Server.
* Sau đăng nhập, menu và chức năng đổi theo quyền.
* Người dùng gọi thẳng URL chức năng quản trị.


* **Gợi Ý Cách Thực Hiện:**
* Xác thực ở ASP.NET, rồi đọc quyền từ database bằng procedure hoặc truy vấn tham số.
* Kiểm tra quyền tại action, không chỉ ẩn nút trên giao diện.
* Phiên đăng nhập hết hạn thì phải đăng nhập lại.


* **Gợi Ý Cách Kiểm Tra:**
* User thường mở chức năng quản trị thì bị từ chối.
* Gọi trực tiếp URL nhạy cảm khi không đủ quyền cũng bị từ chối.
* Đổi quyền trong database thì lần đăng nhập sau phản ánh đúng quyền mới.



#### 17. Đổi Mật Khẩu Và Lưu Mật Khẩu An Toàn

* **Nhóm:** C. Thực hành nâng cao
* **Danh Sách Ví Dụ Mẫu:**
* Chức năng đổi mật khẩu; từ chối mật khẩu yếu.
* Lịch sử đổi mật khẩu; so sánh lưu plain text với lưu hash.


* **Gợi Ý Cách Thực Hiện:**
* Lưu mật khẩu bằng `PasswordHasher` hoặc thuật toán băm thích hợp kèm salt.
* Kiểm tra độ dài và độ phức tạp; không ghi mật khẩu cũ hay mới vào log.
* Buộc nhập mật khẩu hiện tại trước khi đổi.


* **Gợi Ý Cách Kiểm Tra:**
* Mật khẩu yếu bị từ chối.
* `SELECT` bảng người dùng không thấy mật khẩu dạng rõ.
* Log và thông báo lỗi không chứa mật khẩu.



#### 18. Kiểm Soát Truy Cập Dữ Liệu Cá Nhân

* **Nhóm:** C. Thực hành nâng cao
* **Danh Sách Ví Dụ Mẫu:**
* Cổng hồ sơ nhân viên hoặc sinh viên.
* Người dùng A sửa mã hồ sơ trên URL để xem hồ sơ của B.


* **Gợi Ý Cách Thực Hiện:**
* Câu truy vấn hoặc stored procedure luôn gắn với danh tính đang đăng nhập.
* Không lấy bản ghi chỉ vì client gửi một id.
* Có thể kết hợp view hoặc Row-Level Security ở SQL Server.


* **Gợi Ý Cách Kiểm Tra:**
* Đổi id trên URL không trả về hồ sơ người khác.
* Quản lý đúng thẩm quyền vẫn xem được hồ sơ trong phạm vi được giao.
* Thử gọi API với id của người khác và xác nhận bị từ chối.



#### 19. Tìm Kiếm Và Lọc Nhiều Điều Kiện An Toàn

* **Nhóm:** C. Thực hành nâng cao
* **Danh Sách Ví Dụ Mẫu:**
* Tìm theo tên, ngày, trạng thái và phòng ban.
* Một bản nối chuỗi và một bản truy vấn tham số hoặc EF Core.


* **Gợi Ý Cách Thực Hiện:**
* Mọi giá trị lọc đưa vào tham số.
* Nếu cho phép sắp xếp theo cột, chỉ nhận tên cột nằm trong danh sách cho phép.
* Phân trang, không trả toàn bộ bảng khi điều kiện quá rộng.


* **Gợi Ý Cách Kiểm Tra:**
* Nhập chuỗi bất thường để phá logic truy vấn.
* Bản tham số không phát sinh SQL Injection.
* Kết quả vẫn đúng điều kiện lọc hợp lệ.



#### 20. Nhật Ký Thao Tác Từ Ứng Dụng Xuống SQL Server

* **Nhóm:** C. Thực hành nâng cao
* **Danh Sách Ví Dụ Mẫu:**
* Ghi thêm, sửa, xóa và đăng nhập.
* Mỗi dòng log có người thực hiện, thời gian, IP hoặc thiết bị và dữ liệu bị tác động.


* **Gợi Ý Cách Thực Hiện:**
* Ghi log ở ASP.NET khi thao tác thành công.
* Đồng thời ghi audit ở SQL Server bằng Audit hoặc bảng nhật ký.
* Không đưa mật khẩu và dữ liệu nhạy cảm không cần thiết vào log.


* **Gợi Ý Cách Kiểm Tra:**
* Sửa và xóa bằng nhiều tài khoản.
* Đối chiếu log ứng dụng với log cơ sở dữ liệu: cùng người, cùng thời điểm, cùng bản ghi.
* Thiếu một trong hai tầng log thì chưa đủ truy vết.



#### 21. Bảo Vệ Dữ Liệu Nhạy Cảm Trên Báo Cáo

* **Nhóm:** C. Thực hành nâng cao
* **Danh Sách Ví Dụ Mẫu:**
* Báo cáo có số điện thoại, email, lương hoặc điểm.
* Admin xem bản đủ; user thường xem bản đã che hoặc bị ẩn cột.


* **Gợi Ý Cách Thực Hiện:**
* Kết hợp Dynamic Data Masking hoặc view giới hạn cột với kiểm tra vai trò.
* Giao diện chỉ bind các trường được phép.
* API và JSON không trả trường nhạy cảm cho vai trò thấp.


* **Gợi Ý Cách Kiểm Tra:**
* So sánh cùng một báo cáo khi đăng nhập Admin và User.
* Xem mã trang, response JSON và file xuất để tìm dữ liệu gốc bị lộ.
* User thường không thấy được bản rõ dù mở công cụ trình duyệt.



#### 22. Bảo Mật API Truy Cập Cơ Sở Dữ Liệu

* **Nhóm:** C. Thực hành nâng cao
* **Danh Sách Ví Dụ Mẫu:**
* API thêm, sửa, xóa và xem dữ liệu.
* Gọi API khi chưa đăng nhập, token sai, hoặc role thấp gọi API xóa.


* **Gợi Ý Cách Thực Hiện:**
* Bắt xác thực bằng Identity hoặc token trên toàn bộ API dữ liệu.
* Gắn policy theo vai trò cho từng endpoint, nhất là endpoint ghi và xóa.
* Validate dữ liệu đầu vào; truy vấn xuống SQL vẫn dùng tham số.


* **Gợi Ý Cách Kiểm Tra:**
* Không có token thì 401; token sai thì bị từ chối.
* Role thấp gọi API xóa thì 403 và dữ liệu không đổi.
* Role đúng thực hiện được đúng thao tác được giao.



#### 23. Ngăn Lộ Thông Tin Qua Thông Báo Lỗi

* **Nhóm:** C. Thực hành nâng cao
* **Danh Sách Ví Dụ Mẫu:**
* Sai connection string, lỗi câu truy vấn và lỗi vi phạm ràng buộc.
* So sánh trang lỗi chi tiết với trang lỗi thân thiện.


* **Gợi Ý Cách Thực Hiện:**
* Bắt exception ở ứng dụng; ghi chi tiết vào log nội bộ.
* Người dùng chỉ thấy thông báo chung và mã sự cố.
* Môi trường triển khai tắt Developer Exception Page.


* **Gợi Ý Cách Kiểm Tra:**
* Quan sát phản hồi trước và sau khi cấu hình.
* Phản hồi không còn tên bảng, tên server, câu lệnh SQL hoặc stack trace.
* Log nội bộ vẫn đủ để người quản trị tìm nguyên nhân.



#### 24. Sao Lưu, Khôi Phục Và Tính Sẵn Sàng Của Ứng Dụng

* **Nhóm:** C. Thực hành nâng cao
* **Danh Sách Ví Dụ Mẫu:**
* Ứng dụng đang chạy thì database được backup.
* Mô phỏng xóa nhầm dữ liệu, sau đó restore.
* Trong lúc restore, ứng dụng mất kết nối.


* **Gợi Ý Cách Thực Hiện:**
* Có lịch backup full và log phù hợp mức độ nghiệp vụ.
* Ứng dụng báo lỗi kết nối gọn, không lộ chuỗi kết nối khi database chưa sẵn sàng.


* **Gợi Ý Cách Kiểm Tra:**
* Sau restore, kiểm tra vài nghiệp vụ chính và số bản ghi then chốt.
* Phục hồi được dữ liệu đã xóa nhầm trong bài lab.
* Ứng dụng hoạt động lại sau restore.
* Dữ liệu sau khôi phục khớp bản backup đã chọn, không lệch ràng buộc.



#### 25. Rà Soát Mã Nguồn Và Cấu Hình Triển Khai

* **Nhóm:** C. Thực hành nâng cao
* **Danh Sách Ví Dụ Mẫu:**
* Mật khẩu gắn cứng, connection string lộ trong mã, câu SQL nối chuỗi.
* Thiếu kiểm tra phân quyền; bật debug; secret nằm trong source.


* **Gợi Ý Cách Thực Hiện:**
* Lập danh mục file cần rà: mã nguồn, appsettings, biến môi trường, gói triển khai.
* Chuyển secret ra khỏi repo; tắt trang lỗi chi tiết; siết quyền tài khoản SQL.
* Viết báo cáo ngắn: lỗi tìm thấy, mức rủi ro và cách đã sửa.


* **Gợi Ý Cách Kiểm Tra:**
* So sánh hành vi trước và sau khi sửa.
* Chạy ở môi trường giả lập production: không còn secret trong gói phát hành.
* Các luồng đăng nhập, phân quyền và truy vấn chính vẫn đúng.



---

### D. Thực Hành Chống Tấn Công

#### 26. Giảm Thiểu Từ Chối Dịch Vụ Ở Mức Ứng Dụng

* **Nhóm:** D. Thực hành chống tấn công
* **Danh Sách Ví Dụ Mẫu:**
* Trang truy vấn nhiều bản ghi bị gọi dồn từ môi trường kiểm thử.
* Theo dõi CPU, RAM, số kết nối SQL và thời gian phản hồi.


* **Gợi Ý Cách Thực Hiện:**
* Giới hạn tần suất request, dùng cache cho dữ liệu ít đổi, phân trang và timeout.
* Ghi log khi lưu lượng tăng bất thường.
* Chỉ mô phỏng trong lab, với lưu lượng vừa đủ để quan sát.


* **Gợi Ý Cách Kiểm Tra:**
* Đo trước và sau khi bật rate limiting cùng cache.
* Ứng dụng không treo; SQL Server không cạn kết nối.
* Request vượt ngưỡng bị từ chối có kiểm soát, request hợp lệ vẫn được phục vụ.



#### 27. Phòng Chống Truy Vấn Làm Nghẽn Cơ Sở Dữ Liệu

* **Nhóm:** D. Thực hành chống tấn công
* **Danh Sách Ví Dụ Mẫu:**
* Báo cáo quét cả bảng lớn, không chỉ mục, trả về quá nhiều dòng.
* Cùng báo cáo sau khi thêm chỉ mục, phân trang và giới hạn số dòng.


* **Gợi Ý Cách Thực Hiện:**
* Tìm cột lọc thường dùng để tạo index.
* Bắt buộc phân trang hoặc `TOP`; đưa truy vấn nặng vào stored procedure.
* Theo dõi bằng công cụ hiệu năng của SQL Server.


* **Gợi Ý Cách Kiểm Tra:**
* So sánh thời gian phản hồi của bản chưa tối ưu và bản đã tối ưu.
* Chạy truy vấn nặng liên tục và xem phiên làm việc khác có bị chậm hay không.
* Giải thích vì sao chức năng báo cáo bị lạm dụng có thể làm dịch vụ chậm toàn hệ thống.



#### 28. Kiểm Tra Thư Viện Và Phụ Thuộc Không An Toàn

* **Nhóm:** D. Thực hành chống tấn công
* **Danh Sách Ví Dụ Mẫu:**
* Dự án ASP.NET dùng các gói NuGet cho xác thực, tải tệp hoặc kết nối SQL.
* Gói lỗi thời, gói có cảnh báo bảo mật, gói không còn được duy trì.


* **Gợi Ý Cách Thực Hiện:**
* Liệt kê package và phiên bản đang dùng.
* Quét phụ thuộc bằng công cụ của .NET hoặc NuGet.
* Nâng phiên bản rồi chạy lại các chức năng bị ảnh hưởng.


* **Gợi Ý Cách Kiểm Tra:**
* So sánh báo cáo quét trước và sau khi cập nhật.
* Chỉ ra gói nào đụng xác thực, upload hoặc kết nối cơ sở dữ liệu.
* Ứng dụng vẫn đăng nhập, tải tệp và truy vấn được sau khi nâng phiên bản.



#### 29. Cấu Hình An Toàn Khi Tích Hợp Thư Viện

* **Nhóm:** D. Thực hành chống tấn công
* **Danh Sách Ví Dụ Mẫu:**
* Thư viện upload, xác thực, logging hoặc mã hóa để cấu hình mặc định.
* Bỏ qua kiểm tra kiểu tệp, log lộ dữ liệu, xác thực lỏng.


* **Gợi Ý Cách Thực Hiện:**
* Đọc tài liệu thư viện trước khi dùng; không giữ cấu hình mẫu của nhà cung cấp nếu cấu hình đó mở quyền rộng.
* Tắt ghi log dữ liệu nhạy cảm; bật các kiểm tra mà thư viện yêu cầu khai báo thêm.
* Cấu hình lại rồi mới đưa vào bài lab chính thức.


* **Gợi Ý Cách Kiểm Tra:**
* So sánh hành vi lúc cấu hình mặc định và lúc cấu hình đã siết.
* Log và phản hồi không còn dữ liệu nhạy cảm.
* Ghi lại mục cấu hình nào bắt buộc phải đổi so với mặc định.



#### 30. Kiểm Thử Bảo Mật Chức Năng Đẩy Tệp

* **Nhóm:** D. Thực hành chống tấn công
* **Danh Sách Ví Dụ Mẫu:**
* Tải hồ sơ, ảnh hoặc tài liệu; metadata lưu trong SQL Server.
* Tệp quá dung lượng, sai định dạng, tên lạ hoặc đổi đuôi để ngụy trang.


* **Gợi Ý Cách Thực Hiện:**
* Kiểm tra phần mở rộng, MIME và kích thước; đổi tên ngẫu nhiên.
* Không lưu tệp thực thi trong thư mục public; phân quyền người được tải lên và tải xuống.
* Metadata hiển thị trên trang phải được mã hóa đầu ra.


* **Gợi Ý Cách Kiểm Tra:**
* Các tệp không hợp lệ đều bị chặn và được ghi log.
* Không ghi đè tệp cũ; không truy cập được tệp bằng đường dẫn đoán được.
* Tên tệp và mô tả lưu trong database không thực thi được như script trên trang.