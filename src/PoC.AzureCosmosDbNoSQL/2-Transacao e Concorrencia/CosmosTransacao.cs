using Azure;
using Bogus;
using Microsoft.Azure.Cosmos;

namespace PoC.AzureCosmosDbNoSQL._2_Transacao_e_Concorrencia
{
    internal class CosmosTransacao
    {
        public async Task ControlandoTransacao(Container container, Faker<ProductSimple> productTransactionFaker)
        {
            var products = productTransactionFaker.Generate(2);

            //var response = await container.CreateItemAsync<ProductSimple>(products[0]);

            ProductSimple produtoCasa = products[0];
            ProductSimple produtoCozinha = products[1];

            PartitionKey partitionKey = new("canal-deploy");

            TransactionalBatch batch = container.CreateTransactionalBatch(partitionKey)
                .CreateItem<ProductSimple>(produtoCasa)
                .CreateItem<ProductSimple>(produtoCozinha);
            //.UpsertItem
            //.UpsertItemStream
            //.DeleteItem
            //.CreateItemStream
            //.ReplaceItem
            //.ReplaceItemStream

            using TransactionalBatchResponse responseBatch = await batch.ExecuteAsync();

            if (responseBatch.IsSuccessStatusCode)
            {
                Console.WriteLine("First product created successfully");

                TransactionalBatchOperationResult<ProductSimple> result1 = responseBatch.GetOperationResultAtIndex<ProductSimple>(0);
                ProductSimple firstProductResult = result1.Resource;

                TransactionalBatchOperationResult<ProductSimple> result2 = responseBatch.GetOperationResultAtIndex<ProductSimple>(1);
                ProductSimple secondProductResult = result2.Resource;

            }
            else
            {
                Console.WriteLine("First product failed to be created");
            }
        }
    }
}
