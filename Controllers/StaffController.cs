using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Secure_Academic_Management.ViewModels;
using System.Security.Claims;

namespace Secure_Academic_Management.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var staffUsers = await _userManager.GetUsersInRoleAsync("Staff");
            var staff = new List<StaffListItemViewModel>();

            foreach (var user in staffUsers.OrderBy(user => user.Email))
            {
                var fullName = (await _userManager.GetClaimsAsync(user))
                    .FirstOrDefault(claim => claim.Type == "FullName")?.Value
                    ?? user.UserName
                    ?? string.Empty;

                staff.Add(new StaffListItemViewModel
                {
                    Id = user.Id,
                    FullName = fullName,
                    Email = user.Email ?? string.Empty,
                    LockoutEnd = user.LockoutEnd
                });
            }

            return View(staff);
        }

        [HttpGet]
        public IActionResult CreateStaff() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStaff(StaffCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Email này đã được sử dụng.");
                    return View(model);
                }

                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Staff"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Staff"));
                    }

                    await _userManager.AddToRoleAsync(user, "Staff");
                    await _userManager.AddClaimAsync(user, new Claim("FullName", model.FullName));
                    _logger.LogInformation("Admin {AdminId} created staff account {Email}",
                        User.FindFirstValue(ClaimTypes.NameIdentifier), model.Email);
                    TempData["SuccessMessage"] = $"Đã tạo tài khoản nhân viên '{model.FullName}' thành công.";
                    return RedirectToAction(nameof(Index));
                }

                AddIdentityErrors(result);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditStaff(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Staff"))
            {
                return NotFound();
            }

            var fullName = (await _userManager.GetClaimsAsync(user))
                .FirstOrDefault(claim => claim.Type == "FullName")?.Value ?? string.Empty;

            return View(new StaffEditViewModel
            {
                Id = user.Id,
                FullName = fullName,
                Email = user.Email ?? string.Empty,
                IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStaff(StaffEditViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Staff"))
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var emailOwner = await _userManager.FindByEmailAsync(model.Email);
            if (emailOwner != null && emailOwner.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng.");
                return View(model);
            }

            user.Email = model.Email;
            user.UserName = model.Email;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                AddIdentityErrors(updateResult);
                return View(model);
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var fullNameClaims = claims.Where(claim => claim.Type == "FullName").ToList();
            if (fullNameClaims.Count > 0)
            {
                var removeClaimsResult = await _userManager.RemoveClaimsAsync(user, fullNameClaims);
                if (!removeClaimsResult.Succeeded)
                {
                    AddIdentityErrors(removeClaimsResult);
                    return View(model);
                }
            }

            var addClaimResult = await _userManager.AddClaimAsync(user, new Claim("FullName", model.FullName));
            if (!addClaimResult.Succeeded)
            {
                AddIdentityErrors(addClaimResult);
                return View(model);
            }

            user.LockoutEnd = model.IsLocked ? DateTimeOffset.UtcNow.AddYears(10) : null;
            var lockoutResult = await _userManager.UpdateAsync(user);
            if (!lockoutResult.Succeeded)
            {
                AddIdentityErrors(lockoutResult);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Đã cập nhật nhân viên '{model.FullName}'.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStaff(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Staff"))
            {
                return NotFound();
            }

            var email = user.Email;
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                AddIdentityErrors(result);
                _logger.LogWarning("Failed to delete staff account {UserId}: {Errors}",
                    id, string.Join("; ", result.Errors.Select(error => error.Code)));
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Admin {AdminId} deleted staff account {Email}",
                User.FindFirstValue(ClaimTypes.NameIdentifier), email);
            TempData["SuccessMessage"] = $"Đã xóa tài khoản nhân viên '{email}'.";
            return RedirectToAction(nameof(Index));
        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
