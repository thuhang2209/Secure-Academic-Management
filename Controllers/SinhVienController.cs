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

        // ==========================================
        // 1. INDEX: Danh sách sinh viên
        // ==========================================
        public async Task<IActionResult> Index(string searchString)
        {
            // Checklist M03 #27: Chỉ lấy dữ liệu cần thiết, tránh SELECT *
            var query = _context.SinhViens.AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.HoTen.Contains(searchString) || s.MaSinhVien.Contains(searchString));
            }

            var sinhViens = await query
                .Select(sv => new SinhVien
                {
                    MaSinhVien = sv.MaSinhVien,
                    HoTen = sv.HoTen,
                    Email = sv.Email,
                    NgaySinh = sv.NgaySinh,
                    TrangThai = sv.TrangThai
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(sinhViens);
        }

        // ==========================================
        // 2. DETAILS: Xem chi tiết
        // ==========================================
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var sinhVien = await _context.SinhViens
                .Include(s => s.LichSuTrangThais.OrderByDescending(l => l.NgayThayDoi))
                .FirstOrDefaultAsync(m => m.MaSinhVien == id);

            if (sinhVien == null) return NotFound();

            return View(sinhVien);
        }

        // ==========================================
        // 3. CREATE: Tiếp nhận hồ sơ mới
        // ==========================================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Checklist M03 #12: Chống CSRF
        public async Task<IActionResult> Create(SinhVienCreateViewModel model)
        {
            if (!ModelState.IsValid) // Checklist M03 #11: Validate đầu vào
            {
                _logger.LogWarning("Invalid input detected for SinhVien creation. User: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return View(model);
            }

            try
            {
                // 1. Sinh mã sinh viên tự động
                string maSinhVienMoi = $"SV{DateTime.Now:yyyy}{new Random().Next(1000, 9999)}";

                // 👉 QUAN TRỌNG: Vòng lặp kiểm tra trùng lặp (Giải quyết nỗi lo của bạn)
                while (await _context.SinhViens.AnyAsync(s => s.MaSinhVien == maSinhVienMoi))
                {
                    maSinhVienMoi = $"SV{DateTime.Now:yyyy}{new Random().Next(1000, 9999)}";
                }

                // 2. Map dữ liệu từ ViewModel sang Entity (Checklist #18: Chống Over-posting)
                var sinhVienMoi = new SinhVien
                {
                    MaSinhVien = maSinhVienMoi,
                    HoTen = model.HoTen,
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai,
                    NgaySinh = model.NgaySinh,
                    TrangThai = "Đang học" // Hardcode trạng thái mặc định an toàn
                };

                _context.SinhViens.Add(sinhVienMoi);
                await _context.SaveChangesAsync();

                // Checklist M03 #15 & #20: Ghi nhật ký thao tác (Audit Log)
                _logger.LogInformation("Sinh viên mới được tạo thành công. MaSV: {MaSV}, HoTen: {HoTen}, Admin/Staff: {UserId}",
                    sinhVienMoi.MaSinhVien,
                    sinhVienMoi.HoTen,
                    User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown");

                TempData["SuccessMessage"] = $"Đã tiếp nhận hồ sơ và cấp mã sinh viên: {maSinhVienMoi}";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Checklist M03 #23: Ngăn lộ thông tin nhạy cảm qua thông báo lỗi
                _logger.LogError(ex, "Database error while creating student. User: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
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

        // ==========================================
        // 4. EDIT: Chỉnh sửa thông tin cơ bản
        // ==========================================
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var sinhVien = await _context.SinhViens.FindAsync(id);
            if (sinhVien == null) return NotFound();

            var model = new SinhVienEditViewModel
            {
                MaSinhVien = sinhVien.MaSinhVien,
                HoTen = sinhVien.HoTen,
                Email = sinhVien.Email,
                SoDienThoai = sinhVien.SoDienThoai,
                NgaySinh = sinhVien.NgaySinh,
                TrangThai = sinhVien.TrangThai
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, SinhVienEditViewModel model)
        {
            // Checklist M03 #18: Chống IDOR - Đảm bảo ID trên URL khớp với ID trong form
            if (id != model.MaSinhVien) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var sinhVien = await _context.SinhViens.FindAsync(id);
                    if (sinhVien == null) return NotFound();

                    // Chỉ cập nhật những trường được phép
                    sinhVien.HoTen = model.HoTen;
                    sinhVien.Email = model.Email;
                    sinhVien.SoDienThoai = model.SoDienThoai;
                    sinhVien.NgaySinh = model.NgaySinh;

                    sinhVien.TrangThai = model.TrangThai;

                    _context.Update(sinhVien);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Cập nhật thông tin sinh viên {MaSV} thành công bởi {UserId}",
                        id, User.FindFirstValue(ClaimTypes.NameIdentifier));

                    TempData["SuccessMessage"] = "Cập nhật thông tin sinh viên thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SinhVienExists(model.MaSinhVien)) return NotFound();
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật sinh viên {MaSV}", model.MaSinhVien);
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật dữ liệu.");
                }
            }
            return View(model);
        }

        // ==========================================
        // 5. UPDATE STATUS: Module 2 - Cập nhật trạng thái có Audit Trail
        // ==========================================
        // GET: SinhVien/UpdateStatus
        public async Task<IActionResult> UpdateStatus(string id)
        {
            if (id == null) return NotFound();

            var sinhVien = await _context.SinhViens.FindAsync(id);
            if (sinhVien == null) return NotFound();

            var model = new CapNhatTrangThaiViewModel
            {
                MaSinhVien = id,
                TrangThaiMoi = sinhVien.TrangThai // Set giá trị hiện tại
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(CapNhatTrangThaiViewModel model)
        {
            // 1. Kiểm tra validation từ ViewModel (Lý do, Mã SV...)
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = $"Dữ liệu không hợp lệ: {errors}";
                return RedirectToAction(nameof(Details), new { id = model.MaSinhVien });
            }

            try
            {
                var sinhVien = await _context.SinhViens.FindAsync(model.MaSinhVien);
                if (sinhVien == null)
                {
                    return NotFound();
                }

                string trangThaiCu = sinhVien.TrangThai ?? string.Empty;
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";

                // Tránh lỗi so sánh do khoảng trắng hoặc viết hoa/thường
                string trangThaiMoiChuan = model.TrangThaiMoi?.Trim() ?? string.Empty;
                trangThaiCu = trangThaiCu.Trim();

                // 2. Kiểm tra nếu trạng thái KHÔNG đổi
                if (trangThaiCu.Equals(trangThaiMoiChuan, StringComparison.OrdinalIgnoreCase))
                {
                    TempData["InfoMessage"] = $"Sinh viên hiện đã ở trạng thái '{trangThaiCu}'. Vui lòng chọn trạng thái khác để cập nhật.";
                    return RedirectToAction(nameof(Details), new { id = model.MaSinhVien });
                }

                // 3. Xử lý file upload nếu có
                string? duongDanFile = null;
                string? tenFileGoc = null;

                if (model.TaiLieuDinhKem != null && model.TaiLieuDinhKem.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "status-changes");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = $"{model.MaSinhVien}_{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetFileName(model.TaiLieuDinhKem.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.TaiLieuDinhKem.CopyToAsync(stream);
                    }

                    duongDanFile = $"/uploads/status-changes/{fileName}";
                    tenFileGoc = model.TaiLieuDinhKem.FileName;
                }

                // 4. Cập nhật Model SinhVien & Thêm Log Lịch sử
                sinhVien.TrangThai = trangThaiMoiChuan;
                _context.Update(sinhVien); // Đảm bảo EF Core đánh dấu Modified

                var lichSu = new LichSuTrangThai
                {
                    MaSinhVien = model.MaSinhVien,
                    TrangThaiCu = trangThaiCu,
                    TrangThaiMoi = trangThaiMoiChuan,
                    LyDo = model.LyDo,
                    DuongDanFile = duongDanFile,
                    TenFileGoc = tenFileGoc,
                    NgayThayDoi = DateTime.UtcNow,
                    NguoiThucHienId = userId
                };

                _context.LichSuTrangThais.Add(lichSu);

                // 5. Lưu xuống Database
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "TRẠNG THÁI SINH VIÊN THAY ĐỔI | MaSV: {MaSV} | Từ: '{Cu}' Sang: '{Moi}' | Lý do: {LyDo} | File: {File} | Nguồn: {UserId}",
                    model.MaSinhVien, trangThaiCu, trangThaiMoiChuan, model.LyDo, tenFileGoc ?? "Không có", userId);

                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái sinh viên {model.MaSinhVien} từ '{trangThaiCu}' sang '{trangThaiMoiChuan}'.";
                return RedirectToAction(nameof(Details), new { id = model.MaSinhVien });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái sinh viên {MaSV}", model.MaSinhVien);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi hệ thống khi cập nhật trạng thái: " + ex.Message;
                return RedirectToAction(nameof(Details), new { id = model.MaSinhVien });
            }
        }

        // ==========================================
        // 6. DELETE: Xóa hồ sơ
        // ==========================================
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var sinhVien = await _context.SinhViens.FirstOrDefaultAsync(m => m.MaSinhVien == id);
            if (sinhVien == null) return NotFound();

            return View(sinhVien);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var sinhVien = await _context.SinhViens.FindAsync(id);
            if (sinhVien != null)
            {
                _context.SinhViens.Remove(sinhVien);
                await _context.SaveChangesAsync();

                _logger.LogWarning("Sinh viên {MaSV} đã bị XÓA vĩnh viễn bởi {UserId}",
                    id, User.FindFirstValue(ClaimTypes.NameIdentifier));

                TempData["SuccessMessage"] = "Đã xóa hồ sơ sinh viên.";
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper method
        private bool SinhVienExists(string id)
        {
            return _context.SinhViens.Any(e => e.MaSinhVien == id);
        }
    }
}