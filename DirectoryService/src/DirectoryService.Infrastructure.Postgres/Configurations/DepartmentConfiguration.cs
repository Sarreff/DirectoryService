using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Path = DirectoryService.Domain.Departments.ValueObjects.Path;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public static class DepartmentIndex
{
    public const string NAME = "ix_department_name";
    public const string IDENTIFIER = "ix_department_identifier";
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

        builder.Property(d => d.Name)
            .HasConversion(
                n => n.Value,
                v => new Name(v))
            .IsRequired()
            .HasMaxLength(LengthConstants.LENGTH150)
            .HasColumnName("name");

        builder.Property(d => d.Identifier)
            .HasConversion(
                i => i.Value,
                v => new Identifier(v))
            .IsRequired()
            .HasMaxLength(LengthConstants.LENGTH150)
            .HasColumnName("identifier");

        builder.Property(d => d.ParentId)
            .HasConversion(
                p => p!.Value,
                pid => new DepartmentId(pid))
            .IsRequired(false)
            .HasColumnName("parent_id");

        builder.Property(d => d.Path)
            .HasColumnName("path")
            .HasColumnType("ltree")
            .IsRequired()
            .HasConversion(
                p => p.Value,
                v => Path.Create(v));

        builder.HasIndex(d => d.Path)
            .HasMethod("gist")
            .HasDatabaseName("ix_departments_path");

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

        builder.HasIndex(d => d.Name)
            .IsUnique()
            .HasDatabaseName(DepartmentIndex.NAME)
            .HasDatabaseName(DepartmentIndex.NAME) // Это НЕ имя constraint, а имя индекса
            .HasFilter(null);

        builder.HasIndex(d => d.Identifier)
            .IsUnique()
            .HasDatabaseName(DepartmentIndex.IDENTIFIER)
            .HasDatabaseName(DepartmentIndex.IDENTIFIER)
            .HasFilter(null);
    }
}