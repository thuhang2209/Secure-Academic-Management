using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Secure_Academic_Management.ViewModels
{
    public class CapNhatTrangThaiViewModel
    {
        [Required]
        [MaxLength(20)]
        public string MaSinhVien { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn trạng thái mới")]
        [Display(Name = "Chuyển sang trạng thái")]
        public string TrangThaiMoi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lý do/căn cứ thay đổi")]
        [MaxLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự")]
        [Display(Name = "Căn cứ thay đổi")]
        public string LyDo { get; set; } = string.Empty;

        // File upload cho bằng chứng
        [Display(Name = "Tài liệu đính kèm")]
        public IFormFile? TaiLieuDinhKem { get; set; }

        // Lưu trữ đường dẫn file
        public string? DuongDanFile { get; set; }
    }
}