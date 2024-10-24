using Bogus;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;

namespace PoC.AzureCosmosDbNoSQL.CRUD;

internal class BulkInsert
{
    public BulkInsert()
    {
        Console.WriteLine();
        Console.WriteLine(nameof(BulkInsert));
        Console.WriteLine();
    }
    public async Task<Container> BulkInsertModel(IConfiguration configuration)
    {
        string? endpointServerless = configuration.GetSection("CosmosDBForNoSQL:Serverless:Endpoint").Value;
        string? keyServerless = configuration.GetSection("CosmosDBForNoSQL:Serverless:Key").Value;

        var client = new CosmosClientBuilder(endpointServerless, keyServerless).Build();

        //mode 1 - SERVERLESS
        client = new(endpointServerless, keyServerless, new CosmosClientOptions()
        {
            //ApplicationRegion = Regions.BrazilSouth,
            SerializerOptions = new CosmosSerializationOptions()
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                IgnoreNullValues = true
            },


            AllowBulkExecution = true, // <--- Bulk support


            ApplicationPreferredRegions = new List<string> { "brazilsouth" },
            ConnectionMode = ConnectionMode.Direct,
            ConsistencyLevel = ConsistencyLevel.Session,
            ApplicationName = "Canal DEPLOY - Azure Cosmos DB NoSQL"
        });


        DatabaseResponse databaseResponse = await client.CreateDatabaseIfNotExistsAsync("CanalDEPLOY-Bulk");

        Microsoft.Azure.Cosmos.Database databaseCanalDEPLOY = client.GetDatabase(databaseResponse.Database.Id);
        Console.WriteLine("Created Database: {0}\n", databaseResponse.Database.Id);


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

        //ThroughputProperties throughputProperties = ThroughputProperties.CreateAutoscaleThroughput(1000);

        container = await databaseCanalDEPLOY.CreateContainerIfNotExistsAsync(containerProperties); // Provimento de Throughput 400 RU/s

        //ThroughputProperties autoscaleThroughputProperties = ThroughputProperties.CreateAutoscaleThroughput(1000);

        //// Read the throughput on a resource
        //ThroughputProperties autoscaleContainerThroughput = await container.ReadThroughputAsync(requestOptions: null);

        //// The autoscale max throughput (RU/s) of the resource
        //int? autoscaleMaxThroughput = autoscaleContainerThroughput.AutoscaleMaxThroughput;

        //// The throughput (RU/s) the resource is currently scaled to
        //int? currentThroughput = autoscaleContainerThroughput.Throughput;

        //// Change the autoscale max throughput (RU/s)
        //await container.ReplaceThroughputAsync(ThroughputProperties.CreateAutoscaleThroughput(1000));



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


        var products = ProductFaker.Generate(300);

        List<Task> concurrentTasks = new List<Task>();
        foreach (Product itemToInsert in products)
        {
            concurrentTasks.Add(container.CreateItemAsync(itemToInsert, new PartitionKey(itemToInsert.CategoryId)));
        }
        
        //await Task.WhenAll(concurrentTasks);

        //foreach (Product itemToInsert in products)
        //{
        //    await Task.Delay(500);
        //    await container.CreateItemAsync(itemToInsert, new PartitionKey(itemToInsert.CategoryId));
        //}

        //RU's

        return container;
    }
}