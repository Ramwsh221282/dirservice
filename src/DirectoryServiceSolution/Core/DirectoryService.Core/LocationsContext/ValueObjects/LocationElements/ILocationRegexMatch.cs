using System.Text.RegularExpressions;

namespace DirectoryService.Core.LocationsContext.ValueObjects.LocationElements;

public interface ILocationRegexMatch
{
    Match Match(string input);
}
