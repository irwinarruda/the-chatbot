using System.ComponentModel;

using ModelContextProtocol.Server;

using TheChatbot.Infra;
using TheChatbot.Services;
using TheChatbot.Utils;

namespace Mcp.Tools;

[McpServerToolType]
class AuthTool(AuthService authService) {

  [McpServerTool(Name = "delete_user_by_phone_number")]
  [Description("Delete a user and all related data. Input: phone_number (E.164). Success: { message }. " + ToolDocs.GenericError)]
  public async Task<string> DeleteUserByPhoneNumber(string phone_number) {
    try {
      await authService.DeleteUserByPhoneNumber(phone_number);
      return Printable.Make(new { message = "The account was deleted successfully", phone_number });
    } catch (Exception ex) {
      var response = ExceptionResponse.Handle(ex);
      return Printable.Make(response);
    }
  }
}
