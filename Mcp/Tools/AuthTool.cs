using System.ComponentModel;

using ModelContextProtocol.Server;

using TheChatbot.Infra;
using TheChatbot.Services;
using TheChatbot.Utils;

namespace Mcp.Tools;

[McpServerToolType]
class AuthTool(AuthService authService) {
  [McpServerTool(Name = "DeleteUserByPhoneNumber")]
  [Description("Delete user from the entire app. Takes a required phone number.")]
  public async Task<string> DeleteUserByPhoneNumber(string phoneNumber) {
    try {
      await authService.DeleteUserByPhoneNumber(phoneNumber);
      return Printable.Make("");
    } catch (Exception ex) {
      var response = ExceptionResponse.Handle(ex);
      return Printable.Make(response);
    }
  }
}
