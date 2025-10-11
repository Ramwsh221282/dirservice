using System.Text.Json;
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
            .HasConversion(toDb => toDb.Value, fromDb => DepartmentIdentifier.Create(fromDb))
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
            .HasColumnType("ltree")
            .HasColumnName("path")
            .HasConversion(toDb => toDb.Value, fromDb => DepartmentPath.Create(fromDb));

        builder.HasIndex(x => x.Path).HasMethod("gist").HasDatabaseName("idx_department_path");

        builder
            .Property(d => d.Depth)
            .IsRequired()
            .HasColumnName("depth")
            .HasConversion(toDb => toDb.Value, fromDb => DepartmentDepth.Create(fromDb));

        builder
            .Property(d => d.Parent)
            .HasColumnName("parent_id")
            .HasConversion(toDb => toDb!.Value.Value, fromDb => DepartmentId.Create(fromDb))
            .IsRequired(false);

        builder
            .Property(d => d.ChildrensCount)
            .HasColumnName("childrens_count")
            .HasConversion(toDb => toDb.Value, fromDb => DepartmentChildrensCount.Create(fromDb))
            .IsRequired();

        builder
            .Property(d => d.Attachments)
            .HasColumnName("attachments")
            .HasColumnType("jsonb")
            .HasConversion(
                toDb => JsonSerializer.Serialize(toDb, JsonSerializerOptions.Default),
                fromDb => DepartmentChildAttachmentsHistory.FromJson(fromDb)
            );
    }
}
