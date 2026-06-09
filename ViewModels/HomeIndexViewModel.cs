using shift_handover_page.Models;

namespace shift_handover_page.ViewModels;

public class HomeIndexViewModel
{
    public int OpenCount { get; set; }

    public int ClaimedCount { get; set; }

    public int ClosedCount { get; set; }

    public List<Shift> RecentShifts { get; set; } = new();
}