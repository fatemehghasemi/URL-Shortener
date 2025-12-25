using Microsoft.EntityFrameworkCore;
using Shortener.Api.Entities;
using Shortener.Api.Data.Configurations;

namespace Shortener.Api.Data;

public class ShortenerDbContext : DbContext
{
    public ShortenerDbContext(DbContextOptions<ShortenerDbContext> options) : base(options)
    {
    }

    public DbSet<Link> Links { get; set; }
    public DbSet<ClickLog> ClickLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new LinkConfiguration());
        modelBuilder.ApplyConfiguration(new ClickLogConfiguration());
    }
}