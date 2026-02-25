using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public static class DepartmentIndex
{
    public const string NAME = "ix_department_name";
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(d => d.Id).HasName("pk_departments");

        builder.Property(d => d.Id)
            .HasConversion(d => d.Value, id => new DepartmentId(id))
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.ComplexProperty(d => d.Name, nb =>
        {
            nb.Property(n => n.Value)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH150)
                .HasColumnName("name");
        });

        builder.ComplexProperty(d => d.Identifier, ib =>
        {
            ib.Property(i => i.Value)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH150)
                .HasColumnName("identifier");
        });

        builder.Property(d => d.ParentId)
            .HasConversion(p => p!.Value, pid => new DepartmentId(pid))
            .IsRequired(false)
            .HasColumnName("parent_id");

        builder.ComplexProperty(d => d.Path, pb =>
        {
            pb.Property(p => p.Value)
                .IsRequired()
                .HasColumnName("path");
        });

        builder.Property(d => d.Depth)
            .IsRequired()
            .HasColumnName("depth");

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(d => d.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.HasMany(d => d.ChildrenDepartments)
            .WithOne()
            .IsRequired(false)
            .HasForeignKey(d => d.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(d => d.ChildrenDepartments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(d => d.DepartmentLocations)
            .WithOne()
            .HasForeignKey(dl => dl.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(d => d.DepartmentLocations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(d => d.DepartmentPositions)
            .WithOne()
            .HasForeignKey(dp => dp.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(d => d.DepartmentPositions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}