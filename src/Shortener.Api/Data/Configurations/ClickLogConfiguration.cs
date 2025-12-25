using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shortener.Api.Entities;

namespace Shortener.Api.Data.Configurations;

public class ClickLogConfiguration : IEntityTypeConfiguration<ClickLog>
{
    public void Configure(EntityTypeBuilder<ClickLog> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.IpAddress)
               .HasMaxLength(45);
               
        builder.Property(e => e.UserAgent)
               .HasMaxLength(512);
               
        builder.Property(e => e.Referer)
               .HasMaxLength(256);
               
        builder.Property(e => e.ClickedAt)
               .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(e => e.LinkId)
               .HasDatabaseName("IX_ClickLogs_LinkId");
               
        builder.HasIndex(e => e.ClickedAt)
               .HasDatabaseName("IX_ClickLogs_ClickedAt");
        
        builder.HasOne(e => e.Link)
               .WithMany(l => l.ClickLogs)
               .HasForeignKey(e => e.LinkId)
               .OnDelete(DeleteBehavior.Cascade)
               .HasConstraintName("FK_ClickLogs_Links_LinkId");
    }
}