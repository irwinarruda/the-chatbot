using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheChatbot.Infra.Migrations;

/// <inheritdoc />
public partial class UpdateChatDelete : Migration {
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder) {
    migrationBuilder.AddColumn<bool>(
      table: "chats",
      name: "is_deleted",
      type: "boolean",
      defaultValue: false,
      nullable: false
    );
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder) {
    migrationBuilder.DropColumn(
      table: "chats",
      name: "is_deleted"
    );
  }
}
