using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;

namespace PoC.AzureCosmosDbNoSQL._7_Performance
{
    internal class CosmosPerformance
    {
        public async Task GetPerformance(Database client)
        {
            Container container = client.GetContainer("products");

            string sql = "SELECT * FROM products p";

            QueryDefinition query = new(sql);

            FeedIterator<Product> iterator = container.GetItemQueryIterator<Product>(query);

            while (iterator.HasMoreResults)
            {
                FeedResponse<Product> response = await iterator.ReadNextAsync();
                foreach (Product product in response)
                {
                    Console.WriteLine($"[{product.Id}]\t{product.Name,35}\t{product.Price,15:C}");
                }
            }

            QueryRequestOptions options = new()
            {
                PopulateIndexMetrics = true
            };

            FeedIterator<Product> iterator2 = container.GetItemQueryIterator<Product>(query, requestOptions: options);

            while (iterator2.HasMoreResults)
            {
                FeedResponse<Product> response = await iterator2.ReadNextAsync();
                foreach (Product product in response)
                {
                    Console.WriteLine($"[{product.Id}]\t{product.Name,35}\t{product.Price,15:C}");
                }

                Console.WriteLine(response.IndexMetrics);
            }

            FeedIterator<Product> iterator3 = container.GetItemQueryIterator<Product>(query, requestOptions: options);

            //Total RU´s
            double totalRUs = 0;
            while (iterator3.HasMoreResults)
            {
                FeedResponse<Product> response = await iterator3.ReadNextAsync();
                foreach (Product product in response)
                {
                    // Do something with each product
                }

                Console.WriteLine($"RUs:\t\t{response.RequestCharge:0.00}");

                totalRUs += response.RequestCharge;
            }

            Console.WriteLine($"Total RUs:\t{totalRUs:0.00}");

            //Cache

            //Nível do item
            ItemRequestOptions operationOptions = new()
            {
                ConsistencyLevel = ConsistencyLevel.Eventual,
                DedicatedGatewayRequestOptions = new()
                {
                    MaxIntegratedCacheStaleness = TimeSpan.FromMinutes(15)
                },
                IfMatchEtag = "etag"
            };

            var item = new Product
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Product 1",
                Price = 100
            };

            Microsoft.Azure.Cosmos.PartitionKey partitionKey = new("category");

            await container.UpsertItemAsync<Product>(item, partitionKey, requestOptions: operationOptions);


            //Nível da Pesquisa
            QueryRequestOptions queryOptions = new()
            {
                ConsistencyLevel = ConsistencyLevel.Eventual,
                DedicatedGatewayRequestOptions = new()
                {
                    MaxIntegratedCacheStaleness = TimeSpan.FromSeconds(120)
                }
            };

            FeedIterator<Product> iterator5 = container.GetItemQueryIterator<Product>(query, requestOptions: queryOptions);
        }   
    }
}
