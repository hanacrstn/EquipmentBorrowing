using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed partial class Lab4Service
{
    public Task<IReadOnlyList<LoanRow>> LoansAsync(LoanCriteria criteria) =>
        repository.LoansAsync(criteria);

    public Task<string> IssueAsync(int? studentId, IReadOnlyList<int> itemIds,
        DateOnly? due, bool multiple)
    {
        if (studentId is null)
            throw new Lab4Problem("Select a student.");
        if ((!multiple && itemIds.Count != 1) || (multiple && itemIds.Count < 2))
            throw new Lab4Problem(multiple ? "Select at least two items." : "Select one item.");
        if (itemIds.Distinct().Count() != itemIds.Count)
            throw new Lab4Problem("Choose each equipment item only once.");

        return repository.WriteAsync(async session =>
        {
            var checkout = DateOnly.FromDateTime(DateTime.Today);
            if (due is null || due.Value <= checkout)
                throw new Lab4Problem("Expected return must be later than checkout.");

            var student = await session.StudentAsync(studentId.Value)
                ?? throw new Lab4Problem("Student is missing. Select again.", refresh: true);
            if (!student.IsAllowedToBorrow)
                throw new Lab4Problem("This student is restricted from new borrowing.", refresh: true);
            if (await session.ActiveCountAsync(student.Id) + itemIds.Count > 3)
                throw new Lab4Problem("The student may have at most three active items.", refresh: true);

            var items = new List<Equipment>();
            foreach (var id in itemIds)
            {
                var item = await session.ItemAsync(id)
                    ?? throw new Lab4Problem("An item is missing. Select again.", refresh: true);
                if (!item.IsAvailable || await session.HasActiveItemAsync(id))
                    throw new Lab4Problem($"{item.Identifier} is unavailable.", refresh: true);
                items.Add(item);
            }

            foreach (var item in items)
            {
                session.AddBorrowing(new Borrowing(Guid.NewGuid(), student.Id, item.Id,
                    checkout, due.Value));
                item.MarkAsBorrowed();
            }

            return $"Issued {items.Count} item(s): {string.Join(", ", items.Select(i => i.Identifier))}.";
        });
    }

    public Task<string> ReturnAsync(Guid? borrowingId)
    {
        if (borrowingId is null)
            throw new Lab4Problem("Select an active borrowing to return.");

        return repository.WriteAsync(async session =>
        {
            var borrowing = await session.BorrowingAsync(borrowingId.Value)
                ?? throw new Lab4Problem("The borrowing is missing. Select again.", refresh: true);
            if (borrowing.Status != BorrowingStatus.Active)
                throw new Lab4Problem("This borrowing was already returned.", refresh: true);
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (today < borrowing.DateBorrowed)
                throw new Lab4Problem("Today's date precedes checkout. Check the system clock.");

            var item = await session.ItemAsync(borrowing.EquipmentId)
                ?? throw new Lab4Problem("The related equipment is missing.", refresh: true);
            borrowing.MarkReturned(today);
            item.MarkAsAvailable();
            return $"Returned {item.Identifier}.";
        });
    }
}