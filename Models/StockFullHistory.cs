using System;
using System.Collections.Generic;

namespace TradingSimulator_Backend.Models
{
    public class StockFullHistory
    {
        public string Symbol { get; set; }
        public string Interval { get; set; }
        public string Currency { get; set; }
        public List<StockFullHistoryPoint> Values { get; set; } = new();
    }
}


