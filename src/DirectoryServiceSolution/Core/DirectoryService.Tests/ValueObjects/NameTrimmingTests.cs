using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.Core.PositionsContext.ValueObjects;
using ResultLibrary;

namespace DirectoryService.Tests.ValueObjects;

public class NameTrimmingTests
{
    [Theory]
    [InlineData("  Инженер  ", "Инженер")]
    [InlineData("\tИнженер\t", "Инженер")]
    [InlineData("\r\nИнженер \n", "Инженер")]
    [InlineData("Инженер", "Инженер")]
    [InlineData("Главный  инженер", "Главный  инженер")]
    private void Position_Name_Is_Trimmed(string input, string expected)
    {
        Result<PositionName> name = PositionName.Create(input);

        Assert.True(name.IsSuccess);
        Assert.Equal(expected, name.Value.Value);
    }

    [Theory]
    [InlineData("  Описание должности  ", "Описание должности")]
    [InlineData("\tОписание должности\n", "Описание должности")]
    [InlineData("Описание должности", "Описание должности")]
    private void Position_Description_Is_Trimmed(string input, string expected)
    {
        Result<PositionDescription> description = PositionDescription.Create(input);

        Assert.True(description.IsSuccess);
        Assert.Equal(expected, description.Value.Value);
    }

    [Theory]
    [InlineData("  Отдел разработки  ", "Отдел разработки")]
    [InlineData("\tОтдел разработки\n", "Отдел разработки")]
    [InlineData("Отдел разработки", "Отдел разработки")]
    private void Department_Name_Is_Trimmed(string input, string expected)
    {
        Result<DepartmentName> name = DepartmentName.Create(input);

        Assert.True(name.IsSuccess);
        Assert.Equal(expected, name.Value.Value);
    }

    [Theory]
    [InlineData("  Головной офис  ", "Головной офис")]
    [InlineData("\tГоловной офис\n", "Головной офис")]
    [InlineData("Головной офис", "Головной офис")]
    private void Location_Name_Is_Trimmed(string input, string expected)
    {
        Result<LocationName> name = LocationName.Create(input);

        Assert.True(name.IsSuccess);
        Assert.Equal(expected, name.Value.Value);
    }

    [Fact]
    private void Position_Name_Length_Is_Measured_After_Trimming()
    {
        string padded = new string(' ', 50) + new string('a', PositionName.MaxLength) + new string(' ', 50);

        Result<PositionName> name = PositionName.Create(padded);

        Assert.True(name.IsSuccess);
        Assert.Equal(PositionName.MaxLength, name.Value.Value.Length);
    }

    [Fact]
    private void Position_Description_Length_Is_Measured_After_Trimming()
    {
        string padded = new string(' ', 50) + new string('a', PositionDescription.MaxLength) + new string(' ', 50);

        Result<PositionDescription> description = PositionDescription.Create(padded);

        Assert.True(description.IsSuccess);
        Assert.Equal(PositionDescription.MaxLength, description.Value.Value.Length);
    }

    [Fact]
    private void Department_Name_Length_Is_Measured_After_Trimming()
    {
        string padded = new string(' ', 50) + new string('a', DepartmentName.MaxLength) + new string(' ', 50);

        Result<DepartmentName> name = DepartmentName.Create(padded);

        Assert.True(name.IsSuccess);
        Assert.Equal(DepartmentName.MaxLength, name.Value.Value.Length);
    }

    [Fact]
    private void Location_Name_Length_Is_Measured_After_Trimming()
    {
        string padded = new string(' ', 50) + new string('a', LocationName.MaxLength) + new string(' ', 50);

        Result<LocationName> name = LocationName.Create(padded);

        Assert.True(name.IsSuccess);
        Assert.Equal(LocationName.MaxLength, name.Value.Value.Length);
    }

    [Fact]
    private void Department_Name_Shorter_Than_Minimum_After_Trimming_Fails()
    {
        Result<DepartmentName> name = DepartmentName.Create("  ab  ");

        Assert.True(name.IsFailure);
    }

    [Fact]
    private void Position_Name_Shorter_Than_Minimum_After_Trimming_Fails()
    {
        Result<PositionName> name = PositionName.Create("  ab  ");

        Assert.True(name.IsFailure);
    }

    [Fact]
    private void Position_Name_Longer_Than_Maximum_After_Trimming_Fails()
    {
        string tooLong = "  " + new string('a', PositionName.MaxLength + 1) + "  ";

        Result<PositionName> name = PositionName.Create(tooLong);

        Assert.True(name.IsFailure);
    }
}
