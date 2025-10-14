using ResultLibrary;

namespace DirectoryService.Core.LocationsContext.ValueObjects.LocationElements;

[LocationElement(AoLevel)]
public sealed record BuildingLocationElement : LocationElement
{
    public new const short AoLevel = 4;

    // Матчеры для домов и строений
    private static readonly LocationElementMatcher[] BuildingMatchers =
    [
        // Дом
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"\bдом\s+\d+[а-яё]?\b"),
                new SingleLocationRegexMatch(@"\bд\s*\.\s*\d+[а-яё]?\b"),
                new SingleLocationRegexMatch(@"\bд\s+\d+[а-яё]?\b"),
                new SingleLocationRegexMatch(@"(\bд\d+\b)"),
                new SingleLocationRegexMatch(@"(\bд[.]\d+\b)"),
                new SingleLocationRegexMatch(@"(\bд[.]\s[а-яё]+\b)")
            ),
            "дом",
            "д.",
            AoLevel
        ),
        // Корпус
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"\bкорпус\s+\d+[а-яё]?\b"),
                new SingleLocationRegexMatch(@"\bкорп\s*\.\s*\d+[а-яё]?\b"),
                new SingleLocationRegexMatch(@"\bк\s*\.\s*\d+[а-яё]?\b"),
                new SingleLocationRegexMatch(@"\bк\s+\d+[а-яё]?\b"),
                new SingleLocationRegexMatch(@"\bк[.]\s+\d+[а-яё]?\b"),
                new SingleLocationRegexMatch(@"(\bк[.]\d+\b)"),
                new SingleLocationRegexMatch(@"(\bк[.]\w\b)"),
                new SingleLocationRegexMatch(@"(\bк\d+\b)"),
                new SingleLocationRegexMatch(@"(\bкорп\d+\b)"),
                new SingleLocationRegexMatch(@"(\bкорп\w\b)"),
                new SingleLocationRegexMatch(@"(\bкорп[.]\d+\b)"),
                new SingleLocationRegexMatch(@"(\bкорп[.]\w\b)"),
                new SingleLocationRegexMatch(@"\bк[.]\s+\d+[а-яё]?\b")
            ),
            "корпус",
            "к.",
            AoLevel
        ),
        // Строение
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"\bстроение\s+\d+[а-яё]?\b"),
                new SingleLocationRegexMatch(@"\bстр\s*\.\s*\d+[а-яё]?\b"),
                new SingleLocationRegexMatch(@"\bс\s*\.\s*\d+[а-яё]?\b"),
                new SingleLocationRegexMatch(@"(\bстр[.]\d+)\b"),
                new SingleLocationRegexMatch(@"(\bстр\d+\b)")
            ),
            "строение",
            "стр.",
            AoLevel
        ),
        // Владение
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"\bвладение\s+\d+\b"),
                new SingleLocationRegexMatch(@"\bвл\s*\.\s*\d+\b"),
                new SingleLocationRegexMatch(@"(\bвл[.]\d+\b)"),
                new SingleLocationRegexMatch(@"(\bвл\d+\b)")
            ),
            "владение",
            "вл.",
            AoLevel
        ),
        // Литер / литера
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"\bлитера?\s+[а-яё]\b"),
                new SingleLocationRegexMatch(@"\bлит\s*\.\s*[а-яё]\b"),
                new SingleLocationRegexMatch(@"\bлит\s*\.\s*[а-яё]\b"),
                new SingleLocationRegexMatch(@"(\bлит\w+\b)")
            ),
            "литера",
            "лит.",
            AoLevel
        ),
    ];

    public BuildingLocationElement(string name, string type, string shortName, short aoLevel)
        : base(name, type, shortName, aoLevel) { }

    public static new Result<LocationElement> Create(string input)
    {
        foreach (var matcher in BuildingMatchers)
        {
            Result<LocationElement> result = matcher.TryMap(
                input,
                onError: () => InvalidBuilding(input),
                onSuccess: Create
            );

            if (result.IsSuccess)
                return result.Value;
        }

        return Error.ValidationError("Не удалось распознать здание, корпус или строение.");
    }

    private static Error InvalidBuilding(string input) =>
        Error.ValidationError($"Некорректное обозначение здания: {input}");

    private static BuildingLocationElement Create(
        string name,
        string type,
        string shortName,
        short aoLevel
    ) => new(name, type, shortName, aoLevel);
}
