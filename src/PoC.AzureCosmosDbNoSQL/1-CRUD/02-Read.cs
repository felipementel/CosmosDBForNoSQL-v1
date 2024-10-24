using Azure;
using Microsoft.Azure.Cosmos;

namespace PoC.AzureCosmosDbNoSQL.CRUD;

internal class Read
{
    public Read()
    {
        Console.WriteLine();
        Console.WriteLine(nameof(Read));
        Console.WriteLine();
    }
    public async Task Get(Microsoft.Azure.Cosmos.Container container, Product product)
    {
        var item = container.GetItemLinqQueryable<Product>(allowSynchronousQueryExecution: true)
            .Where(p => p.Price > 10)
            .OrderBy(p => p.Price)
            .Select(p => new { p.Id, p.Name, p.Price });

        string categoryId = product.CategoryId;
        PartitionKey partitionKey = new(categoryId);

        ItemResponse<Product> response = await container.ReadItemAsync<Product>(product.Id, partitionKey);

        Console.WriteLine($"RUs:\t{response.RequestCharge:0.00}");

        string formattedName = $"New Product [${response.Resource}]";

        //how to write a full json at response.Resoure of the item
        Console.WriteLine(response.Resource.Name);
    }
}