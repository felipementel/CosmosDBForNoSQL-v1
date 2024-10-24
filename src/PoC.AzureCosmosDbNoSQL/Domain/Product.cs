using Newtonsoft.Json;
using System.Text.Json.Serialization;

//[JsonProperty(PropertyName = "id")]
//JsonPropertyName
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

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("categoryId")]
    public string CategoryId { get; set; }

    [JsonProperty("category")]
    public string Category { get; set; }

    [JsonProperty("price")]
    public double Price { get; set; }

    [JsonProperty("tags")]
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

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("category")]
    public string Category { get; set; }

    [JsonProperty("price")]
    public decimal Price { get; set; }
}



//Containers
//string endpoint = "https://localhost:8081/";
//string key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";