using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed record StudentRow(int Id, uint Version, string Number, string Name,
    string? Contact, bool Allowed)
{
    public override string ToString() => $"{Number} · {Name} · {(Allowed ? "Eligible" : "Restricted")}";
}

public sealed record ItemRow(int Id, uint Version, string Code, string Name,
    string? Description, bool Available)
{
    public override string ToString() =>
        $"{Code} · {Name} · {(Available ? "Available" : "Unavailable")} · {Description}";
}

public sealed record LoanRow(Guid Id, string Student, string Number, string Item, string Code,
    DateOnly Checkout, DateOnly Due, BorrowingStatus Status, DateOnly? Returned)
{
    public override string ToString() =>
        $"{Number} {Student} | {Code} {Item} | {Checkout:yyyy-MM-dd} → {Due:yyyy-MM-dd}" +
        $" | {Status} | Returned: {Returned?.ToString("yyyy-MM-dd") ?? "—"}";
}

public enum LoanStatusFilter { All, Active, Returned }
public enum LoanSort { Checkout, Due }

public sealed record LoanCriteria(string? Text = null, LoanStatusFilter Status = LoanStatusFilter.All,
    LoanSort Sort = LoanSort.Checkout, bool Descending = true,
    int? StudentId = null, int? EquipmentId = null);

public sealed class Lab4Problem(string message, bool refresh = false, bool unknown = false)
    : Exception(message)
{
    public bool Refresh { get; } = refresh;
    public bool Unknown { get; } = unknown;
}

public interface ILab4Repository
{
    Task<IReadOnlyList<StudentRow>> StudentsAsync();
    Task<IReadOnlyList<ItemRow>> EquipmentAsync(bool availableOnly);
    Task<IReadOnlyList<LoanRow>> LoansAsync(LoanCriteria criteria);
    Task<string> WriteAsync(Func<ILab4WriteSession, Task<string>> action);
}

public interface ILab4WriteSession
{
    Task<Student?> StudentAsync(int id);
    Task<Equipment?> ItemAsync(int id);
    Task<Borrowing?> BorrowingAsync(Guid id);
    Task<bool> NumberTakenAsync(string number, int? except);
    Task<bool> CodeTakenAsync(string code, int? except);
    Task<int> ActiveCountAsync(int studentId);
    Task<bool> HasActiveItemAsync(int equipmentId);
    void AddStudent(Student student);
    void AddItem(Equipment item);
    void AddBorrowing(Borrowing borrowing);
}