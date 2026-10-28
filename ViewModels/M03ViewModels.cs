using System.ComponentModel.DataAnnotations;
using Secure_Academic_Management.Models;

namespace Secure_Academic_Management.ViewModels;

public sealed class ChuongTrinhViewModel
{
    [Required, StringLength(20)]
    [Display(Name = "Mã chương trình")]
    public string MaChuongTrinh { get; set; } = string.Empty;

    [Required, StringLength(150)]
    [Display(Name = "Tên chương trình")]
    public string TenChuongTrinh { get; set; } = string.Empty;

    [Required, StringLength(50)]
    [Display(Name = "Trình độ")]
    public string TrinhDo { get; set; } = string.Empty;

    [StringLength(150)]
    [Display(Name = "Đơn vị quản lý")]
    public string? DonViQuanLy { get; set; }

    [Display(Name = "Đang tuyển sinh")]
    public bool DangTuyenSinh { get; set; } = true;
}

public sealed class LanTheoHocViewModel
{
    [Required, StringLength(20)]
    [Display(Name = "Mã sinh viên")]
    public string MaSinhVien { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [Display(Name = "Mã chương trình")]
    public string MaChuongTrinh { get; set; } = string.Empty;

    [Range(1900, 2200)]
    [Display(Name = "Khóa nhập học")]
    public int KhoaNhapHoc { get; set; } = DateTime.Today.Year;

    [DataType(DataType.Date)]
    [Display(Name = "Ngày bắt đầu")]
    public DateTime NgayBatDau { get; set; } = DateTime.Today;

    [Required, StringLength(50)]
    public string TrangThai { get; set; } = "Đang học";

    [Display(Name = "Chương trình chính")]
    public bool LaChuongTrinhChinh { get; set; }

    [StringLength(500)]
    public string? GhiChu { get; set; }
}

public sealed class M03DashboardViewModel
{
    public IReadOnlyList<ChuongTrinhDaoTao> ChuongTrinhs { get; init; } = Array.Empty<ChuongTrinhDaoTao>();
    public IReadOnlyList<LanTheoHoc> LanTheoHocs { get; init; } = Array.Empty<LanTheoHoc>();
}

public sealed class LienKetNghiepVuViewModel
{
    [Required, StringLength(20)]
    [Display(Name = "Mã sinh viên")]
    public string MaSinhVien { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Loại liên kết")]
    public string LoaiLienKet { get; set; } = "Tài chính";

    [Required, StringLength(100)]
    [Display(Name = "Mã tham chiếu")]
    public string MaThamChieu { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Hệ thống nguồn")]
    public string HeThongNguon { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Phân loại")]
    public string? PhanLoai { get; set; }

    [StringLength(500)]
    public string? MoTa { get; set; }
}
