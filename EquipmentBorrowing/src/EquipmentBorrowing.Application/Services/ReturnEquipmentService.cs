using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class ReturnEquipmentService
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    public ReturnEquipmentService(
        IBorrowingRepository borrowingRepository,
        IEquipmentRepository equipmentRepository)
    {
        _borrowingRepository = borrowingRepository;
        _equipmentRepository = equipmentRepository;
    }

    public async Task<ReturnResult> ExecuteAsync(
        int studentId,
        int equipmentId,
        CancellationToken cancellationToken = default)
    {
        var borrowing = await _borrowingRepository.GetActiveBorrowingAsync(
            studentId, equipmentId, cancellationToken);

        if (borrowing is null)
            return new ReturnResult(false, "No active borrowing found for this student and equipment.");

        borrowing.MarkReturned();
        await _borrowingRepository.UpdateAsync(borrowing, cancellationToken); // ← added

        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
        if (equipment is not null)
        {
            equipment.MarkAsAvailable();
            await _equipmentRepository.UpdateAsync(equipment, cancellationToken); // ← added
        }

        return new ReturnResult(true, null);
    }
}