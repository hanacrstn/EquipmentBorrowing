using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students", table => table.HasCheckConstraint("CK_Students_Required",
            """length(regexp_replace("StudentNumber", '[[:space:]]', '', 'g')) > 0 AND length(regexp_replace("Name", '[[:space:]]', '', 'g')) > 0"""));
        builder.Property(s => s.Id).ValueGeneratedOnAdd();
        builder.Property(s => s.StudentNumber).IsRequired();
        builder.Property(s => s.Version).IsRowVersion();
        builder.Property<string>("NormalizedNumber").HasComputedColumnSql(
            """lower(regexp_replace("StudentNumber", '^[[:space:]]+|[[:space:]]+$', '', 'g'))""", stored: true);
        builder.HasIndex("NormalizedNumber").IsUnique().HasDatabaseName("UX_Students_Number");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired();
        builder.Property(s => s.IsAllowedToBorrow).IsRequired();
    }
}