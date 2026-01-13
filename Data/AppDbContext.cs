using Microsoft.EntityFrameworkCore;
using TradingSimulator_Backend.Models;

namespace TradingSimulator_Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Portfolio> Portfolios { get; set; } = null!;
        public DbSet<Stock> Stocks { get; set; } = null!;
        public DbSet<StockLogoName> StockLogoName { get; set; } = null!;
        public DbSet<StockHistory> StockHistory { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Portfolio>().ToTable("portfolios");
            modelBuilder.Entity<Stock>().ToTable("stocks");
            modelBuilder.Entity<StockLogoName>().ToTable("stocklogoname");
            modelBuilder.Entity<StockHistory>().ToTable("stockhistory");

            modelBuilder.Entity<User>(u =>
            {
                u.OwnsOne(u => u.Friends, f =>
                {
                    f.OwnsMany(x => x.FriendsList);
                    f.OwnsMany(x => x.SentRequests);
                    f.OwnsMany(x => x.ReceivedRequests);
                });
            });

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
        }
    }
}

// using Microsoft.EntityFrameworkCore;
// using TradingSimulator_Backend.Models;

// namespace TradingSimulator_Backend.Data
// {
//     public class AppDbContext : DbContext
//     {
//         public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

//         public DbSet<User> Users { get; set; }
//         public DbSet<Friends> Friends { get; set; } = null!;
//         public DbSet<Portfolio> Portfolios { get; set; }
//         public DbSet<Stock> Stocks { get; set; }
//         public DbSet<StockLogoName> StockLogoName { get; set; }
//         public DbSet<StockHistory> StockHistory { get; set; }

//         protected override void OnModelCreating(ModelBuilder modelBuilder)
//         {
//             modelBuilder.Entity<User>().ToTable("users");
//             modelBuilder.Entity<Friends>().ToTable("friends");
//             modelBuilder.Entity<Portfolio>().ToTable("portfolios");
//             modelBuilder.Entity<Stock>().ToTable("stocks");
//             modelBuilder.Entity<StockLogoName>().ToTable("stocklogoname");
//             modelBuilder.Entity<StockHistory>().ToTable("stockhistory");
        
//             // Friends setup
//             modelBuilder.Entity<Friends>(f =>
//             {
//                 f.HasOne(f => f.User)
//                  .WithOne()
//                  .HasForeignKey<Friends>(f => f.UserId)
//                  .OnDelete(DeleteBehavior.Cascade);
        
//                 f.OwnsMany(f => f.SentRequests);
//                 f.OwnsMany(f => f.ReceivedRequests);
//                 f.OwnsMany(f => f.FriendsList);
//             });
        
//             modelBuilder.Entity<Portfolio>()
//                 .HasOne(p => p.User)
//                 .WithOne()
//                 .HasForeignKey<Portfolio>(p => p.UserId)
//                 .OnDelete(DeleteBehavior.Cascade);
        
//             modelBuilder.Entity<StockLogoName>()
//                 .HasKey(s => s.Symbol);
        
//             modelBuilder.Entity<StockHistory>()
//                 .HasOne(h => h.Stock)
//                 .WithMany(s => s.History)
//                 .HasForeignKey(h => h.StockId)
//                 .OnDelete(DeleteBehavior.Cascade);
        
//             modelBuilder.Entity<Stock>()
//                 .HasOne(s => s.Portfolio)
//                 .WithMany(p => p.Stocks)
//                 .HasForeignKey(s => s.PortfolioId)
//                 .OnDelete(DeleteBehavior.Cascade);
//         }


//         // protected override void OnModelCreating(ModelBuilder modelBuilder)
//         // {
//         //     modelBuilder.Entity<User>().ToTable("users");
//         //     modelBuilder.Entity<Friends>().ToTable("friends");
//         //     modelBuilder.Entity<Portfolio>().ToTable("portfolios");
//         //     modelBuilder.Entity<Stock>().ToTable("stocks");
//         //     modelBuilder.Entity<StockLogoName>().ToTable("stocklogoname");
//         //     modelBuilder.Entity<StockHistory>().ToTable("stockhistory");

//         //     modelBuilder.Entity<User>()
//         //         .OwnsOne(u => u.Friends, f =>
//         //         {
//         //             f.OwnsMany(x => x.FriendsList);
//         //             f.OwnsMany(x => x.SentRequests);
//         //             f.OwnsMany(x => x.ReceivedRequests);
//         //         });

//         //     // modelBuilder.Entity<User>()
//         //     //     .HasOne(u => u.Friends)
//         //     //     .WithOne(f => f.User)
//         //     //     .HasForeignKey<Friends>(f => f.UserId);

//         //     modelBuilder.Entity<Friends>()
//         //         .HasOne(f => f.User)
//         //         .WithOne(u => u.Friends)
//         //         .HasForeignKey<Friends>(f => f.UserId);

//         //     modelBuilder.Entity<Portfolio>()
//         //         .HasOne(p => p.User)
//         //         .WithOne()
//         //         .HasForeignKey<Portfolio>(p => p.UserId)
//         //         .OnDelete(DeleteBehavior.Cascade);

//         //     modelBuilder.Entity<StockLogoName>()
//         //         .HasKey(s => s.Symbol);

//         //     modelBuilder.Entity<StockHistory>()
//         //         .HasOne(h => h.Stock)
//         //         .WithMany(s => s.History)
//         //         .HasForeignKey(h => h.StockId)
//         //         .OnDelete(DeleteBehavior.Cascade);

//         //     modelBuilder.Entity<Stock>()
//         //         .HasOne(s => s.Portfolio)
//         //         .WithMany(p => p.Stocks)
//         //         .HasForeignKey(s => s.PortfolioId)
//         //         .OnDelete(DeleteBehavior.Cascade);
            
//         // }
//     }
// }








