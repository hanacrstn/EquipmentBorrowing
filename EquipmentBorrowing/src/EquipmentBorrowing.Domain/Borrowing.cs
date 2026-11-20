namespace EquipmentBorrowing.Domain;

public class Borrowing
{
    private Borrowing() { }

    public Guid Id { get; private set; }
    public int StudentId { get; private set; }
    public int EquipmentId { get; private set; }
    public DateOnly DateBorrowed { get; private set; }
    public DateOnly ExpectedReturnDate { get; private set; }
    public BorrowingStatus Status { get; private set; }

    public Borrowing(
        Guid id,
        int studentId,
        int equipmentId,
        DateOnly dateBorrowed,
        DateOnly expectedReturnDate)
    {
        if (expectedReturnDate <= dateBorrowed)
            throw new ArgumentException("Expected return must be later than checkout.");

        Id = id;
        StudentId = studentId;
        EquipmentId = equipmentId;
        DateBorrowed = dateBorrowed;
        ExpectedReturnDate = expectedReturnDate;
        Status = BorrowingStatus.Active;
    }

    public DateOnly? ActualReturnDate { get; private set; }

    public void MarkReturned() => MarkReturned(DateOnly.FromDateTime(DateTime.Today));

    public void MarkReturned(DateOnly date)
    {
        if (Status != BorrowingStatus.Active)
            throw new InvalidOperationException("This borrowing was already returned.");
        if (date < DateBorrowed)
            throw new ArgumentException("Actual return cannot precede checkout.", nameof(date));

        ActualReturnDate = date;
        Status = BorrowingStatus.Returned;
    }
}