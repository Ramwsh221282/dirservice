using System.Text.RegularExpressions;

namespace DirectoryService.Core.LocationsContext.ValueObjects.LocationElements;

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
