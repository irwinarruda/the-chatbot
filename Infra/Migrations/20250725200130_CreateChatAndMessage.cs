using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheChatbot.Infra.Migrations;

/// <inheritdoc />
public partial class CreateChatAndMessage : Migration {
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder) {
    migrationBuilder.CreateTable(
      name: "chats",
      schema: "public",
      columns: (table) => new {
        id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
        id_user = table.Column<Guid>(type: "uuid", nullable: true),
        phone_number = table.Column<string>(type: "varchar(20)", nullable: false),
        type = table.Column<string>(type: "varchar(8)", nullable: false),
        created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "timezone('utc', now())"),
        updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "timezone('utc', now())"),
      },
      constraints: (table) => {
        table.PrimaryKey("PK_chats", x => x.id);
        table.ForeignKey(
          name: "FX_chats_id_user",
          column: x => x.id_user,
          principalTable: "users",
          principalColumn: "id",
          onDelete: ReferentialAction.SetNull
        );
      }
    );
    migrationBuilder.CreateTable(
      name: "messages",
      schema: "public",
      columns: (table) => new {
        id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
        id_chat = table.Column<Guid>(type: "uuid", nullable: false),
        user_type = table.Column<string>(type: "varchar(4)", nullable: false),
        text = table.Column<string>(type: "varchar(10000)", nullable: true),
        created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "timezone('utc', now())"),
        updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "timezone('utc', now())"),
      },
      constraints: (table) => {
        table.PrimaryKey("PK_messages", x => x.id);
        table.ForeignKey(
          name: "FX_messages_id_chat",
          column: x => x.id_chat,
          principalTable: "chats",
          principalColumn: "id",
          onDelete: ReferentialAction.SetNull
        );
      }
    );
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder) {
    migrationBuilder.DropTable("messages");
    migrationBuilder.DropTable("chats");
  }
}
