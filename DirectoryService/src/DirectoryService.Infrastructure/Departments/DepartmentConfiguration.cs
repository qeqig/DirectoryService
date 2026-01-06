using DirectoryService.Domain;
using DirectoryService.Domain.Department;
using DirectoryService.Domain.Department.VO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Departments;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(d => d.Id).HasName("pk_departments");

        builder.Property(d => d.Id)
            .HasConversion(
                d => d.Value,
                id => DepartmentId.Current(id))
            .HasColumnName("id");

        builder.OwnsOne(d => d.DepartmentName, nb =>
        {
            nb.Property(v => v.Value)
                .IsRequired()
                .HasMaxLength(LengthConstant.LENGTH_500)
                .HasColumnName("name");
        });

        builder.OwnsOne(d => d.Identifier, nb =>
        {
            nb.Property(v => v.Value)
                .IsRequired()
                .HasMaxLength(LengthConstant.LENGTH_500)
                .HasColumnName("identifier");
        });

        builder.Property(d => d.ParentId)
            .IsRequired(false)
            .HasColumnName("parent_id");

        builder.OwnsOne(d => d.Path, nb =>
        {
            nb.Property(p => p.Value)
                .IsRequired()
                .HasColumnType("ltree")
                .HasMaxLength(LengthConstant.LENGTH_500)
                .HasColumnName("path");

            nb.HasIndex(p => p.Value)
                .HasMethod("gist")
                .HasDatabaseName("ix_department_path");
        });

        builder.Property(d => d.Depth)
            .IsRequired()
            .HasColumnName("depth");

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.HasMany(d => d.ChildDepartments)
            .WithOne()
            .IsRequired(false)
            .HasForeignKey(d => d.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Locations)
            .WithOne()
            .HasForeignKey(d => d.DepartmentId);

        builder.HasMany(d => d.Positions)
            .WithOne()
            .HasForeignKey(d => d.DepartmentId);

        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(d => d.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.Property(d => d.DeletedAt)
            .HasColumnName("deleted_at");
    }
}