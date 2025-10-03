using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.LocationsContext.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.PostgreSQL.EntityFramework.ModelConfigurations.Departments;

public sealed class DepartmentLocationsConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations");

        builder.HasKey(dl => new { dl.DepartmentId, dl.LocationId });

        builder
            .Property(dl => dl.DepartmentId)
            .HasColumnName("department_id")
            .HasConversion(toDb => toDb.Value, fromDb => DepartmentId.Create(fromDb));

        builder
            .Property(dl => dl.LocationId)
            .HasColumnName("location_id")
            .HasConversion(toDb => toDb.Value, fromDb => new LocationId(fromDb));
    }
}
