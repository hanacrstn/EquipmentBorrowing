using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryBorrowingRepository : IBorrowingRepository
{
    private readonly List<Borrowing> _borrowings = new();

    public Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        _borrowings.Add(borrowing);
        return Task.CompletedTask;
    }

    public Task<int> CountActiveByStudentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var count = _borrowings.Count(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active);
        return Task.FromResult(count);
    }
    public Task<IReadOnlyList<Borrowing>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Borrowing> active = _borrowings
            .Where(b => b.Status == BorrowingStatus.Active)
            .ToList();

        return Task.FromResult(active);
    }
    public Task<Borrowing?> GetActiveBorrowingAsync(int studentId, int equipmentId, CancellationToken cancellationToken = default)
    {
        var borrowing = _borrowings.FirstOrDefault(b =>
            b.StudentId == studentId &&
            b.EquipmentId == equipmentId &&
            b.Status == BorrowingStatus.Active);

        return Task.FromResult(borrowing);
    }
}