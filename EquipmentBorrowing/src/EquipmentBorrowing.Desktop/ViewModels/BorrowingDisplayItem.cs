using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentBorrowing.Desktop.ViewModels
{
    public record BorrowingDisplayItem(
        Guid BorrowingId,
        int StudentId,
        string StudentName,
        int EquipmentId,
        string EquipmentName,
        DateOnly ExpectedReturnDate);
}
