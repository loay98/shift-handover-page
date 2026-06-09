using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using shift_handover_page.Data;
using shift_handover_page.Models;

namespace shift_handover_page.Services;

public static class ShiftSeeder
{
    private const string SupervisorRole = "Supervisor";

    private static readonly (string UserName, string Email, string Password)[] DemoUsers =
    [
        ("supervisor1", "supervisor1@demo.local", "Demo123!"),
        ("supervisor2", "supervisor2@demo.local", "Demo123!")
    ];

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        if (!await roleManager.RoleExistsAsync(SupervisorRole))
        {
            await roleManager.CreateAsync(new IdentityRole(SupervisorRole));
        }

        foreach (var userSeed in DemoUsers)
        {
            var user = await userManager.FindByNameAsync(userSeed.UserName);
            if (user is null)
            {
                user = new IdentityUser
                {
                    UserName = userSeed.UserName,
                    Email = userSeed.Email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, userSeed.Password);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(string.Join(", ", createResult.Errors.Select(error => error.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(user, SupervisorRole))
            {
                await userManager.AddToRoleAsync(user, SupervisorRole);
            }
        }

        if (await dbContext.Shifts.AnyAsync())
        {
            return;
        }

        var supervisorOne = await userManager.FindByNameAsync("supervisor1");
        var supervisorTwo = await userManager.FindByNameAsync("supervisor2");

        var shifts = new List<Shift>
        {
            new()
            {
                Title = "Morning Handover - Packaging Line",
                ScheduledFor = DateTime.Today.AddHours(6),
                Status = ShiftStatus.Open
            },
            new()
            {
                Title = "Afternoon Handover - Warehouse",
                ScheduledFor = DateTime.Today.AddHours(14),
                Status = ShiftStatus.Claimed,
                ClaimedAt = DateTime.Now.AddHours(-2),
                ClaimedByUserId = supervisorOne?.Id,
                ClaimedByUserName = supervisorOne?.UserName
            },
            new()
            {
                Title = "Night Handover - Dispatch",
                ScheduledFor = DateTime.Today.AddHours(22),
                Status = ShiftStatus.Closed,
                ClaimedAt = DateTime.Now.AddHours(-9),
                ClaimedByUserId = supervisorTwo?.Id,
                ClaimedByUserName = supervisorTwo?.UserName,
                ClosedAt = DateTime.Now.AddHours(-1),
                ClosedByUserId = supervisorTwo?.Id,
                ClosedByUserName = supervisorTwo?.UserName,
                ClosureSummary = "No outstanding issues. Shift closed cleanly after final manpower check."
            }
        };

        shifts[2].Logs.AddRange(
        [
            new ShiftLog
            {
                LogType = ShiftLogType.Incident,
                OccurredAt = DateTime.Now.AddHours(-6),
                Title = "Forklift near miss",
                Description = "A forklift reversed into a staging lane without a spotter. No injuries and the area was cleared immediately.",
                LoggedAt = DateTime.Now.AddHours(-6),
                LoggedByUserId = supervisorTwo?.Id,
                LoggedByUserName = supervisorTwo?.UserName
            },
            new ShiftLog
            {
                LogType = ShiftLogType.Manpower,
                OccurredAt = DateTime.Now.AddHours(-3),
                Title = "Headcount confirmation",
                Description = "Full crew present for the final loading window.",
                ManpowerCount = 18,
                LoggedAt = DateTime.Now.AddHours(-3),
                LoggedByUserId = supervisorTwo?.Id,
                LoggedByUserName = supervisorTwo?.UserName
            },
            new ShiftLog
            {
                LogType = ShiftLogType.Accident,
                OccurredAt = DateTime.Now.AddHours(-2),
                Title = "Minor slip in loading bay",
                Description = "Operator slipped on a wet patch and reported a minor bruise. First aid was provided and housekeeping isolated the spill.",
                LoggedAt = DateTime.Now.AddHours(-2),
                LoggedByUserId = supervisorTwo?.Id,
                LoggedByUserName = supervisorTwo?.UserName
            }
        ]);

        dbContext.Shifts.AddRange(shifts);
        await dbContext.SaveChangesAsync();
    }
}