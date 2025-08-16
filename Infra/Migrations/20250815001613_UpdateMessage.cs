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
    migrationBuilder.AddColumn<string>(
      name: "id_provider",
      table: "messages",
      type: "varchar(1000)",
      nullable: true
    );
    migrationBuilder.CreateIndex(
      name: "IX_messages_id_provider",
      table: "messages",
      column: "id_provider",
      unique: true,
      filter: "id_provider IS NOT NULL"
    );
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder) {
    migrationBuilder.DropIndex(
      name: "IX_messages_id_provider",
      table: "messages"
    );
    migrationBuilder.DropColumn(
      name: "id_provider",
      table: "messages"
    );
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
