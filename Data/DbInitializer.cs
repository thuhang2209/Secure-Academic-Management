using Microsoft.AspNetCore.Identity;
using Secure_Academic_Management.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Secure_Academic_Management.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var seedPasswords = new Dictionary<string, string?>
            {
                ["admin@university.edu.vn"] = configuration["SeedUsers:AdminPassword"],
                ["staff@university.edu.vn"] = configuration["SeedUsers:StaffPassword"],
                ["student@university.edu.vn"] = configuration["SeedUsers:StudentPassword"]
            };

            // 1. Tạo các Roles cơ bản (Checklist M03 #9)
            // Lưu ý: Dùng "Student" để đúng với nghiệp vụ M03 (Admin, Staff, Student)
            string[] roleNames = { "Admin", "Staff", "Student" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Tạo tài khoản ADMIN mẫu
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

                var result = await userManager.CreateAsync(adminUser, GetSeedPassword(seedPasswords, adminEmail));
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    await userManager.AddClaimAsync(adminUser, new Claim("FullName", "Quản trị viên"));
                }
            }

            // 3. Tạo tài khoản STAFF mẫu
            var staffEmail = "staff@university.edu.vn";
            var staffUser = await userManager.FindByEmailAsync(staffEmail);
            if (staffUser == null)
            {
                staffUser = new IdentityUser
                {
                    UserName = staffEmail,
                    Email = staffEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(staffUser, GetSeedPassword(seedPasswords, staffEmail));
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(staffUser, "Staff");
                    await userManager.AddClaimAsync(staffUser, new Claim("FullName", "Nhân viên Đào tạo"));
                }
            }

            // TẠO TÀI KHOẢN STUDENT MẪU
            var studentEmail = "student@university.edu.vn";
            var studentUser = await userManager.FindByEmailAsync(studentEmail);

            if (studentUser == null)
            {
                studentUser = new IdentityUser
                {
                    UserName = studentEmail,
                    Email = studentEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(studentUser, GetSeedPassword(seedPasswords, studentEmail));
                if (result.Succeeded)
                {
                    // Gán đúng role "Student"
                    await userManager.AddToRoleAsync(studentUser, "Student");

                    // Thêm claim FullName để hiển thị tên trên giao diện
                    await userManager.AddClaimAsync(studentUser, new Claim("FullName", "Sinh viên"));
                }
            }

            var sampleStudent = await dbContext
                .SinhViens
                .FirstOrDefaultAsync(student => student.Email == studentEmail);

            if (sampleStudent != null && sampleStudent.IdentityUserId != studentUser.Id)
            {
                sampleStudent.IdentityUserId = studentUser.Id;
                await dbContext.SaveChangesAsync();
            }

            await SeedSampleStudentsAsync(dbContext, studentUser.Id);
        }

        private static async Task SeedSampleStudentsAsync(
            ApplicationDbContext dbContext,
            string sampleStudentUserId)
        {
            var sampleStudents = new[]
            {
                new StudentSeed("SV20261001", "Nguyễn Minh Anh", "student@university.edu.vn", "0901000001", new DateTime(2004, 3, 15), "Đang học"),
                new StudentSeed("SV20261002", "Trần Hoàng Nam", "hoangnam02@university.edu.vn", "0901000002", new DateTime(2003, 7, 22), "Đang học"),
                new StudentSeed("SV20261003", "Lê Thu Hà", "thuha03@university.edu.vn", "0901000003", new DateTime(2004, 11, 8), "Bảo lưu"),
                new StudentSeed("SV20261004", "Phạm Quốc Bảo", "quocbao04@university.edu.vn", "0901000004", new DateTime(2002, 1, 30), "Tạm ngừng học"),
                new StudentSeed("SV20261005", "Đỗ Ngọc Lan", "ngoclan05@university.edu.vn", "0901000005", new DateTime(2003, 5, 17), "Đã tốt nghiệp"),
                new StudentSeed("SV20261006", "Vũ Đức Anh", "ducanh06@university.edu.vn", "0901000006", new DateTime(2004, 9, 3), "Đã thôi học"),
                new StudentSeed("SV20261007", "Bùi Khánh Linh", "khanhlinh07@university.edu.vn", "0901000007", new DateTime(2005, 2, 25), "Đang học"),
                new StudentSeed("SV20261008", "Ngô Tuấn Kiệt", "tuankiet08@university.edu.vn", "0901000008", new DateTime(2003, 12, 12), "Bảo lưu"),
                new StudentSeed("SV20261009", "Hoàng Yến Nhi", "yennhi09@university.edu.vn", "0901000009", new DateTime(2004, 6, 6), "Đang học"),
                new StudentSeed("SV20261010", "Đặng Gia Huy", "giahuy10@university.edu.vn", "0901000010", new DateTime(2002, 10, 19), "Đã tốt nghiệp")
            };

            foreach (var seed in sampleStudents)
            {
                var exists = await dbContext.SinhViens.AnyAsync(student =>
                    student.MaSinhVien == seed.MaSinhVien || student.Email == seed.Email);

                if (!exists)
                {
                    dbContext.SinhViens.Add(new SinhVien
                    {
                        MaSinhVien = seed.MaSinhVien,
                        HoTen = seed.HoTen,
                        Email = seed.Email,
                        SoDienThoai = seed.SoDienThoai,
                        NgaySinh = seed.NgaySinh,
                        TrangThai = seed.TrangThai
                    });
                }
            }

            await dbContext.SaveChangesAsync();
            var linkedStudent = await dbContext.SinhViens
                .FirstOrDefaultAsync(student => student.Email == "student@university.edu.vn");
            if (linkedStudent != null && linkedStudent.IdentityUserId != sampleStudentUserId)
            {
                linkedStudent.IdentityUserId = sampleStudentUserId;
                await dbContext.SaveChangesAsync();
            }

            await SeedSampleCoursesAndGradesAsync(dbContext, sampleStudents);
            await SeedSampleProgramsAndEnrollmentsAsync(dbContext, sampleStudents);
        }

        private static async Task SeedSampleProgramsAndEnrollmentsAsync(
            ApplicationDbContext dbContext,
            IReadOnlyCollection<StudentSeed> sampleStudents)
        {
            var programs = new[]
            {
                new ChuongTrinhDaoTao
                {
                    MaChuongTrinh = "CNTT",
                    TenChuongTrinh = "Công nghệ thông tin",
                    TrinhDo = "Đại học",
                    DonViQuanLy = "Khoa Công nghệ thông tin"
                },
                new ChuongTrinhDaoTao
                {
                    MaChuongTrinh = "QTKD",
                    TenChuongTrinh = "Quản trị kinh doanh",
                    TrinhDo = "Đại học",
                    DonViQuanLy = "Khoa Kinh tế"
                }
            };

            foreach (var program in programs)
            {
                if (!await dbContext.ChuongTrinhDaoTaos.AnyAsync(item =>
                        item.MaChuongTrinh == program.MaChuongTrinh))
                    dbContext.ChuongTrinhDaoTaos.Add(program);
            }

            await dbContext.SaveChangesAsync();
            foreach (var student in sampleStudents)
            {
                var programCode = student.MaSinhVien.EndsWith("02") ||
                    student.MaSinhVien.EndsWith("04") ||
                    student.MaSinhVien.EndsWith("06")
                    ? "QTKD"
                    : "CNTT";

                if (!await dbContext.LanTheoHocs.AnyAsync(item =>
                        item.MaSinhVien == student.MaSinhVien &&
                        item.MaChuongTrinh == programCode))
                {
                    dbContext.LanTheoHocs.Add(new LanTheoHoc
                    {
                        MaSinhVien = student.MaSinhVien,
                        MaChuongTrinh = programCode,
                        KhoaNhapHoc = student.NgaySinh.Year + 18,
                        NgayBatDau = new DateTime(student.NgaySinh.Year + 18, 9, 1),
                        TrangThai = student.TrangThai,
                        LaChuongTrinhChinh = true
                    });
                }
            }

            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedSampleCoursesAndGradesAsync(
            ApplicationDbContext dbContext,
            IReadOnlyCollection<StudentSeed> sampleStudents)
        {
            var courses = new[]
            {
                new HocPhan { MaHocPhan = "INT1001", TenHocPhan = "Lập trình cơ bản", SoTinChi = 3 },
                new HocPhan { MaHocPhan = "MAT1001", TenHocPhan = "Toán cao cấp", SoTinChi = 3 },
                new HocPhan { MaHocPhan = "ENG1001", TenHocPhan = "Tiếng Anh chuyên ngành", SoTinChi = 2 }
            };

            foreach (var course in courses)
            {
                if (!await dbContext.HocPhans.AnyAsync(item => item.MaHocPhan == course.MaHocPhan))
                {
                    dbContext.HocPhans.Add(course);
                }
            }

            await dbContext.SaveChangesAsync();
            var courseCodes = courses.Select(course => course.MaHocPhan).ToArray();

            foreach (var student in sampleStudents)
            {
                var studentExists = await dbContext.SinhViens.AnyAsync(item => item.MaSinhVien == student.MaSinhVien);
                if (!studentExists)
                    continue;

                for (var index = 0; index < courseCodes.Length; index++)
                {
                    var gradeExists = await dbContext.Diems.AnyAsync(item =>
                        item.MaSinhVien == student.MaSinhVien && item.MaHocPhan == courseCodes[index]);

                    if (!gradeExists)
                    {
                        dbContext.Diems.Add(new Diem
                        {
                            MaSinhVien = student.MaSinhVien,
                            MaHocPhan = courseCodes[index],
                            DiemSo = Math.Round(6.5 + ((student.MaSinhVien[^1] - '0') + index) % 35 / 10.0, 1),
                            NgayCapNhat = DateTime.UtcNow
                        });
                    }
                }
            }

            await dbContext.SaveChangesAsync();
        }

        private sealed record StudentSeed(
            string MaSinhVien,
            string HoTen,
            string Email,
            string SoDienThoai,
            DateTime NgaySinh,
            string TrangThai);

        static string GetSeedPassword(IReadOnlyDictionary<string, string?> passwords, string email)
        {
            var password = passwords[email];
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    $"Seed password for {email} is not configured. Set SeedUsers:{email.Split('@')[0]}Password in User Secrets or environment variables.");
            }

            return password;
        }
    }
}