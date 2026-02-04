using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");

        builder.HasKey(dp => dp.Id).HasName("pk_department_positions");

        builder.Property(dp => dp.Id)
            .HasConversion(dp => dp.Value, id => new DepartmentPositionId(id))
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(dp => dp.DepartmentId)
            .HasConversion(d => d.Value, id => new DepartmentId(id))
            .IsRequired()
            .HasColumnName("department_id");

        builder.Property(dp => dp.PositionId)
            .HasConversion(p => p.Value, id => new PositionId(id))
            .IsRequired()
            .HasColumnName("position_id");
    }
}