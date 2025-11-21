using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheChatbot.Infra.Migrations;

/// <inheritdoc />
public partial class CreateAllowedNumbers : Migration {
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder) {
    migrationBuilder.CreateTable(
      name: "allowed_numbers",
      columns: table => new {
        id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
        phone_number = table.Column<string>(type: "varchar(20)", nullable: false),
        created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "timezone('utc', now())"),
      },
      constraints: table => {
        table.PrimaryKey("PK_allowed_numbers", x => x.id);
        table.UniqueConstraint("UC_allowed_numbers_phone_number", x => x.phone_number);
      }
    );
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder) {
    migrationBuilder.DropTable(name: "allowed_numbers");
  }
}

