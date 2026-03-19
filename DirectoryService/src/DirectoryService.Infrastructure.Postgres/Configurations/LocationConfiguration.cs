using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public static class Index
{
    public const string NAME = "ix_location_name";
    public const string ADDRESSFULLPATH = "ix_location_address_full_path";
}

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");

        builder.HasKey(l => l.Id).HasName("pk_locations");

        builder.Property(l => l.Id)
            .HasConversion(l => l.Value, id => new LocationId(id))
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.OwnsOne(
            l => l.Name, lb =>
            {
                lb.Property(l => l.Value)
                    .HasMaxLength(LengthConstants.LENGTH120)
                    .IsRequired()
                    .HasColumnName("name");

                lb.HasIndex(l => l.Value)
                    .IsUnique()
                    .HasDatabaseName(Index.NAME);
            });

        builder.OwnsOne(l => l.Address, ab =>
        {
            ab.ToJson("address");

            ab.Property(a => a.Country).IsRequired();
            ab.Property(a => a.City).IsRequired();
            ab.Property(a => a.Street).IsRequired();
            ab.Property(a => a.Building);
            ab.Property(a => a.OfficeNumber);
        });

        builder.Property(l => l.FullPath)
            .IsRequired()
            .HasMaxLength(1000)
            .HasColumnName("full_path");

        builder.ComplexProperty(l => l.Timezone, tzb =>
        {
            tzb.Property(t => t.Value)
                .IsRequired()
                .HasColumnName("timezone");
        });

        builder.Property(l => l.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(l => l.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(l => l.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.HasMany(l => l.DepartmentLocations)
            .WithOne()
            .HasForeignKey(dl => dl.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(l => l.DepartmentLocations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(l => l.FullPath)
            .IsUnique()
            .HasDatabaseName(Index.ADDRESSFULLPATH);
    }
}