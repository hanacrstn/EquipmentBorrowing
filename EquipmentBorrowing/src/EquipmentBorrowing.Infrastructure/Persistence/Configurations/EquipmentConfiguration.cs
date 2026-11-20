using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment", table => table.HasCheckConstraint("CK_Equipment_Required",
            """length(regexp_replace("Identifier", '[[:space:]]', '', 'g')) > 0 AND length(regexp_replace("Name", '[[:space:]]', '', 'g')) > 0"""));
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.Identifier).IsRequired();
        builder.Property(e => e.Version).IsRowVersion();
        builder.Property<string>("NormalizedIdentifier").HasComputedColumnSql(
            """lower(regexp_replace("Identifier", '^[[:space:]]+|[[:space:]]+$', '', 'g'))""", stored: true);
        builder.HasIndex("NormalizedIdentifier").IsUnique().HasDatabaseName("UX_Equipment_Identifier");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.IsAvailable).IsRequired();
    }
}