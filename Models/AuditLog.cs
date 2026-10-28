using System.ComponentModel.DataAnnotations;

namespace Secure_Academic_Management.Models;

public sealed class AuditLog
{
    public long Id { get; set; }

    [Required, MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    public string HttpMethod { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Path { get; set; } = string.Empty;

    [MaxLength(450)]
    public string? UserId { get; set; }

    [MaxLength(256)]
    public string? UserName { get; set; }

    [MaxLength(64)]
    public string? IpAddress { get; set; }

    [MaxLength(512)]
    public string? UserAgent { get; set; }

    public int StatusCode { get; set; }
    public long DurationMilliseconds { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? EntityType { get; set; }

    [MaxLength(100)]
    public string? EntityId { get; set; }
}
