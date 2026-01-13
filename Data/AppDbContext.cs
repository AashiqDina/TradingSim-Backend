using Microsoft.EntityFrameworkCore;
using TradingSimulator_Backend.Models;

namespace TradingSimulator_Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options) { }

        // Core tables
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Portfolio> Portfolios { get; set; } = null!;
        public DbSet<Stock> Stocks { get; set; } = null!;
        public DbSet<StockLogoName> StockLogoName { get; set; } = null!;
        public DbSet<StockHistory> StockHistory { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Table names
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Portfolio>().ToTable("portfolios");
            modelBuilder.Entity<Stock>().ToTable("stocks");
            modelBuilder.Entity<StockLogoName>().ToTable("stocklogoname");
            modelBuilder.Entity<StockHistory>().ToTable("stockhistory");

            // User -> Friends (owned entity)
            modelBuilder.Entity<User>().OwnsOne(u => u.Friends, f =>
            {
                f.OwnsMany(x => x.FriendsList, fl =>
                {
                    fl.ToTable("users_FriendsList");
                    fl.WithOwner().HasForeignKey("UserId"); // FK to parent user
                    fl.HasKey("UserId", "Id");
                });

                f.OwnsMany(x => x.SentRequests, sr =>
                {
                    sr.ToTable("users_SentRequests");
                    sr.WithOwner().HasForeignKey("UserId");
                    sr.HasKey("UserId", "Id");
                });

                f.OwnsMany(x => x.ReceivedRequests, rr =>
                {
                    rr.ToTable("users_ReceivedRequests");
                    rr.WithOwner().HasForeignKey("UserId");
                    rr.HasKey("UserId", "Id");
                });
            });

            // Portfolio <-> User
            modelBuilder.Entity<Portfolio>()
                .HasOne(p => p.User)
                .WithOne(u => u.Portfolio)
                .HasForeignKey<Portfolio>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Stock <-> Portfolio
            modelBuilder.Entity<Stock>()
                .HasOne(s => s.Portfolio)
                .WithMany(p => p.Stocks)
                .HasForeignKey(s => s.PortfolioId)
                .OnDelete(DeleteBehavior.Cascade);

            // StockHistory <-> Stock
            modelBuilder.Entity<StockHistory>()
                .HasOne(h => h.Stock)
                .WithMany(s => s.History)
                .HasForeignKey(h => h.StockId)
                .OnDelete(DeleteBehavior.Cascade);

            // StockLogoName primary key
            modelBuilder.Entity<StockLogoName>()
                .HasKey(s => s.Symbol);
        }
    }
}
