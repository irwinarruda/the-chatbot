using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TheChatbot.Infra;

namespace TheChatbot.Controllers;


public class Status {
  public DateTime UpdatedAt { get; set; }
  public required DatabaseStatus Database { get; set; }
}

public class DatabaseStatus {
  public required string ServerVersion { get; set; }
}

[ApiController]
[Route("[controller]")]
public class StatusController : ControllerBase {
  private readonly AppDbContext context;
  public StatusController(AppDbContext _context) {
    context = _context;
  }

  [HttpGet]
  public async Task<ActionResult> Get() {
    try {
      var query = (FormattableString)$"SHOW server_version;";
      var version = await context.Database.SqlQuery<string>(query).ToListAsync();
      var status = new Status {
        UpdatedAt = DateTime.UtcNow,
        Database = new DatabaseStatus {
          ServerVersion = version[0],
        }
      };
      return Ok(status);
    } catch (Exception ex) {
      return Ok(ex.Message);
    }
  }
}
