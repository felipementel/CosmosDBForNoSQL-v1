using Microsoft.Azure.Cosmos;

namespace PoC.AzureCosmosDbNoSQL._6_Conflito
{
    internal class CosmosConflict
    {
        public async Task CreateNewContainer(Database database)
        {
            ContainerProperties properties = new("products", "/categoryId")
            {
                ConflictResolutionPolicy = new ConflictResolutionPolicy()
                {
                    Mode = ConflictResolutionMode.LastWriterWins,
                    ResolutionPath = "/sortableTimestamp",

                    //O padrão é _ts

                    //Mode = ConflictResolutionMode.Custom,
                    //ResolutionProcedure = $"dbs/{databaseName}/colls/{containerName}/sprocs/{sprocName}",

                    //StoredProcedureProperties properties = new(sprocName, File.ReadAllText(@"code.js"))
                    //await container.Scripts.CreateStoredProcedureAsync(properties);
                }
            };

            Container container = await database.CreateContainerIfNotExistsAsync(properties);
        }
    }
}
