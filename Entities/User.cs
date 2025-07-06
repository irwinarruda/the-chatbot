using TheChatbot.Infra;

namespace TheChatbot.Entities;

public class User {
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string PhoneNumber { get; set; } = string.Empty;
  public bool IsInactive { get; set; } = false;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
  public User() {
    InitializeDefaults();
  }

  public User(string name, string phoneNumber) {
    if (name.Length == 30) throw new ValidationException(
      "User name cannot have more than 29 characters",
      "Chose another name and continue"
    );
    if (phoneNumber.Length == 20) throw new ValidationException(
      "User phone number cannot have more than 19 characters",
      "Chose another phone number and continue"
    );
    Name = name;
    PhoneNumber = phoneNumber;
    InitializeDefaults();
  }

  void InitializeDefaults() {
    Id = Guid.NewGuid();
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
  }
}
