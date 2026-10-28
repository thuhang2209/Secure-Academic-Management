using Secure_Academic_Management.Models;

namespace Secure_Academic_Management.ViewModels
{
    public class AccountProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
        public SinhVien? StudentProfile { get; set; }
    }
}
