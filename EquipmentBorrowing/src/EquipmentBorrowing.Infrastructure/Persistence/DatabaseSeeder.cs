using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(EquipmentBorrowingDbContext context)
    {
        if (await context.Students.AnyAsync() || await context.Equipment.AnyAsync())
            return; // already seeded — never overwrite existing data

        context.Students.AddRange(
            new Student(1, "Juan Dela Cruz"),
            new Student(2, "Maria Santos", isAllowedToBorrow: false));

        context.Equipment.AddRange(
            new Equipment(100, "Digital Multimeter"),
            new Equipment(101, "Oscilloscope", isAvailable: false));

        await context.SaveChangesAsync();
    }
}