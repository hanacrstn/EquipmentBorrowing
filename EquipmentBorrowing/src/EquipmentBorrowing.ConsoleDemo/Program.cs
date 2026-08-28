using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;

// --- Composition root: this is the ONLY place concrete classes are wired together ---
var studentRepository = new InMemoryStudentRepository();
var equipmentRepository = new InMemoryEquipmentRepository();
var borrowingRepository = new InMemoryBorrowingRepository();

studentRepository.Seed(new Student(id: 1, name: "Juan Dela Cruz", isAllowedToBorrow: true));
studentRepository.Seed(new Student(id: 2, name: "Maria Santos", isAllowedToBorrow: false));

equipmentRepository.Seed(new Equipment(id: 100, name: "Digital Multimeter"));
equipmentRepository.Seed(new Equipment(id: 101, name: "Oscilloscope", isAvailable: false));

var service = new BorrowEquipmentService(studentRepository, equipmentRepository, borrowingRepository);

var today = DateOnly.FromDateTime(DateTime.Today);
var dueDate = today.AddDays(7);

Console.WriteLine("=== Borrow Equipment ===");

Console.WriteLine("\n--- Case 1: Successful borrow ---");
var success = await service.ExecuteAsync(studentId: 1, equipmentId: 100, today, dueDate);
Console.WriteLine(success.Success
    ? $"Approved. Borrowing transaction #{success.BorrowingId} created."
    : $"Rejected: {success.Error}");

Console.WriteLine("\n--- Case 2: Failure - equipment unavailable ---");
var failUnavailable = await service.ExecuteAsync(studentId: 1, equipmentId: 101, today, dueDate);
Console.WriteLine(failUnavailable.Success
    ? $"Approved. Borrowing transaction #{failUnavailable.BorrowingId} created."
    : $"Rejected: {failUnavailable.Error}");

Console.WriteLine("\n--- Case 3: Failure - student not allowed to borrow ---");
var failNotAllowed = await service.ExecuteAsync(studentId: 2, equipmentId: 100, today, dueDate);
Console.WriteLine(failNotAllowed.Success
    ? $"Approved. Borrowing transaction #{failNotAllowed.BorrowingId} created."
    : $"Rejected: {failNotAllowed.Error}");

Console.WriteLine("\n--- Case 4: Failure - equipment does not exist ---");
var failNotFound = await service.ExecuteAsync(studentId: 1, equipmentId: 999, today, dueDate);
Console.WriteLine(failNotFound.Success
    ? $"Approved. Borrowing transaction #{failNotFound.BorrowingId} created."
    : $"Rejected: {failNotFound.Error}");