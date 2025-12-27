using Microsoft.EntityFrameworkCore;
using TradingSimulator_Backend.Models;

namespace TradingSimulator_Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<StockLogoName> StockLogoName { get; set; }
        public DbSet<StockHistory> StockHistory { get; set; }
        public DbSet<Friends> Friends { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Portfolio>().ToTable("portfolios");
            modelBuilder.Entity<Stock>().ToTable("stocks");
            modelBuilder.Entity<StockLogoName>().ToTable("stocklogoname");
            modelBuilder.Entity<StockHistory>().ToTable("stockhistory");
            modelBuilder.Entity<Friends>().ToTable("friends");

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName().ToLower());
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.Name.ToLower());
                }
            }

            modelBuilder.Entity<Portfolio>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<Portfolio>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StockLogoName>()
                .HasKey(s => s.Symbol);

            modelBuilder.Entity<StockHistory>()
                .HasOne(h => h.Stock)
                .WithMany(s => s.History)
                .HasForeignKey(h => h.StockId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Stock>()
                .HasOne(s => s.Portfolio)
                .WithMany(p => p.Stocks)
                .HasForeignKey(s => s.PortfolioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Friends>(f =>
            {
                f.HasOne(f => f.User)
                 .WithOne()
                 .HasForeignKey<Friends>(f => f.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                f.OwnsMany(f => f.SentRequests).ToTable("friends_sentrequests");
                f.OwnsMany(f => f.ReceivedRequests).ToTable("friends_receivedrequests");
                f.OwnsMany(f => f.FriendsList).ToTable("friends_friendslist");

                f.OwnsMany(f => f.SentRequests, sr =>
                {
                    foreach (var prop in sr.OwnedEntityType.GetProperties())
                        prop.SetColumnName(prop.Name.ToLower());
                });
                f.OwnsMany(f => f.ReceivedRequests, rr =>
                {
                    foreach (var prop in rr.OwnedEntityType.GetProperties())
                        prop.SetColumnName(prop.Name.ToLower());
                });
                f.OwnsMany(f => f.FriendsList, fl =>
                {
                    foreach (var prop in fl.OwnedEntityType.GetProperties())
                        prop.SetColumnName(prop.Name.ToLower());
                });
            });
        }
    }
}
