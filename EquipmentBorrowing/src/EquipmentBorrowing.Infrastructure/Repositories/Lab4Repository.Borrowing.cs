using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed partial class Lab4Repository
{
	public Task<IReadOnlyList<LoanRow>> LoansAsync(LoanCriteria criteria) =>
		ReadAsync<LoanRow>(async db =>
		{
			if (criteria.StudentId is int studentId &&
				!await db.Students.AnyAsync(s => s.Id == studentId))
				throw new Lab4Problem("Student no longer exists. Refresh and reselect.");

			if (criteria.EquipmentId is int equipmentId &&
				!await db.Equipment.AnyAsync(e => e.Id == equipmentId))
				throw new Lab4Problem("Equipment no longer exists. Refresh and reselect.");

			var query = from loan in db.Borrowings.AsNoTracking()
						join student in db.Students.AsNoTracking() on loan.StudentId equals student.Id
						join item in db.Equipment.AsNoTracking() on loan.EquipmentId equals item.Id
						select new { Loan = loan, Student = student, Item = item };

			if (criteria.StudentId is int selectedStudent)
				query = query.Where(r => r.Loan.StudentId == selectedStudent);
			if (criteria.EquipmentId is int selectedItem)
				query = query.Where(r => r.Loan.EquipmentId == selectedItem);
			if (criteria.Status == LoanStatusFilter.Active)
				query = query.Where(r => r.Loan.Status == BorrowingStatus.Active);
			if (criteria.Status == LoanStatusFilter.Returned)
				query = query.Where(r => r.Loan.Status == BorrowingStatus.Returned);

			if (!string.IsNullOrWhiteSpace(criteria.Text))
			{
				var text = criteria.Text.Trim().Replace("!", "!!").Replace("%", "!%").Replace("_", "!_");
				var pattern = $"%{text}%";
				query = query.Where(r => EF.Functions.ILike(r.Student.Name, pattern, "!") ||
					EF.Functions.ILike(r.Student.StudentNumber, pattern, "!") ||
					EF.Functions.ILike(r.Item.Name, pattern, "!"));
			}

			var ordered = (criteria.Sort, criteria.Descending) switch
			{
				(LoanSort.Due, true) => query.OrderByDescending(r => r.Loan.ExpectedReturnDate),
				(LoanSort.Due, false) => query.OrderBy(r => r.Loan.ExpectedReturnDate),
				(_, true) => query.OrderByDescending(r => r.Loan.DateBorrowed),
				_ => query.OrderBy(r => r.Loan.DateBorrowed)
			};

			return await ordered.ThenBy(r => r.Loan.Id)
				.Select(r => new LoanRow(r.Loan.Id, r.Student.Name, r.Student.StudentNumber,
					r.Item.Name, r.Item.Identifier, r.Loan.DateBorrowed, r.Loan.ExpectedReturnDate,
					r.Loan.Status, r.Loan.ActualReturnDate)).ToListAsync();
		});

	private sealed partial class Session
	{
		public Task<Borrowing?> BorrowingAsync(Guid id) =>
			db.Borrowings.SingleOrDefaultAsync(b => b.Id == id);

		public Task<int> ActiveCountAsync(int studentId) =>
			db.Borrowings.CountAsync(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active);

		public Task<bool> HasActiveItemAsync(int equipmentId) =>
			db.Borrowings.AnyAsync(b => b.EquipmentId == equipmentId && b.Status == BorrowingStatus.Active);

		public void AddBorrowing(Borrowing borrowing) => db.Borrowings.Add(borrowing);
	}
}