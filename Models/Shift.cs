using System.ComponentModel.DataAnnotations;

namespace shift_handover_page.Models;

public class Shift
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    public DateTime ScheduledFor { get; set; }

    public ShiftStatus Status { get; set; } = ShiftStatus.Open;

    [MaxLength(450)]
    public string? ClaimedByUserId { get; set; }

    [MaxLength(256)]
    public string? ClaimedByUserName { get; set; }

    public DateTime? ClaimedAt { get; set; }

    [MaxLength(450)]
    public string? ClosedByUserId { get; set; }

    [MaxLength(256)]
    public string? ClosedByUserName { get; set; }

    public DateTime? ClosedAt { get; set; }

    [MaxLength(1000)]
    public string? ClosureSummary { get; set; }

    public List<ShiftLog> Logs { get; set; } = new();
}