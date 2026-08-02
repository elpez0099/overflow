using Contracts;
using JasperFx.Events.Projections;
using SearchService.Models;
using Typesense;

namespace SearchService.MessageHandlers;

public class QuestionDeletedHandler(ITypesenseClient client)
{
    public async Task HandleAsync(QuestionDeleted message)
    {
        await client.DeleteDocuments("questions",message.QuestionId);
    }
}