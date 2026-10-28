using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Secure_Academic_Management.Models;

public sealed class LanTheoHoc
{
    [Key]
    public long Id { get; set; }

    [Required, StringLength(20)]
    public string MaSinhVien { get; set; } = string.Empty;

    [ForeignKey(nameof(MaSinhVien))]
    public SinhVien SinhVien { get; set; } = null!;

    [Required, StringLength(20)]
    public string MaChuongTrinh { get; set; } = string.Empty;

    [ForeignKey(nameof(MaChuongTrinh))]
    public ChuongTrinhDaoTao ChuongTrinh { get; set; } = null!;

    [Range(1900, 2200)]
    public int KhoaNhapHoc { get; set; }

    public DateTime NgayBatDau { get; set; }
    public DateTime? NgayKetThuc { get; set; }

    [Required, StringLength(50)]
    public string TrangThai { get; set; } = "Đang học";

    public bool LaChuongTrinhChinh { get; set; }

    [StringLength(500)]
    public string? GhiChu { get; set; }
}
