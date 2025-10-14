using System.Text.RegularExpressions;
using ResultLibrary;

namespace DirectoryService.Core.LocationsContext.ValueObjects.LocationElements;

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
