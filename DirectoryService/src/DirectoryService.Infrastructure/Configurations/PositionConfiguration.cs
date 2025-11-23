using DirectoryService.Domain;
using DirectoryService.Domain.Position;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");

        builder.HasKey(p => p.Id).HasName("pk_positions");

        builder.Property(p => p.Id).HasColumnName("id");

        builder.ComplexProperty(p => p.Name, nb =>
        {
            nb.Property(p => p.Value)
                .IsRequired()
                .HasMaxLength(LengthConstant.LENGTH_500)
                .HasColumnName("name");
        });

        builder.Property(p => p.Description)
            .IsRequired(false)
            .HasMaxLength(LengthConstant.LENGTH_500)
            .HasColumnName("description");

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(p => p.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");
    }
}