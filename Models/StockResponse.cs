namespace TradingSimulator_Backend.Models{
    using Newtonsoft.Json;
    using System;


    public class StockResponse
    {
        [JsonProperty("price")]
        public decimal? Price { get; set; }
    }
}