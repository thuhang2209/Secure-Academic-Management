using System.ComponentModel.DataAnnotations;
using Secure_Academic_Management.Models;

namespace Secure_Academic_Management.ViewModels;

public sealed class CourseListViewModel
{
    public IReadOnlyList<HocPhan> Courses { get; init; } = [];
    public string? Search { get; init; }
}

public sealed class CourseViewModel
{
    [Required, StringLength(20)]
    [Display(Name = "Mã học phần")]
    public string MaHocPhan { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Tên học phần")]
    public string TenHocPhan { get; set; } = string.Empty;

    [Range(1, 10)]
    [Display(Name = "Số tín chỉ")]
    public int SoTinChi { get; set; }
}

public sealed class GradeListViewModel
{
    public SinhVien Student { get; init; } = null!;
    public IReadOnlyList<Diem> Grades { get; init; } = [];
}

public sealed class GradeViewModel
{
    [Required]
    public string MaSinhVien { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Học phần")]
    public string MaHocPhan { get; set; } = string.Empty;

    [Range(0, 10)]
    [Display(Name = "Điểm số")]
    public double DiemSo { get; set; }
}
