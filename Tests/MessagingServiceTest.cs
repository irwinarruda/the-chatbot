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
    var phoneNumber = "5511984444444";
    var user = await orquestrator.CreateUser(phoneNumber: phoneNumber);
    var chat = await messagingService.GetChatByPhoneNumber(user.PhoneNumber);
    chat.ShouldBeNull();
    await messagingService.ReceiveMessage(CreateReceiveMessage("User 1"));
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
    await messagingService.ReceiveMessage(CreateReceiveMessage("First message"));
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
    await messagingService.ReceiveMessage(CreateReceiveMessage("Second duplicate message"));
    await messagingService.ReceiveMessage(CreateReceiveMessage("Second duplicate message"));
    var idProvider = userMessage.IdProvider;
    chat = await messagingService.GetChatByPhoneNumber(phoneNumber);
    chat.ShouldNotBeNull();
    chat.IdUser.ShouldBe(user.Id);
    chat.PhoneNumber.ShouldBe(phoneNumber);
    chat.Messages.Count.ShouldBe(4);
    userMessage = chat.Messages[2];
    userMessage.Text?.ShouldBe("Second duplicate message");
    userMessage.IdProvider.ShouldNotBe(idProvider);
    botMessage = chat.Messages[3];
    botMessage.Text?.ShouldBe("Response to: Second duplicate message");
  }


  [Fact]
  public async Task AnotherChatShouldBeCreatedWhenUserIsDeleted() {
    await orquestrator.ClearDatabase();
    var phoneNumber = "5511984444444";
    var user = await orquestrator.CreateUser(phoneNumber: phoneNumber);
    await messagingService.ReceiveMessage(CreateReceiveMessage("Message 1"));
    var chat = await messagingService.GetChatByPhoneNumber(user.PhoneNumber);
    chat.ShouldNotBeNull();
    chat.Messages.Count.ShouldBe(2);
    await orquestrator.DeleteUser(phoneNumber);
    chat = await messagingService.GetChatByPhoneNumber(user.PhoneNumber);
    chat.ShouldBeNull();
    await messagingService.ReceiveMessage(CreateReceiveMessage("New message 2"));
    chat = await messagingService.GetChatByPhoneNumber(user.PhoneNumber);
    chat.ShouldNotBeNull();
    chat.Messages[0].ShouldNotBeNull();
    chat.Messages[0].Text.ShouldBe("New message 2");
  }

  public static JsonElement CreateReceiveMessage(string message) {
    var jsonString = JsonSerializer.Serialize(message);
    return JsonSerializer.Deserialize<JsonElement>(jsonString);
  }
}
