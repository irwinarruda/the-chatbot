using Microsoft.AspNetCore.Mvc;

namespace TheChatbot.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase {

  readonly IConfiguration configuration;

  public TestController(IConfiguration _configuration) {
    configuration = _configuration;
  }

  [HttpGet]
  public ActionResult GetUserTasks() {
    var taskListId = "test";
    return Ok(taskListId);
  }
}
