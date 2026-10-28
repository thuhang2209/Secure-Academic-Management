using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Secure_Academic_Management.Data;
using Secure_Academic_Management.Models;
using Secure_Academic_Management.ViewModels;

namespace Secure_Academic_Management.Controllers;

[Authorize(Roles = "Admin,Staff")]
public sealed class M03Controller : Controller
{
    private static readonly string[] EnrollmentStatuses =
        ["Đang học", "Bảo lưu", "Tạm ngừng học", "Đã tốt nghiệp", "Đã thôi học"];

    private readonly ApplicationDbContext _context;

    public M03Controller(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        return View(new M03DashboardViewModel
        {
            ChuongTrinhs = await _context.ChuongTrinhDaoTaos.AsNoTracking()
                .OrderBy(item => item.MaChuongTrinh).ToListAsync(),
            LanTheoHocs = await _context.LanTheoHocs.AsNoTracking()
                .Include(item => item.ChuongTrinh)
                .Include(item => item.SinhVien)
                .OrderByDescending(item => item.NgayBatDau).Take(100).ToListAsync()
        });
    }

    [HttpGet]
    public IActionResult CreateProgram() => View(new ChuongTrinhViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProgram(ChuongTrinhViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        model.MaChuongTrinh = model.MaChuongTrinh.Trim().ToUpperInvariant();
        if (await _context.ChuongTrinhDaoTaos.AnyAsync(item => item.MaChuongTrinh == model.MaChuongTrinh))
        {
            ModelState.AddModelError(nameof(model.MaChuongTrinh), "Mã chương trình đã tồn tại.");
            return View(model);
        }

        _context.ChuongTrinhDaoTaos.Add(new ChuongTrinhDaoTao
        {
            MaChuongTrinh = model.MaChuongTrinh,
            TenChuongTrinh = model.TenChuongTrinh.Trim(),
            TrinhDo = model.TrinhDo.Trim(),
            DonViQuanLy = model.DonViQuanLy?.Trim(),
            DangTuyenSinh = model.DangTuyenSinh
        });
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã tạo chương trình đào tạo.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> CreateEnrollment()
    {
        await PopulateEnrollmentChoices();
        return View(new LanTheoHocViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEnrollment(LanTheoHocViewModel model)
    {
        if (!EnrollmentStatuses.Contains(model.TrangThai, StringComparer.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(model.TrangThai), "Trạng thái lần theo học không hợp lệ.");

        var student = await _context.SinhViens.FirstOrDefaultAsync(item => item.MaSinhVien == model.MaSinhVien);
        var program = await _context.ChuongTrinhDaoTaos.FirstOrDefaultAsync(item => item.MaChuongTrinh == model.MaChuongTrinh);
        if (student == null)
            ModelState.AddModelError(nameof(model.MaSinhVien), "Mã sinh viên không tồn tại.");
        if (program == null)
            ModelState.AddModelError(nameof(model.MaChuongTrinh), "Mã chương trình không tồn tại.");

        if (await _context.LanTheoHocs.AnyAsync(item =>
                item.MaSinhVien == model.MaSinhVien &&
                item.MaChuongTrinh == model.MaChuongTrinh &&
                item.NgayBatDau == model.NgayBatDau))
            ModelState.AddModelError(string.Empty, "Lần theo học này đã tồn tại.");

        if (!ModelState.IsValid)
        {
            await PopulateEnrollmentChoices();
            return View(model);
        }

        if (model.LaChuongTrinhChinh)
        {
            var currentPrimary = await _context.LanTheoHocs
                .Where(item => item.MaSinhVien == model.MaSinhVien && item.LaChuongTrinhChinh)
                .ToListAsync();
            currentPrimary.ForEach(item => item.LaChuongTrinhChinh = false);
        }

        _context.LanTheoHocs.Add(new LanTheoHoc
        {
            MaSinhVien = model.MaSinhVien.Trim(),
            MaChuongTrinh = model.MaChuongTrinh.Trim().ToUpperInvariant(),
            KhoaNhapHoc = model.KhoaNhapHoc,
            NgayBatDau = model.NgayBatDau.Date,
            TrangThai = EnrollmentStatuses.First(item =>
                item.Equals(model.TrangThai, StringComparison.OrdinalIgnoreCase)),
            LaChuongTrinhChinh = model.LaChuongTrinhChinh,
            GhiChu = model.GhiChu?.Trim()
        });
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã ghi nhận lần theo học.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> CreateReference()
    {
        await PopulateStudents();
        return View(new LienKetNghiepVuViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateReference(LienKetNghiepVuViewModel model)
    {
        var studentExists = await _context.SinhViens.AnyAsync(item => item.MaSinhVien == model.MaSinhVien);
        if (!studentExists)
            ModelState.AddModelError(nameof(model.MaSinhVien), "Mã sinh viên không tồn tại.");

        var types = new[] { "Tài chính", "Hỗ trợ", "Tốt nghiệp" };
        if (!types.Contains(model.LoaiLienKet, StringComparer.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(model.LoaiLienKet), "Loại liên kết không hợp lệ.");

        if (!ModelState.IsValid)
        {
            await PopulateStudents();
            return View(model);
        }

        var source = model.HeThongNguon.Trim();
        var reference = model.MaThamChieu.Trim();
        var description = model.MoTa?.Trim();
        switch (types.First(type => type.Equals(model.LoaiLienKet, StringComparison.OrdinalIgnoreCase)))
        {
            case "Tài chính":
                _context.LienKetTaiChinhs.Add(new LienKetTaiChinh
                {
                    MaSinhVien = model.MaSinhVien,
                    MaThamChieu = reference,
                    HeThongNguon = source,
                    LoaiNghiaVu = string.IsNullOrWhiteSpace(model.PhanLoai) ? "Chưa phân loại" : model.PhanLoai.Trim(),
                    MoTa = description
                });
                break;
            case "Hỗ trợ":
                _context.LienKetHoTros.Add(new LienKetHoTro
                {
                    MaSinhVien = model.MaSinhVien,
                    MaThamChieu = reference,
                    HeThongNguon = source,
                    LoaiHoTro = string.IsNullOrWhiteSpace(model.PhanLoai) ? "Chưa phân loại" : model.PhanLoai.Trim(),
                    MoTa = description
                });
                break;
            default:
                _context.HoSoTotNghieps.Add(new HoSoTotNghiep
                {
                    MaSinhVien = model.MaSinhVien,
                    MaThamChieu = reference,
                    HeThongNguon = source,
                    TrangThai = string.IsNullOrWhiteSpace(model.PhanLoai) ? "Đang xử lý" : model.PhanLoai.Trim(),
                    MoTa = description
                });
                break;
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã lưu liên kết nghiệp vụ.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateEnrollmentChoices()
    {
        await PopulateStudents();
        ViewBag.Programs = new SelectList(
            await _context.ChuongTrinhDaoTaos.AsNoTracking().OrderBy(item => item.MaChuongTrinh).ToListAsync(),
            nameof(ChuongTrinhDaoTao.MaChuongTrinh), nameof(ChuongTrinhDaoTao.TenChuongTrinh));
    }

    private async Task PopulateStudents()
    {
        ViewBag.Students = new SelectList(
            await _context.SinhViens.AsNoTracking().OrderBy(item => item.MaSinhVien).ToListAsync(),
            nameof(SinhVien.MaSinhVien), nameof(SinhVien.MaSinhVien));
    }
}
