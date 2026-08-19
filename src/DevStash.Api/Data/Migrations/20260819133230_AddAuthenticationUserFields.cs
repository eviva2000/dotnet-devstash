using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevStash.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthenticationUserFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_application_users_normalized_email",
                schema: "devstash_dotnet",
                table: "application_users");

            migrationBuilder.AddColumn<string>(
                name: "display_name",
                schema: "devstash_dotnet",
                table: "application_users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ux_application_users_normalized_email",
                schema: "devstash_dotnet",
                table: "application_users",
                column: "normalized_email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_application_users_normalized_email",
                schema: "devstash_dotnet",
                table: "application_users");

            migrationBuilder.DropColumn(
                name: "display_name",
                schema: "devstash_dotnet",
                table: "application_users");

            migrationBuilder.CreateIndex(
                name: "ix_application_users_normalized_email",
                schema: "devstash_dotnet",
                table: "application_users",
                column: "normalized_email");
        }
    }
}
