    using Newtonsoft.Json;

public class StockApiInfo
{
    public string Symbol { get; set; }
    public string Name { get; set; }
    public string Exchange { get; set; }
    [JsonProperty("mic_code")]
    public string MicCode { get; set; }
    public string Currency { get; set; }
    public string Datetime { get; set; }
    public long Timestamp { get; set; } 

    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }
    [JsonProperty("previous_close")]
    public decimal PreviousClose { get; set; }
    public decimal Change { get; set; }
    [JsonProperty("percent_change")]
    public decimal PercentChange { get; set; }
    [JsonProperty("average_volume")]
    public long AverageVolume { get; set; }

    [JsonProperty("rolling_1day_change")]
    public decimal Rolling1DChange { get; set; }
    [JsonProperty("rolling_7day_change")]
    public decimal Rolling7DChange { get; set; }
    [JsonProperty("rolling_period_change")]
    public decimal RollingPeriodChange { get; set; }

    [JsonProperty("is_market_open")]
    public bool IsMarketOpen { get; set; }
    [JsonProperty("fifty_two_week")]
    public FiftyTwoWeek FiftyTwoWeek { get; set; }

    [JsonProperty("extended_change")]
    public decimal ExtendedChange { get; set; }
    [JsonProperty("extended_percent_change")]
    public decimal ExtendedPercentChange { get; set; }
    [JsonProperty("extended_price")]
    public decimal ExtendedPrice { get; set; }
    [JsonProperty("extended_timestamp")]
    public long ExtendedTimestamp { get; set; } 
}
public class FiftyTwoWeek
{
    [JsonProperty("low")]
    public decimal Low { get; set; }

    [JsonProperty("high")]
    public decimal High { get; set; }

    [JsonProperty("low_change")]
    public decimal LowChange { get; set; }

    [JsonProperty("high_change")]
    public decimal HighChange { get; set; }

    [JsonProperty("low_change_percent")]
    public decimal LowChangePercent { get; set; }

    [JsonProperty("high_change_percent")]
    public decimal HighChangePercent { get; set; }

    [JsonProperty("range")]
    public string Range { get; set; }
}