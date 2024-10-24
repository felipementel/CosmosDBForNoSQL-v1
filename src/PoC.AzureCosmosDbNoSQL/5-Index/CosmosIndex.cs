using Microsoft.Azure.Cosmos;

namespace PoC.AzureCosmosDbNoSQL._5_Index
{
    internal class CosmosIndex
    {
        public async Task CreateIndex(Microsoft.Azure.Cosmos.Database database)
        {
            IndexingPolicy policy = new()
            {
                IndexingMode = IndexingMode.Consistent,
                Automatic = true,
            };

            //policy.IncludedPaths.Add(
            //    new IncludedPath { Path = "/name/?" }
            //    );

            //policy.IncludedPaths.Add(
            //    new IncludedPath { Path = "/categoryName/?" }
            //    );

            policy.CompositeIndexes.Add(
                new System.Collections.ObjectModel.Collection<CompositePath>
                {
                    new CompositePath { Path = "/name", Order = CompositePathSortOrder.Ascending },
                    new CompositePath { Path = "/price", Order = CompositePathSortOrder.Descending },
                }
            );

            ContainerProperties options = new()
            {
                Id = "productsIndex",
                PartitionKeyPath = "/categoryId",
                IndexingPolicy = policy,
            };

            Container container = await database.CreateContainerIfNotExistsAsync(options); //throughput: 400
        }
    }
}
