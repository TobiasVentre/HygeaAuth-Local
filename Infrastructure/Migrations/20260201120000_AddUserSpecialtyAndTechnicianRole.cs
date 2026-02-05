using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSpecialtyAndTechnicianRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Specialty",
                table: "Users",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE Users
                SET Role = 'Technician',
                    Specialty = COALESCE(NULLIF(LTRIM(RTRIM(Specialty)), ''), 'Fumigator')
                WHERE Role = 'Fumigator'
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Users
                SET Role = 'Fumigator'
                WHERE Role = 'Technician'
                  AND Specialty = 'Fumigator'
            ");

            migrationBuilder.DropColumn(
                name: "Specialty",
                table: "Users");
        }
    }
}
