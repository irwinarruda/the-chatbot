---
applyTo: "**"
---

# Agent Guidelines

## Commands

- Build: `dotnet build`
- Test all: `make test-local` (Local), `make test-dev` (Dev), `make test-prev` (Preview). Most of the time you will be doing dev tests
- Single test: `dotnet test --filter "FullyQualifiedName=Namespace.TestClass.TestMethod"`
- Database: `make migrations-up` (apply), `make migrations-down` (revert), `make migrations-create name=Name` (create)
- Run locally: `make run-local` (starts API + ngrok)

## Code Style

**Note: The following patterns are specific to this project and may differ from other projects.**

- Indentation: 2 spaces
- Line endings: LF
- File encoding: UTF-8
- C# braces: Same line (no new line before braces)
- Imports: System directives first, separated groups
- Naming: PascalCase for types/methods, camelCase for parameters/variables
- Private properties/fields should never use underscore prefix (e.g., use `private string name` instead of `private string _name`)
- Use dependency injection and interfaces for services
- Handle exceptions using custom Infra.Exception class
- Return types must be explicitly declared
- Use dependency injection via constructor parameters
- Follow Resource/Service/Controller architecture pattern
- Do not add code comments to the files
- Always use short namespace declaration instead of the full with curly braces
- Prefer type inference: use `var` instead of explicit types (e.g., `var name = "value"` instead of `string name = "value"`)
- Prefer target-typed `new()` expressions: use `new() {}` instead of `new ClassName() {}` when the type can be inferred

## Database Migrations

### Naming Convention

Migrations must follow the `{Action}{WhatAction}` naming pattern:

- `CreateUsers` - Creating a new users table
- `CreateChatAndMessage` - Creating multiple related tables
- `UpdateMessage` - Updating an existing message table
- `UpdateChatWithDates` - Updating chat table with date columns
- `DeleteMessages` - Deleting messages table

### Creating a Migration

1. Run the migration creation command:

   ```bash
   make migrations-create name=CreateTableName
   ```

2. This generates two files in `Infra/Migrations/`:
   - `{timestamp}_{Name}.cs` - The migration file you edit
   - `{timestamp}_{Name}.Designer.cs` - Auto-generated designer file DO NOT EDIT IT

### Writing Migrations Manually

This project does NOT use entity tracking. All migrations must be written manually using `migrationBuilder`.

**Example: Creating a table**

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

namespace TheChatbot.Infra.Migrations;

public partial class CreateUsers : Migration {
  protected override void Up(MigrationBuilder migrationBuilder) {
    migrationBuilder.CreateTable(
      name: "users",
      schema: "public",
      columns: (table) => new {
        id = table.Column<Guid>(type: "uuid", defaultValueSql: "gen_random_uuid()"),
        name = table.Column<string>(type: "varchar(30)", nullable: false),
        phone_number = table.Column<string>(type: "varchar(20)", nullable: false),
        created_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "timezone('utc', now())"),
        updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "timezone('utc', now())"),
      },
      constraints: (table) => {
        table.PrimaryKey("PK_users", x => x.id);
        table.UniqueConstraint("UC_users_phone_number", x => x.phone_number);
      }
    );
  }

  protected override void Down(MigrationBuilder migrationBuilder) {
    migrationBuilder.DropTable(name: "users");
  }
}
```

**Example: Updating a table (adding columns)**

```csharp
public partial class UpdateMessage : Migration {
  protected override void Up(MigrationBuilder migrationBuilder) {
    migrationBuilder.AddColumn<string>(
      table: "messages",
      name: "button_reply",
      type: "varchar(10000)",
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

  protected override void Down(MigrationBuilder migrationBuilder) {
    migrationBuilder.DropIndex(name: "IX_messages_id_provider", table: "messages");
    migrationBuilder.DropColumn(table: "messages", name: "button_reply");
  }
}
```

### Applying Migrations

```bash
make migrations-up    # Apply pending migrations
make migrations-down  # Revert the last migration
```

## Configuration Management

### Adding New Configuration

When adding a new configuration, you must update three places:

#### Step 1: Create/Update Config Class in `Infra/`

Create a new file or edit an existing one in the `Infra/` directory:

```csharp
namespace TheChatbot.Infra;

public class NewServiceConfig {
  public string ApiKey { get; set; } = string.Empty;
  public string Endpoint { get; set; } = string.Empty;
  public int TimeoutSeconds { get; set; } = 30;
}
```

#### Step 2: Add to `appsettings.json` (root)

Add the configuration section with placeholder values:

```json
{
  "NewServiceConfig": {
    "ApiKey": "ApiKey",
    "Endpoint": "Endpoint",
    "TimeoutSeconds": 30
  }
}
```

#### Step 3: Add to `appsettings.Development.json`

Add the configuration section with development/test values:

```json
{
  "NewServiceConfig": {
    "ApiKey": "dev-api-key",
    "Endpoint": "https://dev.example.com",
    "TimeoutSeconds": 60
  }
}
```

#### Step 4: Inject in `Program.cs`

Add the configuration to the dependency injection container:

```csharp
builder.Services.AddSingleton(builder.Configuration.GetSection("NewServiceConfig").Get<NewServiceConfig>()!);
```

#### Step 5: Inject in `Tests/Orquestrator.cs`

Add the configuration to the test orchestrator:

```csharp
public class Orquestrator : WebApplicationFactory<Program> {
  readonly public NewServiceConfig newServiceConfig;

  public Orquestrator() {
    configuration = Configurable.Make();
    newServiceConfig = configuration.GetSection("NewServiceConfig").Get<NewServiceConfig>()!;

    var services = new ServiceCollection();
    services.AddSingleton(newServiceConfig);
  }
}
```
