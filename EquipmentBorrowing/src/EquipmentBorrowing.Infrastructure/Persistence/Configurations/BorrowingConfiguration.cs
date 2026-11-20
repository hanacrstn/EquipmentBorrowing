using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class BorrowingConfiguration : IEntityTypeConfiguration<Borrowing>
{
    public void Configure(EntityTypeBuilder<Borrowing> builder)
    {
        builder.ToTable("Borrowings", table =>
        {
            table.HasCheckConstraint("CK_Borrowings_Due", """
                "ExpectedReturnDate" > "DateBorrowed"
                """);
            table.HasCheckConstraint("CK_Borrowings_Status", """
                ("Status" = 0 AND "ActualReturnDate" IS NULL) OR
                ("Status" = 1 AND "ActualReturnDate" IS NOT NULL AND "ActualReturnDate" >= "DateBorrowed")
                """);
        });
        builder.Property(b => b.ActualReturnDate);
        builder.HasIndex(b => b.EquipmentId).IsUnique()
            .HasFilter(""" "Status" = 0 """).HasDatabaseName("UX_Borrowings_ActiveItem");
        builder.HasIndex(b => new { b.StudentId, b.Status });
        builder.HasKey(b => b.Id);

        builder.Property(b => b.StudentId).IsRequired();
        builder.Property(b => b.EquipmentId).IsRequired();
        builder.Property(b => b.DateBorrowed).IsRequired();
        builder.Property(b => b.ExpectedReturnDate).IsRequired();


        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.HasIndex(b => b.Status);

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(b => b.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Equipment>()
            .WithMany()
            .HasForeignKey(b => b.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}