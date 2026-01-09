using TradingSimulator_Backend.Models;

public class Portfolio{
    public long Id { get; set; }

    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public List<Stock> Stocks { get; set; } = new();

    public decimal TotalInvested =>
        Stocks.Sum(stock => stock.PurchasePrice * stock.Quantity);

    public decimal CurrentValue =>
        Stocks.Sum(stock => stock.TotalValue);

    public decimal ProfitLoss =>
        CurrentValue - TotalInvested;
}
