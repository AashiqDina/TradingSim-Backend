using Newtonsoft.Json;
using TradingSim-Backend.Models;

public class TimeSeriesApiResponse
{
    [JsonProperty("status")]
    public string? Status { get; set; }

    [JsonProperty("code")]
    public int? Code { get; set; }

    [JsonProperty("values")]
    public List<StockFullHistoryPoint>? Values { get; set; }

}
