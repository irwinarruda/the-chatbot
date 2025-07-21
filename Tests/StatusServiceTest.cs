using Shouldly;

using TheChatbot.Services;

namespace Tests;

public class StatusServiceTest : IClassFixture<Orquestrator> {
  private readonly Orquestrator orquestrator;
  private readonly StatusService statusService;
  public StatusServiceTest(Orquestrator _orquestrator) {
    orquestrator = _orquestrator;
    statusService = _orquestrator.statusService;
  }

  [Fact]
  public async Task GetShouldWork() {
    var dto = await statusService.GetStatus();
    dto.ShouldNotBeNull();
    var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
    dto.UpdatedAt.ToString("yyyy-MM-dd").ShouldBe(date);
    dto.Database.ServerVersion.ShouldBe(orquestrator.databaseConfig.ServerVersion);
    dto.Database.MaxConnections.ShouldBeGreaterThan(0);
    dto.Database.OpenConnections.ShouldBeGreaterThanOrEqualTo(1);
  }
}
