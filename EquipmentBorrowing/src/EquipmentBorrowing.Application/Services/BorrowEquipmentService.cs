using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class BorrowEquipmentService
{
    private const int MaxActiveBorrowingsPerStudent = 3;

    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;

    public BorrowEquipmentService(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _borrowingRepository = borrowingRepository;
    }

    public async Task<BorrowingResult> ExecuteAsync(
        int studentId,
        int equipmentId,
        DateOnly dateBorrowed,
        DateOnly expectedReturnDate,
        CancellationToken cancellationToken = default)
    {
        // Rule 1: Does the student exist?
        var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
        if (student is null)
            return BorrowingResult.Fail("Student does not exist.");

        // Rule 2: Is the student allowed to borrow?
        if (!student.IsAllowedToBorrow)
            return BorrowingResult.Fail("Student is not currently allowed to borrow equipment.");

        // Rule 3: Does the equipment exist?
        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
        if (equipment is null)
            return BorrowingResult.Fail("Equipment does not exist.");

        // Rule 4: Is the equipment currently available?
        if (!equipment.IsAvailable)
            return BorrowingResult.Fail("Equipment is not currently available.");

        // Rule 5: Has the student reached the allowed number of active borrowings?
        var activeCount = await _borrowingRepository.CountActiveByStudentAsync(studentId, cancellationToken);
        if (activeCount >= MaxActiveBorrowingsPerStudent)
            return BorrowingResult.Fail("Student has reached the maximum number of active borrowings.");

        // Rule 6: All rules satisfied — create the borrowing record.
        equipment.MarkAsBorrowed();
        await _equipmentRepository.UpdateAsync(equipment, cancellationToken);

        var borrowingId = new Random().Next(1000, 9999); // simple id generation for this in-memory activity
        var borrowing = new Borrowing(borrowingId, studentId, equipmentId, dateBorrowed, expectedReturnDate);
        await _borrowingRepository.AddAsync(borrowing, cancellationToken);

        return BorrowingResult.Ok(borrowingId);
    }
}
