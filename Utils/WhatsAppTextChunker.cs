namespace TheChatbot.Utils;

public static class WhatsAppTextChunker {
  public const int MaxChunkSize = 4096;

  public static List<string> Chunk(string text) {
    if (string.IsNullOrEmpty(text) || text.Length <= MaxChunkSize) {
      return [text];
    }
    var chunks = new List<string>();
    var remaining = text;
    while (remaining.Length > MaxChunkSize) {
      var splitIndex = FindSplitIndex(remaining);
      chunks.Add(remaining[..splitIndex].TrimEnd());
      remaining = remaining[splitIndex..].TrimStart();
    }
    if (remaining.Length > 0) {
      chunks.Add(remaining);
    }
    return chunks;
  }

  private static int FindSplitIndex(string text) {
    var searchRegion = text[..MaxChunkSize];
    var paragraphBreak = searchRegion.LastIndexOf("\n\n");
    if (paragraphBreak > MaxChunkSize / 4) {
      return paragraphBreak + 2;
    }
    var lineBreak = searchRegion.LastIndexOf('\n');
    if (lineBreak > MaxChunkSize / 4) {
      return lineBreak + 1;
    }
    var sentenceEnd = FindLastSentenceEnd(searchRegion);
    if (sentenceEnd > MaxChunkSize / 4) {
      return sentenceEnd;
    }
    var space = searchRegion.LastIndexOf(' ');
    if (space > MaxChunkSize / 4) {
      return space + 1;
    }
    return MaxChunkSize;
  }

  private static int FindLastSentenceEnd(string text) {
    var best = -1;
    for (var i = text.Length - 1; i >= 0; i--) {
      if (text[i] is '.' or '!' or '?') {
        if (i + 1 < text.Length && char.IsWhiteSpace(text[i + 1])) {
          best = i + 2;
          break;
        }
      }
    }
    return best;
  }
}
