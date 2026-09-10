using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IBorrowingRepository
{
    Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default);

    Task<int> CountActiveByStudentAsync(int studentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Borrowing>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<Borrowing?> GetActiveBorrowingAsync(int studentId, int equipmentId, CancellationToken cancellationToken = default);
}
