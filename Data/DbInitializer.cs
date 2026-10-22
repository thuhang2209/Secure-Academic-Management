using Microsoft.AspNetCore.Identity;
using Secure_Academic_Management.Models;
using System.Security.Claims;

namespace Secure_Academic_Management.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Tạo roles
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Staff"))
                await roleManager.CreateAsync(new IdentityRole("Staff"));

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            // Tạo tài khoản Admin mẫu
            var adminEmail = "admin@university.edu.vn";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    //  THÊM CLAIM FULLNAME CHO ADMIN
                    await userManager.AddClaimAsync(adminUser, new Claim("FullName", "Administrator"));
                    await userManager.AddClaimAsync(adminUser, new Claim(ClaimTypes.Email, adminEmail));
                }
            }
        }
    }
}