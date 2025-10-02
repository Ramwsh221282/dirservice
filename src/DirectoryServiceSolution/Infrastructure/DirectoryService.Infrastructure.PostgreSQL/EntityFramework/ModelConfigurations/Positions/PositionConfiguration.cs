using DirectoryService.Core.PositionsContext;
using DirectoryService.Core.PositionsContext.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.PostgreSQL.EntityFramework.ModelConfigurations.Positions;

public sealed class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");

        builder.HasKey(p => p.Id).HasName("pk_positions");

        builder
            .Property(p => p.Id)
            .HasColumnName("id")
            .HasConversion(toDb => toDb.Value, fromDb => new PositionId(fromDb));

        builder
            .Property(p => p.Name)
            .HasColumnName("name")
            .HasConversion(toDb => toDb.Value, fromDb => PositionName.Create(fromDb))
            .IsRequired()
            .HasMaxLength(PositionName.MaxLength);

        builder
            .Property(p => p.Description)
            .HasColumnName("description")
            .HasConversion(toDb => toDb.Value, fromDb => PositionDescription.Create(fromDb))
            .IsRequired()
            .HasMaxLength(PositionDescription.MaxLength);

        builder.ComplexProperty(
            p => p.LifeCycle,
            cpb =>
            {
                cpb.Property(p => p.CreatedAt).HasColumnName("created_at");
                cpb.Property(p => p.DeletedAt).HasColumnName("deleted_at");
                cpb.Property(p => p.UpdatedAt).HasColumnName("updated_at");
                cpb.Ignore(p => p.IsDeleted);
            }
        );

        builder.HasIndex(l => l.Name).IsUnique();
    }
}
