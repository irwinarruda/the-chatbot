namespace TheChatbot.Infra;

public record ResponseException(
  string Message,
  string Action,
  string Name,
  int StatusCode
);

public class ValidationException : Exception {
  public string Action { get; set; }
  public string Name { get; set; }
  public int StatusCode { get; set; }
  public ValidationException(
    string? message = null,
    string? action = null
  ) : base(message ?? "A validation error occurred.") {
    Name = "ValidationException";
    Action = action ?? "Adjust the provided data and try again.";
    StatusCode = 400;
  }
  public ResponseException ToResponseError() {
    return new ResponseException(
      Message: Message,
      Action: Action,
      Name: Name,
      StatusCode: StatusCode
    );
  }
}

public class ServiceException : Exception {
  public string Action { get; set; }
  public string Name { get; set; }
  public int StatusCode { get; set; }
  public ServiceException(
    Exception? cause = null,
    string? message = null
  ) : base(message ?? "Service is currently unavailable.", cause) {
    Name = "ServiceException";
    Action = "Check if the service is available and try again.";
    StatusCode = 503;
  }
  public ResponseException ToResponseError() {
    return new ResponseException(
      Message: Message,
      Action: Action,
      Name: Name,
      StatusCode: StatusCode
    );
  }
}

public class NotFoundException : Exception {
  public string Action { get; set; }
  public string Name { get; set; }
  public int StatusCode { get; set; }

  public NotFoundException(
    string? message = null,
    string? action = null
  ) : base(message ?? "The resource was not found") {
    Name = "NotFoundException";
    Action = action ?? "Change the filters and try again";
    StatusCode = 404;
  }
  public ResponseException ToResponseError() {
    return new ResponseException(
      Message: Message,
      Action: Action,
      Name: Name,
      StatusCode: StatusCode
    );
  }
}
public class MethodNotAllowedException : Exception {
  public string Action { get; set; }
  public string Name { get; set; }
  public int StatusCode { get; set; }

  public MethodNotAllowedException(
    string? message = null,
    string? action = null
  ) : base(message ?? "The method is not allowed.") {
    Name = "MethodNotAllowedException";
    Action = action ?? "Check the HTTP method for this endpoint.";
    StatusCode = 405;
  }
  public ResponseException ToResponseError() {
    return new ResponseException(
      Message: Message,
      Action: Action,
      Name: Name,
      StatusCode: StatusCode
    );
  }
}

public class InternalServerException : Exception {
  public string Action { get; set; }
  public string Name { get; set; }
  public int StatusCode { get; set; }
  public InternalServerException(
    Exception? cause = null,
    int? statusCode = null
  ) : base("An unexpected error occurred.", cause) {
    Name = "InternalServerException";
    Action = "Please contact our support team for assistance.";
    StatusCode = statusCode ?? 500;
  }
  public ResponseException ToResponseError() {
    return new ResponseException(
      Message: Message,
      Action: Action,
      Name: Name,
      StatusCode: StatusCode
    );
  }
}

public class DeveloperException : Exception {
  public string Action { get; set; }
  public string Name { get; set; }
  public int StatusCode { get; set; }
  public DeveloperException(
    string context,
    string? action = "Please redo your last steps to debugg the problem."
  ) : base(context + " " + action) {
    Name = "DeveloperException";
    Action = action!;
    StatusCode = 501;
  }
}
