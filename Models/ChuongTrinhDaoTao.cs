using System.ComponentModel.DataAnnotations;

namespace Secure_Academic_Management.Models;

public sealed class ChuongTrinhDaoTao
{
    [Key, StringLength(20)]
    public string MaChuongTrinh { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string TenChuongTrinh { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string TrinhDo { get; set; } = string.Empty;

    [StringLength(150)]
    public string? DonViQuanLy { get; set; }

    public bool DangTuyenSinh { get; set; } = true;

    public ICollection<LanTheoHoc> LanTheoHocs { get; set; } = new List<LanTheoHoc>();
}
