using DirectoryService.Domain;
using DirectoryService.Domain.Department;
using DirectoryService.Domain.Position;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");

        builder.HasKey(d => d.Id).HasName("pk_department_positions");

        builder.Property(d => d.Id).HasColumnName("id");

        builder
            .HasOne<Department>()
            .WithMany(d => d.Positions)
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_department_position_department_id");

        builder
            .HasOne<Position>()
            .WithMany()
            .HasForeignKey(d => d.PositionId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_department_position_position_id");
    }
}