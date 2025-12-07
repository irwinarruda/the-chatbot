using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheChatbot.Infra.Migrations {
  /// <inheritdoc />
  public partial class UpdateMessageWithMediaId : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.AddColumn<string>(
        table: "messages",
        name: "media_id",
        type: "varchar(1000)",
        nullable: true
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.DropColumn(
        table: "messages",
        name: "media_id"
      );
    }
  }
}
