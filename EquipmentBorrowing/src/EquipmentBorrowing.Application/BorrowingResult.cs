namespace EquipmentBorrowing.Application;

public class BorrowingResult
{
    public bool Success { get; }
    public string? Error { get; }
    public Guid? BorrowingId { get; }

    private BorrowingResult(bool success, string? error, Guid? borrowingId)
    {
        Success = success;
        Error = error;
        BorrowingId = borrowingId;
    }

    public static BorrowingResult Ok(Guid borrowingId) => new(true, null, borrowingId);

    public static BorrowingResult Fail(string error) => new(false, error, null);
}