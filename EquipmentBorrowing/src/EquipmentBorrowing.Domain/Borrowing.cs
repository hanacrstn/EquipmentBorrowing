using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentBorrowing.Domain;

public class Borrowing
{
    public int Id { get; }
    public int StudentId { get; }
    public int EquipmentId { get; }
    public DateOnly DateBorrowed { get; }
    public DateOnly ExpectedReturnDate { get; }
    public BorrowingStatus Status { get; private set; }

    public Borrowing(
        int id,
        int studentId,
        int equipmentId,
        DateOnly dateBorrowed,
        DateOnly expectedReturnDate)
    {
        if (expectedReturnDate < dateBorrowed)
            throw new ArgumentException("Expected return date cannot be before the borrow date.");

        Id = id;
        StudentId = studentId;
        EquipmentId = equipmentId;
        DateBorrowed = dateBorrowed;
        ExpectedReturnDate = expectedReturnDate;
        Status = BorrowingStatus.Active;
    }

    public void MarkReturned()
    {
        if (Status == BorrowingStatus.Returned)
            throw new InvalidOperationException("Borrowing is already marked as returned.");

        Status = BorrowingStatus.Returned;
    }
}