using Microsoft.AspNetCore.Mvc;

using TheChatbot.Utils;

namespace TheChatbot.Controllers;

[Route("/privacy")]
public class PrivacyController : ControllerBase {
  [HttpGet]
  public ContentResult GetPrivacyPolicy() {
    var template = PageLoader.GetPage(PageTemplate.PrivacyPolicy, locale: PageLocale.PtBr);
    return Content(template, "text/html");
  }
}

