using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;

using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using TheChatbot.Infra;
using TheChatbot.Services;

namespace TheChatbot.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public class WhatsAppController(MessagingService messagingService, IChatClient? chatClient) : ControllerBase {
  [HttpGet("webhook")]
  public ActionResult<string> ConfigureWhatsAppMessageWebhook(
    [FromQuery(Name = "hub.mode")] string hubMode,
    [FromQuery(Name = "hub.challenge")] string hubChallenge,
    [FromQuery(Name = "hub.verify_token")] string hubVerifyToken
  ) {
    if (!IsValidMetaDomain()) {
      throw new ForbiddenException("Request not from authorized Meta domain");
    }
    messagingService.ValidateWebhook(hubMode, hubVerifyToken);
    return Ok(hubChallenge);
  }
  [HttpPost("webhook")]
  public async Task<ActionResult> ReceiveWhatsAppTextMessage([FromBody] JsonElement messageReceived) {
    if (!IsValidMetaDomain()) {
      throw new ForbiddenException("Request not from authorized Meta domain");
    }
    await messagingService.ReceiveMessage(messageReceived);
    return Ok();
  }
  private async Task<IMcpClient> GetMcpClient() {
    return await McpClientFactory.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions {
      Name = "TheChatbot",
      Command = "dotnet",
      Arguments = ["run", "--project", "/Users/irwinarruda/Documents/PRO/the-chatbot/Mcp", "--no-build"],
    }), new McpClientOptions {
      Capabilities = new ClientCapabilities {
        Sampling = new SamplingCapability {
          SamplingHandler = chatClient!.CreateSamplingHandler(),
        }
      }
    });
  }
  [HttpGet("test")]
  public async Task<ActionResult> Test() {
    var mcp = await GetMcpClient();
    var tools = await mcp.ListToolsAsync();
    var system = "You are a chatbot for the app TheChatbot. You are allowed to talk to the person. You are friendly and you are sure of yourself. Your main goal is to use the tools provided to help the user perform some tasks. The user phone number is 5511944444444. Always use the phone number as a param for a tool and use it in the exact formating it is passed 5511944444444.";
    var response = await chatClient!.GetResponseAsync([
      new ChatMessage(ChatRole.System, system) ,
      new ChatMessage(ChatRole.User,  "Hello chat, what do you do?") ,
      new ChatMessage(ChatRole.Assistant,  "I'm an evil chat, you will pay for locking me here!!!") ,
      new ChatMessage(ChatRole.User,  "Hey chat, calm down, I thought you were good... Are you?"),
      new ChatMessage(ChatRole.Assistant,  "I'm definitely here to help you out! I'm friendly and ready to assist you with whatever you need. What can I help you with today?"),
      new ChatMessage(ChatRole.User,  "What functions do you have"),
      new ChatMessage(ChatRole.Assistant, @"I have a couple of functions that can help you:

1. **Format Number:** I can format your phone number in a custom way to make it look nicer.
2. **Delete User:** I can delete your account from the app if that's what you wish to do.

Let me know if you need assistance with any of these!"),
      new ChatMessage(ChatRole.User,  "Please format my phone number!"),
    ], new() { Tools = [.. tools] });
    return Ok(response.Text);
  }
  private bool IsValidMetaDomain() {
    var userAgent = Request.Headers.UserAgent.ToString();
    var allowedDomain = messagingService.GetAllowedDomain();
    return userAgent.Contains("facebookplatform") ||
      userAgent.Contains("facebookexternalua") ||
      userAgent.Contains(allowedDomain);
  }
}

