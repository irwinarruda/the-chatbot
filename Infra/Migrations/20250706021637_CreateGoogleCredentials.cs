using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheChatbot.Infra.Migrations;

/// <inheritdoc />
public partial class CreateGoogleCredentials : Migration {
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder) {
    migrationBuilder.CreateTable(
      name: "google_credentials",
      schema: "public",
      columns: (table) => new {
        id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
        id_user = table.Column<Guid>(type: "uuid", nullable: false),
        access_token = table.Column<string>(type: "varchar(200)", nullable: false),
        refresh_token = table.Column<string>(type: "varchar(200)", nullable: false),
        expires_in_seconds = table.Column<long>(type: "bigint", nullable: true),
        created_at = table.Column<DateTime>(type: "timestamptz", defaultValueSql: "timezone('utc', now())"),
        updated_at = table.Column<DateTime>(type: "timestamptz", defaultValueSql: "timezone('utc', now())"),
      },
      constraints: (table) => {
        table.PrimaryKey("PK_google_credentials", x => x.id);
        table.ForeignKey(
          name: "FX_google_credentials_users_id_user",
          column: x => x.id_user,
          principalTable: "users",
          principalColumn: "id",
          onDelete: ReferentialAction.Cascade
        );
      }
    );
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder) {
    migrationBuilder.DropTable(name: "google_credentials");
  }
}
