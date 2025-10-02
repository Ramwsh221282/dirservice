using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.PostgreSQL.EntityFramework.ModelConfigurations.Departments;

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(d => d.Id).HasName("pk_departments");

        builder
            .Property(d => d.Id)
            .HasColumnName("id")
            .HasConversion(toDb => toDb.Value, fromDb => DepartmentId.Create(fromDb));

        builder
            .Property(d => d.Identifier)
            .HasColumnName("identifier")
            .HasConversion(toDb => toDb.Value, fromDb => DepartmentIdentifier.CreateNode(fromDb))
            .HasMaxLength(DepartmentIdentifier.MaxLength);

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

        builder.Ignore(d => d.Deleted);

        builder
            .Property(d => d.Name)
            .IsRequired()
            .HasColumnName("name")
            .HasMaxLength(DepartmentName.MaxLength)
            .HasConversion(toDb => toDb.Value, fromDb => DepartmentName.Create(fromDb));

        builder
            .Property(d => d.Path)
            .IsRequired()
            .HasColumnName("path")
            .HasConversion(toDb => toDb.Value, fromDb => DepartmentPath.Create(fromDb));

        builder
            .Property(d => d.Depth)
            .IsRequired()
            .HasConversion(toDb => toDb.Value, fromDb => DepartmentDepth.Create(fromDb));

        builder
            .Property(d => d.Parent)
            .HasConversion(toDb => toDb!.Value.Value, fromDb => DepartmentId.Create(fromDb))
            .IsRequired(false);
    }
}
