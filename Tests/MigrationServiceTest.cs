using Shouldly;

using TheChatbot.Infra;
using TheChatbot.Services;

namespace Tests;

public class MigrationServiceTest : IClassFixture<Orquestrator> {
  public Orquestrator orquestrator;
  public MigrationService migrationService;
  public AuthConfig authConfig;
  public MigrationServiceTest(Orquestrator _orquestrator) {
    orquestrator = _orquestrator;
    migrationService = _orquestrator.migrationService;
    authConfig = _orquestrator.authConfig;
  }

  [Fact]
  public async Task TestMigration() {
    await orquestrator.WipeDatabase();
    const int migrationCount = 10;
    var migrations = await migrationService.ListPendingMigrations();
    migrations.ShouldNotBeEmpty();
    migrations.Count.ShouldBe(migrationCount);
    await migrationService.RunPendingMigrations(authConfig.HashPassword);
    migrations = await migrationService.ListPendingMigrations();
    migrations.ShouldBeEmpty();
    await migrationService.ResetMigrations(authConfig.HashPassword);
    migrations = await migrationService.ListPendingMigrations();
    migrations.Count.ShouldBe(migrationCount);
  }

  [Fact]
  public async Task TestMigrationAuth() {
    await orquestrator.WipeDatabase();
    await Should.ThrowAsync<UnauthorizedException>(() => migrationService.RunPendingMigrations("WrongPassword"));
    await Should.ThrowAsync<UnauthorizedException>(() => migrationService.ResetMigrations("WrongPassword"));
  }
}
