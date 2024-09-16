using Microsoft.Azure.Cosmos;

namespace PoC.AzureCosmosDbNoSQL._4_CustomQuery
{
    internal class CosmosCustomQuery
    {
        public async Task ExecuteQuery(Container container)
        {
            QueryDefinition query = new("SELECT * FROM simpleProducts p");

            using (FeedIterator<ProductSimple> feedIterator = container.GetItemQueryIterator<ProductSimple>(
                query,
                null,
                new QueryRequestOptions() { }))
            {
                while (feedIterator.HasMoreResults)
                {
                    foreach (var item in await feedIterator.ReadNextAsync())
                    {
                        Console.WriteLine($"[{item.Id}]\t{item.Name,35} \t {item.Price,15:C}");
                    }
                }
            }
        }

        public async Task ExecuteQueryWithFilter(Container container)
        {
            QueryDefinition query = new QueryDefinition("SELECT * FROM simpleProducts p WHERE p.Price < @price")
                .WithParameter("@price", 10);

            using (FeedIterator<ProductSimple> feedIterator = container.GetItemQueryIterator<ProductSimple>(
                query,
                null,
                new QueryRequestOptions() { }))
            {
                while (feedIterator.HasMoreResults)
                {
                    foreach (var item in await feedIterator.ReadNextAsync())
                    {
                        Console.WriteLine($"[{item.Id}]\t{item.Name,35}\t{item.Price,15:C}");
                    }
                }
            }

            string sql = "SELECT p.name, t.name AS tag FROM simpleProducts p JOIN t IN p.tags WHERE p.Price >= @lower AND p.Price <= @upper";
            
            QueryDefinition query2 = new QueryDefinition(sql)
                .WithParameter("@lower", 500)
                .WithParameter("@upper", 1000);

            using (FeedIterator<ProductSimple> feedIterator = container.GetItemQueryIterator<ProductSimple>(
                query2,
                null,
                new QueryRequestOptions() { MaxItemCount = 3})) //item importante
            {
                while (feedIterator.HasMoreResults)
                {
                    foreach (var item in await feedIterator.ReadNextAsync())
                    {
                        Console.WriteLine($"[{item.Id}]\t{item.Name,35} \t {item.Price,15:C}");
                    }
                }
            }
        }
    }
}
