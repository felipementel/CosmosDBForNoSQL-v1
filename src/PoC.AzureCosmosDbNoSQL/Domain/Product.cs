using Newtonsoft.Json;
using System.Text.Json.Serialization;

public class Product
{
    public Product()
    {
            
    }

    public Product(
        string id,
        string name,
        string categoryId,
        string category,
        double price,
        string[] tags,
        int? tTL)
    {
        Id = id;
        Name = name;
        CategoryId = categoryId;
        Category = category;
        Price = price;
        Tags = tags;
        TTL = tTL;
    }

    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    public string Name { get; set; }

    public string CategoryId { get; set; }

    public string Category { get; set; }

    public double Price { get; set; }

    public string[] Tags { get; set; }

    //[JsonProperty(PropertyName = "ttl", NullValueHandling = NullValueHandling.Ignore)]
    [JsonPropertyName("ttl")]
    public int? TTL { get; set; }
}

public class ProductSimple
{
    public ProductSimple()
    {
            
    }
    public ProductSimple(string id, string name, string category, decimal price)
    {
        Id = id;
        Name = name;
        Category = category;
        Price = price;
    }

    //[JsonPropertyName("id")]
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    //[JsonPropertyName("name")]
    public string Name { get; set; }

    //[JsonPropertyName("category")]
    public string Category { get; set; }

    public decimal Price { get; set; }
}



//Containers
//string endpoint = "https://localhost:8081/";
//string key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";