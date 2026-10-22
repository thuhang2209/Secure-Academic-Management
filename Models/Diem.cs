using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Secure_Academic_Management.Models
{
    public class Diem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MaSinhVien { get; set; } = string.Empty;
        public virtual SinhVien SinhVien { get; set; } = null!;

        [Required]
        public string MaHocPhan { get; set; } = string.Empty;
        public virtual HocPhan HocPhan { get; set; } = null!;

        [Range(0.0, 10.0)]
        public double DiemSo { get; set; }

        public DateTime NgayCapNhat { get; set; } = DateTime.UtcNow;

        // Audit: Ghi nhận ai là người nhập/sửa điểm này (Checklist M03 - Mục 20)
        public string? NguoiCapNhatId { get; set; }
    }
}