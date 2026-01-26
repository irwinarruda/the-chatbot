using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

using TheChatbot.Entities;
using TheChatbot.Resources;
using TheChatbot.Services;
using TheChatbot.Utils;

namespace Tests;

public class MessagingServiceTest : IClassFixture<Orquestrator> {
  public Orquestrator orquestrator;
  public MessagingService messagingService;
  public IMediator mediator;
  public ServiceProvider serviceProvider;
  public ITestOutputHelper output;
  public int delay = 10;
  public MessagingServiceTest(Orquestrator _orquestrator, ITestOutputHelper _output) {
    orquestrator = _orquestrator;
    messagingService = _orquestrator.messagingService;
    mediator = _orquestrator.mediator;
    serviceProvider = _orquestrator.serviceProvider;
    output = _output;
  }

  [Fact]
  public async Task SendMessage() {
    await orquestrator.ClearDatabase();
    var phoneNumber = TestWhatsAppMessagingGateway.PhoneNumber;
    await messagingService.AddAllowedNumber(phoneNumber);
    var user = await orquestrator.CreateUser(phoneNumber: phoneNumber);
    var chat = await messagingService.GetChatByPhoneNumber(user.PhoneNumber);
    chat.ShouldBeNull();
    await messagingService.ReceiveMessage(CreateReceiveMessage("User 1"), "sig");
    await Task.Delay(delay, TestContext.Current.CancellationToken);
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
    var phoneNumber = TestWhatsAppMessagingGateway.PhoneNumber;
    await messagingService.AddAllowedNumber(phoneNumber);
    var chat = await messagingService.GetChatByPhoneNumber(phoneNumber);
    chat.ShouldBeNull();
    await messagingService.ReceiveMessage(CreateReceiveMessage("First message"), "sig");
    await Task.Delay(delay, TestContext.Current.CancellationToken);
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
    await messagingService.ReceiveMessage(CreateReceiveMessage("Second duplicate message"), "sig");
    await Task.Delay(delay, TestContext.Current.CancellationToken);
    await messagingService.ReceiveMessage(CreateReceiveMessage("Second duplicate message"), "sig");
    await Task.Delay(delay, TestContext.Current.CancellationToken);
    var idProvider = userMessage.IdProvider;
    chat = await messagingService.GetChatByPhoneNumber(phoneNumber);
    output.WriteLine(Printable.Make(chat));
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
    var phoneNumber = TestWhatsAppMessagingGateway.PhoneNumber;
    await messagingService.AddAllowedNumber(phoneNumber);
    var user = await orquestrator.CreateUser(phoneNumber: phoneNumber);
    await messagingService.ReceiveMessage(CreateReceiveMessage("Message 1"), "sig");
    await Task.Delay(100, TestContext.Current.CancellationToken);
    await Task.Delay(delay, TestContext.Current.CancellationToken);
    var chat = await messagingService.GetChatByPhoneNumber(user.PhoneNumber);
    chat.ShouldNotBeNull();
    chat.Messages.Count.ShouldBe(2);
    await orquestrator.DeleteUser(phoneNumber);
    chat = await messagingService.GetChatByPhoneNumber(user.PhoneNumber);
    chat.ShouldBeNull();
    await messagingService.ReceiveMessage(CreateReceiveMessage("New message 2"), "sig");
    await Task.Delay(delay, TestContext.Current.CancellationToken);
    chat = await messagingService.GetChatByPhoneNumber(user.PhoneNumber);
    chat.ShouldNotBeNull();
    chat.Messages[0].ShouldNotBeNull();
    chat.Messages[0].Text.ShouldBe("New message 2");
  }


  [Fact]
  public async Task ShouldNotReceiveMessageIfNumberIsNotAllowed() {
    await orquestrator.ClearDatabase();
    var phoneNumber = TestWhatsAppMessagingGateway.PhoneNumber;
    await messagingService.ReceiveMessage(CreateReceiveMessage("Message never reaches"), "sig");
    await Task.Delay(delay, TestContext.Current.CancellationToken);
    var chat = await messagingService.GetChatByPhoneNumber(phoneNumber);
    chat.ShouldBeNull();
    await messagingService.AddAllowedNumber(phoneNumber);
    await messagingService.ReceiveMessage(CreateReceiveMessage("Message never reaches"), "sig");
    await Task.Delay(delay, TestContext.Current.CancellationToken);
    chat = await messagingService.GetChatByPhoneNumber(phoneNumber);
    chat.ShouldNotBeNull();
  }

  [Fact]
  public async Task SummarizationShouldNotTriggerBeforeThreshold() {
    await orquestrator.ClearDatabase();
    var phoneNumber = TestWhatsAppMessagingGateway.PhoneNumber;
    await messagingService.AddAllowedNumber(phoneNumber);
    await orquestrator.CreateUser(phoneNumber: phoneNumber);
    for (var i = 0; i < 9; i++) {
      await messagingService.ReceiveMessage(CreateReceiveMessage($"Message {i}"), "sig");
      await Task.Delay(delay, TestContext.Current.CancellationToken);
    }
    await Task.Delay(100, TestContext.Current.CancellationToken);
    var chat = await messagingService.GetChatByPhoneNumber(phoneNumber);
    chat.ShouldNotBeNull();
    chat.Messages.Count.ShouldBe(18);
    chat.Summary.ShouldBeNull();
    chat.SummarizedUntilId.ShouldBeNull();
    chat.EffectiveMessages.Count.ShouldBe(18);
  }

  [Fact]
  public async Task SummarizationTriggeredAfterThreshold() {
    await orquestrator.ClearDatabase();
    var phoneNumber = TestWhatsAppMessagingGateway.PhoneNumber;
    await messagingService.AddAllowedNumber(phoneNumber);
    await orquestrator.CreateUser(phoneNumber: phoneNumber);
    for (var i = 0; i < 10; i++) {
      await messagingService.ReceiveMessage(CreateReceiveMessage($"Message {i}"), "sig");
      await Task.Delay(delay, TestContext.Current.CancellationToken);
    }
    await Task.Delay(100, TestContext.Current.CancellationToken);
    var chat = await messagingService.GetChatByPhoneNumber(phoneNumber);
    chat.ShouldNotBeNull();
    chat.Messages.Count.ShouldBe(20);
    chat.Summary.ShouldNotBeNull();
    chat.Summary.ShouldContain("Summary of 20 messages");
    chat.SummarizedUntilId.ShouldNotBeNull();
    chat.EffectiveMessages.Count.ShouldBe(0);
  }

  [Fact]
  public async Task SummarizationIncrementedOnNextThreshold() {
    await orquestrator.ClearDatabase();
    var phoneNumber = TestWhatsAppMessagingGateway.PhoneNumber;
    await messagingService.AddAllowedNumber(phoneNumber);
    await orquestrator.CreateUser(phoneNumber: phoneNumber);
    for (var i = 0; i < 10; i++) {
      await messagingService.ReceiveMessage(CreateReceiveMessage($"Message {i}"), "sig");
      await Task.Delay(delay, TestContext.Current.CancellationToken);
    }
    await Task.Delay(100, TestContext.Current.CancellationToken);
    var chat = await messagingService.GetChatByPhoneNumber(phoneNumber);
    chat.ShouldNotBeNull();
    var firstSummary = chat.Summary;
    firstSummary.ShouldNotBeNull();
    for (var i = 10; i < 20; i++) {
      await messagingService.ReceiveMessage(CreateReceiveMessage($"Message {i}"), "sig");
      await Task.Delay(delay, TestContext.Current.CancellationToken);
    }
    await Task.Delay(100, TestContext.Current.CancellationToken);
    chat = await messagingService.GetChatByPhoneNumber(phoneNumber);
    chat.ShouldNotBeNull();
    chat.Messages.Count.ShouldBe(40);
    chat.Summary.ShouldNotBeNull();
    chat.Summary.ShouldContain(firstSummary);
    chat.Summary.ShouldContain("Summary of 20 messages + Summary of 20 messages");
  }

  [Fact]
  public void ChatEffectiveMessagesReturnsAllWhenSummarizedIdNotFound() {
    var chat = new Chat { PhoneNumber = TestWhatsAppMessagingGateway.PhoneNumber };
    chat.AddUserTextMessage("Hello");
    chat.AddBotTextMessage("Hi there");
    chat.SetSummary("Some summary", Guid.NewGuid());
    chat.EffectiveMessages.Count.ShouldBe(2);
  }


  public static string CreateReceiveMessage(string message) {
    return JsonSerializer.Serialize(message);
  }
}
