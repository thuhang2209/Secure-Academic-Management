using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Secure_Academic_Management.Data;
using System.ComponentModel.DataAnnotations;

namespace Secure_Academic_Management.Controllers;

[ApiController]
[Route("api/student")]
[Authorize(Roles = "Admin,Staff,Student")]
public sealed class StudentApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StudentApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var query = _context.SinhViens.AsNoTracking();

        if (User.IsInRole("Student"))
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            query = query.Where(student =>
                student.MaSinhVien == id && student.IdentityUserId == userId);
        }
        else
        {
            query = query.Where(student => student.MaSinhVien == id);
        }

        var student = await query
            .Include(item => item.LanTheoHocs)
                .ThenInclude(enrollment => enrollment.ChuongTrinh)
            .Select(item => new
            {
                item.MaSinhVien,
                item.HoTen,
                item.Email,
                item.NgaySinh,
                item.TrangThai,
                LanTheoHoc = item.LanTheoHocs
                    .OrderByDescending(enrollment => enrollment.LaChuongTrinhChinh)
                    .ThenByDescending(enrollment => enrollment.NgayBatDau)
                    .Select(enrollment => new
                    {
                        enrollment.MaChuongTrinh,
                        TenChuongTrinh = enrollment.ChuongTrinh.TenChuongTrinh,
                        enrollment.KhoaNhapHoc,
                        enrollment.TrangThai,
                        enrollment.NgayBatDau,
                        enrollment.NgayKetThuc,
                        enrollment.LaChuongTrinhChinh
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        return student == null ? NotFound() : Ok(student);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentApiCreateRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var exists = await _context.SinhViens.AnyAsync(item => item.Email == request.Email);
        if (exists)
            return Conflict(new { message = "Email đã được sử dụng." });

        string studentCode;
        do
        {
            studentCode = $"SV{DateTime.UtcNow:yyyy}{Random.Shared.Next(1000, 10000)}";
        }
        while (await _context.SinhViens.AnyAsync(item => item.MaSinhVien == studentCode));

        var student = new Models.SinhVien
        {
            MaSinhVien = studentCode,
            HoTen = request.HoTen.Trim(),
            Email = request.Email.Trim(),
            SoDienThoai = request.SoDienThoai?.Trim(),
            NgaySinh = request.NgaySinh,
            TrangThai = "Đang học",
            TrangThaiHoSo = "Đã cấp mã",
            NgayTiepNhanUtc = DateTime.UtcNow,
            NgayKiemTraHoSoUtc = DateTime.UtcNow,
            NguoiTiepNhanId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        };

        _context.SinhViens.Add(student);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = studentCode }, new { MaSinhVien = studentCode });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var student = await _context.SinhViens.FirstOrDefaultAsync(item => item.MaSinhVien == id);
        if (student == null)
            return NotFound();

        _context.SinhViens.Remove(student);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public sealed class StudentApiCreateRequest
{
    [Required, StringLength(100)]
    public string HoTen { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Phone, StringLength(15)]
    public string? SoDienThoai { get; set; }

    [Required]
    public DateTime NgaySinh { get; set; }
}
