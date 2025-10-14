using ResultLibrary;

namespace DirectoryService.Core.LocationsContext.ValueObjects.LocationElements;

[LocationElement(AoLevel)]
public sealed record MunicipalLocationElement : LocationElement
{
    public new const short AoLevel = 2;

    private static readonly LocationElementMatcher[] MunicipalMatchers =
    [
        // Город (разные формы)
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"\bгород\s+[а-яё]+(?:\s+[а-яё]+)*\b"),
                new SingleLocationRegexMatch(@"\bг\s*\.\s*[а-яё]+(?:\s+[а-яё]+)*\b"),
                new SingleLocationRegexMatch(@"\bг\s+[а-яё]+(?:\s+[а-яё]+)*\b"),
                new SingleLocationRegexMatch(
                    @"^(?:г\.?\s*)?(?<name>москва|санкт-петербург|севастополь|санкт петербург)\.?$"
                )
            ),
            "город",
            "г.",
            AoLevel
        ),
        // Район
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"\bрайон\s+[а-яё]+(?:\s+[а-яё]+)*(?:\s+район)?\b"),
                new SingleLocationRegexMatch(@"\bр\s*\.\s*[а-яё]+(?:\s+[а-яё]+)*\b"),
                new SingleLocationRegexMatch(@"\bр\s+[а-яё]+(?:\s+[а-яё]+)*\b"),
                new SingleLocationRegexMatch(@"(\b[а-яё]+ий\sрайон\b)")
            ),
            "район",
            "р.",
            AoLevel
        ),
        // Посёлок городского типа (пгт)
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(
                    @"\bпос[ёе]лок\s+городского\s+типа\s+[а-яё]+(?:\s+[а-яё]+)*\b"
                ),
                new SingleLocationRegexMatch(@"\bпгт\s+[а-яё]+(?:\s+[а-яё]+)*\b")
            ),
            "посёлок городского типа",
            "пгт",
            AoLevel
        ),
        // Посёлок
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"\bпос[ёе]лок\s+[а-яё]+(?:\s+[а-яё]+)*\b"),
                new SingleLocationRegexMatch(@"\bп\s*\.\s*[а-яё]+(?:\s+[а-яё]+)*\b"),
                new SingleLocationRegexMatch(@"\bп\s+[а-яё]+(?:\s+[а-яё]+)*\b")
            ),
            "посёлок",
            "п.",
            AoLevel
        ),
        // Село
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"\bсело\s+[а-яё]+(?:\s+[а-яё]+)*\b"),
                new SingleLocationRegexMatch(@"\bс\s*\.\s*[а-яё]+(?:\s+[а-яё]+)*\b"),
                new SingleLocationRegexMatch(@"\bс\s+[а-яё]+(?:\s+[а-яё]+)*\b")
            ),
            "село",
            "с.",
            AoLevel
        ),
        // Деревня
        new(
            new SingleLocationRegexMatch(@"\bдеревня\s+[а-яё]+(?:\s+[а-яё]+)*\b"),
            "деревня",
            "д.",
            AoLevel
        ),
        // Станица
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"\bстаница\s+[а-яё]+(?:\s+[а-яё]+)*\b"),
                new SingleLocationRegexMatch(@"\bст\s*\.\s*[а-яё]+(?:\s+[а-яё]+)*\b"),
                new SingleLocationRegexMatch(@"\bст\s+[а-яё]+(?:\s+[а-яё]+)*\b")
            ),
            "станица",
            "ст.",
            AoLevel
        ),
        // Хутор
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"\bхутор\s+[а-яё]+(?:\s+[а-яё]+)*\b"),
                new SingleLocationRegexMatch(@"\bх\s*\.\s*[а-яё]+(?:\s+[а-яё]+)*\b"),
                new SingleLocationRegexMatch(@"\bх\s+[а-яё]+(?:\s+[а-яё]+)*\b")
            ),
            "хутор",
            "х.",
            AoLevel
        ),
    ];

    public MunicipalLocationElement(string name, string type, string shortName, short aoLevel)
        : base(name, type, shortName, aoLevel) { }

    public static new Result<LocationElement> Create(string input)
    {
        foreach (LocationElementMatcher matcher in MunicipalMatchers)
        {
            Result<LocationElement> result = matcher.TryMap(
                input,
                onError: () => InvalidLocationSubject(input),
                onSuccess: Create
            );
            if (result.IsSuccess)
                return result.Value;
        }

        return Error.ValidationError("Не удается распознать тип локации.");
    }

    private static Error InvalidLocationSubject(string input) =>
        Error.ValidationError($"Некорректный субъект в адресе - {input}");

    private static MunicipalLocationElement Create(
        string name,
        string type,
        string shortName,
        short aoLevel
    ) => new(name, type, shortName, aoLevel);
}
