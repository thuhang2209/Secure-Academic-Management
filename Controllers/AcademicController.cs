using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Secure_Academic_Management.Data;
using Secure_Academic_Management.Models;
using Secure_Academic_Management.ViewModels;

namespace Secure_Academic_Management.Controllers;

[Authorize(Roles = "Admin,Staff")]
public sealed class AcademicController : Controller
{
    private readonly ApplicationDbContext _context;

    public AcademicController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Courses(string? search)
    {
        var query = _context.HocPhans.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(course =>
                course.MaHocPhan.Contains(search) || course.TenHocPhan.Contains(search));
        }

        return View(new CourseListViewModel
        {
            Courses = await query.OrderBy(course => course.MaHocPhan).ToListAsync(),
            Search = search
        });
    }

    [HttpGet]
    public IActionResult CreateCourse() => View(new CourseViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(CourseViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var code = model.MaHocPhan.Trim().ToUpperInvariant();
        if (await _context.HocPhans.AnyAsync(course => course.MaHocPhan == code))
        {
            ModelState.AddModelError(nameof(model.MaHocPhan), "Mã học phần đã tồn tại.");
            return View(model);
        }

        _context.HocPhans.Add(new HocPhan
        {
            MaHocPhan = code,
            TenHocPhan = model.TenHocPhan.Trim(),
            SoTinChi = model.SoTinChi
        });
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã tạo học phần.";
        return RedirectToAction(nameof(Courses));
    }

    public async Task<IActionResult> Grades(string? studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return BadRequest("Thiếu mã sinh viên.");

        var student = await _context.SinhViens.AsNoTracking()
            .FirstOrDefaultAsync(item => item.MaSinhVien == studentId);
        if (student == null)
            return NotFound();

        var grades = await _context.Diems.AsNoTracking()
            .Include(grade => grade.HocPhan)
            .Where(grade => grade.MaSinhVien == studentId)
            .OrderBy(grade => grade.HocPhan.TenHocPhan)
            .ToListAsync();

        return View(new GradeListViewModel { Student = student, Grades = grades });
    }

    [HttpGet]
    public async Task<IActionResult> AddGrade(string studentId)
    {
        if (!await _context.SinhViens.AnyAsync(item => item.MaSinhVien == studentId))
            return NotFound();

        await PopulateCourses();
        return View(new GradeViewModel { MaSinhVien = studentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddGrade(GradeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCourses();
            return View(model);
        }

        if (!await _context.SinhViens.AnyAsync(item => item.MaSinhVien == model.MaSinhVien)
            || !await _context.HocPhans.AnyAsync(item => item.MaHocPhan == model.MaHocPhan))
        {
            ModelState.AddModelError(string.Empty, "Sinh viên hoặc học phần không tồn tại.");
            await PopulateCourses();
            return View(model);
        }

        var grade = await _context.Diems.FirstOrDefaultAsync(item =>
            item.MaSinhVien == model.MaSinhVien && item.MaHocPhan == model.MaHocPhan);
        if (grade == null)
        {
            grade = new Diem
            {
                MaSinhVien = model.MaSinhVien,
                MaHocPhan = model.MaHocPhan,
                NguoiCapNhatId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };
            _context.Diems.Add(grade);
        }

        grade.DiemSo = model.DiemSo;
        grade.NgayCapNhat = DateTime.UtcNow;
        grade.NguoiCapNhatId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Đã lưu kết quả học tập.";
        return RedirectToAction(nameof(Grades), new { studentId = model.MaSinhVien });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGrade(int id, string studentId)
    {
        var grade = await _context.Diems.FirstOrDefaultAsync(item =>
            item.Id == id && item.MaSinhVien == studentId);
        if (grade == null)
            return NotFound();

        _context.Diems.Remove(grade);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã xóa kết quả học tập.";
        return RedirectToAction(nameof(Grades), new { studentId });
    }

    private async Task PopulateCourses()
    {
        ViewBag.Courses = new SelectList(
            await _context.HocPhans.AsNoTracking().OrderBy(item => item.MaHocPhan).ToListAsync(),
            nameof(HocPhan.MaHocPhan),
            nameof(HocPhan.TenHocPhan));
    }
}
