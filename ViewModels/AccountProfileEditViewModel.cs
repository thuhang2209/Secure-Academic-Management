using System.ComponentModel.DataAnnotations;
using Secure_Academic_Management.Models;

namespace Secure_Academic_Management.ViewModels
{
    public class AccountProfileEditViewModel
    {
        [Required, StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(50)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone, StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        [CustomValidation(typeof(AccountProfileEditViewModel), nameof(ValidateBirthDate))]
        public DateTime? NgaySinh { get; set; }

        public bool IsStudent { get; set; }
        public string RoleText { get; set; } = "Người dùng";
        public SinhVien? StudentProfile { get; set; }

        public static ValidationResult? ValidateBirthDate(DateTime? ngaySinh)
        {
            if (!ngaySinh.HasValue)
                return ValidationResult.Success;

            var age = DateTime.Today.Year - ngaySinh.Value.Year;
            if (ngaySinh.Value.Date > DateTime.Today.AddYears(-age))
                age--;

            return age < 17
                ? new ValidationResult("Sinh viên phải đủ 17 tuổi trở lên.")
                : ValidationResult.Success;
        }
    }
}
