using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SettingModule.Presistence.Migrations
{
    /// <inheritdoc />
    public partial class addIsenable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "SettingDefinitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "SettingDefinitions");
        }
    }
}
