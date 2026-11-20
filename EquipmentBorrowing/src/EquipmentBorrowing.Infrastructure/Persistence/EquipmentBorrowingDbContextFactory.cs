using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public sealed class EquipmentBorrowingDbContextFactory
    : IDesignTimeDbContextFactory<EquipmentBorrowingDbContext>
{
    public static void Configure(DbContextOptionsBuilder options)
    {
        var local = Environment.GetEnvironmentVariable("EQUIPMENT_BORROWING_DB");
        options.UseNpgsql(string.IsNullOrWhiteSpace(local)
            ? "Host=localhost;Database=equipment_borrowing_lab4;Username=lab4_app;Timeout=5"
            : local);
    }

    public EquipmentBorrowingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<EquipmentBorrowingDbContext>();
        Configure(options);
        return new EquipmentBorrowingDbContext(options.Options);
    }
}