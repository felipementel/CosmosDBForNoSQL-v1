using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using System.Net;

namespace PoC.AzureCosmosDbNoSQL.CRUD
{
    internal class Create
    {
        public async Task Insert(Container container, Product product)
        {
            try
            {
                ItemResponse<Product> response = await container.CreateItemAsync<Product>(product);

                Console.WriteLine($"RUs:\t{response.RequestCharge:0.00}");

                HttpStatusCode status = response.StatusCode;

                string token = response.Headers.Session;
                Microsoft.Azure.Cosmos.PartitionKey partitionKey = new(product.CategoryId);

                ItemResponse<Product> readResponse = await container.ReadItemAsync<Product>(product.Id, partitionKey, requestOptions: new ItemRequestOptions() { SessionToken = token });

                Product product1 = response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                // Add logic to handle conflicting ids
            }
            catch (CosmosException)
            {
                // Add general exception handling logic
            }
            catch (Exception)
            {
                // Add general exception handling logic
            }
        }
    }
}
