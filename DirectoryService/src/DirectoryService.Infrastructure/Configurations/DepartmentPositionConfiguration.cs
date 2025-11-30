using DirectoryService.Domain.Department;
using DirectoryService.Domain.Department.VO;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Position;
using DirectoryService.Domain.Position.VO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");

        builder.HasKey(d => d.Id).HasName("pk_department_positions");

        builder.Property(d => d.Id)
            .HasColumnName("id")
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => new DepartmentPositionId(value));

        builder.Property(d => d.DepartmentId)
            .IsRequired()
            .HasConversion(
                d => d.Value,
                id => DepartmentId.Current(id))
            .HasColumnName("department_id");

        builder.Property(dp => dp.PositionId)
            .IsRequired()
            .HasConversion(
                dp => dp.Value,
                id => PositionId.Current(id))
            .HasColumnName("position_id");


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