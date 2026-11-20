using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed partial class Lab4Repository : ILab4Repository
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> factory;

    public Lab4Repository(IDbContextFactory<EquipmentBorrowingDbContext> factory) => this.factory = factory;
    public Task<IReadOnlyList<StudentRow>> StudentsAsync() => ReadAsync<StudentRow>(async db =>
        await db.Students.AsNoTracking().OrderBy(s => s.Name).ThenBy(s => s.Id)
            .Select(s => new StudentRow(s.Id, s.Version, s.StudentNumber, s.Name,
                s.Contact, s.IsAllowedToBorrow)).ToListAsync());

    public Task<IReadOnlyList<ItemRow>> EquipmentAsync(bool availableOnly) => ReadAsync<ItemRow>(async db =>
    {
        var items = db.Equipment.AsNoTracking();
        if (availableOnly)
            items = items.Where(e => e.IsAvailable && !db.Borrowings.Any(b =>
                b.EquipmentId == e.Id && b.Status == BorrowingStatus.Active));

        return await items.OrderBy(e => e.Name).ThenBy(e => e.Id)
            .Select(e => new ItemRow(e.Id, e.Version, e.Identifier, e.Name,
                e.Description, e.IsAvailable)).ToListAsync();
    });

    private async Task<IReadOnlyList<T>> ReadAsync<T>(
        Func<EquipmentBorrowingDbContext, Task<IReadOnlyList<T>>> read)
    {
        try
        {
            await using var db = await factory.CreateDbContextAsync();
            return await read(db);
        }
        catch (Exception ex) when (ex is NpgsqlException or TimeoutException)
        {
            throw new Lab4Problem("Database load failed. Check the connection, then retry.");
        }
    }

    public async Task<string> WriteAsync(Func<ILab4WriteSession, Task<string>> action)
    {
        var committing = false;
        try
        {
            await using var db = await factory.CreateDbContextAsync();
            await using var tx = await db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted);

            // One transaction-scoped lock shared by all Lab 4 write routes.
            await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(8104, 1)");
            var message = await action(new Session(db));
            await db.SaveChangesAsync();

            committing = true;
            await tx.CommitAsync();
            return message;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new Lab4Problem("The record changed. Review the refreshed data.", refresh: true);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg)
        {
            throw DatabaseRejection(pg);
        }
        catch (PostgresException ex)
        {
            // A connection/shutdown error during commit may hide its acknowledgement.
            if (committing && (ex.SqlState.StartsWith("08", StringComparison.Ordinal) ||
                ex.SqlState is "57P01" or "57P02" or "57P03"))
                throw UnknownCommit();
            throw DatabaseRejection(ex);
        }
        catch (Exception ex) when (ex is NpgsqlException or TimeoutException or DbUpdateException)
        {
            if (committing)
                throw UnknownCommit();
            throw new Lab4Problem("Database write failed. Restore the connection, then refresh before retrying.",
                refresh: true);
        }
    }

    private static Lab4Problem UnknownCommit() => new(
        "Commit outcome is unknown. Reconnect and refresh. Review the records before deciding whether to retry.",
        unknown: true);

    private static Lab4Problem DatabaseRejection(PostgresException ex) => ex.ConstraintName switch
    {
        "UX_Students_Number" => new Lab4Problem("That student number already exists."),
        "UX_Equipment_Identifier" => new Lab4Problem("That equipment identifier already exists."),
        "UX_Borrowings_ActiveItem" => new Lab4Problem(
            "An item was already issued. Review the refreshed list.", refresh: true),
        _ => new Lab4Problem("PostgreSQL rejected the write. No request changes were saved.",
            refresh: true)
    };

    private sealed partial class Session : ILab4WriteSession
    {
        private readonly EquipmentBorrowingDbContext db;

        public Session(EquipmentBorrowingDbContext db) => this.db = db;

        public Task<Student?> StudentAsync(int id) =>
            db.Students.SingleOrDefaultAsync(s => s.Id == id);

        public Task<Equipment?> ItemAsync(int id) =>
            db.Equipment.SingleOrDefaultAsync(e => e.Id == id);

        public Task<bool> NumberTakenAsync(string number, int? except) =>
            db.Database.SqlQuery<bool>($"""
                SELECT EXISTS (
                    SELECT 1 FROM "Students"
                    WHERE "NormalizedNumber" = lower({number}) AND "Id" IS DISTINCT FROM {except}
                ) AS "Value"
                """).SingleAsync();

        public Task<bool> CodeTakenAsync(string code, int? except) =>
            db.Database.SqlQuery<bool>($"""
                SELECT EXISTS (
                    SELECT 1 FROM "Equipment"
                    WHERE "NormalizedIdentifier" = lower({code}) AND "Id" IS DISTINCT FROM {except}
                ) AS "Value"
                """).SingleAsync();

        public void AddStudent(Student student) => db.Students.Add(student);
        public void AddItem(Equipment item) => db.Equipment.Add(item);
    }
}