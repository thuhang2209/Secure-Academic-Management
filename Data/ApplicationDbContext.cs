using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Secure_Academic_Management.Models;

namespace Secure_Academic_Management.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {

        }

        public DbSet<SinhVien> SinhViens { get; set; }
        public DbSet<HocPhan> HocPhans { get; set; }
        public DbSet<Diem> Diems { get; set; }
        public DbSet<LichSuTrangThai> LichSuTrangThais { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<ChuongTrinhDaoTao> ChuongTrinhDaoTaos { get; set; }
        public DbSet<LanTheoHoc> LanTheoHocs { get; set; }
        public DbSet<LienKetTaiChinh> LienKetTaiChinhs { get; set; }
        public DbSet<LienKetHoTro> LienKetHoTros { get; set; }
        public DbSet<HoSoTotNghiep> HoSoTotNghieps { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Gọi phương thức gốc để tạo các bảng Identity (AspNetUsers, AspNetRoles, v.v.)
            base.OnModelCreating(builder);

            builder.Entity<Diem>()
                .HasOne(d => d.SinhVien)
                .WithMany(s => s.Diems)
                .HasForeignKey(d => d.MaSinhVien);

            builder.Entity<Diem>()
                .HasOne(d => d.HocPhan)
                .WithMany()
                .HasForeignKey(d => d.MaHocPhan);

            builder.Entity<Diem>()
                .HasIndex(d => d.MaSinhVien);

            builder.Entity<SinhVien>()
                .HasIndex(s => s.Email)
                .IsUnique();

            builder.Entity<AuditLog>()
                .HasIndex(a => a.OccurredAtUtc);

            builder.Entity<AuditLog>()
                .HasIndex(a => new { a.UserId, a.Action });

            builder.Entity<LichSuTrangThai>()
                .HasOne(l => l.SinhVien)
                .WithMany(s => s.LichSuTrangThais)
                .HasForeignKey(l => l.MaSinhVien)
                .OnDelete(DeleteBehavior.Restrict); // Không cho xóa sinh viên nếu còn lịch sử

            builder.Entity<LanTheoHoc>()
                .HasOne(item => item.SinhVien)
                .WithMany(student => student.LanTheoHocs)
                .HasForeignKey(item => item.MaSinhVien)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<LanTheoHoc>()
                .HasOne(item => item.ChuongTrinh)
                .WithMany(program => program.LanTheoHocs)
                .HasForeignKey(item => item.MaChuongTrinh)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LanTheoHoc>()
                .HasIndex(item => new { item.MaSinhVien, item.MaChuongTrinh, item.NgayBatDau })
                .IsUnique();

            builder.Entity<LienKetTaiChinh>().ToTable("LienKetTaiChinhs");
            builder.Entity<LienKetHoTro>().ToTable("LienKetHoTros");
            builder.Entity<HoSoTotNghiep>().ToTable("HoSoTotNghieps");

            builder.Entity<LienKetTaiChinh>()
                .HasOne(item => item.SinhVien)
                .WithMany(student => student.LienKetTaiChinhs)
                .HasForeignKey(item => item.MaSinhVien)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<LienKetHoTro>()
                .HasOne(item => item.SinhVien)
                .WithMany(student => student.LienKetHoTros)
                .HasForeignKey(item => item.MaSinhVien)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<HoSoTotNghiep>()
                .HasOne(item => item.SinhVien)
                .WithMany(student => student.HoSoTotNghieps)
                .HasForeignKey(item => item.MaSinhVien)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
