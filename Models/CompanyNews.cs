using Newtonsoft.Json;

public class CompanyNews
{
    [JsonProperty("category")]
    public string Category { get; set; }

    [JsonProperty("datetime")]
    public long Datetime { get; set; }

    [JsonProperty("headline")]
    public string Headline { get; set; }

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("image")]
    public string Image { get; set; }

    [JsonProperty("related")]
    public string Related { get; set; }

    [JsonProperty("source")]
    public string Source { get; set; }

    [JsonProperty("summary")]
    public string Summary { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}
