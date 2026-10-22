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
        }
    }
}
