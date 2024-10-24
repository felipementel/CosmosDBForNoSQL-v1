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

Conexao conexao = new();
CosmosClient client = await conexao.ObterConexao(configuration);

//Microsoft.Azure.Cosmos.Database databaseCanalDEPLOY = client.GetDatabase("CanalDEPLOY");
//if (databaseCanalDEPLOY != null)
//{
//    await databaseCanalDEPLOY.DeleteAsync();
//}


//cosmos-provisioned-throughput-unifeso
//cosmos-serverless-unifeso


DatabaseResponse databaseResponse = await client.CreateDatabaseIfNotExistsAsync("CanalDEPLOY");

Microsoft.Azure.Cosmos.Database databaseCanalDEPLOY = client.GetDatabase(databaseResponse.Database.Id);
Console.WriteLine("Created Database: {0}\n", databaseResponse.Database.Id);


{
    Container container;
    ContainerProperties containerProperties = new("products", "/categoryId");
    containerProperties.IndexingPolicy.Automatic = true;
    containerProperties.IndexingPolicy.IndexingMode = IndexingMode.Consistent;
    containerProperties.ConflictResolutionPolicy = new ConflictResolutionPolicy
    {
        Mode = ConflictResolutionMode.LastWriterWins,
        ResolutionPath = "/_ts"
    };
    //containerProperties.DefaultTimeToLive = 10;

    container = await databaseCanalDEPLOY.CreateContainerIfNotExistsAsync(containerProperties); // Provimento de Throughput 400 RU/s

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
    //Create create = new();
    //await create.Insert(container, product);

    //BulkInsert bulkInsert = new();
    //Container containerBulk = await bulkInsert.BulkInsertModel(configuration);

    ////Leitura
    //Read read = new();
    //await read.Get(container, product);

    //ReadList readList = new();
    //await readList.GetList(containerBulk);

    //update
    Update update = new();
    await update.Updating_Upser(container, product);
    await update.Updating_Replace(container, product);

    //ttl
    product.TTL = 5;
    await container.UpsertItemAsync<Product>(product);

    //delete
    //Delete delete = new();
    //await delete.DeleteItem(container, product);

}
// ==================================================
{
    Container container2 = await databaseCanalDEPLOY.CreateContainerIfNotExistsAsync(
    "simpleProducts",
    "/category");

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
    //BulkCosmos bulkCosmos = new();
    //await bulkCosmos.BulkMode(container2, productSimpleFaker, configuration);


    //Custom Queries
    CosmosCustomQuery customQuery = new();
    await customQuery.ExecuteQuery(container2);
    await customQuery.ExecuteQueryWithFilter(container2);

    //Index
    CosmosIndex cosmosIndex = new();
    await cosmosIndex.CreateIndex(databaseCanalDEPLOY);


    //Conflito
    CosmosConflict cosmosConflict = new();
    await cosmosConflict.CreateNewContainer(databaseCanalDEPLOY);

    //Performance
    CosmosPerformance cosmosPerformance = new();
    await cosmosPerformance.GetPerformance(databaseCanalDEPLOY);
}