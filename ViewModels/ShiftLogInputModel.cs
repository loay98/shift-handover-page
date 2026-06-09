using System.ComponentModel.DataAnnotations;
using shift_handover_page.Models;

namespace shift_handover_page.ViewModels;

public class ShiftLogInputModel
{
    public int ShiftId { get; set; }

    [Required]
    public ShiftLogType LogType { get; set; }

    [Required]
    public DateTime OccurredAt { get; set; } = DateTime.Now;

    [Required]
    [MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 1000)]
    public int? ManpowerCount { get; set; }
}