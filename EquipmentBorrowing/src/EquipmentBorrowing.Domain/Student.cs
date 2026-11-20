namespace EquipmentBorrowing.Domain;

public class Student
{
    private Student() { }

    public string StudentNumber { get; private set; } = null!;
    public string? Contact { get; private set; }
    public uint Version { get; private set; }

    public Student(string number, string name, string? contact)
    {
        ChangeDetails(number, name, contact);
        IsAllowedToBorrow = true;
    }

    public void ChangeDetails(string number, string name, string? contact)
    {
        StudentNumber = number;
        Name = name;
        Contact = contact;
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsAllowedToBorrow { get; private set; }

    public Student(int id, string name, bool isAllowedToBorrow = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Student name is required.", nameof(name));

        Id = id;
        StudentNumber = $"LEGACY-ST-{id}";
        Name = name;
        IsAllowedToBorrow = isAllowedToBorrow;
    }

    public void Suspend() => IsAllowedToBorrow = false;

    public void Reinstate() => IsAllowedToBorrow = true;
}