using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public class DemonstrationQueries
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _contextFactory;

    public DemonstrationQueries(IDbContextFactory<EquipmentBorrowingDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    // Query 1 — Available Equipment
    public async Task<List<Equipment>> GetAvailableEquipmentAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Equipment.AsNoTracking().Where(e => e.IsAvailable).ToListAsync();
    }

    // Query 2 — Active Borrowings with related student and equipment info
    public async Task<List<object>> GetActiveBorrowingsWithDetailsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await (from b in context.Borrowings.AsNoTracking()
                      join s in context.Students on b.StudentId equals s.Id
                      join e in context.Equipment on b.EquipmentId equals e.Id
                      where b.Status == BorrowingStatus.Active
                      select new { s.Name, EquipmentName = e.Name, b.DateBorrowed, b.ExpectedReturnDate } as object)
                      .ToListAsync();
    }

    // Query 3 — Borrowings due before a specified date
    public async Task<List<Borrowing>> GetBorrowingsDueBeforeAsync(DateOnly cutoff)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Borrowings.AsNoTracking()
            .Where(b => b.Status == BorrowingStatus.Active && b.ExpectedReturnDate < cutoff)
            .ToListAsync();
    }
}