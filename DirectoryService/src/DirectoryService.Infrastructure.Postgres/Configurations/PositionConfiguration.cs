using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");

        builder.HasKey(p => p.Id).HasName("pk_positions");

        builder.Property(p => p.Id)
            .HasConversion(p => p.Value, id => new PositionId(id))
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.ComplexProperty(p => p.Name, nb =>
        {
            nb.Property(n => n.Value)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH100)
                .HasColumnName("name");
        });

        builder.OwnsOne(p => p.Description, db =>
        {
            db.Property(p => p.Value)
                .IsRequired(false)
                .HasMaxLength(LengthConstants.LENGTH1000)
                .HasColumnName("description");
        });

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(p => p.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.HasMany(p => p.DepartmentPositions)
            .WithOne()
            .HasForeignKey(dp => dp.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(p => p.DepartmentPositions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}