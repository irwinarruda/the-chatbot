using System.Text.Json;

using Shouldly;

using TheChatbot.Entities;
using TheChatbot.Services;

namespace Tests;

public class MessagingServiceTest : IClassFixture<Orquestrator> {
  public Orquestrator orquestrator;
  public MessagingService messagingService;
  public MessagingServiceTest(Orquestrator _orquestrator) {
    orquestrator = _orquestrator;
    messagingService = _orquestrator.messagingService;
  }

  [Fact]
  public async Task SendMessage() {
    await orquestrator.ClearDatabase();
    var user = await orquestrator.CreateUser(phoneNumber: "5511984444444");
    var chat = await messagingService.GetChatByPhoneNumber(user.PhoneNumber);
    chat.ShouldBeNull();
    var jsonString = JsonSerializer.Serialize("User 1");
    await messagingService.ReceiveMessage(JsonSerializer.Deserialize<JsonElement>(jsonString));
    chat = await messagingService.GetChatByPhoneNumber(user.PhoneNumber);
    chat.ShouldNotBeNull();
    chat.IdUser.ShouldBe(user.Id);
    chat.Messages.Count.ShouldBe(2);
    var userMessage = chat.Messages[0];
    userMessage.Text.ShouldBe("User 1");
    userMessage.UserType.ShouldBe(MessageUserType.User);
    var responseMessage = chat.Messages[1];
    responseMessage.Text.ShouldBe("Response to: " + userMessage.Text);
    responseMessage.UserType.ShouldBe(MessageUserType.Bot);
    await messagingService.SendTextMessage(user.PhoneNumber, "Bot 1");
    chat = await messagingService.GetChatByPhoneNumber(user.PhoneNumber);
    chat.ShouldNotBeNull();
    chat.Messages.Count.ShouldBe(3);
  }

  [Fact]
  public async Task ReceiveMessage() {
    await orquestrator.ClearDatabase();
    var phoneNumber = "5511984444444";
    var chat = await messagingService.GetChatByPhoneNumber(phoneNumber);
    chat.ShouldBeNull();
    await messagingService.ReceiveMessage(
      JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize("First message"))
    );
    chat = await messagingService.GetChatByPhoneNumber(phoneNumber);
    chat.ShouldNotBeNull();
    chat.IdUser.ShouldBeNull();
    chat.PhoneNumber.ShouldBe(phoneNumber);
    chat.Messages.Count.ShouldBe(2);
    var userMessage = chat.Messages[0];
    userMessage.ShouldNotBeNull();
    userMessage.Text.ShouldBe("First message");
    userMessage.UserType.ShouldBe(MessageUserType.User);
    var botMessage = chat.Messages[1];
    botMessage.ShouldNotBeNull();
    botMessage.Text?.ShouldContain("✅ Thank you for using *The Chatbot*");
    var user = await orquestrator.CreateUser(phoneNumber: phoneNumber);
    await messagingService.ReceiveMessage(
      JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize("Second message"))
    );
    chat = await messagingService.GetChatByPhoneNumber(phoneNumber);
    chat.ShouldNotBeNull();
    chat.Messages.Count.ShouldBe(4);
    chat.IdUser.ShouldBe(user.Id);
    chat.PhoneNumber.ShouldBe(phoneNumber);
    chat.Messages[2].Text.ShouldBe("Second message");
    chat.Messages[3].Text.ShouldBe("Response to: Second message");
  }
}
