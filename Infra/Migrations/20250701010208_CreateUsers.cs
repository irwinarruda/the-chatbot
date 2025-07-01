using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheChatbot.Infra.Migrations {
  /// <inheritdoc />
  public partial class CreateUsers : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.CreateTable(
        name: "users",
        columns: (table) => new {
          Id = table.Column<Guid>(type: "uuid", nullable: false),
          Name = table.Column<string>(type: "varchar(30)", nullable: false),
        },
        constraints: (table) => {
          table.PrimaryKey("PK_users", x => x.Id);
        }
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropTable(name: "users");
    }
  }
}
