using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Secure_Academic_Management.Data;
using Secure_Academic_Management.Models;
using Secure_Academic_Management.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;

namespace Secure_Academic_Management.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var fullName = User.FindFirstValue("FullName") ?? user.UserName ?? user.Email ?? "Người dùng";
            var roles = await _userManager.GetRolesAsync(user);
            SinhVien? studentProfile = null;

            if (roles.Contains("Student"))
            {
                studentProfile = await FindStudentProfileAsync(user);
            }

            return View(new AccountProfileEditViewModel
            {
                FullName = studentProfile?.HoTen ?? fullName,
                Email = studentProfile?.Email ?? user.Email ?? user.UserName ?? string.Empty,
                PhoneNumber = studentProfile?.SoDienThoai ?? user.PhoneNumber,
                RoleText = roles.Count == 0 ? "Người dùng" : string.Join(", ", roles),
                IsStudent = studentProfile != null,
                NgaySinh = studentProfile?.NgaySinh,
                StudentProfile = studentProfile
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(AccountProfileEditViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var roles = await _userManager.GetRolesAsync(user);
            var studentProfile = roles.Contains("Student")
                ? await FindStudentProfileAsync(user)
                : null;

            model.IsStudent = studentProfile != null;
            model.RoleText = roles.Count == 0 ? "Người dùng" : string.Join(", ", roles);

            if (studentProfile != null && !model.NgaySinh.HasValue)
                ModelState.AddModelError(nameof(model.NgaySinh), "Vui lòng nhập ngày sinh.");

            if (await _userManager.FindByEmailAsync(model.Email.Trim()) is { } existingUser
                && existingUser.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng.");
            }

            if (!ModelState.IsValid)
            {
                model.StudentProfile = studentProfile;
                return View("Profile", model);
            }

            var fullName = model.FullName.Trim();
            var email = model.Email.Trim();
            user.Email = email;
            user.UserName = email;
            user.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();

            var currentNameClaims = await _userManager.GetClaimsAsync(user);
            foreach (var claim in currentNameClaims.Where(claim => claim.Type == "FullName").ToList())
                await _userManager.RemoveClaimAsync(user, claim);
            await _userManager.AddClaimAsync(user, new Claim("FullName", fullName));

            if (studentProfile != null)
            {
                studentProfile.HoTen = fullName;
                studentProfile.Email = email;
                studentProfile.SoDienThoai = user.PhoneNumber;
                studentProfile.NgaySinh = model.NgaySinh!.Value;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                model.StudentProfile = studentProfile;
                return View("Profile", model);
            }

            await _context.SaveChangesAsync();
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Đã cập nhật thông tin cá nhân.";
            return RedirectToAction(nameof(Profile));
        }

        private async Task<SinhVien?> FindStudentProfileAsync(IdentityUser user)
        {
            var student = await _context.SinhViens
                .FirstOrDefaultAsync(candidate => candidate.IdentityUserId == user.Id);

            if (student != null)
                return student;

            // Repair legacy records that were created before IdentityUserId was populated.
            student = await _context.SinhViens
                .FirstOrDefaultAsync(candidate =>
                    candidate.Email == user.Email && candidate.IdentityUserId == null);

            if (student != null)
            {
                student.IdentityUserId = user.Id;
                await _context.SaveChangesAsync();
            }

            return student;
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var result = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty,
                        error.Code == "PasswordMismatch"
                            ? "Mật khẩu hiện tại không chính xác."
                            : error.Description);
                }

                _logger.LogWarning(
                    "Password change failed for user {UserId}",
                    user.Id);
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation(
                "Password changed successfully for user {UserId}",
                user.Id);
            TempData["SuccessMessage"] = "Đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [EnableRateLimiting("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation(
                        "User logged in successfully: {Email}; IP: {IpAddress}",
                        model.Email,
                        HttpContext.Connection.RemoteIpAddress?.ToString());
                    return RedirectToAction("Index", "Home");
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out: {Email}", model.Email);
                    ModelState.AddModelError("", "Tài khoản đã bị khóa do nhập sai quá nhiều lần. Vui lòng thử lại sau.");
                    return View(model);
                }

                _logger.LogWarning(
                    "Invalid login attempt: {Email}; IP: {IpAddress}",
                    model.Email,
                    HttpContext.Connection.RemoteIpAddress?.ToString());
                ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
                return View(model);
            }

            return View(model);
        }

        //  REGISTER - GET
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // REGISTER - POST 
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Email này đã được đăng ký. Vui lòng sử dụng email khác.");
                    return View(model);
                }

                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    EmailConfirmed = false,
                    LockoutEnabled = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // THÊM CLAIM FULLNAME VÀO USER
                    if (!string.IsNullOrEmpty(model.FullName))
                    {
                        await _userManager.AddClaimAsync(user, new Claim("FullName", model.FullName));
                    }

                    // Thêm claim Email
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Email, user.Email));

                    // Gán role mặc định
                    if (!await _roleManager.RoleExistsAsync("Student"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Student"));
                    }

                    await _userManager.AddToRoleAsync(user, "Student");

                        var studentProfile = await _context.SinhViens
                            .FirstOrDefaultAsync(student => student.Email == user.Email);
                        if (studentProfile != null && string.IsNullOrEmpty(studentProfile.IdentityUserId))
                        {
                            studentProfile.IdentityUserId = user.Id;
                            await _context.SaveChangesAsync();
                        }

                    _logger.LogInformation("User created a new student account: {Email}", user.Email);

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    _logger.LogInformation("User logged in after registration: {Email}", user.Email);

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // LOGOUT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation(
                "User logged out; UserId: {UserId}; IP: {IpAddress}",
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                HttpContext.Connection.RemoteIpAddress?.ToString());
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}