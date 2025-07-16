using Microsoft.AspNetCore.Diagnostics;

using TheChatbot.Utils;

namespace TheChatbot.Infra;

public class Controller {
  public static async Task HandleInternal(StatusCodeContext context) {
    if (context.HttpContext.Response.StatusCode == 404) {
      context.HttpContext.Response.ContentType = "application/json";
      var notFoundEx = new NotFoundException(
        "The requested endpoint was not found.",
        "Make sure you are using the correct URL and HTTP method."
      );
      await context.HttpContext.Response.WriteAsync(Printable.Make(notFoundEx.ToResponseError()));
    }
    if (context.HttpContext.Response.StatusCode == 405) {
      context.HttpContext.Response.ContentType = "application/json";
      var methodNotAllowedEx = new MethodNotAllowedException(
        "The requested endpoint is not allowed.",
        "Make sure you are using the correct URL and HTTP method."
      );
      await context.HttpContext.Response.WriteAsync(Printable.Make(methodNotAllowedEx.ToResponseError()));
    }
  }
  public static async Task HandleException(HttpContext context) {
    ResponseException response;
    var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
    var ex = exceptionFeature?.Error;

    if (ex is ValidationException validationEx) {
      context.Response.StatusCode = validationEx.StatusCode;
      response = validationEx.ToResponseError();
    } else if (ex is NotFoundException notFoundEx) {
      context.Response.StatusCode = notFoundEx.StatusCode;
      response = notFoundEx.ToResponseError();
    } else {
      int? statusCode = null;
      if (ex is ServiceException serviceEx) {
        statusCode = serviceEx.StatusCode;
      } else if (ex is DeveloperException developerEx) {
        statusCode = developerEx.StatusCode;
      }
      var internalServer = new InternalServerException(ex, statusCode);
      context.Response.StatusCode = internalServer.StatusCode;
      response = internalServer.ToResponseError();
    }
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(Printable.Make(response));
  }
}
