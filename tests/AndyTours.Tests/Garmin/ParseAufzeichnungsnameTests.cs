using AndyTours.Import.Garmin;

namespace AndyTours.Tests.Garmin;

/// <summary>
/// Tests für <see cref="ParseAufzeichnungsname"/>.
/// </summary>
public class ParseAufzeichnungsnameTests
{
    [Fact]
    public void Name_des_Edge_liefert_Datum_und_Startzeit()
    {
        Assert.Equal((new DateOnly(2026, 9, 26), new TimeOnly(8, 2, 49)), ParseAufzeichnungsname.Ausfuehren("2026-09-26-08-02-49.fit"));
        Assert.Equal((new DateOnly(2026, 9, 26), new TimeOnly(8, 2, 49)), ParseAufzeichnungsname.Ausfuehren("2026-09-26-08-02-49.FIT"));
    }

    [Theory]
    [InlineData("Tour.fit")]
    [InlineData("2026-09-26.fit")]
    [InlineData("2026-13-26-08-02-49.fit")]
    public void Anderer_Name_liefert_nichts(string name)
    {
        Assert.Null(ParseAufzeichnungsname.Ausfuehren(name));
    }
}
