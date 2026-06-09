using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shift_handover_page.Data;
using shift_handover_page.Models;
using shift_handover_page.Services;
using shift_handover_page.ViewModels;

namespace shift_handover_page.Controllers;

[Authorize(Roles = "Supervisor")]
public class ShiftsController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;

    public ShiftsController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var currentUserId = _userManager.GetUserId(User);
        var shifts = await _dbContext.Shifts
            .Include(shift => shift.Logs)
            .OrderByDescending(shift => shift.ScheduledFor)
            .ToListAsync();

        ViewData["CurrentUserId"] = currentUserId;
        return View(shifts);
    }

    public async Task<IActionResult> Details(int id)
    {
        var shift = await LoadShiftAsync(id);
        if (shift is null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User);
        ViewData["CurrentUserId"] = currentUserId;
        ViewData["CanManage"] = CanManageShift(shift, currentUserId);
        ViewData["CanClaim"] = shift.Status == ShiftStatus.Open;
        ViewData["CanClose"] = CanManageShift(shift, currentUserId) && shift.Status != ShiftStatus.Closed;
        ViewData["CanReport"] = shift.Status == ShiftStatus.Closed;
        ViewData["StatusMessage"] = TempData["StatusMessage"];

        return View(shift);
    }

    public async Task<IActionResult> Log(int id)
    {
        var shift = await LoadShiftAsync(id);
        if (shift is null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User);
        if (!CanManageShift(shift, currentUserId))
        {
            return Forbid();
        }

        return View(new ShiftLogInputModel
        {
            ShiftId = shift.Id,
            OccurredAt = DateTime.Now
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Log(ShiftLogInputModel model)
    {
        var shift = await LoadShiftAsync(model.ShiftId);
        if (shift is null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User);
        if (!CanManageShift(shift, currentUserId))
        {
            return Forbid();
        }

        if (model.LogType == ShiftLogType.Manpower && !model.ManpowerCount.HasValue)
        {
            ModelState.AddModelError(nameof(model.ManpowerCount), "Manpower logs need a headcount.");
        }

        if (shift.Status == ShiftStatus.Closed)
        {
            ModelState.AddModelError(string.Empty, "Closed shifts cannot accept new logs.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUserName = _userManager.GetUserName(User) ?? User.Identity?.Name ?? "Supervisor";
        shift.Logs.Add(new ShiftLog
        {
            ShiftId = shift.Id,
            LogType = model.LogType,
            OccurredAt = model.OccurredAt,
            Title = model.Title,
            Description = model.Description,
            ManpowerCount = model.LogType == ShiftLogType.Manpower ? model.ManpowerCount : null,
            LoggedAt = DateTime.Now,
            LoggedByUserId = currentUserId,
            LoggedByUserName = currentUserName
        });

        await _dbContext.SaveChangesAsync();
        TempData["StatusMessage"] = "Log entry saved.";
        return RedirectToAction(nameof(Details), new { id = shift.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Claim(int id)
    {
        var shift = await _dbContext.Shifts.FirstOrDefaultAsync(item => item.Id == id);
        if (shift is null)
        {
            return NotFound();
        }

        if (shift.Status != ShiftStatus.Open)
        {
            TempData["StatusMessage"] = "That shift has already been claimed or closed.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var currentUserId = _userManager.GetUserId(User);
        var currentUserName = _userManager.GetUserName(User) ?? User.Identity?.Name ?? "Supervisor";

        shift.Status = ShiftStatus.Claimed;
        shift.ClaimedAt = DateTime.Now;
        shift.ClaimedByUserId = currentUserId;
        shift.ClaimedByUserName = currentUserName;

        await _dbContext.SaveChangesAsync();
        TempData["StatusMessage"] = "Shift claimed successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id, string? closureSummary)
    {
        var shift = await _dbContext.Shifts.FirstOrDefaultAsync(item => item.Id == id);
        if (shift is null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User);
        if (!CanManageShift(shift, currentUserId))
        {
            return Forbid();
        }

        if (shift.Status == ShiftStatus.Closed)
        {
            TempData["StatusMessage"] = "That shift is already closed.";
            return RedirectToAction(nameof(Details), new { id });
        }

        shift.Status = ShiftStatus.Closed;
        shift.ClosedAt = DateTime.Now;
        shift.ClosedByUserId = currentUserId;
        shift.ClosedByUserName = _userManager.GetUserName(User) ?? User.Identity?.Name ?? "Supervisor";
        shift.ClosureSummary = string.IsNullOrWhiteSpace(closureSummary)
            ? "Shift closed without additional notes."
            : closureSummary.Trim();

        await _dbContext.SaveChangesAsync();
        TempData["StatusMessage"] = "Shift closed and ready for PDF export.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Report(int id)
    {
        var shift = await LoadShiftAsync(id);
        if (shift is null)
        {
            return NotFound();
        }

        if (shift.Status != ShiftStatus.Closed)
        {
            TempData["StatusMessage"] = "Close the shift before generating the PDF report.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var pdf = ShiftReportDocument.Generate(shift);
        var fileName = $"shift-{shift.Id}-report.pdf";
        return File(pdf, "application/pdf", fileName);
    }

    private async Task<Shift?> LoadShiftAsync(int id)
    {
        return await _dbContext.Shifts
            .Include(shift => shift.Logs)
            .FirstOrDefaultAsync(shift => shift.Id == id);
    }

    private static bool CanManageShift(Shift shift, string? currentUserId)
    {
        return shift.Status == ShiftStatus.Claimed &&
               !string.IsNullOrWhiteSpace(currentUserId) &&
               string.Equals(shift.ClaimedByUserId, currentUserId, StringComparison.Ordinal);
    }
}