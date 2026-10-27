using AndyTours.Core.Berechnungen;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="Eingabetext"/>.
/// </summary>
public class EingabetextTests
{
    [Theory]
    [InlineData("12,5", 12.5)]
    [InlineData("12.5", 12.5)]
    [InlineData(" 100 ", 100)]
    [InlineData("-3,5", -3.5)]
    public void Zahl_mit_Komma_oder_Punkt(string text, double erwartet)
    {
        Assert.True(Eingabetext.LeseZahl(text, out double? zahl));
        Assert.Equal(erwartet, zahl);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.234,5")]
    [InlineData("NaN")]
    public void Ungueltige_Zahl(string text)
    {
        Assert.False(Eingabetext.LeseZahl(text, out double? zahl));
        Assert.Null(zahl);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Leeres_Feld_ist_gueltig_und_null(string? text)
    {
        Assert.True(Eingabetext.LeseZahl(text, out double? zahl));
        Assert.Null(zahl);
        Assert.True(Eingabetext.LeseGanzzahl(text, out int? ganz));
        Assert.Null(ganz);
        Assert.True(Eingabetext.LeseDatum(text, out DateOnly? datum));
        Assert.Null(datum);
        Assert.True(Eingabetext.LeseUhrzeit(text, out TimeOnly? uhrzeit));
        Assert.Null(uhrzeit);
        Assert.True(Eingabetext.LeseDauer(text, out TimeSpan? dauer));
        Assert.Null(dauer);
    }

    [Theory]
    [InlineData("1234", 1234)]
    [InlineData("-5", -5)]
    public void Ganzzahl(string text, int erwartet)
    {
        Assert.True(Eingabetext.LeseGanzzahl(text, out int? zahl));
        Assert.Equal(erwartet, zahl);
    }

    [Theory]
    [InlineData("12,5")]
    [InlineData("1.234")]
    [InlineData("zehn")]
    public void Ungueltige_Ganzzahl(string text)
    {
        Assert.False(Eingabetext.LeseGanzzahl(text, out _));
    }

    [Theory]
    [InlineData("24.10.2026")]
    [InlineData("24.10.26")]
    [InlineData("24.10.2026 ")]
    public void Datum_vierstellig_oder_zweistellig(string text)
    {
        Assert.True(Eingabetext.LeseDatum(text, out DateOnly? datum));
        Assert.Equal(new DateOnly(2026, 10, 24), datum);
    }

    [Fact]
    public void Datum_mit_einstelligem_Tag_und_Monat()
    {
        Assert.True(Eingabetext.LeseDatum("3.5.2025", out DateOnly? datum));
        Assert.Equal(new DateOnly(2025, 5, 3), datum);
    }

    [Theory]
    [InlineData("31.02.2026")]
    [InlineData("2026-10-24")]
    [InlineData("gestern")]
    public void Ungueltiges_Datum(string text)
    {
        Assert.False(Eingabetext.LeseDatum(text, out _));
    }

    [Theory]
    [InlineData("9:30", 9, 30)]
    [InlineData("09:30", 9, 30)]
    [InlineData("23:59", 23, 59)]
    public void Uhrzeit(string text, int stunde, int minute)
    {
        Assert.True(Eingabetext.LeseUhrzeit(text, out TimeOnly? uhrzeit));
        Assert.Equal(new TimeOnly(stunde, minute), uhrzeit);
    }

    [Theory]
    [InlineData("24:00")]
    [InlineData("9.30")]
    [InlineData("9:3")]
    public void Ungueltige_Uhrzeit(string text)
    {
        Assert.False(Eingabetext.LeseUhrzeit(text, out _));
    }

    [Theory]
    [InlineData("3:45:12", 3, 45, 12)]
    [InlineData("0:05:07", 0, 5, 7)]
    [InlineData("30:00:00", 30, 0, 0)]
    [InlineData("3:45", 3, 45, 0)]
    [InlineData("12:05", 12, 5, 0)]
    public void Dauer(string text, int stunden, int minuten, int sekunden)
    {
        Assert.True(Eingabetext.LeseDauer(text, out TimeSpan? dauer));
        Assert.Equal(new TimeSpan(stunden, minuten, sekunden), dauer);
    }

    [Theory]
    [InlineData("3")]
    [InlineData("3:5")]
    [InlineData("3:60")]
    [InlineData("3:45:")]
    [InlineData("3:60:00")]
    [InlineData("3:45:7")]
    [InlineData("-1:00:00")]
    [InlineData("drei Stunden")]
    public void Ungueltige_Dauer(string text)
    {
        Assert.False(Eingabetext.LeseDauer(text, out _));
    }

    [Fact]
    public void Ausgabe_ohne_Tausenderpunkt_und_ohne_Stellenverlust()
    {
        Assert.Equal("1234,5", Eingabetext.AlsText(1234.5));
        Assert.Equal("52,34567", Eingabetext.AlsText(52.34567));
        Assert.Equal("1234", Eingabetext.AlsText(1234));
        Assert.Equal("03.05.2025", Eingabetext.AlsText(new DateOnly(2025, 5, 3)));
        Assert.Equal("9:05", Eingabetext.AlsText(new TimeOnly(9, 5)));
        Assert.Equal("30:05:07", Eingabetext.AlsText(new TimeSpan(30, 5, 7)));
    }

    [Fact]
    public void Leere_Werte_ergeben_leeren_Text()
    {
        Assert.Equal("", Eingabetext.AlsText((double?)null));
        Assert.Equal("", Eingabetext.AlsText((int?)null));
        Assert.Equal("", Eingabetext.AlsText((DateOnly?)null));
        Assert.Equal("", Eingabetext.AlsText((TimeOnly?)null));
        Assert.Equal("", Eingabetext.AlsText((TimeSpan?)null));
    }

    [Fact]
    public void Ausgabe_laesst_sich_wieder_lesen()
    {
        Assert.True(Eingabetext.LeseZahl(Eingabetext.AlsText(52.34567), out double? zahl));
        Assert.Equal(52.34567, zahl);
        Assert.True(Eingabetext.LeseDatum(Eingabetext.AlsText(new DateOnly(2025, 5, 3)), out DateOnly? datum));
        Assert.Equal(new DateOnly(2025, 5, 3), datum);
        Assert.True(Eingabetext.LeseUhrzeit(Eingabetext.AlsText(new TimeOnly(9, 5)), out TimeOnly? uhrzeit));
        Assert.Equal(new TimeOnly(9, 5), uhrzeit);
        Assert.True(Eingabetext.LeseDauer(Eingabetext.AlsText(new TimeSpan(30, 5, 7)), out TimeSpan? dauer));
        Assert.Equal(new TimeSpan(30, 5, 7), dauer);
    }
}
