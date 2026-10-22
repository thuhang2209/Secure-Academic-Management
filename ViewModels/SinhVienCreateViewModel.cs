using System.ComponentModel.DataAnnotations;

namespace Secure_Academic_Management.ViewModels
{
    public class SinhVienCreateViewModel
    {
        // KHÔNG cho phép người dùng tự nhập MaSinhVien qua form (tránh giả mạo)
        // MaSinhVien sẽ được hệ thống tự động sinh ra ở Controller

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [MaxLength(15)]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime NgaySinh { get; set; }

        // Lưu ý: Chúng ta KHÔNG đưa các trường như "DiemSo", "TrangThai", "IdentityUserId" vào đây
        // để ngăn chặn tấn công Over-posting (Checklist M03 #11, #18)
    }
}