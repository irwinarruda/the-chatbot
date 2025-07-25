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
    var user = await orquestrator.CreateUser();
    var conversation = await messagingService.GetConversationByUserPhoneNumber(user.PhoneNumber);
    conversation.ShouldBeNull();
    var text = "Test 1";
    await messagingService.SendMessage(user.PhoneNumber, text);
    conversation = await messagingService.GetConversationByUserPhoneNumber(user.PhoneNumber);
    conversation.ShouldNotBeNull();
    conversation.Messages.ShouldNotBeEmpty();
    var userMessage = conversation.Messages[0];
    userMessage.Text.ShouldBe(text);
    userMessage.UserType.ShouldBe(MessageUserType.User);
    var responseMessage = conversation.Messages[1];
    responseMessage.Text.ShouldBe("Response for: " + text);
    responseMessage.UserType.ShouldBe(MessageUserType.Bot);
  }
}
