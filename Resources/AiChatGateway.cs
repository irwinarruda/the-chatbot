using Microsoft.Extensions.AI;

using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using System.Text.RegularExpressions;

using TheChatbot.Utils;

namespace TheChatbot.Resources;

public class AiChatGateway(IChatClient chatClient) : IAiChatGateway {
  private static ChatMessage GetSystemPrompt(string phoneNumber) {
    var full = PromptLoader.GetAiChatGateway(PromptLocale.PtBr, new AiChatGatewayParams(phoneNumber));
    return new ChatMessage(ChatRole.System, full);
  }

  private async Task<IMcpClient> GetMcpClient() {
    return await McpClientFactory.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions {
      Name = "TheChatbot",
      Command = "dotnet",
      Arguments = ["run", "--project", "/Users/irwinarruda/Documents/PRO/the-chatbot/Mcp", "--no-build"],
    }), new McpClientOptions {
      Capabilities = new ClientCapabilities {
        Sampling = new SamplingCapability {
          SamplingHandler = chatClient.CreateSamplingHandler(),
        }
      }
    });
  }

  private static ChatRole ConvertAiChatRole(AiChatRole role) {
    return role switch {
      AiChatRole.System => ChatRole.System,
      AiChatRole.Assistant => ChatRole.Assistant,
      AiChatRole.User => ChatRole.User,
      _ => ChatRole.Assistant
    };
  }

  public async Task<AiChatResponse> GetResponse(string phoneNumber, List<AiChatMessage> messages) {
    var mcp = await GetMcpClient();
    var tools = await mcp.ListToolsAsync();
    var chatMessages = messages.Select((m) => new ChatMessage(ConvertAiChatRole(m.Role), m.Text));
    var response = await chatClient!.GetResponseAsync([GetSystemPrompt(phoneNumber), .. chatMessages], new() { Tools = [.. tools], AllowMultipleToolCalls = true });
    var raw = response.Text?.Trim() ?? string.Empty;
    var llmResponse = new AiChatResponse {
      Type = AiChatResponseType.Text,
      Text = raw,
      Buttons = []
    };
    if (string.IsNullOrEmpty(raw)) return llmResponse;
    var buttonMatch = Regex.Match(raw, @"^\s*\[(Button)\]\s*\[(?<btns>[^\]]+)\](?<rest>.*)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    if (buttonMatch.Success) {
      llmResponse.Type = AiChatResponseType.Button;
      var restText = buttonMatch.Groups["rest"].Value.Trim();
      var btns = buttonMatch.Groups["btns"].Value.Split(';')
        .Select(b => b.Trim())
        .Where(b => !string.IsNullOrWhiteSpace(b))
        .Take(3)
        .ToList();
      if (btns.Count > 0) {
        llmResponse.Buttons = btns;
      }
      llmResponse.Text = restText;
      return llmResponse;
    }
    var textMatch = Regex.Match(raw, @"^\s*\[(Text)\](?<rest>.*)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    if (textMatch.Success) {
      var restText = textMatch.Groups["rest"].Value.Trim();
      llmResponse.Text = restText;
      return llmResponse;
    }
    return llmResponse;
  }
}
