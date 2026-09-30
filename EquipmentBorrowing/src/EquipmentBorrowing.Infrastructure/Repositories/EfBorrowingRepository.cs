using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfBorrowingRepository : IBorrowingRepository
{
    private readonly EquipmentBorrowingDbContext _context;

    public EfBorrowingRepository(EquipmentBorrowingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        _context.Borrowings.Add(borrowing);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountActiveByStudentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Borrowings
            .AsNoTracking()
            .CountAsync(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active, cancellationToken);
    }

    public async Task<Borrowing?> GetActiveBorrowingAsync(
        int studentId, int equipmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Borrowings.FirstOrDefaultAsync(
            b => b.StudentId == studentId && b.EquipmentId == equipmentId && b.Status == BorrowingStatus.Active,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Borrowings
            .AsNoTracking()
            .Where(b => b.Status == BorrowingStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}