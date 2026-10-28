using System.ComponentModel.DataAnnotations;

namespace Secure_Academic_Management.ViewModels
{
    public class StaffEditViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        public bool IsLocked { get; set; }
    }
}
