using System.Collections;

namespace ResultLibrary;

public sealed record ErrorsCollection : IEnumerable<Error>
{
    private readonly List<Error> _errors = [];

    public IEnumerator<Error> GetEnumerator() => _errors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(Error error) => _errors.Add(error);
}
