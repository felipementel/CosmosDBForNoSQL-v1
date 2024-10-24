using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;

namespace PoC.AzureCosmosDbNoSQL
{
    internal class Conexao
    {
        // Preferencialmente deve ser singleton
        public async Task<CosmosClient> ObterConexao(IConfiguration configuration)
        {
            string? endpointServerless = configuration.GetSection("CosmosDBForNoSQL:Serverless:Endpoint").Value;
            string? keyServerless = configuration.GetSection("CosmosDBForNoSQL:Serverless:Key").Value;

            string? endpointThroughput = configuration.GetSection("CosmosDBForNoSQL:Throughput:Endpoint").Value;
            string? keyThroughput = configuration.GetSection("CosmosDBForNoSQL:Throughput:Key").Value;

            CosmosClient clientServerless, clientThroughput, clientContainer = null;

            //mode 0 - SERVERLESS
            clientServerless = new CosmosClientBuilder(endpointServerless, keyServerless)
            //.WithApplicationPreferredRegions(new List<string> { "francecentral" })
            .Build();


            clientServerless = new(endpointServerless, keyServerless, new CosmosClientOptions()
            {
                MaxRetryAttemptsOnRateLimitedRequests = 9,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30),
                //ApplicationRegion = Regions.BrazilSouth,
                SerializerOptions = new CosmosSerializationOptions()
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                    IgnoreNullValues = true
                },
                ApplicationPreferredRegions = new List<string> { "francecentral" },
                ConnectionMode = ConnectionMode.Direct,
                ConsistencyLevel = ConsistencyLevel.Eventual,
                ApplicationName = "Canal DEPLOY - Azure Cosmos DB NoSQL"
            });

            //mode 2 - PROVISIONED THROUGHPUT
            clientThroughput = new CosmosClientBuilder(endpointThroughput, keyThroughput)
                .WithThrottlingRetryOptions(
                    TimeSpan.FromSeconds(60),
                    5
                )
                .WithApplicationPreferredRegions(
                    new List<string>
                    {
                        Regions.JapanEast
                    }
                )
                .WithSerializerOptions(
                    new CosmosSerializationOptions
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                        IgnoreNullValues = true
                    }
                )
                .WithConnectionModeDirect()
                .WithConsistencyLevel(ConsistencyLevel.Session)
                .WithApplicationName("Canal DEPLOY - Azure Cosmos DB NoSQL")
                .Build();

            //mode 3           

            string connectionString = $"AccountEndpoint={endpointServerless};AccountKey={keyServerless}";

            CosmosClient client3 = new(connectionString);

            //mode Containers
            //string endpoint = "https://localhost:8081/";
            //string key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

            // leitura das propriedades da conta (Caso ocorra erro, verificar em Settings/Networking se seu IP esta habilitado)

            AccountProperties accountGatewayEventual = await clientServerless.ReadAccountAsync();

            Console.WriteLine("** Serverless");
            Console.WriteLine(accountGatewayEventual.Id);
            Console.WriteLine("   * Readable Regions");
            Console.WriteLine(string.Join(Environment.NewLine, accountGatewayEventual.ReadableRegions.Select(e => $" Nome: {e.Name} | Endpoint {e.Endpoint}")));
            Console.WriteLine("   * Writable Regions");
            Console.WriteLine(string.Join(Environment.NewLine, accountGatewayEventual.WritableRegions.Select(e => $" Nome: {e.Name} | Endpoint {e.Endpoint}")));
            Console.WriteLine(accountGatewayEventual.Consistency.DefaultConsistencyLevel);

            AccountProperties accountDirectStrong = await clientThroughput.ReadAccountAsync();

            Console.WriteLine("** Throughput");
            Console.WriteLine(accountDirectStrong.Id);
            Console.WriteLine("   * Readable Regions");
            Console.WriteLine(string.Join(Environment.NewLine, accountDirectStrong.ReadableRegions.Select(e => $" Nome: {e.Name} | Endpoint {e.Endpoint}")));
            Console.WriteLine("   * Writable Regions");
            Console.WriteLine(string.Join(Environment.NewLine, accountDirectStrong.WritableRegions.Select(e => $" Nome: {e.Name} | Endpoint {e.Endpoint}")));
            Console.WriteLine(accountDirectStrong.Consistency.DefaultConsistencyLevel);

            //return clientServerless;
            return clientServerless;
        }
    }
}