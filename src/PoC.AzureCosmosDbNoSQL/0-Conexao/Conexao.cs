using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;

namespace PoC.AzureCosmosDbNoSQL
{
    internal class Conexao
    {
        // Preferencialmente deve ser singleton
        public async Task<CosmosClient> ObterConexao(string? endpoint, string? key)
        {
            CosmosClient cosmosClient = new CosmosClientBuilder(endpoint, key)
            .WithApplicationPreferredRegions(new List<string> { "brazilsouth" })
            .Build();

            //mode 1
            CosmosClient clientGatewayEventual = new(endpoint, key, new CosmosClientOptions()
            {
                //ApplicationRegion = Regions.BrazilSouth,
                SerializerOptions = new CosmosSerializationOptions()
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                    IgnoreNullValues = true
                },
                ApplicationPreferredRegions = new List<string> { "brazilsouth" },
                ConnectionMode = ConnectionMode.Direct,
                ConsistencyLevel = ConsistencyLevel.Session
                //,ApplicationName = "PoC Azure Cosmos DB NoSQL"
            });

            //mode 2
            CosmosClient clientDirectStrong = new CosmosClientBuilder(endpoint, key)
                .WithApplicationPreferredRegions(
                    new List<string>
                    {
                        Regions.BrazilSouth
                    }
                )
                .WithConnectionModeDirect()
                .WithConsistencyLevel(ConsistencyLevel.Session)
                //.WithApplicationName("PoC Azure Cosmos DB NoSQL")
                .Build();

            //mode 3           

            string connectionString = $"AccountEndpoint={endpoint};AccountKey={key}";

            CosmosClient client3 = new(connectionString);

            //mode Containers
            //string endpoint = "https://localhost:8081/";
            //string key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

            //mode 4

            //TokenCredential servicePrincipal = new ClientSecretCredential(
            //    "<azure-ad-tenant-id>",
            //    "<client-application-id>",
            //    "<client-application-secret>");
            //CosmosClient client4 = new CosmosClient("<account-endpoint>", servicePrincipal);


            // leitura das propriedades da conta (Caso ocorra erro, verificar em Settings/Networking se seu IP esta habilitado)
            AccountProperties accountGatewayEventua = await clientDirectStrong.ReadAccountAsync();

            Console.WriteLine(accountGatewayEventua.Id);
            Console.WriteLine("** Readable Regions");
            Console.WriteLine(string.Join(Environment.NewLine, accountGatewayEventua.ReadableRegions.Select(e => $" Nome: {e.Name} | Endpoint {e.Endpoint}")));
            Console.WriteLine("** Writable Regions");
            Console.WriteLine(string.Join(Environment.NewLine, accountGatewayEventua.WritableRegions.Select(e => $" Nome: {e.Name} | Endpoint {e.Endpoint}")));
            Console.WriteLine(accountGatewayEventua.Consistency.DefaultConsistencyLevel);

            AccountProperties accountDirectStrong = await clientDirectStrong.ReadAccountAsync();

            Console.WriteLine(accountDirectStrong.Id);
            Console.WriteLine("** Readable Regions");
            Console.WriteLine(string.Join(Environment.NewLine, accountDirectStrong.ReadableRegions.Select(e => $" Nome: {e.Name} | Endpoint {e.Endpoint}")));
            Console.WriteLine("** Writable Regions");
            Console.WriteLine(string.Join(Environment.NewLine, accountDirectStrong.WritableRegions.Select(e => $" Nome: {e.Name} | Endpoint {e.Endpoint}")));
            Console.WriteLine(accountDirectStrong.Consistency.DefaultConsistencyLevel);

            //retorno do client (DbContext)
            return clientGatewayEventual;
        }
    }
}




