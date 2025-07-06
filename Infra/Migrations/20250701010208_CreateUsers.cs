using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheChatbot.Infra.Migrations {
  /// <inheritdoc />
  public partial class CreateUsers : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.CreateTable(
        name: "users",
        schema: "public",
        columns: (table) => new {
          id = table.Column<Guid>(type: "uuid", defaultValueSql: "gen_random_uuid()"),
          name = table.Column<string>(type: "varchar(30)", nullable: false),
          phone_number = table.Column<string>(type: "varchar(20)", nullable: false),
          is_inactive = table.Column<string>(type: "boolean", nullable: false, defaultValue: false),
          created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "timezone('utc', now())"),
          updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "timezone('utc', now())"),
        },
        constraints: (table) => {
          table.PrimaryKey("PK_users", x => x.id);
          table.UniqueConstraint("UC_users_phone_number", x => x.phone_number);
        }
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropTable(name: "users");
    }
  }
}
