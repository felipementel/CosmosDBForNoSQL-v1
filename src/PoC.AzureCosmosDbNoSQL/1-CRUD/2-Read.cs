using Microsoft.Azure.Cosmos;

namespace PoC.AzureCosmosDbNoSQL.CRUD;

internal class Read
{
    public async Task Get(Microsoft.Azure.Cosmos.Container container, Product product)
    {
        var item = container.GetItemLinqQueryable<Product>(allowSynchronousQueryExecution: true)
            .Where(p => p.Price > 10)
            .OrderBy(p => p.Price)
            .Select(p => new { p.Id, p.Name, p.Price });

        string categoryId = product.CategoryId;
        PartitionKey partitionKey = new(categoryId);

        ItemResponse<Product> response1 = await container.ReadItemAsync<Product>(product.Id, partitionKey);

        string formattedName = $"New Product [${response1.Resource}]";
        Console.WriteLine(formattedName);
    }
}