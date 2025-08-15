using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheChatbot.Infra.Migrations;

/// <inheritdoc />
public partial class UpdateMessage : Migration {
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder) {
    migrationBuilder.AddColumn<string>(
      table: "messages",
      name: "button_reply",
      type: "varchar(10000)",
      nullable: true
    );
    migrationBuilder.AddColumn<string>(
      table: "messages",
      name: "button_reply_options",
      type: "varchar(100)",
      nullable: true
    );
    migrationBuilder.AddColumn<string>(
      table: "messages",
      name: "type",
      type: "varchar(11)",
      nullable: false
    );
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder) {
    migrationBuilder.DropColumn(
      table: "messages",
      name: "type"
    );
    migrationBuilder.DropColumn(
      table: "messages",
      name: "button_reply_options"
    );
    migrationBuilder.DropColumn(
      table: "messages",
      name: "button_reply"
    );
  }
}
