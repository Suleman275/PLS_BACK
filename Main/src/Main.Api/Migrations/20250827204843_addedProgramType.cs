using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class addedProgramType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProgramTypeId",
                schema: "V2",
                table: "UniversityPrograms",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_UniversityPrograms_ProgramTypeId",
                schema: "V2",
                table: "UniversityPrograms",
                column: "ProgramTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_UniversityPrograms_ProgramTypes_ProgramTypeId",
                schema: "V2",
                table: "UniversityPrograms",
                column: "ProgramTypeId",
                principalSchema: "V2",
                principalTable: "ProgramTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UniversityPrograms_ProgramTypes_ProgramTypeId",
                schema: "V2",
                table: "UniversityPrograms");

            migrationBuilder.DropIndex(
                name: "IX_UniversityPrograms_ProgramTypeId",
                schema: "V2",
                table: "UniversityPrograms");

            migrationBuilder.DropColumn(
                name: "ProgramTypeId",
                schema: "V2",
                table: "UniversityPrograms");
        }
    }
}
