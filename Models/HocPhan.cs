using System.ComponentModel.DataAnnotations;

namespace Secure_Academic_Management.Models
{
    public class HocPhan
    {
        [Key]
        public string MaHocPhan { get; set; } = string.Empty; // VD: INT1001

        [Required, MaxLength(100)]
        public string TenHocPhan { get; set; } = string.Empty;

        public int SoTinChi { get; set; }

    }
}
