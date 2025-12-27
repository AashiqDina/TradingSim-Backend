protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

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

        f.OwnsMany(f => f.SentRequests);
        f.OwnsMany(f => f.ReceivedRequests);
        f.OwnsMany(f => f.FriendsList);
    });
}
