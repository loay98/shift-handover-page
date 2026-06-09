using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using shift_handover_page.Models;

namespace shift_handover_page.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Shift> Shifts => Set<Shift>();

    public DbSet<ShiftLog> ShiftLogs => Set<ShiftLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Shift>(entity =>
        {
            entity.Property(x => x.Status).HasConversion<string>();
            entity.Property(x => x.Title).HasMaxLength(120);
            entity.Property(x => x.ClaimedByUserId).HasMaxLength(450);
            entity.Property(x => x.ClaimedByUserName).HasMaxLength(256);
            entity.Property(x => x.ClosedByUserId).HasMaxLength(450);
            entity.Property(x => x.ClosedByUserName).HasMaxLength(256);
            entity.Property(x => x.ClosureSummary).HasMaxLength(1000);

            entity.HasMany(x => x.Logs)
                .WithOne(x => x.Shift)
                .HasForeignKey(x => x.ShiftId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ShiftLog>(entity =>
        {
            entity.Property(x => x.LogType).HasConversion<string>();
            entity.Property(x => x.Title).HasMaxLength(120);
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.LoggedByUserId).HasMaxLength(450);
            entity.Property(x => x.LoggedByUserName).HasMaxLength(256);
        });
    }
}
