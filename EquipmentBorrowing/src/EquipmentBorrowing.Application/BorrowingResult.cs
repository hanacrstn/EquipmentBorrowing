namespace EquipmentBorrowing.Application;

public class BorrowingResult
{
    public bool Success { get; }
    public string? Error { get; }
    public int? BorrowingId { get; }

    private BorrowingResult(bool success, string? error, int? borrowingId)
    {
        Success = success;
        Error = error;
        BorrowingId = borrowingId;
    }

    public static BorrowingResult Ok(int borrowingId) => new(true, null, borrowingId);

    public static BorrowingResult Fail(string error) => new(false, error, null);
}
