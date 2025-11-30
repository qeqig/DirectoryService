using DirectoryService.Domain.Department;
using DirectoryService.Domain.Department.VO;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Location;
using DirectoryService.Domain.Location.VO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations");

        builder.HasKey(d => d.Id).HasName("pk_department_locations");

        builder.Property(d => d.Id)
            .HasColumnName("id")
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => new DepartmentLocationId(value));

        builder.Property(d => d.DepartmentId)
            .IsRequired()
            .HasConversion(
                d => d.Value,
                id => DepartmentId.Current(id))
            .HasColumnName("department_id");

        builder.Property(dl => dl.LocationId)
            .IsRequired()
            .HasConversion(
                dl => dl.Value,
                locationId => LocationId.Current(locationId))
            .HasColumnName("location_id");

        builder
            .HasOne<Department>()
            .WithMany(d => d.Locations)
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_department_locations_department_id");

        builder
            .HasOne<Location>()
            .WithMany(l => l.Departments)
            .HasForeignKey(l => l.LocationId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_department_locations_location_id");
    }
}