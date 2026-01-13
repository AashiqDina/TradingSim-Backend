namespace TradingSimulator_Backend.Models
{
    using Newtonsoft.Json;
    
    public class StockResponseNameLogo
    {
        [JsonProperty("meta")]
        public MetaInfo Meta { get; set; }

        [JsonProperty("url")]
        public string url { get; set; }
    }

    public class MetaInfo
    {
        [JsonProperty("symbol")]
        public string symbol { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("currency")]
        public string currency { get; set; }

        [JsonProperty("exchange")]
        public string exchange { get; set; }

        [JsonProperty("mic_code")]
        public string micCode { get; set; }

        [JsonProperty("exchange_timezone")]
        public string exchangeTimezone { get; set; }
    }
}


