using Microsoft.Azure.Cosmos;

namespace PoC.AzureCosmosDbNoSQL.CRUD
{
    internal class Update
    {
        public async Task Updating_Upser(Container container, Product product)
        {
            product.Price = 35.00d;
            await container.UpsertItemAsync<Product>(product);

            product.Tags = new string[] { "brown", "new", "crisp" };
            await container.UpsertItemAsync<Product>(product);
        }

        public async Task Updating_Replace(Container container, Product product)
        {
            ItemResponse<Product> productResponse = await container.ReadItemAsync<Product>(product.Id, new PartitionKey(product.CategoryId));
            var itemBody = productResponse.Resource;

            itemBody.Price = 30d;

            // replace the item with the updated content
            productResponse = await container.ReplaceItemAsync<Product>(itemBody, itemBody.Id, new PartitionKey(product.CategoryId));
            Console.WriteLine("Updated [{0},{1}].\n \tBody is now: {2}\n", itemBody.Price, itemBody.Id, productResponse.Resource);
        }
    }
}
