namespace EquipmentBorrowing.Domain;

public class Equipment
{
    private Equipment() { }

    public string Identifier { get; private set; } = null!;
    public string? Description { get; private set; }
    public uint Version { get; private set; }

    public Equipment(string identifier, string name, string? description)
    {
        ChangeDetails(identifier, name, description);
        IsAvailable = true;
    }

    public void ChangeDetails(string identifier, string name, string? description)
    {
        Identifier = identifier;
        Name = name;
        Description = description;
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsAvailable { get; private set; }

    public Equipment(int id, string name, bool isAvailable = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Equipment name is required.", nameof(name));

        Id = id;
        Identifier = $"LEGACY-EQ-{id}";
        Name = name;
        IsAvailable = isAvailable;
    }

    public void MarkAsBorrowed()
    {
        if (!IsAvailable)
            throw new InvalidOperationException($"Equipment '{Name}' is already borrowed.");

        IsAvailable = false;
    }

    public void MarkAsAvailable() => IsAvailable = true;
}