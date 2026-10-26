using System.ComponentModel.DataAnnotations;

namespace Secure_Academic_Management.ViewModels
{
    public class SinhVienEditViewModel
    {
        [Required]
        [MaxLength(20)]

        [Display(Name = "Mã sinh viên")]
        public string MaSinhVien { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Phone, MaxLength(15)]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        [CustomValidation(typeof(SinhVienEditViewModel), nameof(ValidateAge))]
        public DateTime NgaySinh { get; set; }


        // Method validation tuổi (tối thiểu 17 tuổi)
        public static ValidationResult? ValidateAge(DateTime ngaySinh)
        {
            var tuoi = DateTime.Now.Year - ngaySinh.Year;

            if (ngaySinh.Date > DateTime.Now.AddYears(-tuoi))
                tuoi--;

            if (tuoi < 17)
            {
                return new ValidationResult("Sinh viên phải đủ 17 tuổi trở lên (ngày sinh trước ngày " +
                    DateTime.Now.AddYears(-17).ToString("dd/MM/yyyy") + ")");
            }

            return ValidationResult.Success;
        }

        [Required, MaxLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Đang học";
    }
}