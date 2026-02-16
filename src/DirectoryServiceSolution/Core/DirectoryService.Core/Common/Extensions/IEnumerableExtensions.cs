
namespace DirectoryService.Core.Common.Extensions;

public static class IEnumerableExtensions
{
    public static bool IsEmpty<T>(this List<T> source)
    {
        return source.Count == 0;
    }

    public static bool IsEmpty<T>(this T[] source)
    {
        return source.Length == 0;
    }

    public static bool IsEmpty<T>(this IEnumerable<T> source)
    {
        return !source.Any();
    }

    public static IEnumerable<T> ExtractDuplicates<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> duplicatesSelector
    )
    {
        T[] distinctValues = [.. source.DistinctBy(duplicatesSelector)];
        T[] initialValues = [.. source];

        if (distinctValues.Length == initialValues.Length)
        {
            return [];
        }

        return initialValues
            .GroupBy(duplicatesSelector)
            .Where(group => group.Count() > 1)
            .SelectMany(item => item);
    }
}
