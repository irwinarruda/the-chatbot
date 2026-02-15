using Shouldly;

using TheChatbot.Utils;

namespace Tests;

public class WhatsAppTextChunkerTest {
  [Fact]
  public void ShortTextReturnsOneChunk() {
    var text = "Hello, World!";
    var chunks = WhatsAppTextChunker.Chunk(text);
    chunks.Count.ShouldBe(1);
    chunks[0].ShouldBe(text);
  }

  [Fact]
  public void EmptyTextReturnsOneChunk() {
    var chunks = WhatsAppTextChunker.Chunk("");
    chunks.Count.ShouldBe(1);
    chunks[0].ShouldBe("");
  }

  [Fact]
  public void NullTextReturnsOneChunk() {
    var chunks = WhatsAppTextChunker.Chunk(null!);
    chunks.Count.ShouldBe(1);
    chunks[0].ShouldBeNull();
  }

  [Fact]
  public void ExactlyAtLimitReturnsOneChunk() {
    var text = new string('A', WhatsAppTextChunker.MaxChunkSize);
    var chunks = WhatsAppTextChunker.Chunk(text);
    chunks.Count.ShouldBe(1);
    chunks[0].ShouldBe(text);
  }

  [Fact]
  public void SplitsOnParagraphBreak() {
    var paragraph1 = new string('A', 3000);
    var paragraph2 = new string('B', 3000);
    var text = paragraph1 + "\n\n" + paragraph2;
    var chunks = WhatsAppTextChunker.Chunk(text);
    chunks.Count.ShouldBe(2);
    chunks[0].ShouldBe(paragraph1);
    chunks[1].ShouldBe(paragraph2);
  }

  [Fact]
  public void SplitsOnLineBreak() {
    var line1 = new string('A', 3000);
    var line2 = new string('B', 3000);
    var text = line1 + "\n" + line2;
    var chunks = WhatsAppTextChunker.Chunk(text);
    chunks.Count.ShouldBe(2);
    chunks[0].ShouldBe(line1);
    chunks[1].ShouldBe(line2);
  }

  [Fact]
  public void SplitsOnSentenceEnd() {
    var sentence1 = new string('A', 2500) + ".";
    var sentence2 = " " + new string('B', 2500);
    var text = sentence1 + sentence2;
    var chunks = WhatsAppTextChunker.Chunk(text);
    chunks.Count.ShouldBe(2);
    chunks[0].ShouldBe(sentence1);
    chunks[1].ShouldBe(new string('B', 2500));
  }

  [Fact]
  public void SplitsOnWordBoundary() {
    var words = string.Join(" ", Enumerable.Repeat("word", 2000));
    var chunks = WhatsAppTextChunker.Chunk(words);
    chunks.Count.ShouldBeGreaterThan(1);
    foreach (var chunk in chunks) {
      chunk.Length.ShouldBeLessThanOrEqualTo(WhatsAppTextChunker.MaxChunkSize);
    }
  }

  [Fact]
  public void HardCutsWhenNoBreakPointsExist() {
    var text = new string('A', 5000);
    var chunks = WhatsAppTextChunker.Chunk(text);
    chunks.Count.ShouldBe(2);
    chunks[0].Length.ShouldBe(WhatsAppTextChunker.MaxChunkSize);
    chunks[1].Length.ShouldBe(5000 - WhatsAppTextChunker.MaxChunkSize);
  }

  [Fact]
  public void MultipleChunksPreserveAllContent() {
    var parts = Enumerable.Range(0, 5).Select(i => new string((char)('A' + i), 2000));
    var text = string.Join("\n\n", parts);
    var chunks = WhatsAppTextChunker.Chunk(text);
    var reassembled = string.Join("\n\n", chunks);
    reassembled.ShouldBe(text);
  }

  [Fact]
  public void AllChunksRespectMaxSize() {
    var paragraphs = Enumerable.Range(0, 10).Select(i => new string('X', 1000 + (i * 100)));
    var text = string.Join("\n\n", paragraphs);
    var chunks = WhatsAppTextChunker.Chunk(text);
    foreach (var chunk in chunks) {
      chunk.Length.ShouldBeLessThanOrEqualTo(WhatsAppTextChunker.MaxChunkSize);
    }
  }
}
