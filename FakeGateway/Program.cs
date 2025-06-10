using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var rootFolder = Path.Combine(Directory.GetCurrentDirectory(), "ReceivedData");

app.MapPost("/api/data", async (HttpRequest request) =>
{	
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);

    if (data == null ||
        !data.TryGetValue("subfolder", out var subfolder) ||
        !data.TryGetValue("filename", out var filename) ||
        !data.TryGetValue("content", out var content))
    {
        return Results.BadRequest("missing 'subfolder', 'filename' or 'content'");
    }

    if (!filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        filename += ".json";

    var targetFolder = Path.Combine(Directory.GetCurrentDirectory(), "ReceivedData", subfolder);
    Directory.CreateDirectory(targetFolder);

    var fullPath = Path.Combine(targetFolder, filename);
    await File.WriteAllTextAsync(fullPath, content);

    Console.WriteLine($"Received file: {fullPath}");

   return Results.Ok(new { message = "Send success", path = fullPath });
});

app.MapGet("/api/check-path", (HttpRequest request) =>
{
    var query = request.Query;

    if (!query.TryGetValue("subfolder", out var subfolderVal) || string.IsNullOrWhiteSpace(subfolderVal))
        return Results.BadRequest("missing parameter 'subfolder'");

    if (!query.TryGetValue("filename", out var filenameVal) || string.IsNullOrWhiteSpace(filenameVal))
        return Results.BadRequest("missing parameter 'filename'");

    var subfolder = subfolderVal.ToString();
    var filename = filenameVal.ToString();

    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "ReceivedData", subfolder, filename);
    bool exists = File.Exists(fullPath);

    return Results.Ok(new { subfolder, filename, exists });
});

app.Run("http://localhost:5000");
