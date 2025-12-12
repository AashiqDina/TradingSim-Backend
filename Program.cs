using Microsoft.EntityFrameworkCore;
using TradingSimulator_Backend.Data;
using TradingSimulator_Backend.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", 
        policy =>
        {
            policy.WithOrigins("http://localhost:5048")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddSwaggerGen(options =>
{
    options.AddServer(new Microsoft.OpenApi.Models.OpenApiServer
    {
        Url = "http://localhost:3000",
        Description = "API Server"
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddHttpClient<StockService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddHttpClient<NewsService>();


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowAll");
app.UseSession();
app.UseAuthorization();
app.MapControllers();

app.Run();

