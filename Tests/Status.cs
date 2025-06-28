using Newtonsoft.Json;

using Microsoft.Extensions.Configuration;

using Shouldly;

using TheChatbot.Infra;
using TheChatbot.Utils;

using Xunit.v3.Priority;

namespace Tests;

[TestCaseOrderer(typeof(PriorityOrderer))]
public class Status : IClassFixture<CustomWebApplicationFactory> {
  private readonly HttpClient client;
  private readonly DatabaseConfig databaseConfig;
  private record StatusDto(
    DateTime UpdatedAt,
    DatabaseDto Database
  );
  private record DatabaseDto(
     string ServerVersion
  );

  public Status(CustomWebApplicationFactory _factory) {
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
  }
}
