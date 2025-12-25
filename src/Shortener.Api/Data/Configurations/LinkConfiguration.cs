using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shortener.Api.Entities;

namespace Shortener.Api.Data.Configurations;

public class LinkConfiguration : IEntityTypeConfiguration<Link>
{
    public void Configure(EntityTypeBuilder<Link> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.OriginalUrl)
               .IsRequired()
               .HasMaxLength(2048);
               
        builder.Property(e => e.ShortCode)
               .IsRequired()
               .HasMaxLength(10);
               
        builder.Property(e => e.CreatedAt)
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
               
        builder.Property(e => e.IsActive)
               .HasDefaultValue(true);
               
        builder.Property(e => e.ClickCount)
               .HasDefaultValue(0);

        builder.HasIndex(e => e.ShortCode)
               .IsUnique()
               .HasDatabaseName("IX_Links_ShortCode");
               
        builder.HasIndex(e => e.CreatedAt)
               .HasDatabaseName("IX_Links_CreatedAt");
               
        builder.HasIndex(e => e.IsActive)
               .HasDatabaseName("IX_Links_IsActive");
    }
}