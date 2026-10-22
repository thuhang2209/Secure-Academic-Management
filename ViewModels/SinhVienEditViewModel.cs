using System.ComponentModel.DataAnnotations;

namespace Secure_Academic_Management.ViewModels
{
    public class SinhVienEditViewModel
    {
        [Required]
        [MaxLength(20)]
        public string MaSinhVien { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Phone, MaxLength(15)]
        public string? SoDienThoai { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime NgaySinh { get; set; }

        [Required, MaxLength(50)]
        public string TrangThai { get; set; } = "Đang học";
    }
}