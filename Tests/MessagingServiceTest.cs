using System.Text.Json;

using Microsoft.VisualBasic;

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
    var chat = await messagingService.GetChatByUserPhoneNumber(user.PhoneNumber);
    chat.ShouldBeNull();
    var jsonString = JsonSerializer.Serialize("User 1");
    await messagingService.ReceiveMessage(JsonSerializer.Deserialize<JsonElement>(jsonString));
    chat = await messagingService.GetChatByUserPhoneNumber(user.PhoneNumber);
    chat.ShouldNotBeNull();
    chat.IdUser.ShouldBe(user.Id);
    chat.Messages.Count.ShouldBe(2);
    var userMessage = chat.Messages[0];
    userMessage.Text.ShouldBe("User 1");
    userMessage.IdUser.ShouldBe(user.Id);
    userMessage.UserType.ShouldBe(MessageUserType.User);
    var responseMessage = chat.Messages[1];
    responseMessage.Text.ShouldBe("Response to: " + userMessage.Text);
    responseMessage.IdUser.ShouldBeNull();
    responseMessage.UserType.ShouldBe(MessageUserType.Bot);
    await messagingService.SendTextMessage(user.PhoneNumber, "Bot 1");
    chat = await messagingService.GetChatByUserPhoneNumber(user.PhoneNumber);
    chat.ShouldNotBeNull();
    chat.Messages.Count.ShouldBe(3);
  }
}
