using Bogus;
using Microsoft.Azure.Cosmos;

namespace PoC.AzureCosmosDbNoSQL._2_Transacao_e_Concorrencia;

internal class CosmosConcorrencia
{
    public async Task ControleDeConcorrencia(Container container, Faker<ProductSimple> productTransactionFaker)
    {
        PartitionKey partitionKey = new("canal-deploy");


        var products = productTransactionFaker.Generate(2);

        await container.CreateItemAsync<ProductSimple>(
            products[0]);        

        ItemResponse<ProductSimple> responseRU = await container.ReadItemAsync<ProductSimple>(products[0].Id, partitionKey);
        ProductSimple product1 = responseRU.Resource;
        string eTag = responseRU.ETag;

        ItemRequestOptions optionsRU = new ItemRequestOptions { IfMatchEtag = eTag };
        await container.UpsertItemAsync<ProductSimple>(product1, requestOptions: optionsRU);



        // Simulando concorrência
        {
            ItemResponse<ProductSimple> Product_ORIGINAL = await container.ReadItemAsync<ProductSimple>(product1.Id, partitionKey);
            Product_ORIGINAL.Resource.Name = "Sorvete";

            {
                //intervalo
            }


            // Outro processo alterando o mesmo dado
            ItemResponse<ProductSimple> Product_CONCORRENTE = await container.ReadItemAsync<ProductSimple>(product1.Id, partitionKey);
            Product_CONCORRENTE.Resource.Name = "Picolé";

            ItemRequestOptions options_CONCORRENTE = new ItemRequestOptions { IfMatchEtag = Product_CONCORRENTE.ETag };

            await container.UpsertItemAsync<ProductSimple>(Product_CONCORRENTE, partitionKey, requestOptions: options_CONCORRENTE);




            //Fim do intervalo

            ItemRequestOptions options_ORIGINAL = new ItemRequestOptions { IfMatchEtag = Product_ORIGINAL.ETag };

            try
            {
                await container.UpsertItemAsync<ProductSimple>(Product_ORIGINAL, partitionKey, requestOptions: options_ORIGINAL);
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");

                await container.UpsertItemAsync<ProductSimple>(Product_ORIGINAL, partitionKey);
            }
        }
    }
}
