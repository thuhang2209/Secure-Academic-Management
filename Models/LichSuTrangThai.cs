using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Secure_Academic_Management.Models
{
    public class LichSuTrangThai
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string MaSinhVien { get; set; } = string.Empty;

        [ForeignKey(nameof(MaSinhVien))]
        public virtual SinhVien SinhVien { get; set; } = null!;

        [Required, MaxLength(50)]
        public string TrangThaiCu { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string TrangThaiMoi { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string LyDo { get; set; } = string.Empty;

        // Đường dẫn file đính kèm
        [MaxLength(500)]
        public string? DuongDanFile { get; set; }

        [MaxLength(255)]
        public string? TenFileGoc { get; set; }

        public DateTime NgayThayDoi { get; set; } = DateTime.UtcNow;
        public DateTime NgayHieuLuc { get; set; } = DateTime.UtcNow;

        [MaxLength(450)]
        public string? NguoiThucHienId { get; set; }
    }
}