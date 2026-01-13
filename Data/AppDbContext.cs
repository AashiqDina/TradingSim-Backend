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
        public DbSet<UserFriend> UsersFriendsList { get; set; } = null!;
        public DbSet<UserSentRequest> UsersSentRequests { get; set; } = null!;
        public DbSet<UserReceivedRequest> UsersReceivedRequests { get; set; } = null!;
        public DbSet<Portfolio> Portfolios { get; set; } = null!;
        public DbSet<Stock> Stocks { get; set; } = null!;
        public DbSet<StockLogoName> StockLogoName { get; set; } = null!;
        public DbSet<StockHistory> StockHistory { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Table names
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<UserFriend>().ToTable("users_FriendsList");
            modelBuilder.Entity<UserSentRequest>().ToTable("users_SentRequests");
            modelBuilder.Entity<UserReceivedRequest>().ToTable("users_ReceivedRequests");
            modelBuilder.Entity<Portfolio>().ToTable("portfolios");
            modelBuilder.Entity<Stock>().ToTable("stocks");
            modelBuilder.Entity<StockLogoName>().ToTable("stocklogoname");
            modelBuilder.Entity<StockHistory>().ToTable("stockhistory");

            // ----------------- FRIENDS & REQUESTS ----------------- //

            // UserFriend
            modelBuilder.Entity<UserFriend>()
                .HasKey(f => new { f.FriendsUserId, f.Id }); // Composite key

            modelBuilder.Entity<UserFriend>()
                .HasOne(f => f.FriendsUser)
                .WithMany(u => u.FriendsList)
                .HasForeignKey(f => f.FriendsUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserSentRequest
            modelBuilder.Entity<UserSentRequest>()
                .HasKey(s => new { s.FriendsUserId, s.Id });

            modelBuilder.Entity<UserSentRequest>()
                .HasOne(s => s.FriendsUser)
                .WithMany(u => u.SentRequests)
                .HasForeignKey(s => s.FriendsUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserReceivedRequest
            modelBuilder.Entity<UserReceivedRequest>()
                .HasKey(r => new { r.FriendsUserId, r.Id });

            modelBuilder.Entity<UserReceivedRequest>()
                .HasOne(r => r.FriendsUser)
                .WithMany(u => u.ReceivedRequests)
                .HasForeignKey(r => r.FriendsUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ----------------- PORTFOLIO & STOCKS ----------------- //

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
