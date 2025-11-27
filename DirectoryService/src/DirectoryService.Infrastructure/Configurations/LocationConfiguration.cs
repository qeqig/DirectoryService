using DirectoryService.Domain;
using DirectoryService.Domain.Location;
using DirectoryService.Domain.Location.VO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");

        builder.HasKey(l => l.Id).HasName("pk_locations");

        builder.Property(l => l.Id)
            .HasConversion(
                l => l.Value,
                id => LocationId.Current(id))
            .HasColumnName("id");

        builder.ComplexProperty(l => l.LocationName, nb =>
        {
            nb.Property(v => v.Value)
                .IsRequired()
                .HasMaxLength(LengthConstant.LENGTH_500)
                .HasColumnName("name");
        });

        builder.OwnsOne(l => l.Address, nb =>
        {
            nb.ToJson("address");
            nb.Property(l => l.Country)
                .IsRequired()
                .HasColumnName("country");
            nb.Property(l => l.City)
                .IsRequired()
                .HasColumnName("city");
            nb.Property(l => l.Street)
                .IsRequired()
                .HasColumnName("street");
            nb.Property(l => l.HouseNumber)
                .IsRequired()
                .HasColumnName("house_number");
        });

        builder.ComplexProperty(l => l.LocationTimezone, nb =>
        {
            nb.Property(l => l.Value)
                .HasMaxLength(LengthConstant.LENGTH_500)
                .IsRequired()
                .HasColumnName("time_zone");
        });

        builder.Property(l => l.IsActive)
            .HasColumnName("is_active");

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at");
    }
}