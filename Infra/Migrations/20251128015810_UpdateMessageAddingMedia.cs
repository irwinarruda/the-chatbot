using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheChatbot.Infra.Migrations;

/// <inheritdoc />
public partial class UpdateMessageAddingMedia : Migration {
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder) {
    migrationBuilder.AddColumn<string>(
      table: "messages",
      name: "media_url",
      type: "varchar(1000)",
      nullable: true
    );
    migrationBuilder.AddColumn<string>(
      table: "messages",
      name: "mime_type",
      type: "varchar(100)",
      nullable: true
    );
    migrationBuilder.AddColumn<string>(
      table: "messages",
      name: "transcript",
      type: "text",
      nullable: true
    );
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder) {
    migrationBuilder.DropColumn(
      table: "messages",
      name: "transcript"
    );
    migrationBuilder.DropColumn(
      table: "messages",
      name: "mime_type"
    );
    migrationBuilder.DropColumn(
      table: "messages",
      name: "media_url"
    );
  }
}
