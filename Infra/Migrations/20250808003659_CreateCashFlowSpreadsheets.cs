using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheChatbot.Infra.Migrations;

/// <inheritdoc />
public partial class CreateCashFlowSpreadsheets : Migration {
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder) {
    migrationBuilder.CreateTable(
      name: "cash_flow_spreadsheets",
      schema: "public",
      columns: (table) => new {
        id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
        id_user = table.Column<Guid>(type: "uuid", nullable: false),
        id_sheet = table.Column<string>(type: "varchar(100)", nullable: false),
        type = table.Column<string>(type: "varchar(6)", nullable: false),
        created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "timezone('utc', now())"),
        updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "timezone('utc', now())"),
      },
      constraints: (table) => {
        table.PrimaryKey("PK_cash_flow_spreadsheets", x => x.id);
        table.ForeignKey(
          name: "FX_cash_flow_spreadsheets_id_user",
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
    migrationBuilder.DropTable("cash_flow_spreadsheets");
  }
}
