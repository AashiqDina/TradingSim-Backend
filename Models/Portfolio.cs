using TradingSimulator_Backend.Models; 

public class Portfolio
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; }
    
    public List<Stock> Stocks { get; set; } = new List<Stock>();

    public decimal TotalInvested => Stocks.Sum(stock => stock.PurchasePrice * stock.Quantity); 
    public decimal CurrentValue => Stocks.Sum(stock => stock.TotalValue);
    public decimal ProfitLoss => CurrentValue - TotalInvested; 
}
