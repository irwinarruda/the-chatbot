---
applyTo: "*.cs"
---

# AGENTS Instructions

This file aggregates agent-facing guidance for this repository. It is a concatenation of the instruction fragments in `.github/instructions/` with minor formatting to make the content easier to scan. The original instruction fragments and legacy `.cursor` rules have been removed to keep a single source of truth here.

## Contents

- [Architecture Documentation](#the-chatbot---architecture-documentation)
- [Agent Guidelines](#agent-guidelines)

---

# The Chatbot - Architecture Documentation

This document defines the architecture patterns and conventions for the project. Follow these guidelines strictly when implementing new features or modifying existing code.

## Architecture Overview

The application follows a layered architecture with clear separation of concerns:

```
Controller → Service → Entity
     ↓
  Resource (Gateway)
     ↓
  Infra (Database, External APIs)
```

### Layer Responsibilities

| Layer          | Folder         | Responsibility                                            |
| -------------- | -------------- | --------------------------------------------------------- |
| **Controller** | `Controllers/` | Handle HTTP requests/responses, route to services         |
| **Service**    | `Services/`    | Orchestrate business logic, manage data flow, execute SQL |
| **Entity**     | `Entities/`    | Domain objects with behavior and validation               |
| **Resource**   | `Resources/`   | External integrations via interfaces (Gateways)           |
| **Infra**      | `Infra/`       | Infrastructure concerns (DB, configs, exceptions)         |
| **Utils**      | `Utils/`       | Shared utilities (Mediator, loaders, configuration)       |

---

## Controllers

Controllers are thin and focused solely on HTTP concerns. They delegate all business logic to Services.

### Rules

1. Controllers must be annotated with `[ApiController]` and `[Route("/api/v1/[controller]")]`
2. Inject services via primary constructor
3. Never put business logic in controllers
4. Return appropriate HTTP status codes and content types
5. Use `[FromQuery]`, `[FromBody]`, `[FromRoute]` for parameter binding

### Examples

```csharp
[ApiController]
[Route("/api/v1/[controller]")]
public class StatusController(StatusService statusService) : ControllerBase {
  [HttpGet]
  public async Task<ActionResult> GetStatus() {
    var status = await statusService.GetStatus();
    return Ok(status);
  }
}
```

---

## Services

Services contain business logic and orchestrate operations. They are the **only layer** that interacts with the database.

### Rules

1. Inject dependencies via primary constructor
2. Services coordinate between Entities, Resources, and Database
3. All SQL operations happen in Services (never in Controllers or Entities)
4. Use private methods for SQL queries to keep public methods clean
5. Services can call other services when needed

### Examples

**Service with multiple dependencies:**

```csharp
public class MessagingService(
  AppDbContext database,
  AuthService authService,
  IMediator mediator,
  IWhatsAppMessagingGateway whatsAppMessagingGateway,
  IAiChatGateway aiChatGateway,
  IStorageGateway storageGateway,
  ISpeechToTextGateway speechToTextGateway
) {
  public async Task SendTextMessage(string phoneNumber, string text, Chat? chat = null) {
    chat ??= await GetChatByPhoneNumber(phoneNumber);
    if (chat == null) {
      throw new ValidationException(
        "The user does not have an open chat",
        "Please create a chat first before continuing"
      );
    }
    var message = chat.AddBotTextMessage(text);
    await CreateMessage(message);
    await whatsAppMessagingGateway.SendTextMessage(new() {
      To = phoneNumber,
      Text = text
    });
  }
}
```

**Service calling another service:**

```csharp
public class CashFlowService(AppDbContext database, AuthService authService, ICashFlowSpreadsheetGateway spreadsheetResource) {
  public async Task AddSpreadsheetUrl(string phoneNumber, string url) {
    var user = await authService.GetUserByPhoneNumber(phoneNumber) ?? throw new NotFoundException("User not found");
    var existing = await GetSpreadsheetByUserId(user.Id);
    if (existing != null) {
      throw new ValidationException("User already has a financial planning spreadsheet configured");
    }
    var sheetId = spreadsheetResource.GetSpreadsheetIdByUrl(url);
    await CreateCashFlowSpreadsheet(new() {
      IdUser = user.Id,
      IdSheet = sheetId,
      Type = CashFlowSpreadsheetType.Google,
    });
  }
}
```

---

## SQL Operations in Services

The `Infra/Database.cs` file provides two methods for database operations:

- `Query<TResult>(sql)` - For SELECT queries that return data
- `Execute(sql)` - For INSERT, UPDATE, DELETE operations

### Rules

1. All SQL operations must be done in the **Service layer**
2. SQL queries should preferably be implemented via **private methods**
3. Use interpolated strings with parameters for SQL injection protection
4. Define **DbRecord** types (records) inside Services for database mapping
5. Map DbRecords to Entities manually (no ORM mapping)

### DbRecord Pattern

Define private record types to map database rows. These are internal to the Service:

```csharp
public class MessagingService(AppDbContext database) {
  // ... public methods ...

  private record DbChat(
    Guid Id,
    Guid? IdUser,
    string Type,
    string PhoneNumber,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsDeleted
  );
}
```

### Examples

**Querying list with transformation:**

```csharp
public async Task<List<User>> GetUsers() {
  var dbUsers = await database.Query<DbUser>($"SELECT * FROM users").ToListAsync();
  var users = dbUsers.Select((u) => new User {
    Id = u.Id,
    Name = u.Name,
    PhoneNumber = u.PhoneNumber,
    IsInactive = u.IsInactive,
    CreatedAt = u.CreatedAt,
    UpdatedAt = u.UpdatedAt,
  }).ToList();
  return users;
}
```

**Private method pattern for complex queries:**

```csharp
public class CashFlowService(AppDbContext database) {
  public async Task<CashFlowSpreadsheet?> GetSheetForUser(Guid userId) {
    return await GetSpreadsheetByUserId(userId);
  }

  private async Task<CashFlowSpreadsheet?> GetSpreadsheetByUserId(Guid userId) {
    var dbEntity = await database.Query<DbCashFlowSpreadsheet>($@"
      SELECT * FROM cash_flow_spreadsheets
      WHERE id_user = {userId}
    ").FirstOrDefaultAsync();
    if (dbEntity == null) return null;
    return new CashFlowSpreadsheet {
      Id = dbEntity.Id,
      IdUser = dbEntity.IdUser,
      IdSheet = dbEntity.IdSheet,
      Type = Enum.Parse<CashFlowSpreadsheetType>(dbEntity.Type),
      CreatedAt = dbEntity.CreatedAt,
      UpdatedAt = dbEntity.UpdatedAt,
    };
  }

  private record DbCashFlowSpreadsheet(
    Guid Id,
    Guid IdUser,
    string IdSheet,
    string Type,
    DateTime CreatedAt,
    DateTime UpdatedAt
  );
}
```

---

## Entities (Domain Model)

Entities are **rich domain objects** with behavior. We follow DDD principles and avoid anemic domain models.

### Rules

1. Entities contain validation logic in constructors
2. Entities have methods that modify their own state
3. Related operations belong to the entity that owns the data
4. Use `DateTime.UtcNow.TruncateToMicroseconds()` for timestamps
5. Always generate `Guid.NewGuid()` for new IDs
6. Throw `ValidationException` for invalid states

### Examples

```csharp
public class Message {
  public Guid Id { get; set; }
  public string? MediaUrl { get; set; }
  public string? Transcript { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public Message(string transcript) {
    if (transcript == null) throw new ValidationException(
      "Transcript cannot be empty",
      "Please provide a valid transcript"
    );
    Id = Guid.NewGuid();
    CreatedAt = DateTime.UtcNow.TruncateToMicroseconds();
    UpdatedAt = DateTime.UtcNow.TruncateToMicroseconds();
  }
  public void AddAudioTranscript(string transcript) {
    Transcript = transcript;
    UpdatedAt = DateTime.UtcNow.TruncateToMicroseconds();
  }
}
```

---

## Resources (Gateway Pattern)

Resources are external integrations accessed through interfaces. This enables testing with mocks.

### Rules

1. Define interfaces in `Resources/` prefixed with `I` (e.g., `IWhatsAppMessagingGateway`)
2. Implement production gateways (e.g., `WhatsAppMessagingGateway`)
3. Implement test gateways prefixed with `Test` (e.g., `TestWhatsAppMessagingGateway`)
4. Define DTOs for input/output in the interface file
5. Register production implementations in `Program.cs`
6. Register test implementations in `Tests/Orquestrator.cs`

### Examples

**Interface with DTOs:**

```csharp
public class SendTextMessageDTO {
  public required string Text { get; set; }
  public required string To { get; set; }
}

public interface IWhatsAppMessagingGateway {
  Task SendTextMessage(SendTextMessageDTO textMessage);
}
```

## Exception Handling

Always use the custom exceptions live in `Infra/Exception.cs`. Use these for consistent error responses.

### Examples

**Throwing ValidationException:**

```csharp
if (name.Length >= 30) throw new ValidationException(
  "User name cannot have more than 29 characters",
  "Chose another name and continue"
);
```

---

## Mediator Pattern

The Mediator (`Utils/Mediator.cs`) enables loose coupling between services for cross-cutting events.

### Rules

1. Register handlers in `Program.cs` for production
2. Register handlers in `Tests/Orquestrator.cs` for tests
3. Use fire-and-forget (`_ = mediator.Send(...)`) for non-blocking events
4. Define event records near the service that dispatches them

### Examples

**Defining an event:**

```csharp
public record RespondToMessageEvent {
  public required Chat Chat;
  public required Message Message;
}
```

**Registering handlers in Program.cs:**

```csharp
mediator.Register("SaveUserByGoogleCredential", async (string phoneNumber) => {
  var messagingService = app.Services.GetRequiredService<MessagingService>();
  await messagingService.SendSignedInMessage(phoneNumber);
});
```

**Dispatching events (fire-and-forget):**

```csharp
_ = mediator.Send("RespondToMessage", new RespondToMessageEvent {
  Chat = chat,
  Message = message,
});
```

**Dispatching events (await):**

```csharp
await mediator.Send("SaveUserByGoogleCredential", user.PhoneNumber);
```

---

## Dependency Injection

All dependencies are registered in `Program.cs` with appropriate lifetimes.

### Lifetimes

| Lifetime    | Use Case                                         |
| ----------- | ------------------------------------------------ |
| `Singleton` | Stateless gateways, configs, services without DB |
| `Transient` | Services with database access, DbContext         |

---

## Testing

Tests use `xUnit` with a shared `Orquestrator` fixture that provides test implementations of all gateways.

### Rules

1. Tests inherit from `IClassFixture<Orquestrator>`
2. Use `orquestrator.ClearDatabase()` before tests that need clean state
3. Use test gateway implementations (prefixed with `Test`)
4. Use `Shouldly` for assertions

### Examples

**Test class setup:**

```csharp
public class AuthServiceTest : IClassFixture<Orquestrator> {
  private readonly Orquestrator orquestrator;
  private readonly AuthService authService;

  public AuthServiceTest(Orquestrator _orquestrator) {
    orquestrator = _orquestrator;
    authService = _orquestrator.authService;
  }
}
```

**Test with database cleanup:**

```csharp
[Fact]
public async Task CreateUser() {
  await orquestrator.ClearDatabase();
  var phoneNumber = "5511984444444";
  var user = new User("Irwin Arruda", phoneNumber);
  await authService.CreateUser(user);
  user.Name.ShouldBe("Irwin Arruda");
  user.PhoneNumber.ShouldBe(phoneNumber);
  user.GoogleCredential.ShouldBeNull();
}
```

**Test using orquestrator helpers:**

```csharp
[Fact]
public async Task GetUsers() {
  await orquestrator.ClearDatabase();
  var user = await orquestrator.CreateUser();
  var users = await authService.GetUsers();
  users.Count.ShouldBe(1);
  users[0].Id.ShouldBe(user.Id);
}
```
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
