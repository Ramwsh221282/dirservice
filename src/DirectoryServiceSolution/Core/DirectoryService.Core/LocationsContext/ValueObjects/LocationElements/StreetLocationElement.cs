using ResultLibrary;

namespace DirectoryService.Core.LocationsContext.ValueObjects.LocationElements;

[LocationElement(AoLevel)]
public sealed record StreetLocationElement : LocationElement
{
    public new const short AoLevel = 3;

    private static readonly LocationElementMatcher[] StreetMatchers =
    [
        // Улица
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"\bулица\s+[а-яё][а-яё\s\-0-9]*[а-яё0-9]\b"),
                new SingleLocationRegexMatch(@"\bул\s*\.\s*[а-яё][а-яё\s\-0-9]*[а-яё0-9]\b"),
                new SingleLocationRegexMatch(@"\bул\s+[а-яё][а-яё\s\-0-9]*[а-яё0-9]\b")
            ),
            "улица",
            "ул.",
            AoLevel
        ),
        // Проспект
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"\bпроспект\s+[а-яё][а-яё\s\-0-9]*[а-яё0-9]\b"),
                new SingleLocationRegexMatch(@"\bпр\s*\.\s*[а-яё][а-яё\s\-0-9]*[а-яё0-9]\b"),
                new SingleLocationRegexMatch(@"\bпр\s+[а-яё][а-яё\s\-0-9]*[а-яё0-9]\b")
            ),
            "проспект",
            "пр.",
            AoLevel
        ),
        // Бульвар
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"\bбульвар\s+[а-яё][а-яё\s\-0-9]*[а-яё0-9]\b"),
                new SingleLocationRegexMatch(@"\bб\s*\.\s*[а-яё][а-яё\s\-0-9]*[а-яё0-9]\b"),
                new SingleLocationRegexMatch(@"\bб\s+[а-яё][а-яё\s\-0-9]*[а-яё0-9]\b")
            ),
            "бульвар",
            "б.",
            AoLevel
        ),
        // Переулок
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"\bпереулок\s+[а-яё][а-яё\s\-0-9]*[а-яё0-9]\b"),
                new SingleLocationRegexMatch(@"\bпер\s*\.\s*[а-яё][а-яё\s\-0-9]*[а-яё0-9]\b"),
                new SingleLocationRegexMatch(@"\bпер\s+[а-яё][а-яё\s\-0-9]*[а-яё0-9]\b")
            ),
            "переулок",
            "пер.",
            AoLevel
        ),
        // Шоссе
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"\bшоссе\s+[а-яё][а-яё\s\-0-9]*[а-яё0-9]\b"),
                new SingleLocationRegexMatch(@"\bш\s*\.\s*[а-яё][а-яё\s\-0-9]*[а-яё0-9]\b")
            ),
            "шоссе",
            "ш.",
            AoLevel
        ),
        // Тупик
        new(
            new SingleLocationRegexMatch(@"\bтупик\s+[а-яё][а-яё\s\-0-9]*[а-яё0-9]\b"),
            "тупик",
            "туп.",
            AoLevel
        ),
        // Аллея
        new(
            new SingleLocationRegexMatch(@"\bаллея\s+[а-яё][а-яё\s\-0-9]*[а-яё0-9]\b"),
            "аллея",
            "ал.",
            AoLevel
        ),
        // Микрорайон
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"\bмикрорайон\s+\d+[а-яё]?\b"),
                new SingleLocationRegexMatch(@"\bмкр\s*\.\s*\d+[а-яё]?\b"),
                new SingleLocationRegexMatch(@"\bмкрн\s+\d+[а-яё]?\b"),
                new SingleLocationRegexMatch(@"(\bмкр[.]\s[а-яё]+\b)"),
                new SingleLocationRegexMatch(@"(\bмкр\s[а-яё]+\b)"),
                new SingleLocationRegexMatch(@"(\bмкр-он\s[а-яё]+\b)")
            ),
            "микрорайон",
            "мкр.",
            AoLevel
        ),
        // СНТ / ДНП / ТСН
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(
                    @"\bснт\s+[«""]?[а-яё][а-яё\s\-0-9]*[а-яё0-9][»""]?\b"
                ),
                new SingleLocationRegexMatch(
                    @"\bднп\s+[«""]?[а-яё][а-яё\s\-0-9]*[а-яё0-9][»""]?\b"
                ),
                new SingleLocationRegexMatch(
                    @"\bтсн\s+[«""]?[а-яё][а-яё\s\-0-9]*[а-яё0-9][»""]?\b"
                ),
                new SingleLocationRegexMatch(
                    @"\bсадовое\s+товарищество\s+[«""]?[а-яё][а-яё\s\-0-9]*[а-яё0-9][»""]?\b"
                ),
                new SingleLocationRegexMatch(
                    @"\bдачное\s+некоммерческое\s+партнёрство\s+[«""]?[а-яё][а-яё\s\-0-9]*[а-яё0-9][»""]?\b"
                )
            ),
            "садоводческое некоммерческое товарищество",
            "СНТ",
            AoLevel
        ),
    ];

    public StreetLocationElement(string name, string type, string shortName, short aoLevel)
        : base(name, type, shortName, aoLevel) { }

    public static new Result<LocationElement> Create(string input)
    {
        foreach (var matcher in StreetMatchers)
        {
            Result<LocationElement> result = matcher.TryMap(
                input,
                onError: () => InvalidStreet(input),
                onSuccess: Create
            );
            if (result.IsSuccess)
                return result.Value;
        }

        return Error.ValidationError("Не удается распознать улицу в адресе.");
    }

    private static Error InvalidStreet(string input) =>
        Error.ValidationError($"Некорректный элемент улично-дорожной сети: {input}");

    private static StreetLocationElement Create(
        string name,
        string type,
        string shortName,
        short aoLevel
    ) => new(name, type, shortName, aoLevel);
}
