using DirectoryService.Core.LocationsContext.ValueObjects.LocationElements;
using ResultLibrary;

namespace DirectoryService.Tests.Locations;

public class LocationsTests
{
    [Theory]
    [InlineData("Республика Татарстан", "респ.", "республика", 1)]
    [InlineData("татарстан респ.", "респ.", "республика", 1)]
    [InlineData("Бурятия республика", "респ.", "республика", 1)]
    [InlineData("Респ. Саха", "респ.", "республика", 1)]
    [InlineData("Краснодарский край", "край", "край", 1)]
    [InlineData("Алтайский край.", "край", "край", 1)]
    [InlineData("приморский край", "край", "край", 1)]
    [InlineData("Ленинградская обл.", "обл.", "область", 1)]
    [InlineData("Московская область", "обл.", "область", 1)]
    [InlineData("Свердловская обл", "обл.", "область", 1)]
    [InlineData("Москва", "г.", "город федерального значения", 1)]
    [InlineData("г. Санкт-Петербург", "г.", "город федерального значения", 1)]
    [InlineData("СЕВАСТОПОЛЬ", "г.", "город федерального значения", 1)]
    [InlineData("г Севастополь", "г.", "город федерального значения", 1)]
    [InlineData("Еврейская автономная обл.", "автономная обл.", "автономная область", 1)]
    [InlineData("автономная область еврейская", "автономная обл.", "автономная область", 1)]
    [InlineData("Чукотский автономный окр.", "автономный окр.", "автономный округ", 1)]
    [InlineData("Ямало-Ненецкий автономный округ", "автономный окр.", "автономный округ", 1)]
    public void Match_Republic_Success(
        string input,
        string expectedShort,
        string expectedFull,
        short expectedLevel
    )
    {
        Result<LocationElement> result = LocationElement.Create(input);
        Assert.True(result.IsSuccess);
        LocationElement part = result;
        Assert.Equal(input, part.Value);
        Assert.Equal(expectedShort, part.ShortValue);
        Assert.Equal(expectedFull, part.Type);
        Assert.Equal(expectedLevel, part.AoLevel);
    }

    [Theory]
    [InlineData("город Казань", "город", "г.", 2)]
    [InlineData("г. Москва", "город", "г.", 2)]
    [InlineData("г Новосибирск", "город", "г.", 2)]
    [InlineData("г.Санкт-Петербург", "город", "г.", 2)]
    [InlineData("город Екатеринбург", "город", "г.", 2)]
    [InlineData("хутор Весёлый", "хутор", "х.", 2)]
    [InlineData("х. Рассвет", "хутор", "х.", 2)]
    [InlineData("х Новоалексеевка", "хутор", "х.", 2)]
    [InlineData("х.Грушевка", "хутор", "х.", 2)]
    [InlineData("район Ленинский", "район", "р.", 2)]
    [InlineData("Ленинский район", "район", "р.", 2)]
    [InlineData("р. Октябрьский", "район", "р.", 2)]
    [InlineData("р Кировский", "район", "р.", 2)]
    [InlineData("р.Советский", "район", "р.", 2)]
    [InlineData("посёлок городского типа Зеленогорск", "посёлок городского типа", "пгт", 2)]
    [InlineData("пгт Солнечный", "посёлок городского типа", "пгт", 2)]
    [InlineData("поселок городского типа Углегорск", "посёлок городского типа", "пгт", 2)]
    [InlineData("посёлок Берёзово", "посёлок", "п.", 2)]
    [InlineData("п. Ивня", "посёлок", "п.", 2)]
    [InlineData("п Новоивановка", "посёлок", "п.", 2)]
    [InlineData("п.Сосновка", "посёлок", "п.", 2)]
    [InlineData("село Александровка", "село", "с.", 2)]
    [InlineData("с. Никольское", "село", "с.", 2)]
    [InlineData("с Ново-Покровка", "село", "с.", 2)]
    [InlineData("с.Ильинка", "село", "с.", 2)]
    [InlineData("деревня Малиновка", "деревня", "д.", 2)]
    [InlineData("д. Васильево", "деревня", "д.", 2)]
    [InlineData("д Сосново", "деревня", "д.", 2)]
    [InlineData("д.Горки", "деревня", "д.", 2)]
    [InlineData("станица Старощербиновская", "станица", "ст.", 2)]
    [InlineData("ст. Ейская", "станица", "ст.", 2)]
    [InlineData("ст Кущёвская", "станица", "ст.", 2)]
    [InlineData("ст.Динская", "станица", "ст.", 2)]
    public void Create_City_Success(
        string input,
        string expectedType,
        string expectedShortName,
        short expectedLevel
    )
    {
        Result<LocationElement> result = MunicipalLocationElement.Create(input);
        Assert.True(result.IsSuccess);
        LocationElement element = result.Value;
        Assert.Equal(input, element.Value);
        Assert.Equal(expectedType, element.Type);
        Assert.Equal(expectedShortName, element.ShortValue);
        Assert.Equal(expectedLevel, element.AoLevel);
    }

    [Theory]
    [InlineData("улица Ленина", "улица", "ул.")]
    [InlineData("ул. Гагарина", "улица", "ул.")]
    [InlineData("ул Пушкина", "улица", "ул.")]
    [InlineData("проспект Мира", "проспект", "пр.")]
    [InlineData("пр. Победы", "проспект", "пр.")]
    [InlineData("пр Жукова", "проспект", "пр.")]
    [InlineData("бульвар Рокоссовского", "бульвар", "б.")]
    [InlineData("б. Гагарина", "бульвар", "б.")]
    [InlineData("б Центральный", "бульвар", "б.")]
    [InlineData("переулок Садовый", "переулок", "пер.")]
    [InlineData("пер. Лесной", "переулок", "пер.")]
    [InlineData("пер Новый", "переулок", "пер.")]
    [InlineData("шоссе Энтузиастов", "шоссе", "ш.")]
    [InlineData("ш. Калужское", "шоссе", "ш.")]
    [InlineData("тупик Солнечный", "тупик", "туп.")]
    [InlineData("аллея Парковая", "аллея", "ал.")]
    [InlineData("микрорайон 7", "микрорайон", "мкр.")]
    [InlineData("мкр. 12А", "микрорайон", "мкр.")]
    [InlineData("мкрн 5", "микрорайон", "мкр.")]
    [InlineData("снт Рассвет", "садоводческое некоммерческое товарищество", "СНТ")]
    [InlineData("днп Зелёный угол", "садоводческое некоммерческое товарищество", "СНТ")]
    [InlineData("тсн Берёзка", "садоводческое некоммерческое товарищество", "СНТ")]
    [InlineData(
        "садовое товарищество «Плодовое»",
        "садоводческое некоммерческое товарищество",
        "СНТ"
    )]
    [InlineData(
        "дачное некоммерческое партнёрство «Лесное»",
        "садоводческое некоммерческое товарищество",
        "СНТ"
    )]
    [InlineData("промышленная зона Северная", "территория", "тер.")]
    [InlineData("база Ока", "территория", "тер.")]
    [InlineData("военный городок №3", "территория", "тер.")]
    public void Create_StreetElement_Success(
        string input,
        string expectedType,
        string expectedShortName
    )
    {
        Result<LocationElement> result = StreetLocationElement.Create(input);
        Assert.True(result.IsSuccess);
        LocationElement element = result.Value;
        Assert.Equal(input, element.Value);
        Assert.Equal(expectedType, element.Type);
        Assert.Equal(expectedShortName, element.ShortValue);
        Assert.Equal((short)3, element.AoLevel);
    }

    [Theory]
    [InlineData("дом 42", "дом", "д.")]
    [InlineData("д. 15А", "дом", "д.")]
    [InlineData("д 7", "дом", "д.")]
    [InlineData("дом 100", "дом", "д.")]
    [InlineData("д.1", "дом", "д.")]
    [InlineData("д23", "дом", "д.")]
    [InlineData("корпус 3", "корпус", "к.")]
    [InlineData("корп. 2Б", "корпус", "к.")]
    [InlineData("к. 5", "корпус", "к.")]
    [InlineData("к 10", "корпус", "к.")]
    [InlineData("корп1", "корпус", "к.")]
    [InlineData("к.А", "корпус", "к.")]
    [InlineData("строение 1", "строение", "стр.")]
    [InlineData("стр. 10", "строение", "стр.")]
    [InlineData("с. 4", "строение", "стр.")]
    [InlineData("строение 5В", "строение", "стр.")]
    [InlineData("стр2", "строение", "стр.")]
    [InlineData("владение 12", "владение", "вл.")]
    [InlineData("вл. 8", "владение", "вл.")]
    [InlineData("владение 1", "владение", "вл.")]
    [InlineData("вл5", "владение", "вл.")]
    [InlineData("литера А", "литера", "лит.")]
    [InlineData("лит. Б", "литера", "лит.")]
    [InlineData("литера Я", "литера", "лит.")]
    [InlineData("литЁ", "литера", "лит.")]
    public void Create_BuildingElement_Success(
        string input,
        string expectedType,
        string expectedShortName
    )
    {
        var result = BuildingLocationElement.Create(input);
        Assert.True(result.IsSuccess);
        var element = result.Value;
        Assert.Equal(input, element.Value);
        Assert.Equal(expectedType, element.Type);
        Assert.Equal(expectedShortName, element.ShortValue);
        Assert.Equal((short)4, element.AoLevel);
    }
}
