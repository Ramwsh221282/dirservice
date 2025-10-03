using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.PositionsContext.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.PostgreSQL.EntityFramework.ModelConfigurations.Departments;

public sealed class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");

        builder.HasKey(dp => new { dp.DepartmentId, dp.PositionId });

        builder
            .Property(dp => dp.DepartmentId)
            .HasColumnName("department_id")
            .HasConversion(toDb => toDb.Value, fromDb => DepartmentId.Create(fromDb));

        builder
            .Property(dp => dp.PositionId)
            .HasColumnName("position_id")
            .HasConversion(toDb => toDb.Value, fromDb => new PositionId(fromDb));
    }
}
