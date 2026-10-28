using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Secure_Academic_Management.Data;
using Secure_Academic_Management.Models;
using Secure_Academic_Management.ViewModels;
using System.Security.Claims;

namespace Secure_Academic_Management.Controllers
{
    // Chỉ Admin và Staff được quản lý sinh viên.
    [Authorize(Roles = "Admin, Staff")]
    public class SinhVienController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SinhVienController> _logger;
        private readonly IWebHostEnvironment _environment;

        private static readonly HashSet<string> TrangThaiHopLe =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Đang học",
                "Bảo lưu",
                "Tạm ngừng học",
                "Đã tốt nghiệp",
                "Đã thôi học"
            };

        private static readonly HashSet<string> PhanMoRongHopLe =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".pdf",
                ".jpg",
                ".jpeg",
                ".png"
            };

        private static readonly HashSet<string> LoaiNoiDungHopLe =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "application/pdf",
                "image/jpeg",
                "image/png"
            };

        private const long DungLuongFileToiDa = 5 * 1024 * 1024;

        public SinhVienController(
            ApplicationDbContext context,
            ILogger<SinhVienController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        // ==========================================
        // 1. INDEX: Danh sách sinh viên
        // ==========================================
        public async Task<IActionResult> Index(
            string? searchString,
            string? trangThai,
            int page = 1,
            int pageSize = 20)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 10, 100);
            var query = _context.SinhViens.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                query = query.Where(s =>
                    s.HoTen.Contains(searchString) ||
                    s.MaSinhVien.Contains(searchString));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                trangThai = trangThai.Trim();
                query = query.Where(s => s.TrangThai == trangThai);
            }

            var totalItems = await query.CountAsync();
            var sinhViens = await query
                .OrderBy(s => s.MaSinhVien)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SinhVien
                {
                    MaSinhVien = s.MaSinhVien,
                    HoTen = s.HoTen,
                    Email = s.Email,
                    NgaySinh = s.NgaySinh,
                    TrangThai = s.TrangThai
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = trangThai;
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalItems"] = totalItems;

            return View(sinhViens);
        }
                // ==========================================
        // 2. DETAILS: Chi tiết sinh viên và lịch sử
        // ==========================================
        public async Task<IActionResult> Details(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var sinhVien = await _context.SinhViens
                .AsNoTracking()
                .Include(s => s.LichSuTrangThais)
                .Include(s => s.LanTheoHocs)
                    .ThenInclude(enrollment => enrollment.ChuongTrinh)
                .Include(s => s.LienKetTaiChinhs)
                .Include(s => s.LienKetHoTros)
                .Include(s => s.HoSoTotNghieps)
                .FirstOrDefaultAsync(s => s.MaSinhVien == id);

            if (sinhVien == null)
                return NotFound();

            // Sắp xếp lịch sử mới nhất lên đầu.
            sinhVien.LichSuTrangThais = sinhVien.LichSuTrangThais
                .OrderByDescending(l => l.NgayThayDoi)
                .ToList();

            return View(sinhVien);
        }

        // ==========================================
        // 3. CREATE: Hiển thị form tiếp nhận sinh viên
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // ==========================================
        // 4. CREATE: Lưu sinh viên mới
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SinhVienCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                string maSinhVienMoi;

                do
                {
                    maSinhVienMoi =
                        $"SV{DateTime.Now:yyyy}{Random.Shared.Next(1000, 10000)}";
                }
                while (await _context.SinhViens
                    .AnyAsync(s => s.MaSinhVien == maSinhVienMoi));

                var sinhVienMoi = new SinhVien
                {
                    MaSinhVien = maSinhVienMoi,
                    HoTen = model.HoTen,
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai,
                    NgaySinh = model.NgaySinh,
                    TrangThai = "Đang học",
                    TrangThaiHoSo = "Đã cấp mã",
                    NgayTiepNhanUtc = DateTime.UtcNow,
                    NgayKiemTraHoSoUtc = DateTime.UtcNow,
                    NguoiTiepNhanId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };

                _context.SinhViens.Add(sinhVienMoi);

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Tạo sinh viên {MaSV} bởi {UserId}; IP: {IpAddress}",
                    maSinhVienMoi,
                    User.FindFirstValue(ClaimTypes.NameIdentifier),
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["SuccessMessage"] =
                    $"Đã tiếp nhận hồ sơ sinh viên: {maSinhVienMoi}";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex,
                    "Lỗi database khi tạo sinh viên.");

                ModelState.AddModelError(
                    "",
                    "Không thể lưu hồ sơ sinh viên. Vui lòng kiểm tra dữ liệu và thử lại.");

                return View(model);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Lỗi không xác định khi tạo sinh viên.");

                ModelState.AddModelError(
                    "",
                    "Đã xảy ra lỗi hệ thống khi tạo hồ sơ.");

                return View(model);
            }
        }

        // ==========================================
        // 5. EDIT: Hiển thị form chỉnh sửa
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var sinhVien = await _context.SinhViens
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MaSinhVien == id);

            if (sinhVien == null)
                return NotFound();

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

        // ==========================================
        // 6. EDIT: Lưu thông tin cơ bản
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            SinhVienEditViewModel model)
        {
            // Kiểm tra ID trên URL và ID trong form.
            if (id != model.MaSinhVien)
                return NotFound();

            // Trạng thái không được chỉnh sửa qua form Edit.
            ModelState.Remove(nameof(SinhVienEditViewModel.TrangThai));

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var sinhVien = await _context.SinhViens
                    .FirstOrDefaultAsync(s => s.MaSinhVien == id);

                if (sinhVien == null)
                    return NotFound();

                // Chỉ cập nhật các trường được phép.
                sinhVien.HoTen = model.HoTen;
                sinhVien.Email = model.Email;
                sinhVien.SoDienThoai = model.SoDienThoai;
                sinhVien.NgaySinh = model.NgaySinh;

                // Không cập nhật TrangThai tại đây.

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Cập nhật thông tin sinh viên {MaSV} bởi {UserId}",
                    id,
                    User.FindFirstValue(ClaimTypes.NameIdentifier));

                TempData["SuccessMessage"] =
                    "Cập nhật thông tin sinh viên thành công.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex,
                    "Lỗi đồng thời khi cập nhật sinh viên {MaSV}", id);

                if (!await _context.SinhViens.AnyAsync(s => s.MaSinhVien == id))
                    return NotFound();

                ModelState.AddModelError(
                    "",
                    "Dữ liệu đã thay đổi. Vui lòng tải lại trang và thử lại.");

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Lỗi khi cập nhật sinh viên {MaSV}", id);

                ModelState.AddModelError(
                    "",
                    "Đã xảy ra lỗi khi cập nhật dữ liệu.");

                return View(model);
            }
        }

        // ==========================================
        // 7. UPDATE STATUS: Hiển thị form đổi trạng thái
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> UpdateStatus(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var sinhVien = await _context.SinhViens
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MaSinhVien == id);

            if (sinhVien == null)
                return NotFound();

            var model = new CapNhatTrangThaiViewModel
            {
                MaSinhVien = sinhVien.MaSinhVien,

                // Không tự chọn trạng thái mới.
                TrangThaiMoi = string.Empty
            };

            ViewBag.TrangThaiHienTai = sinhVien.TrangThai;

            return View(model);
        }

        // ==========================================
        // 8. UPDATE STATUS: Đổi trạng thái và ghi lịch sử
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            CapNhatTrangThaiViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(
                    " | ",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                TempData["ErrorMessage"] =
                    $"Dữ liệu không hợp lệ: {errors}";

                return RedirectToAction(
                    nameof(UpdateStatus),
                    new { id = model.MaSinhVien });
            }

            // Chỉ chấp nhận những trạng thái đã định nghĩa.
            string trangThaiMoi = model.TrangThaiMoi.Trim();

            var trangThaiChuan = TrangThaiHopLe.FirstOrDefault(
                s => string.Equals(
                    s,
                    trangThaiMoi,
                    StringComparison.OrdinalIgnoreCase));

            if (trangThaiChuan == null)
            {
                TempData["ErrorMessage"] = "Trạng thái được chọn không hợp lệ.";

                return RedirectToAction(
                    nameof(UpdateStatus),
                    new { id = model.MaSinhVien });
            }

            if (model.TaiLieuDinhKem != null)
            {
                if (model.TaiLieuDinhKem.Length > DungLuongFileToiDa)
                {
                    TempData["ErrorMessage"] =
                        "Tài liệu đính kèm không được vượt quá 5 MB.";

                    return RedirectToAction(
                        nameof(UpdateStatus),
                        new { id = model.MaSinhVien });
                }

                string extension = Path.GetExtension(
                    model.TaiLieuDinhKem.FileName).ToLowerInvariant();

                if (!PhanMoRongHopLe.Contains(extension))
                {
                    TempData["ErrorMessage"] =
                        "Chỉ chấp nhận tài liệu PDF, JPG, JPEG hoặc PNG.";

                    return RedirectToAction(
                        nameof(UpdateStatus),
                        new { id = model.MaSinhVien });
                }

                if (!LoaiNoiDungHopLe.Contains(model.TaiLieuDinhKem.ContentType))
                {
                    TempData["ErrorMessage"] =
                        "Định dạng nội dung của tài liệu không được hỗ trợ.";

                    return RedirectToAction(
                        nameof(UpdateStatus),
                        new { id = model.MaSinhVien });
                }

                if (!await HasExpectedFileSignatureAsync(
                    model.TaiLieuDinhKem,
                    extension))
                {
                    TempData["ErrorMessage"] =
                        "Nội dung thực tế của tài liệu không khớp với định dạng đã khai báo.";

                    return RedirectToAction(
                        nameof(UpdateStatus),
                        new { id = model.MaSinhVien });
                }
            }

            string? duongDanFile = null;
            string? tenFileGoc = null;
            string? filePathDaTao = null;

            try
            {
                var sinhVien = await _context.SinhViens
                    .FirstOrDefaultAsync(
                        s => s.MaSinhVien == model.MaSinhVien);

                if (sinhVien == null)
                    return NotFound();

                string trangThaiCu = sinhVien.TrangThai?.Trim() ?? "";

                if (string.Equals(
                    trangThaiCu,
                    trangThaiChuan,
                    StringComparison.OrdinalIgnoreCase))
                {
                    TempData["InfoMessage"] =
                        "Trạng thái mới trùng với trạng thái hiện tại.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id = model.MaSinhVien });
                }

                string userId =
                    User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? "Unknown";

                // Upload file nếu người dùng đính kèm tài liệu.
                if (model.TaiLieuDinhKem is { Length: > 0 })
                {
                    var uploadsFolder = Path.Combine(
                        _environment.ContentRootPath,
                        "App_Data",
                        "status-changes");

                    Directory.CreateDirectory(uploadsFolder);

                    string extension = Path.GetExtension(
                        model.TaiLieuDinhKem.FileName).ToLowerInvariant();

                    string fileName =
                        $"{Guid.NewGuid():N}{extension}";

                    filePathDaTao = Path.Combine(
                        uploadsFolder,
                        fileName);

                    await using (var stream = new FileStream(
                        filePathDaTao,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None))
                    {
                        await model.TaiLieuDinhKem.CopyToAsync(stream);
                    }

                    // Chỉ lưu tên file do hệ thống tạo, không lưu đường dẫn vật lý.
                    duongDanFile = fileName;
                    tenFileGoc = Path.GetFileName(
                        model.TaiLieuDinhKem.FileName);
                }

                // Cập nhật trạng thái sinh viên.
                sinhVien.TrangThai = trangThaiChuan;

                // Ghi lịch sử để có thể truy xuất từng lần thay đổi.
                var lichSu = new LichSuTrangThai
                {
                    MaSinhVien = sinhVien.MaSinhVien,
                    TrangThaiCu = trangThaiCu,
                    TrangThaiMoi = trangThaiChuan,
                    LyDo = model.LyDo.Trim(),
                    DuongDanFile = duongDanFile,
                    TenFileGoc = tenFileGoc,
                    NgayThayDoi = DateTime.UtcNow,
                    NgayHieuLuc = model.NgayHieuLuc.Date,
                    NguoiThucHienId = userId
                };

                _context.LichSuTrangThais.Add(lichSu);

                // SaveChangesAsync wraps these changes in one database transaction.
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Thay đổi trạng thái sinh viên {MaSV}: {Cu} -> {Moi}. UserId: {UserId}; IP: {IpAddress}",
                    model.MaSinhVien,
                    trangThaiCu,
                    trangThaiChuan,
                    userId,
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["SuccessMessage"] =
                    $"Đã cập nhật trạng thái sinh viên {model.MaSinhVien} " +
                    $"từ '{trangThaiCu}' sang '{trangThaiChuan}'.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.MaSinhVien });
            }
            catch (Exception ex)
            {
                // Xóa file nếu upload thành công nhưng thao tác DB thất bại.
                if (!string.IsNullOrWhiteSpace(filePathDaTao) &&
                    System.IO.File.Exists(filePathDaTao))
                {
                    try
                    {
                        System.IO.File.Delete(filePathDaTao);
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogError(deleteEx,
                            "Không thể xóa file tạm {FilePath}",
                            filePathDaTao);
                    }
                }

                _logger.LogError(ex,
                    "Lỗi khi cập nhật trạng thái sinh viên {MaSV}",
                    model.MaSinhVien);

                TempData["ErrorMessage"] =
                    "Đã xảy ra lỗi hệ thống khi cập nhật trạng thái. " +
                    "Vui lòng thử lại.";

                return RedirectToAction(
                    nameof(UpdateStatus),
                    new { id = model.MaSinhVien });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            var history = await _context.LichSuTrangThais
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            if (history == null || string.IsNullOrWhiteSpace(history.DuongDanFile))
                return NotFound();

            var fileName = Path.GetFileName(history.DuongDanFile);
            var filePath = Path.Combine(
                _environment.ContentRootPath,
                "App_Data",
                "status-changes",
                fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            _logger.LogInformation(
                "Downloaded status attachment {HistoryId} for {MaSV} by {UserId}",
                id,
                history.MaSinhVien,
                User.FindFirstValue(ClaimTypes.NameIdentifier));

            return PhysicalFile(filePath, contentType, history.TenFileGoc ?? fileName);
        }

        // ==========================================
        // 9. DELETE: Xác nhận xóa sinh viên
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Delete(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var sinhVien = await _context.SinhViens
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MaSinhVien == id);

            if (sinhVien == null)
                return NotFound();

            return View(sinhVien);
        }

        // ==========================================
        // 10. DELETE: Thực hiện xóa
        // ==========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var sinhVien = await _context.SinhViens
                .FirstOrDefaultAsync(s => s.MaSinhVien == id);

            if (sinhVien == null)
                return NotFound();

            try
            {
                _context.SinhViens.Remove(sinhVien);

                await _context.SaveChangesAsync();

                _logger.LogWarning(
                    "Sinh viên {MaSV} bị xóa bởi {UserId}",
                    id,
                    User.FindFirstValue(ClaimTypes.NameIdentifier));

                TempData["SuccessMessage"] =
                    "Đã xóa hồ sơ sinh viên.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Lỗi khi xóa sinh viên {MaSV}", id);

                TempData["ErrorMessage"] =
                    "Không thể xóa sinh viên. Có thể hồ sơ đang được dữ liệu khác tham chiếu.";

                return RedirectToAction(nameof(Details), new { id });
            }
        }

        private static async Task<bool> HasExpectedFileSignatureAsync(
            IFormFile file,
            string extension)
        {
            var requiredLength = extension == ".pdf" ? 4 : 8;
            var header = new byte[requiredLength];
            await using var stream = file.OpenReadStream();
            var read = 0;
            while (read < header.Length)
            {
                var bytesRead = await stream.ReadAsync(header.AsMemory(read));
                if (bytesRead == 0)
                    break;
                read += bytesRead;
            }

            return extension switch
            {
                ".pdf" => read >= 4
                    && header[0] == 0x25 && header[1] == 0x50
                    && header[2] == 0x44 && header[3] == 0x46,
                ".jpg" or ".jpeg" => read >= 3
                    && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
                ".png" => read >= 8
                    && header[0] == 0x89 && header[1] == 0x50
                    && header[2] == 0x4E && header[3] == 0x47
                    && header[4] == 0x0D && header[5] == 0x0A
                    && header[6] == 0x1A && header[7] == 0x0A,
                _ => false
            };
        }
    }
}
