using System.Text.Json;
using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.PostgreSQL.EntityFramework.ModelConfigurations.Locations;

public sealed class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");

        builder.HasKey(l => l.Id).HasName("pk_locations");

        builder
            .Property(l => l.Id)
            .HasColumnName("id")
            .HasConversion(toDb => toDb.Value, fromDb => new LocationId(fromDb));

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

        builder
            .Property(l => l.Address)
            .HasColumnType("jsonb")
            .HasConversion(ad => AddressToJsonb(ad), jsonb => AddressFromJsonb(jsonb))
            .HasColumnName("address");

        // builder.OwnsOne(
        //     l => l.Address,
        //     onb =>
        //     {
        //         onb.ToJson("address_parts");
        //         onb.OwnsMany(l => l.Parts);
        //     }
        // );

        builder
            .Property(l => l.Name)
            .HasColumnName("name")
            .HasMaxLength(LocationName.MaxLength)
            .IsRequired()
            .HasConversion(toDb => toDb.Value, fromDb => LocationName.Create(fromDb));

        builder
            .Property(l => l.TimeZone)
            .HasColumnName("time_zone")
            .IsRequired()
            .HasConversion(toDb => toDb.Value, fromDb => LocationTimeZone.Create(fromDb));
    }

    private static string AddressToJsonb(LocationAddress address)
    {
        return JsonSerializer.Serialize(address.Parts.Select(p => p.Node));
    }

    private static LocationAddress AddressFromJsonb(string jsonb)
    {
        using JsonDocument document = JsonDocument.Parse(jsonb);
        List<LocationAddressPart> parts = [];
        foreach (JsonElement entry in document.RootElement.EnumerateArray())
        {
            string? addressPart = entry.GetString();
            if (!string.IsNullOrWhiteSpace(addressPart))
                parts.Add(LocationAddressPart.Create(addressPart));
        }
        return LocationAddress.Create(parts);
    }
}
