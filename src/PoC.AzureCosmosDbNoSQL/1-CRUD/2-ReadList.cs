using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace PoC.AzureCosmosDbNoSQL.CRUD;

internal class ReadList
{
    public ReadList()
    {
        Console.WriteLine();
        Console.WriteLine(nameof(ReadList));
        Console.WriteLine();
    }
    public async Task GetList(Microsoft.Azure.Cosmos.Container container)
    {
        string continuationToken = null;
        int page = 0;

        //query to count
        var countTotal = container
            .GetItemLinqQueryable<Product>(allowSynchronousQueryExecution: true, 
            continuationToken: continuationToken,
            new QueryRequestOptions() { MaxItemCount = -1 })
            .Count();

        Console.WriteLine($"Total items: {countTotal}");

        do
        {
            int MaxItens = 100;
            var itemFeedIterator = container
                .GetItemLinqQueryable<Product>(allowSynchronousQueryExecution: true,
                continuationToken: continuationToken,
                new QueryRequestOptions() 
                { 
                    MaxItemCount = MaxItens
                })
                .OrderBy(p => p.Price)
                .ToFeedIterator<Product>();

            int pageTotal = (int)Math.Ceiling((double)countTotal / MaxItens);

            while (itemFeedIterator.HasMoreResults)
            {
                FeedResponse<Product> response = await itemFeedIterator.ReadNextAsync();
                continuationToken = response.ContinuationToken; // Update the continuation token

                int count = response.Count;
                int i = 1;


                foreach (Product product in response)
                {
                    var numero = i.ToString().PadLeft(3,'0');
                    Console.WriteLine($"  Item {numero} of {count} |" +
                        $" page {page} of {pageTotal} |" +
                        $" {product.Name,35}\t{product.Price,15:C}");

                    i++;
                }

                page++;
            }
        } while (continuationToken != null);

        var itemList = container
            .GetItemLinqQueryable<Product>(allowSynchronousQueryExecution: true,
            null,
            new QueryRequestOptions()
            { 
                MaxItemCount = -1
            })
            .Where(p => p.Price > 10)
            .OrderBy(p => p.Price)
            .ToList<Product>(); // <-- Dont do this in production

        foreach (Product product in itemList)
        {
            Console.WriteLine($"[{product.Id}]\t{product.Name,35}\t{product.Price,15:C}");
        }
    }
}
