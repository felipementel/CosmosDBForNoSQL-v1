using Bogus;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using PoC.AzureCosmosDbNoSQL;
using PoC.AzureCosmosDbNoSQL._2_Transacao_e_Concorrencia;
using PoC.AzureCosmosDbNoSQL._3_BulkOperations;
using PoC.AzureCosmosDbNoSQL._4_CustomQuery;
using PoC.AzureCosmosDbNoSQL._5_Index;
using PoC.AzureCosmosDbNoSQL._6_Conflito;
using PoC.AzureCosmosDbNoSQL._7_Performance;
using PoC.AzureCosmosDbNoSQL.CRUD;

ConfigurationBuilder builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>();

IConfigurationRoot configuration = builder.Build();

string? endpoint = configuration.GetSection("CosmosDBForNoSQL:Endpoint").Value;
string? key = configuration.GetSection("CosmosDBForNoSQL:Key").Value;

Conexao conexao = new();
CosmosClient client = await conexao.ObterConexao(endpoint, key);

await client.GetDatabase("CanalDEPLOY").DeleteAsync().ConfigureAwait(false);

DatabaseResponse databaseResponse = await client.CreateDatabaseIfNotExistsAsync("CanalDEPLOY");
Console.WriteLine("Created Database: {0}\n", databaseResponse.Database.Id);


Microsoft.Azure.Cosmos.Database database = client.GetDatabase("CanalDEPLOY");

{
    Container container;
    ContainerProperties containerProperties = new("products", "/CategoryId");
    containerProperties.IndexingPolicy.Automatic = true;
    containerProperties.IndexingPolicy.IndexingMode = IndexingMode.Consistent;
    containerProperties.ConflictResolutionPolicy = new ConflictResolutionPolicy
    {
        Mode = ConflictResolutionMode.LastWriterWins,
        ResolutionPath = "/_ts"
    };
    containerProperties.DefaultTimeToLive = 10;

    container = await database.CreateContainerIfNotExistsAsync(containerProperties, 400);

    //container = await database.CreateContainerIfNotExistsAsync(
    //    "products",
    //    "/CategoryId",
    //    400
    //);

    var ProductFaker = new Faker<Product>("pt_BR")
        .RuleFor(p => p.Id, f => Guid.NewGuid().ToString())
        .RuleFor(p => p.Name, f => f.Commerce.ProductName())
        .RuleFor(p => p.CategoryId, f => Guid.NewGuid().ToString())
        .RuleFor(p => p.Category, f => f.Commerce.Categories(1)[0])
        .RuleFor(p => p.Price, f => f.Random.Double(1, 100))
        .RuleFor(p => p.Tags, f => f.Commerce.Categories(3).ToArray())
        .FinishWith((f, u) =>
        {
            Console.WriteLine("Product Created Id={0}", u.Id);
        });


    var product = ProductFaker.Generate();

    //Escrita
    Create create = new();
    await create.Insert(container, product);

    //Leitura
    Read read = new();
    await read.Get(container, product);

    //update
    Update update = new();
    await update.Updating_Upser(container, product);
    await update.Updating_Replace(container, product);

    //ttl
    product.TTL = 30;
    await container.UpsertItemAsync<Product>(product);

    //delete
    //Delete delete = new();
    //await delete.DeleteItem(container, product);

}
// ==================================================
{
    Container container2 = await database.CreateContainerIfNotExistsAsync(
    "simpleProducts",
    "/Category",
    400);

    var productSimpleFaker = new Faker<ProductSimple>("pt_BR")
        .CustomInstantiator(f => new ProductSimple(
            id: f.Random.Guid().ToString(),
            name: f.Commerce.ProductName(),
            price: f.Random.Decimal(1, 100),
            category: "canal-deploy"))
        .FinishWith((f, u) =>
        {
            Console.WriteLine("Product Created Id={0}", u.Id);
        });

    //Transacao
    CosmosTransacao transacao = new();
    await transacao.ControlandoTransacao(container2, productSimpleFaker);


    // concorrencia entre leitura e escrita
    CosmosConcorrencia concorrencia = new();
    await concorrencia.ControleDeConcorrencia(container2, productSimpleFaker);


    //Bulk Operations
    BulkCosmos bulkCosmos = new();
    await bulkCosmos.BulkMode(container2, productSimpleFaker, configuration);


    //Custom Queries
    CosmosCustomQuery customQuery = new();
    await customQuery.ExecuteQuery(container2);
    await customQuery.ExecuteQueryWithFilter(container2);

    //Index
    CosmosIndex cosmosIndex = new();
    await cosmosIndex.CreateIndex(database);


    //Conflito
    CosmosConflict cosmosConflict = new();
    await cosmosConflict.CreateNewContainer(database);

    //Performance
    CosmosPerformance cosmosPerformance = new();
    await cosmosPerformance.GetPerformance(database);
}