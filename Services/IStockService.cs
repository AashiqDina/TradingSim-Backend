using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradingSimulator_Backend.Models;


public interface IStockService
{
    Task<ApiResponse<decimal?>?> GetStockPriceAsync(string symbol);
    Task<Dictionary<string, ApiResponse<decimal?>?>> GetMultipleStockPricesAsync(List<string> symbols);
    Task<ApiResponse<string?>?> GetStockImage(string symbol);
    Task<ApiResponse<string?>?> ConvertSymbolToName(string symbol);
    // Task<(DateTime? LastUpdated, decimal? LowPrice, decimal? HighPrice, string? FiftyTwoWeekRange, decimal? ClosePrice, decimal? PercentChange)> GetQuickData(string symbol);
    Task<ApiResponse<StockApiInfo>?> FetchStockInfo(string symbol);
    Task<ApiResponse<CompanyProfile?>?> GetStockCompanyProfile(string symbol);
    Dictionary<string, DateTime> GetAllLastUpdated();
    DateTime GetStockLastUpdated(string symbol);
    DateTime GetStockInfoLastUpdated(string symbol);
    Task<ApiResponse<StockFullHistory?>?> GetFullStockHistory(string symbol);
    string[] getTrendingList();
    void updateTrendingMap(string symbol);
}
