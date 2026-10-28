using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Secure_Academic_Management.Data;
using Secure_Academic_Management.Models;

namespace Secure_Academic_Management.Controllers;

[ApiController]
[Route("api/academic")]
[Authorize(Roles = "Admin,Staff,Student")]
public sealed class AcademicApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AcademicApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("students/{studentId}/grades")]
    public async Task<IActionResult> GetGrades(string studentId)
    {
        var studentQuery = _context.SinhViens.AsNoTracking()
            .Where(item => item.MaSinhVien == studentId);
        if (User.IsInRole("Student"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            studentQuery = studentQuery.Where(item => item.IdentityUserId == userId);
        }

        if (!await studentQuery.AnyAsync())
            return NotFound();

        var grades = await _context.Diems.AsNoTracking()
            .Where(item => item.MaSinhVien == studentId)
            .OrderBy(item => item.MaHocPhan)
            .Select(item => new
            {
                item.MaHocPhan,
                TenHocPhan = item.HocPhan.TenHocPhan,
                item.DiemSo,
                item.NgayCapNhat
            })
            .ToListAsync();

        return Ok(grades);
    }

    [HttpPost("students/{studentId}/grades")]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveGrade(string studentId, SaveGradeRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!await _context.SinhViens.AnyAsync(item => item.MaSinhVien == studentId)
            || !await _context.HocPhans.AnyAsync(item => item.MaHocPhan == request.MaHocPhan))
            return NotFound(new { message = "Sinh viên hoặc học phần không tồn tại." });

        var grade = await _context.Diems.FirstOrDefaultAsync(item =>
            item.MaSinhVien == studentId && item.MaHocPhan == request.MaHocPhan);
        if (grade == null)
        {
            grade = new Diem
            {
                MaSinhVien = studentId,
                MaHocPhan = request.MaHocPhan
            };
            _context.Diems.Add(grade);
        }

        grade.DiemSo = request.DiemSo;
        grade.NgayCapNhat = DateTime.UtcNow;
        grade.NguoiCapNhatId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _context.SaveChangesAsync();

        return Ok(new { grade.MaSinhVien, grade.MaHocPhan, grade.DiemSo, grade.NgayCapNhat });
    }
}

public sealed class SaveGradeRequest
{
    [Required, StringLength(20)]
    public string MaHocPhan { get; set; } = string.Empty;

    [Range(0, 10)]
    public double DiemSo { get; set; }
}
