using System.Text.RegularExpressions;

namespace DirectoryService.Core.LocationsContext.ValueObjects.LocationElements;

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
