using System.Reflection;
using ResultLibrary;

namespace DirectoryService.Core.LocationsContext.ValueObjects.LocationElements;

public abstract record LocationElement : ILocationElement
{
    private delegate Result<LocationElement> LocationElementFactory(string input);

    private static readonly LocationElementFactory[] Factories = InspectFactories();
    public string Value { get; }
    public string Type { get; }
    public string ShortValue { get; }
    public short AoLevel { get; }

    protected LocationElement(string name, string type, string shortName, short aoLevel)
    {
        Value = name;
        Type = type;
        ShortValue = shortName;
        AoLevel = aoLevel;
    }

    public static Result<LocationElement> Create(string input)
    {
        foreach (var factory in Factories)
        {
            Result<LocationElement> result = factory(input);
            if (result.IsSuccess)
                return result;
        }

        return Error.ValidationError($"Некорректный узел адреса - {input}");
    }

    private static LocationElementFactory[] InspectFactories()
    {
        Type currentType = typeof(LocationElement);
        Type[] subTypes = currentType
            .Assembly.GetTypes()
            .Where(s => !s.IsAbstract & s.IsSubclassOf(currentType))
            .Where(s => s.GetCustomAttribute<LocationElementAttribute>() != null)
            .OrderBy(s => s.GetCustomAttribute<LocationElementAttribute>()!.AoLevel)
            .ToArray();

        List<LocationElementFactory> factories = [];
        foreach (Type subType in subTypes)
        {
            MethodInfo method = subType
                .GetMethods()
                .First(m =>
                    m.Name == nameof(Create)
                    && m.GetParameters().Length == 1
                    && m.GetParameters()[0].ParameterType == typeof(string)
                );

            Result<LocationElement> Factory(string value)
            {
                Result<LocationElement> result =
                    (Result<LocationElement>)method.Invoke(null, [value])!;
                return result;
            }

            factories.Add(Factory);
        }

        return factories.ToArray();
    }
}
