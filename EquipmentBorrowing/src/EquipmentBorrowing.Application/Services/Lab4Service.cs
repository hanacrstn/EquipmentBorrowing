using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed partial class Lab4Service
{
    private readonly ILab4Repository repository;

    public Lab4Service(ILab4Repository repository) => this.repository = repository;
    public Task<IReadOnlyList<StudentRow>> StudentsAsync() => repository.StudentsAsync();
    public Task<IReadOnlyList<ItemRow>> EquipmentAsync(bool onlyAvailable = false) =>
        repository.EquipmentAsync(onlyAvailable);

    public Task<string> SaveStudentAsync(StudentRow? selected, string? numberInput,
        string? nameInput, string? contactInput)
    {
        var number = Required(numberInput, "Student number");
        var name = Required(nameInput, "Full name");
        if (selected is null && contactInput is { Length: > 0 } &&
            string.IsNullOrWhiteSpace(contactInput))
            throw new Lab4Problem("Enter a nonblank contact or leave it empty.");
        var contact = Optional(contactInput);

        return repository.WriteAsync(async session =>
        {
            var student = selected is null ? null : await CurrentStudentAsync(session, selected);
            if (await session.NumberTakenAsync(number, selected?.Id))
                throw new Lab4Problem("That student number already exists.");
            if (student is null)
                session.AddStudent(new Student(number, name, contact));
            else
                student.ChangeDetails(number, name, contact);

            return $"Saved student {number}.";
        });
    }

    public Task<string> SetEligibilityAsync(StudentRow? selected, bool allowed)
    {
        if (selected is null)
            throw new Lab4Problem("Select a student first.");

        return repository.WriteAsync(async session =>
        {
            var student = await CurrentStudentAsync(session, selected);
            if (student.IsAllowedToBorrow == allowed)
                return allowed ? "Student is already eligible." : "Student is already restricted.";
            if (allowed)
                student.Reinstate();
            else
                student.Suspend();
            return allowed ? "Student is now eligible." : "Student is now restricted.";
        });
    }

    public Task<string> SaveEquipmentAsync(ItemRow? selected, string? codeInput,
        string? nameInput, string? descriptionInput)
    {
        var code = Required(codeInput, "Equipment identifier");
        var name = Required(nameInput, "Equipment name");
        var description = Optional(descriptionInput);

        return repository.WriteAsync(async session =>
        {
            var item = selected is null ? null : await session.ItemAsync(selected.Id);
            if (selected is not null && (item is null || item.Version != selected.Version))
                throw new Lab4Problem("That item changed or is missing. Select it again.", refresh: true);
            if (await session.CodeTakenAsync(code, selected?.Id))
                throw new Lab4Problem("That equipment identifier already exists.");
            if (item is null)
                session.AddItem(new Equipment(code, name, description));
            else
                item.ChangeDetails(code, name, description);

            return $"Saved equipment {code}.";
        });
    }

    private static async Task<Student> CurrentStudentAsync(ILab4WriteSession session, StudentRow selected)
    {
        var student = await session.StudentAsync(selected.Id);
        if (student is null || student.Version != selected.Version)
            throw new Lab4Problem("That student changed or is missing. Select again.", refresh: true);
        return student;
    }

    private static string Required(string? input, string label) =>
        Optional(input) ?? throw new Lab4Problem($"{label} is required.");

    private static string? Optional(string? input) =>
        string.IsNullOrWhiteSpace(input) ? null : input.Trim();
}