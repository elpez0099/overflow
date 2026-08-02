using Typesense;

namespace SearchService.Data;

public static class SearchInitializer
{
    public static async Task EnsureIndexExistsAsync(ITypesenseClient client)
    {
        const string schemaName = "questions";
        try
        {
            await client.RetrieveCollection(schemaName);
            Console.WriteLine($"Collection {schemaName} created already.");
            return;
        }
        catch (TypesenseApiNotFoundException)
        {
            Console.WriteLine($"Collection {schemaName} has not  been created yet.");

            var fieldset = new List<Field>
            {
                new("id", FieldType.String),
                new("title", FieldType.String),
                new("content", FieldType.String),
                new("createdAt", FieldType.Int64),
                new("answerCount", FieldType.Int32),
                new("hasAcceptedAnswer", FieldType.Bool),
            };
            
            var schema = new Schema(schemaName, fieldset)
            {
                DefaultSortingField = "createdAt"
            };
            
            await client.CreateCollection(schema);
            Console.WriteLine($"Collection {schemaName} created.");
        }
    }
}