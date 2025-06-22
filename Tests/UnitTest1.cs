namespace Tests;

public class UnitTest1 : IClassFixture<CustomWebApplicationFactory> {
  private readonly CustomWebApplicationFactory _factory;
  private readonly ITestOutputHelper _testOutputHelper;

  public UnitTest1(CustomWebApplicationFactory factory, ITestOutputHelper testOutputHelper) {
    _factory = factory;
    _testOutputHelper = testOutputHelper;
  }

  [Fact]
  public async Task Test_TestController() {
    var client = _factory.CreateClient();

    var response = await client.GetAsync("/test", TestContext.Current.CancellationToken);
    response.EnsureSuccessStatusCode();

    var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    _testOutputHelper.WriteLine(content);
    Assert.Contains("test", content);
  }

  [Fact]
  public async Task Test_Excel() {
    var client = _factory.CreateClient();
    var response = await client.GetAsync("/google/sheet", TestContext.Current.CancellationToken);
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    _testOutputHelper.WriteLine(content);
    Assert.Contains("test", content);
  }
}
