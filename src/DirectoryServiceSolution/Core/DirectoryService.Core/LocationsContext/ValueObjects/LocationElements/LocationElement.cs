using System.Text.RegularExpressions;
using ResultLibrary;

namespace DirectoryService.Core.LocationsContext.ValueObjects;

public interface ILocationElement
{
    public string Value { get; }
    public string Type { get; }
    public string ShortValue { get; }
    public short AoLevel { get; }
}

public abstract record LocationElement : ILocationElement
{
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
}

public sealed record StreetLocationElement : LocationElement
{
    private new const short AoLevel = 6;

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
                new SingleLocationRegexMatch(@"\bмкрн\s+\d+[а-яё]?\b")
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
        // Промышленная зона / база / военный городок
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(
                    @"\bпромышленная\s+зона\s+[а-яё][а-яё\s\-0-9]*[а-яё0-9]\b"
                ),
                new SingleLocationRegexMatch(@"\bбаза\s+[а-яё][а-яё\s\-0-9]*[а-яё0-9]\b"),
                new SingleLocationRegexMatch(
                    @"\bвоенный\s+городок\s+[а-яё][а-яё\s\-0-9]*[а-яё0-9]\b"
                )
            ),
            "территория",
            "тер.",
            AoLevel
        ),
    ];

    public StreetLocationElement(string name, string type, string shortName, short aoLevel)
        : base(name, type, shortName, aoLevel) { }

    public static Result<LocationElement> Create(string input)
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

        return new StreetLocationElement(input, "территория", "тер.", AoLevel);
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

public sealed record MunicipalLocationElement : LocationElement
{
    private new const short AoLevel = 3;

    private static readonly LocationElementMatcher[] MunicipalMatchers =
    [
        // Город (разные формы)
        new(
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"\bгород\s+[а-яё]+(?:\s+[а-яё]+)*\b"),
                new SingleLocationRegexMatch(@"\bг\s*\.\s*[а-яё]+(?:\s+[а-яё]+)*\b"),
                new SingleLocationRegexMatch(@"\bг\s+[а-яё]+(?:\s+[а-яё]+)*\b")
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
            new CompositeLocationRegexMatch(
                new SingleLocationRegexMatch(@"\bдеревня\s+[а-яё]+(?:\s+[а-яё]+)*\b"),
                new SingleLocationRegexMatch(@"\bд\s*\.\s*[а-яё]+(?:\s+[а-яё]+)*\b"),
                new SingleLocationRegexMatch(@"\bд\s+[а-яё]+(?:\s+[а-яё]+)*\b")
            ),
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

    public static Result<LocationElement> Create(string input)
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

public sealed record SubjectLocationElement : LocationElement
{
    private new const short AoLevel = 1;

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
        // Город федерального значения
        new(
            new SingleLocationRegexMatch(
                @"^(?:г\.?\s*)?(?<name>москва|санкт-петербург|севастополь|санкт петербург)\.?$"
            ),
            "город федерального значения",
            "г.",
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

public interface ILocationRegexMatch
{
    Match Match(string input);
}

public sealed class SingleLocationRegexMatch : ILocationRegexMatch
{
    private const RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.Compiled;
    private readonly Regex _regex;

    public SingleLocationRegexMatch(string template)
    {
        _regex = new Regex(template, Options);
    }

    public Match Match(string input)
    {
        return _regex.Match(input);
    }
}

public sealed class CompositeLocationRegexMatch : ILocationRegexMatch
{
    private readonly ILocationRegexMatch[] _matches;

    public CompositeLocationRegexMatch(params ILocationRegexMatch[] matches) => _matches = matches;

    public Match Match(string input)
    {
        foreach (ILocationRegexMatch match in _matches)
        {
            Match result = match.Match(input);
            if (result.Success)
                return result;
        }

        return System.Text.RegularExpressions.Match.Empty;
    }
}

public sealed class LocationElementMatcher
{
    private readonly ILocationRegexMatch _regex;
    private readonly string _type;
    private readonly string _shortName;
    private readonly short _aoLevel;

    public LocationElementMatcher(
        ILocationRegexMatch regex,
        string type,
        string shortName,
        short aoLevel
    )
    {
        _regex = regex;
        _type = type;
        _shortName = shortName;
        _aoLevel = aoLevel;
    }

    public Result<LocationElement> TryMap(
        string input,
        Func<Error> onError,
        Func<string, string, string, short, LocationElement> onSuccess
    )
    {
        Match match = _regex.Match(input);
        return !match.Success ? onError() : onSuccess(input, _type, _shortName, _aoLevel);
    }
}
