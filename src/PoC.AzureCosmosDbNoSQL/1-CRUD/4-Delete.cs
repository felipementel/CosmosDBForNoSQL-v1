using Microsoft.Azure.Cosmos;

namespace PoC.AzureCosmosDbNoSQL.CRUD
{
    internal class Delete
    {
        public async Task DeleteItem(Container container, Product product)
        {
            PartitionKey partitionKey = new(product.CategoryId);

            await container.DeleteItemAsync<Product>(product.Id, partitionKey);
        }
    }
}
