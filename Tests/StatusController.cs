using Microsoft.Extensions.Configuration;

using Shouldly;

using TheChatbot.Infra;
using TheChatbot.Utils;

using Xunit.v3.Priority;

namespace Tests;

[TestCaseOrderer(typeof(PriorityOrderer))]
public class StatusController : IClassFixture<CustomWebApplicationFactory> {
  private readonly HttpClient client;
  private readonly DatabaseConfig databaseConfig;
  private record StatusDto(
    DateTime UpdatedAt,
    DatabaseDto Database
  );
  private record DatabaseDto(
    string ServerVersion,
    int MaxConnections,
    int OpenConnections
  );

  public StatusController(CustomWebApplicationFactory _factory) {
    databaseConfig = _factory.configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>()!;
    client = _factory.CreateClient();
  }

  [Fact]
  public async Task GetShouldWork() {
    var result = await client.GetAsync("/status", TestContext.Current.CancellationToken);
    result.IsSuccessStatusCode.ShouldBeTrue();
    var json = await result.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    var dto = Printable.Convert<StatusDto>(json);
    dto.ShouldNotBeNull();
    dto.Database.ServerVersion.ShouldBe(databaseConfig.ServerVersion);
    dto.Database.MaxConnections.ShouldBeGreaterThan(0);
    dto.Database.OpenConnections.ShouldBe(1);
  }

  [Fact]
  public async Task OtherMethodsShouldNotWork() {
    var result = await client.PutAsync("/status", null, TestContext.Current.CancellationToken);
    var json = await result.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    result.IsSuccessStatusCode.ShouldBeFalse();
    var dto = Printable.Convert<ResponseException>(json);
    dto.ShouldNotBeNull();
    dto.Name.ShouldBe("MethodNotAllowedException");
    dto.StatusCode.ShouldBe(405);
  }
}
