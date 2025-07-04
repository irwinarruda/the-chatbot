using Shouldly;

using TheChatbot.Entities;
using TheChatbot.Infra;
using TheChatbot.Utils;

using Xunit.v3.Priority;

namespace Tests;

[TestCaseOrderer(typeof(PriorityOrderer))]
public class StatusApi : IClassFixture<CustomWebApplicationFactory> {
  private readonly HttpClient client;
  private readonly CustomWebApplicationFactory factory;

  public StatusApi(CustomWebApplicationFactory _factory) {
    factory = _factory;
    client = _factory.CreateClient();
  }

  [Fact]
  public async Task GetShouldWork() {
    using var result = await client.GetAsync("/api/v1/status", TestContext.Current.CancellationToken);
    result.IsSuccessStatusCode.ShouldBeTrue();
    var json = await result.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    var dto = Printable.Convert<Status>(json);

    dto.ShouldNotBeNull();
    var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
    dto.UpdatedAt.ToString("yyyy-MM-dd").ShouldBe(date);
    dto.Database.ServerVersion.ShouldBe(factory.databaseConfig.ServerVersion);
    dto.Database.MaxConnections.ShouldBeGreaterThan(0);
    dto.Database.OpenConnections.ShouldBe(1);
  }

  [Fact]
  public async Task OtherMethodsShouldNotWork() {
    using var result = await client.PutAsync("/api/v1/status", null, TestContext.Current.CancellationToken);
    var json = await result.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    result.IsSuccessStatusCode.ShouldBeFalse();
    var dto = Printable.Convert<ResponseException>(json);
    dto.ShouldNotBeNull();
    dto.Name.ShouldBe("MethodNotAllowedException");
    dto.StatusCode.ShouldBe(405);
  }
}
