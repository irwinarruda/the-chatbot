using System.Text.Json;

using Shouldly;

using TheChatbot.Entities;
using TheChatbot.Services;
using TheChatbot.Utils;

namespace Tests;

public class MessagingServiceTest : IClassFixture<Orquestrator> {
  public Orquestrator orquestrator;
  public MessagingService messagingService;
  public ITestOutputHelper h;
  public MessagingServiceTest(Orquestrator _orquestrator, ITestOutputHelper _h) {
    h = _h;
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
    await messagingService.ReceiveMessage(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize("First message")));
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
    botMessage.Text?.ShouldContain("👋");
    var user = await orquestrator.CreateUser(phoneNumber: phoneNumber);
    await messagingService.ReceiveMessage(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize("Second duplicate message")));
    await messagingService.ReceiveMessage(JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize("Second duplicate message")));
    var idProvider = userMessage.IdProvider;
    chat = await messagingService.GetChatByPhoneNumber(phoneNumber);
    chat.ShouldNotBeNull();
    chat.IdUser.ShouldBe(user.Id);
    chat.PhoneNumber.ShouldBe(phoneNumber);
    chat.Messages.Count.ShouldBe(4);
    h.WriteLine(Printable.Make(chat.Messages));
    userMessage = chat.Messages[2];
    userMessage.Text?.ShouldBe("Second duplicate message");
    userMessage.IdProvider.ShouldNotBe(idProvider);
    botMessage = chat.Messages[3];
    botMessage.Text?.ShouldBe("Response to: Second duplicate message");
  }
}
