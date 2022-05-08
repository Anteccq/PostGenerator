using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using MessagePack;

var sw = Stopwatch.StartNew();

if(args.Length != 2)
    throw new ArgumentException("Need 2 arguments.");

var (inputDirectoryPath, outputDirectoryPath) = (args[0], args[1]);

const string fileNamePattern = "([0-9]+)-(.*)-(.*)";

var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
var sequence =  Directory.EnumerateFiles(inputDirectoryPath).AsParallel()
    .Select(x =>
    {
        var fileName = Path.GetFileNameWithoutExtension(x);
        var postContent = ExtractContent(x);
        var text = Markdown.ToPlainText(postContent);
        var summary = text[..Math.Min(80, postContent.Length)].TrimEnd(' ') + "...";
        var renderedContent = Markdown.ToHtml(postContent, pipeline);

        var regex = new Regex(fileNamePattern);
        var match = regex.Match(fileName);
        return new Content
        {
            Id = long.Parse(match.Groups[1].Value),
            Title = match.Groups[2].Value,
            Summary = summary,
            RenderedContent = renderedContent,
            Tags = match.Groups[3].Value.Split(',').Select(y => y.TrimStart(' ')).ToArray(),
            CreatedAt = new DateTimeOffset(File.GetCreationTimeUtc(x), TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(File.GetLastWriteTimeUtc(x), TimeSpan.Zero)
        };
    }).ToArray();

if (sequence.GroupBy(x => x.Id).Any(x => x.Count() > 1))
    throw new Exception("Duplicate Id Detected.");

await Task.WhenAll(sequence.Select(x =>
{
    WriteContentToConsole(x);
    return WriteContentToStreamAsync(outputDirectoryPath, x);
}));

Console.WriteLine($"Owattayo. {sw.ElapsedMilliseconds} ms");

static async Task WriteContentToStreamAsync(string outputDirectoryPath, Content content)
{
    await using var fs = new FileStream(Path.Combine(outputDirectoryPath, content.Id.ToString()), FileMode.Create);
    await MessagePackSerializer.SerializeAsync(fs, content);
}

static string ExtractContent(string filePath)
{
    var builder = new StringBuilder();
    foreach (var line in File.ReadLines(filePath))
    {
        builder.AppendLine(line);
    }

    return builder.ToString();
}

static void WriteContentToConsole(Content content)
{
    var sb = new StringBuilder();
    sb.AppendLine("Id: " + content.Id);
    sb.AppendLine("Title: " + content.Title);
    sb.AppendLine("Tags: " + string.Join(", ", content.Tags));
    sb.AppendLine("Summary: " + content.Summary);
    sb.AppendLine("CreatedAt: " + content.CreatedAt);
    sb.AppendLine("UpdatedAt: " + content.UpdatedAt);
    Console.WriteLine(sb.ToString());
}

[MessagePackObject]
public class Content
{
    [Key(0)]
    public long Id { get; set; }
    [Key(1)]
    public string Title { get; set; }
    [Key(2)]
    public string Summary { get; set; }
    [Key(3)]
    public string RenderedContent { get; set; }
    [Key(4)]
    public string[] Tags { get; set; }
    [Key(5)]
    public DateTimeOffset CreatedAt { get; set; }
    [Key(6)]
    public DateTimeOffset UpdatedAt { get; set; }
}