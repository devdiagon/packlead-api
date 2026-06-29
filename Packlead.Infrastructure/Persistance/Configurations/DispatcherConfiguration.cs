using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Packlead.Domain.Entities;

namespace Packlead.Infrastructure.Persistence.Configurations;

public class DispatcherConfiguration : IEntityTypeConfiguration<Dispatcher>
{
    public void Configure(EntityTypeBuilder<Dispatcher> builder)
    {
        builder.ToTable("Dispatchers");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.FirebaseUid)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(d => d.FirebaseUid)
            .IsUnique();

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Vehicle)
            .HasMaxLength(100);

        builder.Property(d => d.LicensePlate)
            .HasMaxLength(20);

        builder.Property(d => d.State)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}