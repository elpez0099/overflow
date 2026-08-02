using Contracts;
using Helpers;
using SearchService.Models;
using Typesense;

namespace SearchService.MessageHandlers;

public class QuestionUpdatedHandler(ITypesenseClient client)
{
    public async Task HandleAsync(QuestionUpdated message, CancellationToken cancellationToken)
    {
        var doc = new SearchQuestion
        {
            Id = message.QuestionId,
            Title = message.Title,
            Content = StringHelpers.StripHtml(message.Content),
            Tags = message.Tags.ToArray(),
        };
        
        await client.UpdateDocument("questions", doc.Id, doc);
        Console.WriteLine($"Updated question {message.QuestionId}");
    }
}