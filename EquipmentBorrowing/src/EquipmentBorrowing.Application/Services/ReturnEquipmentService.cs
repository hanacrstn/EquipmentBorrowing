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

        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
        if (equipment is not null)
            equipment.MarkAsAvailable();

        return new ReturnResult(true, null);
    }
}