using System.Text.RegularExpressions;
using Contracts;
using Helpers;
using SearchService.Models;
using Typesense;

namespace SearchService.MessageHandlers;

public class QuestionCreatedHandler(ITypesenseClient client)
{
    public async Task HandleAsync(QuestionCreated message, CancellationToken cancellationToken)
    {
        var created = new DateTimeOffset(message.CreatedAt).ToUnixTimeSeconds();
        var doc = new SearchQuestion
        {
            Id = message.QuestionId,
            Title = message.Title,
            Content = StringHelpers.StripHtml(message.Content),
            CreatedAt = created,
            Tags = message.Tags.ToArray(),
        };
        
        await client.CreateDocument("questions", doc);
        Console.WriteLine($"Created question {message.QuestionId}");
    }

    /*
    private static string StripHtml(string content)
    {
        return Regex.Replace(content, @"<.*?>", string.Empty);
    }
    */
}