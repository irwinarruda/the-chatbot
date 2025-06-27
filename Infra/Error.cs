namespace TheChatbot.Infra;

public class ValidationException : Exception {
  public string Action;
  public string Name;
  public int StatusCode;
  public ValidationException(
    string message = "A validation error occurred.",
    string action = "Adjust the provided data and try again."
  ) : base(message) {
    Name = "ValidationException";
    Action = action;
    StatusCode = 400;
  }
}

public class ServiceException : Exception {
  public string Action;
  public string Name;
  public int StatusCode;
  public ServiceException(
    Exception? cause,
    string message = "Service is currently unavailable."
  ) : base(message, cause) {
    Name = "ServiceException";
    Action = "Check if the service is available and try again.";
    StatusCode = 503;
  }
}

public class InternalServerException : Exception {
  public string Action;
  public string Name;
  public int StatusCode;
  public InternalServerException(
    Exception? cause,
    int statusCode = 500
  ) : base("An unexpected error occurred.", cause) {
    Name = "InternalServerException";
    Action = "Please contact our support team for assistance.";
    StatusCode = statusCode;
  }
}
