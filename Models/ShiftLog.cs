using System.ComponentModel.DataAnnotations;

namespace shift_handover_page.Models;

public class ShiftLog
{
    public int Id { get; set; }

    public int ShiftId { get; set; }

    public Shift Shift { get; set; } = null!;

    public ShiftLogType LogType { get; set; }

    public DateTime OccurredAt { get; set; }

    [Required]
    [MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public int? ManpowerCount { get; set; }

    [MaxLength(450)]
    public string? LoggedByUserId { get; set; }

    [MaxLength(256)]
    public string? LoggedByUserName { get; set; }

    public DateTime LoggedAt { get; set; }
}