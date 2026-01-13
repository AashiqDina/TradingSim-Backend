using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

using TradingSimulatorBackend.Models;

public class StockHistory
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("stockId")]
        public long StockId { get; set; }

        [ForeignKey("StockId")]
        [System.Text.Json.Serialization.JsonIgnore]
        public Stock Stock { get; set; } = null!;

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("quantity")]
        public decimal Quantity { get; set; }
    }

