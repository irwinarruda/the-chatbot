namespace TheChatbot.Utils;

public interface IMediator {
  Task Send<T>(string eventName, T payload);
  void Register<T>(string eventName, Func<T, Task> handler);
  void Register<T>(string eventName, Action<T> handler);
}

public class Mediator : IMediator {
  private readonly Dictionary<string, List<Delegate>> _handlers = [];
  public async Task Send<T>(string eventName, T payload) {
    if (!_handlers.TryGetValue(eventName, out List<Delegate>? handlers)) return;
    var tasks = new List<Task>();
    foreach (var handler in handlers) {
      if (handler is Func<T, Task> asyncHandler) {
        tasks.Add(asyncHandler(payload));
      } else if (handler is Action<T> syncHandler) {
        syncHandler(payload);
      }
    }
    if (tasks.Count > 0) {
      await Task.WhenAll(tasks);
    }
  }

  public void Register<T>(string eventName, Func<T, Task> handler) {
    if (!_handlers.ContainsKey(eventName)) {
      _handlers[eventName] = [];
    }
    _handlers[eventName].Add(handler);
  }

  public void Register<T>(string eventName, Action<T> handler) {
    if (!_handlers.ContainsKey(eventName)) {
      _handlers[eventName] = [];
    }
    _handlers[eventName].Add(handler);
  }
}
