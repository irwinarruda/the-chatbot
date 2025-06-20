using Google.Apis.Tasks.v1;
using Microsoft.AspNetCore.Mvc;
using TheChatbot.Dtos;
using Google.Apis.Services;

namespace TheChatbot.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase {

  readonly IConfiguration configuration;

  public TestController(IConfiguration _configuration) {
    configuration = _configuration;
  }

  [HttpGet]
  [HttpGet("user")]
  public ActionResult GetUserTasks() {
    var taskListId = "test";
    return Ok(taskListId);
  }
}
