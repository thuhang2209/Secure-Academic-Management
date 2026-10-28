using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Secure_Academic_Management.Data;
using System.Security.Claims;

namespace Secure_Academic_Management.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudentController> _logger;

        public StudentController(ApplicationDbContext context, ILogger<StudentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Student/Profile
        public async Task<IActionResult> Profile()
        {
            // Lấy ID của sinh viên đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Tìm hồ sơ sinh viên theo IdentityUserId
            var sinhVien = await _context.SinhViens
                .Include(student => student.LanTheoHocs)
                    .ThenInclude(enrollment => enrollment.ChuongTrinh)
                .Include(student => student.LienKetTaiChinhs)
                .Include(student => student.LienKetHoTros)
                .Include(student => student.HoSoTotNghieps)
                .FirstOrDefaultAsync(s => s.IdentityUserId == userId);

            if (sinhVien == null)
            {
                _logger.LogWarning("Student profile not found for user: {UserId}", userId);
                return View("NoProfile"); // Hoặc redirect về trang thông báo
            }

            return View(sinhVien);
        }
    }
}