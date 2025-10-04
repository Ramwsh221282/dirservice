namespace DirectoryService.Core.Common.Extensions;

public static class IEnumerableExtensions
{
    public static bool IsEmpty<T>(this List<T> source) => source.Count == 0;

    public static bool IsEmpty<T>(this T[] source) => source.Length == 0;

    public static bool IsEmpty<T>(this IEnumerable<T> source) => !source.Any();
}
