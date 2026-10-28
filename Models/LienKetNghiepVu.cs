using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Secure_Academic_Management.Models;

public abstract class LienKetSinhVien
{
    [Key]
    public long Id { get; set; }

    [Required, StringLength(20)]
    public string MaSinhVien { get; set; } = string.Empty;

    [ForeignKey(nameof(MaSinhVien))]
    public SinhVien SinhVien { get; set; } = null!;

    [Required, StringLength(100)]
    public string MaThamChieu { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string HeThongNguon { get; set; } = string.Empty;

    [StringLength(500)]
    public string? MoTa { get; set; }

    public DateTime NgayLienKetUtc { get; set; } = DateTime.UtcNow;
}

public sealed class LienKetTaiChinh : LienKetSinhVien
{
    [Required, StringLength(50)]
    public string LoaiNghiaVu { get; set; } = string.Empty;
}

public sealed class LienKetHoTro : LienKetSinhVien
{
    [Required, StringLength(100)]
    public string LoaiHoTro { get; set; } = string.Empty;
}

public sealed class HoSoTotNghiep : LienKetSinhVien
{
    public DateTime? NgayTotNghiep { get; set; }

    [StringLength(50)]
    public string TrangThai { get; set; } = "Đang xử lý";
}
