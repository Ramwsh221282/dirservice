using System.Globalization;

namespace DirectoryService.Core.Common.Extensions;

public static class StringExtensions
{
    public static bool LessThan(this string input, short value)
    {
        return input.Length < value;
    }

    public static bool GreaterThan(this string input, short value)
    {
        return input.Length > value;
    }

    public static bool LessThan(this string input, int value)
    {
        return input.Length < value;
    }

    public static bool GreaterThan(this string input, int value)
    {
        return input.Length > value;
    }

    public static string FormatForName(this string input)
    {
        return input.Trim().ToLower(CultureInfo.InvariantCulture).MakeFirstLetterCapital();
    }

    public static string MakeFirstLetterCapital(this string input)
    {
        return MakeFirstLetterCapital(input.AsSpan());
    }

    public static string MakeFirstLetterCapital(this ReadOnlySpan<char> input)
    {
        char firstLetter = char.ToUpper(input[0], CultureInfo.InvariantCulture);
        ReadOnlySpan<char> otherPart = input[1..];
        return $"{firstLetter}{otherPart}";
    }

    public static bool IsLatinOnly(this string input)
    {
        return input.All(l => (l >= 'a' && l <= 'z') || (l >= 'A' && l <= 'Z') || l == '-');
    }
}
