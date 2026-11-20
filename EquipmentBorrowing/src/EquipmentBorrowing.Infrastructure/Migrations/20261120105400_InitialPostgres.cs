using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EquipmentBorrowing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    NormalizedIdentifier = table.Column<string>(type: "text", nullable: true, computedColumnSql: "lower(regexp_replace(\"Identifier\", '^[[:space:]]+|[[:space:]]+$', '', 'g'))", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                    table.CheckConstraint("CK_Equipment_Required", "length(regexp_replace(\"Identifier\", '[[:space:]]', '', 'g')) > 0 AND length(regexp_replace(\"Name\", '[[:space:]]', '', 'g')) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentNumber = table.Column<string>(type: "text", nullable: false),
                    Contact = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsAllowedToBorrow = table.Column<bool>(type: "boolean", nullable: false),
                    NormalizedNumber = table.Column<string>(type: "text", nullable: true, computedColumnSql: "lower(regexp_replace(\"StudentNumber\", '^[[:space:]]+|[[:space:]]+$', '', 'g'))", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.CheckConstraint("CK_Students_Required", "length(regexp_replace(\"StudentNumber\", '[[:space:]]', '', 'g')) > 0 AND length(regexp_replace(\"Name\", '[[:space:]]', '', 'g')) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Borrowings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    EquipmentId = table.Column<int>(type: "integer", nullable: false),
                    DateBorrowed = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpectedReturnDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ActualReturnDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Borrowings", x => x.Id);
                    table.CheckConstraint("CK_Borrowings_Due", "\"ExpectedReturnDate\" > \"DateBorrowed\"");
                    table.CheckConstraint("CK_Borrowings_Status", "(\"Status\" = 0 AND \"ActualReturnDate\" IS NULL) OR\n(\"Status\" = 1 AND \"ActualReturnDate\" IS NOT NULL AND \"ActualReturnDate\" >= \"DateBorrowed\")");
                    table.ForeignKey(
                        name: "FK_Borrowings_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Borrowings_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Borrowings_Status",
                table: "Borrowings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Borrowings_StudentId_Status",
                table: "Borrowings",
                columns: new[] { "StudentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "UX_Borrowings_ActiveItem",
                table: "Borrowings",
                column: "EquipmentId",
                unique: true,
                filter: " \"Status\" = 0 ");

            migrationBuilder.CreateIndex(
                name: "UX_Equipment_Identifier",
                table: "Equipment",
                column: "NormalizedIdentifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Students_Number",
                table: "Students",
                column: "NormalizedNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Borrowings");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
