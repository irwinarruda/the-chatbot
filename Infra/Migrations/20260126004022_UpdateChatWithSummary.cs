using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheChatbot.Infra.Migrations;

public partial class UpdateChatWithSummary : Migration {
  protected override void Up(MigrationBuilder migrationBuilder) {
    migrationBuilder.AddColumn<string>(
      table: "chats",
      name: "summary",
      type: "text",
      nullable: true
    );
    migrationBuilder.AddColumn<Guid>(
      table: "chats",
      name: "summarized_until_id",
      type: "uuid",
      nullable: true
    );
  }

  protected override void Down(MigrationBuilder migrationBuilder) {
    migrationBuilder.DropColumn(table: "chats", name: "summarized_until_id");
    migrationBuilder.DropColumn(table: "chats", name: "summary");
  }
}
