using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TradingSimulator_Backend.Models;

namespace TradingSimulator_Backend.Models
{
    public class StockHistory
    {
        [Column("Id")]
        public long Id { get; set; }

        [Column("StockId")]
        public long StockId { get; set; }

        [ForeignKey("StockId")]
        [JsonIgnore]
        public Stock Stock { get; set; } = null!;

        [Column("Timestamp")]
        public DateTime Timestamp { get; set; }

        [Column("Price")]
        public decimal Price { get; set; }

        [Column("Quantity")]
        public decimal Quantity { get; set; }
    }
}
