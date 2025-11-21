using TheChatbot.Entities.Extensions;

namespace TheChatbot.Entities;

public class AllowedNumber {
  public Guid Id { get; set; }
  public string PhoneNumber { get; set; }
  public DateTime CreatedAt { get; set; }
  public AllowedNumber(string phoneNumber) {
    Id = Guid.NewGuid();
    CreatedAt = DateTime.UtcNow.TruncateToMicroseconds();
    PhoneNumber = PhoneNumberUtils.AddDigitNine(phoneNumber);
  }
}
