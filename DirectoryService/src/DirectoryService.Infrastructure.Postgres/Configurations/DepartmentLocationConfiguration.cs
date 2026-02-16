using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentLocations.ValueObjects;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations");

        builder.HasKey(dl => dl.Id).HasName("pk_department_locations");

        builder.Property(dl => dl.Id)
            .HasConversion(dl => dl.Value, id => new DepartmentLocationId(id))
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(dl => dl.DepartmentId)
            .HasConversion(d => d.Value, id => new DepartmentId(id))
            .IsRequired()
            .HasColumnName("department_id");

        builder.Property(dl => dl.LocationId)
            .HasConversion(l => l.Value, id => new LocationId(id))
            .IsRequired()
            .HasColumnName("location_id");
    }
}