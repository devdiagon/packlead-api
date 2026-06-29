using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Packlead.Domain.Entities;

namespace Packlead.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.ClientName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.ClientPhoneNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(o => o.Address)
            .HasMaxLength(300);

        builder.Property(o => o.Zone)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.State)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(o => o.DeliveryDate)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        // Location como owned entity: columnas Lat/Lng embebidas en Orders (sin tabla)
        builder.OwnsOne(o => o.Location, loc =>
        {
            loc.Property(l => l.Lat).HasColumnName("Lat").IsRequired();
            loc.Property(l => l.Lng).HasColumnName("Lng").IsRequired();
        });

        // FK nullable hacia Dispatcher
        builder.HasOne<Dispatcher>()
            .WithMany()
            .HasForeignKey(o => o.DispatcherId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}