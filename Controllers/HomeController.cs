using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shift_handover_page.Data;
using shift_handover_page.Models;
using shift_handover_page.ViewModels;

namespace shift_handover_page.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext dbContext, ILogger<HomeController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new HomeIndexViewModel
        {
            OpenCount = await _dbContext.Shifts.CountAsync(shift => shift.Status == ShiftStatus.Open),
            ClaimedCount = await _dbContext.Shifts.CountAsync(shift => shift.Status == ShiftStatus.Claimed),
            ClosedCount = await _dbContext.Shifts.CountAsync(shift => shift.Status == ShiftStatus.Closed),
            RecentShifts = await _dbContext.Shifts
                .OrderByDescending(shift => shift.ScheduledFor)
                .Take(6)
                .ToListAsync()
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
