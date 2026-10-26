using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Secure_Academic_Management.Models
{
    public class SinhVien
    {
        [Key]
        public string MaSinhVien { get; set; } = string.Empty; // Mã chuẩn (VD: SV2024001)

        [Required, MaxLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? SoDienThoai { get; set; } // Sẽ áp dụng Dynamic Data Masking sau

        public DateTime NgaySinh { get; set; }
        public string TrangThai { get; set; } = "Đang học"; // Đang học, Bảo lưu, Đã tốt nghiệp

        // Liên kết với IdentityUser để quản lý tài khoản đăng nhập của sinh viên đó
        public string? IdentityUserId { get; set; }
        public virtual IdentityUser? IdentityUser { get; set; }

        // Quan hệ 1-nhiều
        public virtual ICollection<Diem> Diems { get; set; } = new List<Diem>();
        public virtual ICollection<LichSuTrangThai> LichSuTrangThais { get; set; } = new List<LichSuTrangThai>();
    }
}
