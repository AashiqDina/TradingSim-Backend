namespace TradingSimulator_Backend.Services
{
    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using System;
    using TradingSimulator_Backend.Models;
    using TradingSimulator_Backend.Data;
    using System.Collections.Concurrent;


    public class StockService : IStockService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly string _apiKey = Environment.GetEnvironmentVariable("STOCK_API_KEY");
        
        private static ConcurrentDictionary<string, (decimal? Price, DateTime Timestamp)> _stockCache = new ConcurrentDictionary<string, (decimal? Price, DateTime Timestamp)>();
        private static ConcurrentDictionary<string, (string? Logo, string? Name)> _stockImageCache = new ConcurrentDictionary<string, (string? Logo, string? Name)>();
        private static ConcurrentDictionary<string, (StockApiInfo? Info, DateTime Timestamp)> _stockApiInfoCache = new ConcurrentDictionary<string, (StockApiInfo? Info, DateTime Timestamp)>();
        private static ConcurrentDictionary<string, CompanyProfile?> _CompanyDetailsCache = new ConcurrentDictionary<string, CompanyProfile?>();
        private static ConcurrentDictionary<string, (StockFullHistory? History, DateTime Timestamp)> _StockFullHistoryCache = new ConcurrentDictionary<string, (StockFullHistory? History, DateTime Timestamp)>();
        private static (ConcurrentDictionary<string, int> Map, DateTime Timestamp) TrendingMap = (new ConcurrentDictionary<string, int>(), DateTime.UtcNow);
        public static string[] TrendingList = new string[] { "AAPL", "MSFT", "GOOG", "AMZN", "TSLA", "NVDA", "META", "NFLX", "AMD", "JPM" };

        public StockService(HttpClient httpClient, AppDbContext appDbContext)
        {
            _httpClient = httpClient;
            _context = appDbContext;
        }

        public string[] getTrendingList(){
            return TrendingList;
        }

        public async Task<ApiResponse<bool>> DeleteStockLogo(string symbol)
        {
            // 1. Remove from cache if present
            if (_stockImageCache.ContainsKey(symbol))
            {
                _stockImageCache.Remove(symbol, out _);
            }
        
            // 2. Find DB record
            var dbStock = await _context.StockLogoName.FindAsync(symbol);
        
            if (dbStock == null)
            {
                return new ApiResponse<bool>
                {
                    Data = false,
                    HasError = true,
                    ErrorCode = "NOT_FOUND"
                };
            }
        
            // 3. Remove from DB
            _context.StockLogoName.Remove(dbStock);
            await _context.SaveChangesAsync();
        
            return new ApiResponse<bool>
            {
                Data = true,
                HasError = false,
                ErrorCode = null
            };
        }


        public void updateTrendingMap(string symbol)
        {
            if (DateTime.UtcNow.Date > TrendingMap.Timestamp.Date)
            {
                TrendingMap.Map.Clear();
                TrendingMap = (TrendingMap.Map, DateTime.UtcNow);
                TrendingList = new string[] { "AAPL", "MSFT", "GOOG", "AMZN", "TSLA", "NVDA", "META", "NFLX", "AMD", "JPM" };
            }
            if (TrendingMap.Map.ContainsKey(symbol))
            {
                TrendingMap.Map[symbol]++;
                updateTrendingList(symbol);
            }
            else
            {
                TrendingMap.Map[symbol] = 1;
                updateTrendingList(symbol);
            }
        }

        public static void updateTrendingList(string symbol){

            if (TrendingList.Contains(symbol)){
                return;
            }

            for (int i = 0; i < TrendingList.Length; i++)
            {
                if (!TrendingMap.Map.ContainsKey(TrendingList[i]))
                {
                    TrendingList[i] = symbol;
                    return;
                }
            }
            
            int smallestCount = int.MaxValue;
            int smallestStock = -1;


            for (int i = 0; i < TrendingList.Length; i++)
            {

                int count = TrendingMap.Map[TrendingList[i]];

                if (count < smallestCount)
                {
                    smallestCount = count;
                    smallestStock = i;
                }
            }

            TrendingList[smallestStock] = symbol;
        }

        public Dictionary<string, DateTime> GetAllLastUpdated()
        {
            return _stockCache.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Timestamp
            );
        }

        public DateTime GetStockLastUpdated(string symbol){
            return _stockCache[symbol].Timestamp;
        }

        public DateTime GetStockInfoLastUpdated(string symbol){
            var timestamp = _stockApiInfoCache[symbol].Info.Timestamp;
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            DateTime localTime = dateTimeOffset.LocalDateTime;

            return localTime;
        }

        public async Task<ApiResponse<StockFullHistory?>> GetFullStockHistory(string symbol){

            if(_StockFullHistoryCache.ContainsKey(symbol) && DateTime.Now - _StockFullHistoryCache[symbol].Timestamp < TimeSpan.FromMinutes(480)){
                return new ApiResponse<StockFullHistory?>{
                    Data = _StockFullHistoryCache[symbol].History,
                    HasError = false,
                    ErrorCode = null
                };
            }

            var History = await FetchStockFullHistory(symbol);

            if(History != null && !History.HasError){
                _StockFullHistoryCache[symbol] = (History.Data, DateTime.Now);
            }

            return History;
        }

        public async Task<ApiResponse<decimal?>> GetStockPriceAsync(string symbol)
        {
            if (_stockCache.ContainsKey(symbol) && DateTime.Now - _stockCache[symbol].Timestamp < TimeSpan.FromMinutes(480))
            {
                return new ApiResponse<decimal?>{
                    Data = _stockCache[symbol].Price,
                    HasError = false,
                    ErrorCode = null
                };
            }

            var response = await FetchStockPriceFromApi(symbol);

            if(!response.HasError){
                _stockCache[symbol] = (response.Data, DateTime.Now);
            }

            return response;
        }

        public async Task<Dictionary<string, ApiResponse<decimal?>>> GetMultipleStockPricesAsync(List<string> symbols)
        {
            var stockPrices = new Dictionary<string, ApiResponse<decimal?>>();

            foreach (var symbol in symbols)
            {
                if (_stockCache.ContainsKey(symbol) && DateTime.Now - _stockCache[symbol].Timestamp < TimeSpan.FromMinutes(480))
                {
                    stockPrices[symbol] = new ApiResponse<decimal?>
                    {
                        Data = _stockCache[symbol].Price,
                        HasError = false,
                        ErrorCode = null
                    };
                }
                else
                {
                    var response = await FetchStockPriceFromApi(symbol);
                    stockPrices[symbol] = response;

                    if (!response.HasError)
                    {
                        _stockCache[symbol] = (response.Data, DateTime.Now);
                    }
                }
            }

            return stockPrices;
        }

        public async Task<ApiResponse<string?>> GetStockImage(string symbol)
        {
            if (_stockImageCache.ContainsKey(symbol)){
                Console.WriteLine($"Before: {_stockImageCache[symbol].Logo}");
                return new ApiResponse<string?>{
                    Data = _stockImageCache[symbol].Logo,
                    HasError = false,
                    ErrorCode = null  
                };
            }

            var dbStock = await _context.StockLogoName.FindAsync(symbol);

            if (dbStock != null && !string.IsNullOrWhiteSpace(dbStock.Logo))
            {
                Console.WriteLine($"Found {symbol} name in DB");
                return new ApiResponse<string?>{
                    Data = dbStock.Logo,
                    HasError = false,
                    ErrorCode = null  
                };
            }

            var result = await FetchLogoAndNameFromApi(symbol);
            
            if (result.Data == null || string.IsNullOrWhiteSpace(result.Data.Value.Item1)){
                Console.WriteLine($"No image found for {symbol}.");
                return new ApiResponse<string?>{
                    Data = null,
                    HasError = true,
                    ErrorCode = result.ErrorCode
                };
            }

            if (result.Data != null){
                _stockImageCache[symbol] = (result.Data.Value.Item1, result.Data.Value.Item2);
                dbStock = await _context.StockLogoName.FindAsync(symbol);
                if (dbStock == null)
                    {
                        var stock = new StockLogoName
                        {
                            Symbol = symbol,
                            Name = null,
                            Logo = result.Data.Value.Item1
                        };
                        _context.StockLogoName.Add(stock);
                    }
                    else
                    {
                        dbStock.Logo = result.Data.Value.Item1;
                    }
                    await _context.SaveChangesAsync();
            }

            return new ApiResponse<string?>{
                    Data = result.Data.Value.Item1,
                    HasError = false,
                    ErrorCode = result.ErrorCode
            };
        }

        public async Task<ApiResponse<string?>?> ConvertSymbolToName(string symbol)
        {
            if(_stockApiInfoCache.ContainsKey(symbol)){
                return new ApiResponse<string?>{
                    Data = _stockApiInfoCache[symbol].Info.Name,
                    HasError = false,
                    ErrorCode = null
                };
            }

            var dbStock = await _context.StockLogoName.FindAsync(symbol);

            if (dbStock != null && !string.IsNullOrWhiteSpace(dbStock.Name))
                {
                    Console.WriteLine($"Found {symbol} name in DB");
                    return new ApiResponse<string?>{
                        Data = dbStock.Name,
                        HasError = false,
                        ErrorCode = null
                    };
                }

            var result = await FetchStockInfoFromApi(symbol);

            if(result.Data == null || string.IsNullOrWhiteSpace(result.Data?.Name)){
                Console.WriteLine("Unable to find name, returning symbol");
                return new ApiResponse<string?>{
                    Data = symbol,
                    HasError = true,
                    ErrorCode = 404
                };
            }

            if(result.Data != null){
                Console.WriteLine($"Fetched {symbol} → Name: '{result.Data?.Name}'");
                _stockApiInfoCache[symbol] = (result.Data, DateTime.Now);
                dbStock = await _context.StockLogoName.FindAsync(symbol);
                if (dbStock == null)
                    {
                        var stock = new StockLogoName
                        {
                            Symbol = symbol,
                            Name = result.Data?.Name,
                            Logo = null
                        };
                        _context.StockLogoName.Add(stock);
                    }
                    else
                    {
                        dbStock.Name = result.Data?.Name;
                    }
                    await _context.SaveChangesAsync();
            }

            return new ApiResponse<string?>{
                Data = result.Data?.Name,
                HasError = false,
                ErrorCode = null
            };
            
        }

        // public async Task<(DateTime? LastUpdated, decimal? LowPrice, decimal? HighPrice, string? FiftyTwoWeekRange, decimal? ClosePrice, decimal? PercentChange)> GetQuickData(string symbol)
        // {
        //     if (StockInfoDateTime == null)
        //     {
        //         StockInfoDateTime = DateTime.Now;
        //     }

        //     if (_stockApiInfoCache.ContainsKey(symbol) && (DateTime.Now - StockInfoDateTime) < TimeSpan.FromMinutes(480))
        //     {
        //         var cachedData = _stockApiInfoCache[symbol];
        //         Console.WriteLine(cachedData.FiftyTwoWeek);
        //         return (TryParseDateTime(cachedData.Datetime), cachedData.Low, cachedData.High, cachedData.FiftyTwoWeek?.Range, cachedData.Close, cachedData.PercentChange);
        //     }

        //     var result = await FetchStockInfoFromApi(symbol);

        //     if (result == null)
        //     {
        //         Console.WriteLine("Unable to find information");
        //         return (null, null, null, null, null, null);
        //     }

        //     _stockApiInfoCache[symbol] = result;
        //     StockInfoDateTime = DateTime.Now;
        //     return (TryParseDateTime(result.Datetime), result.Low, result.High, result.FiftyTwoWeek?.Range, result.Close, result.PercentChange);
        // }

        public async Task<ApiResponse<CompanyProfile?>> GetStockCompanyProfile(string symbol){
            if(_CompanyDetailsCache.ContainsKey(symbol)){
                var cachedData = _CompanyDetailsCache[symbol];
                return new ApiResponse<CompanyProfile?>{
                    Data = cachedData,
                    HasError = false,
                    ErrorCode = null
                };
            }

            var result = await FetchCompanyProfile(symbol);

            if(result != null && !result.HasError){
                _CompanyDetailsCache[symbol] = result.Data;
            }

            return result;
        }

        public async Task<ApiResponse<StockApiInfo?>?> FetchStockInfo(string symbol){

            if (_stockApiInfoCache.ContainsKey(symbol) && (DateTime.Now - _stockApiInfoCache[symbol].Timestamp) < TimeSpan.FromMinutes(480))
            {
                var cachedData = _stockApiInfoCache[symbol];
                return new ApiResponse<StockApiInfo?>{
                    Data = cachedData.Info,
                    HasError = false,
                    ErrorCode = null
                };
                
            }

            var result = await FetchStockInfoFromApi(symbol);

            if(!result.HasError){
                _stockApiInfoCache[symbol] = (result.Data, DateTime.Now);
            }

            return result;

        }

        private DateTime? TryParseDateTime(string datetime)
        {
            if (DateTime.TryParse(datetime, out var parsedDate))
            {
                return parsedDate;
            }
            return null; 
        }





        private async Task<ApiResponse<decimal?>> FetchStockPriceFromApi(string symbol)
        {
            var url = $"https://api.twelvedata.com/price?symbol={symbol}&apikey={_apiKey}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode){
                return new ApiResponse<decimal?>{
                    Data = null,
                    HasError = true,
                    ErrorCode = (int)response.StatusCode
                };
            }

            var json = await response.Content.ReadAsStringAsync();

            var data = JObject.Parse(json);

            if (data["status"]?.ToString() == "error")
            {
                int errorCode = data["code"]?.Value<int>() ?? -1;

                Console.WriteLine($"API error for {symbol}: {json}");
                return new ApiResponse<decimal?>{
                    Data = null,
                    HasError = true,
                    ErrorCode = errorCode
                };
            }

            try
            {
                var price = data["price"]?.Value<decimal>() ?? 0;
                return new ApiResponse<decimal?>{
                    Data = price,
                    HasError = false,
                    ErrorCode = null
                };
            }
            catch (JsonException)
            {
                return new ApiResponse<decimal?>{
                    Data = null,
                    HasError = true,
                    ErrorCode = -1
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error fetching stock for {symbol}: {ex.Message}");
                return new ApiResponse<decimal?>{
                    Data = null,
                    HasError = true,
                    ErrorCode = -2
                };
            }
        }

        private async Task<ApiResponse<(string?, string?)?>> FetchLogoAndNameFromApi(string symbol)
        {
            var encodedSymbol = Uri.EscapeDataString(symbol);
            var url = $"https://api.twelvedata.com/logo?symbol={symbol}&apikey={_apiKey}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return new ApiResponse<(string?, string?)?>{
                    Data = null,
                    HasError = true,
                    ErrorCode = (int)response.StatusCode
                };
            }

            var json = await response.Content.ReadAsStringAsync();

            var data = JObject.Parse(json);

            if (data["status"]?.ToString() == "error")
            {
                int errorCode = data["code"]?.Value<int>() ?? -1;

                Console.WriteLine($"API error for {symbol}: {json}");
                return new ApiResponse<(string?, string?)?>
                {
                    Data = null,
                    HasError = true,
                    ErrorCode = errorCode
                };
            }

            string logo = data["logo_base"]?.ToString()
                        ?? data["url"]?.ToString()
                        ?? data["logo_quote"]?.ToString();

            string name = data["meta"]?["name"]?.ToString()
                        ?? data["meta"]?["symbol"]?.ToString()
                        ?? "Unknown";

            return new ApiResponse<(string?, string?)?>{
                    Data = (logo, name),
                    HasError = false,
                    ErrorCode = null
                };
        }

        private async Task<ApiResponse<StockApiInfo?>?> FetchStockInfoFromApi(string symbol){
            var url = $"https://api.twelvedata.com/quote?symbol={symbol}&apikey={_apiKey}";
            var response = await _httpClient.GetAsync(url);

            if(!response.IsSuccessStatusCode){
                Console.WriteLine($"Error: {response.StatusCode}");
                return new ApiResponse<StockApiInfo?>{
                    Data = null,
                    HasError = true,
                    ErrorCode = (int)response.StatusCode
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Raw API Response: {json}");

            var data = JObject.Parse(json);

            if (data["status"]?.ToString() == "error")
            {
                int errorCode = data["code"]?.Value<int>() ?? -1;

                Console.WriteLine($"API error for {symbol}: {json}");
                return new ApiResponse<StockApiInfo?>
                {
                    Data = null,
                    HasError = true,
                    ErrorCode = errorCode
                };
            }

            try{
                var stockInfo = JsonConvert.DeserializeObject<StockApiInfo>(json);

                return new ApiResponse<StockApiInfo?>{
                    Data = stockInfo,
                    HasError = false,
                    ErrorCode = null
                };
            }
            catch{
                return new ApiResponse<StockApiInfo?>{
                    Data = null,
                    HasError = true,
                    ErrorCode = -1
                };
            }
        }

        private async Task<ApiResponse<CompanyProfile?>?> FetchCompanyProfile(string symbol){
            var url = $"https://api.twelvedata.com/profile?symbol={symbol}&apikey={_apiKey}";
            var response = await _httpClient.GetAsync(url);

            if(!response.IsSuccessStatusCode){
                Console.WriteLine($"Error: {response.StatusCode}");
                return new ApiResponse<CompanyProfile?>{
                    Data = null,
                    HasError = true,
                    ErrorCode = (int)response.StatusCode
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Raw Company Profile API Response: {json}");

            var data = JObject.Parse(json);

            if (data["status"]?.ToString() == "error")
            {
                int errorCode = data["code"]?.Value<int>() ?? -1;

                Console.WriteLine($"API error for {symbol}: {json}");
                
                return new ApiResponse<CompanyProfile?>
                {
                    Data = null,
                    HasError = true,
                    ErrorCode = errorCode
                };
            }

            try{
                var CompanyProfile = JsonConvert.DeserializeObject<CompanyProfile>(json);
                return new ApiResponse<CompanyProfile?>{
                    Data = CompanyProfile,
                    HasError = false,
                    ErrorCode = null
                };
            }
            catch{
                return new ApiResponse<CompanyProfile?>{
                    Data = null,
                    HasError = true,
                    ErrorCode = -1
                };
            }  
        }

        private async Task<ApiResponse<StockFullHistory?>> FetchStockFullHistory(string symbol){
            var url = $"https://api.twelvedata.com/time_series?symbol={symbol}&interval=1day&outputsize=5000&apikey={_apiKey}";
            var response = await _httpClient.GetAsync(url);

            if(!response.IsSuccessStatusCode){
                Console.WriteLine($"Error: {response.StatusCode}");
                return new ApiResponse<StockFullHistory?>{
                    Data = null,
                    HasError = true,
                    ErrorCode = (int)response.StatusCode
                };
            }

            try{
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<TimeSeriesApiResponse>(json);

                if(result?.Status == "error")
                {
                    Console.WriteLine($"API error for {symbol}: {json}");
                    return new ApiResponse<StockFullHistory?>
                    {
                        Data = null,
                        HasError = true,
                        ErrorCode = result.Code ?? -1
                    };
                }

                    var history = new StockFullHistory{
                    Symbol = symbol,
                    Interval = null,
                    Currency = null,
                    Values = result?.Values ?? new List<StockFullHistoryPoint>()
                };

                return new ApiResponse<StockFullHistory?>{
                    Data = history,
                    HasError = false,
                    ErrorCode = null
                };

            }
            catch(Exception ex){
                Console.WriteLine($"Deserialization error for {symbol}: {ex.Message}");
                return new ApiResponse<StockFullHistory?>{
                        Data = null,
                        HasError = true,
                        ErrorCode = -1
                    };
            }
        }
    }
}




