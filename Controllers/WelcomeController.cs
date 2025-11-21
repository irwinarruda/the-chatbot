using Microsoft.AspNetCore.Mvc;

using TheChatbot.Utils;

namespace TheChatbot.Controllers;

[Route("/")]
public class WelcomeController : ControllerBase {
  [HttpGet]
  public ContentResult GetWelcome() {
    var template = PageLoader.GetPage(PageTemplate.Welcome, locale: PageLocale.PtBr);
    return Content(template, "text/html");
  }
}

