using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Secure_Academic_Management.Models
{
    public class SinhVien
    {
        [Key]
        [StringLength(20)]
        public string MaSinhVien { get; set; } = string.Empty; // Mã chuẩn (VD: SV2024001)

        [Required, StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Email { get; set; } = string.Empty;

        [StringLength(15)]
        public string? SoDienThoai { get; set; } // Sẽ áp dụng Dynamic Data Masking sau

        public DateTime NgaySinh { get; set; }
        public string TrangThai { get; set; } = "Đang học"; // Đang học, Bảo lưu, Đã tốt nghiệp

        [Required, StringLength(30)]
        public string TrangThaiHoSo { get; set; } = "Đã cấp mã";

        public DateTime NgayTiepNhanUtc { get; set; } = DateTime.UtcNow;
        public DateTime? NgayKiemTraHoSoUtc { get; set; }

        [StringLength(450)]
        public string? NguoiTiepNhanId { get; set; }

        // Liên kết với IdentityUser để quản lý tài khoản đăng nhập của sinh viên đó
        public string? IdentityUserId { get; set; }
        public virtual IdentityUser? IdentityUser { get; set; }

        // Quan hệ 1-nhiều
        public virtual ICollection<Diem> Diems { get; set; } = new List<Diem>();
        public virtual ICollection<LichSuTrangThai> LichSuTrangThais { get; set; } = new List<LichSuTrangThai>();
        public virtual ICollection<LanTheoHoc> LanTheoHocs { get; set; } = new List<LanTheoHoc>();
        public virtual ICollection<LienKetTaiChinh> LienKetTaiChinhs { get; set; } = new List<LienKetTaiChinh>();
        public virtual ICollection<LienKetHoTro> LienKetHoTros { get; set; } = new List<LienKetHoTro>();
        public virtual ICollection<HoSoTotNghiep> HoSoTotNghieps { get; set; } = new List<HoSoTotNghiep>();
    }
}
