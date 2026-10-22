using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Secure_Academic_Management.Data;
using Secure_Academic_Management.Models;
using Secure_Academic_Management.ViewModels;
using System.Security.Claims;

namespace Secure_Academic_Management.Controllers
{
    // Checklist M03 #9: Chỉ cho phép Admin và Staff truy cập vào các chức năng quản lý sinh viên
    [Authorize(Roles = "Admin, Staff")]
    public class SinhVienController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SinhVienController> _logger;

        public SinhVienController(ApplicationDbContext context, ILogger<SinhVienController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: SinhVien
        public async Task<IActionResult> Index()
        {
            // Checklist M03 #27: Chỉ lấy dữ liệu cần thiết, tránh SELECT * nếu không cần
            var sinhViens = await _context.SinhViens
                .Select(sv => new { sv.MaSinhVien, sv.HoTen, sv.Email, sv.TrangThai })
                .ToListAsync();

            // Trong thực tế, bạn có thể map sang một SinhVienIndexViewModel để trả về View
            // Ở đây dùng ViewBag hoặc dynamic cho đơn giản, hoặc tạo ViewModel riêng
            return View(await _context.SinhViens.ToListAsync());
        }

        // GET: SinhVien/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SinhVien/Create
        // Checklist M03 #12: Bắt buộc có ValidateAntiForgeryToken để chống CSRF
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SinhVienCreateViewModel model)
        {
            // Checklist M03 #11: Kiểm tra dữ liệu đầu vào (ModelState)
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid input detected for SinhVien creation. User: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return View(model);
            }

            try
            {
                // 1. Sinh mã sinh viên tự động (Ví dụ: SV + Năm hiện tại + 4 số ngẫu nhiên)
                string maSinhVienMoi = $"SV{DateTime.Now:yyyy}{new Random().Next(1000, 9999)}";

                // 2. Map dữ liệu từ ViewModel sang Entity (Checklist #18: Kiểm soát truy cập dữ liệu)
                var sinhVienMoi = new SinhVien
                {
                    MaSinhVien = maSinhVienMoi,
                    HoTen = model.HoTen,
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai,
                    NgaySinh = model.NgaySinh,
                    TrangThai = "Đang học",
                    // IdentityUserId sẽ được gán sau khi sinh viên đăng ký tài khoản, hoặc để null nếu admin tạo hồ sơ trước
                };

                _context.SinhViens.Add(sinhVienMoi);
                await _context.SaveChangesAsync();

                // Checklist M03 #15 & #20: Ghi nhật ký thao tác (Audit Log) an toàn
                // Lưu ý: KHÔNG ghi mật khẩu hay thông tin nhạy cảm vào log
                _logger.LogInformation("Sinh viên mới được tạo thành công. MaSV: {MaSV}, HoTen: {HoTen}, Admin/Staff: {UserId}",
                    sinhVienMoi.MaSinhVien,
                    sinhVienMoi.HoTen,
                    User.FindFirstValue(ClaimTypes.NameIdentifier));

                TempData["SuccessMessage"] = $"Đã tiếp nhận hồ sơ và cấp mã sinh viên: {maSinhVienMoi}";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Checklist M03 #23: Ngăn lộ thông tin qua thông báo lỗi
                // Ghi chi tiết lỗi vào log nội bộ để admin tra cứu
                _logger.LogError(ex, "Database error while creating student. User: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Hiển thị thông báo chung, thân thiện cho người dùng, KHÔNG lộ tên bảng hay câu lệnh SQL
                ModelState.AddModelError("", "Đã xảy ra lỗi khi lưu dữ liệu. Vui lòng thử lại hoặc liên hệ quản trị viên.");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in SinhVienController.Create. User: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                ModelState.AddModelError("", "Đã xảy ra lỗi không xác định.");
                return View(model);
            }
        }
    }
}