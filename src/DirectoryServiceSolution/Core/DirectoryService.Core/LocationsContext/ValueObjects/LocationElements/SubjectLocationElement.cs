using ResultLibrary;

namespace DirectoryService.Core.LocationsContext.ValueObjects.LocationElements;

[LocationElement(AoLevel)]
public sealed record SubjectLocationElement : LocationElement
{
    public new const short AoLevel = 1;

    private static readonly LocationElementMatcher[] SubjectMatchers =
    [
        // Республика
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"(\bреспублика\b)"),
                new SingleLocationRegexMatch(@"(\bресп[.]\b)"),
                new SingleLocationRegexMatch(@"(\bресп\b)"),
                new SingleLocationRegexMatch(@"(\bресп[а-яё]+ика\b)"),
                new SingleLocationRegexMatch(@"(\bресп[а-яё]+ика\b)"),
                new SingleLocationRegexMatch(@"(\bресп\.\s[а-яё]+\b)"),
                new SingleLocationRegexMatch(@"(\b[а-яё]+\sресп\.)"),
                new SingleLocationRegexMatch(@"(\b[а-яё]+\sресп[а-яё]+ика\b)")
            ),
            "республика",
            "респ.",
            AoLevel
        ),
        // Автономная область
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"(\bавтономная\sобласть\s[а-яё]+\b)"),
                new SingleLocationRegexMatch(@"(\bавтономная\sобл.\b)"),
                new SingleLocationRegexMatch(@"([а-яё]+\sавтономная\sобл.\b)"),
                new SingleLocationRegexMatch(@"(\bавтономная\sобл\b)"),
                new SingleLocationRegexMatch(@"([а-яё]+\sавтономная\sобл\b)"),
                new SingleLocationRegexMatch(@"(\bавтономная\sобл[а-яё]+ть\b)")
            ),
            "автономная область",
            "автономная обл.",
            AoLevel
        ),
        // Автономный округ
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(
                    @"(\bавтономный\sокр.\b)|([а-яё]+\sавтономный\sокр.\b)"
                ),
                new SingleLocationRegexMatch(@"(\bавтономный\sокр\b)|([а-яё]+\sавтономный\sокр\b)"),
                new SingleLocationRegexMatch(@"(\b.*\sавтономный\sокруг\b)")
            ),
            "автономный округ",
            "автономный окр.",
            AoLevel
        ),
        // Край
        new(new SingleLocationRegexMatch(@"^(?<name>.*?)\s*(?:край\.?)$"), "край", "край", 1),
        // Область
        new(
            new SingleLocationRegexMatch(@"^(?<name>.*?)\s*(?:обл(?:асть)?\.?)$"),
            "область",
            "обл.",
            AoLevel
        ),
    ];

    private SubjectLocationElement(string name, string type, string shortName, short aoLevel)
        : base(name, type, shortName, aoLevel) { }

    public static Result<LocationElement> Create(string input)
    {
        foreach (LocationElementMatcher matcher in SubjectMatchers)
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

    private static SubjectLocationElement Create(
        string name,
        string type,
        string shortName,
        short aoLevel
    ) => new(name, type, shortName, aoLevel);
}
