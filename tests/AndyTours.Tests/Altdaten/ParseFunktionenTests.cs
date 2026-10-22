using AndyTours.Import.Altdaten;

namespace AndyTours.Tests.Altdaten;

/// <summary>
/// Tests für die kleinen Umwandlungsfunktionen (Zahl, Datum, Fahrzeit, Hochkommas, Anhangpfad).
/// </summary>
public class ParseFunktionenTests
{
    [Theory]
    [InlineData("77.83", 77.83)]
    [InlineData("0,75", 0.75)]
    [InlineData("-3", -3)]
    [InlineData("", 0)]
    [InlineData(" ", 0)]
    public void ParseZahl_liest_Punkt_und_Komma(string text, double erwartet)
    {
        Assert.Equal(erwartet, ParseZahl.Ausfuehren(text), precision: 6);
    }

    [Fact]
    public void ParseZahl_meldet_ungueltigen_Text()
    {
        Assert.Throws<FormatException>(() => ParseZahl.Ausfuehren("abc"));
    }

    [Theory]
    [InlineData("95.0", 95)]
    [InlineData("2.5", 3)]
    public void ParseGanzzahl_rundet_kaufmaennisch(string text, int erwartet)
    {
        Assert.Equal(erwartet, ParseGanzzahl.Ausfuehren(text));
    }

    [Fact]
    public void ParseAltesDatum_akzeptiert_beide_Schreibweisen()
    {
        Assert.Equal(new DateOnly(2016, 3, 5), ParseAltesDatum.Ausfuehren("05.03.2016"));
        Assert.Equal(new DateOnly(2016, 4, 13), ParseAltesDatum.Ausfuehren("13.4.2016"));
    }

    [Theory]
    [InlineData("01.01.1900")]
    [InlineData("")]
    public void ParseAltesDatum_liefert_null_fuer_Platzhalter(string text)
    {
        Assert.Null(ParseAltesDatum.Ausfuehren(text));
    }

    [Fact]
    public void ParseFahrzeit_erlaubt_mehr_als_24_Stunden()
    {
        Assert.Equal(new TimeSpan(4, 33, 23), ParseFahrzeit.Ausfuehren("04:33:23"));
        Assert.Equal(TimeSpan.FromHours(25.5), ParseFahrzeit.Ausfuehren("25:30:00"));
        Assert.Equal(new TimeSpan(8, 40, 0), ParseFahrzeit.Ausfuehren("08:40"));
    }

    [Fact]
    public void ParseFahrzeit_meldet_ungueltigen_Text()
    {
        Assert.Throws<FormatException>(() => ParseFahrzeit.Ausfuehren("4h33"));
    }

    [Theory]
    [InlineData("'mit Sabine'", "mit Sabine")]
    [InlineData("''", "")]
    [InlineData("Namur", "Namur")]
    [InlineData("'Bernie's Runde'", "Bernie's Runde")]
    public void EntferneHochkommas_entfernt_nur_die_aeusseren(string text, string erwartet)
    {
        Assert.Equal(erwartet, EntferneHochkommas.Ausfuehren(text));
    }

    [Theory]
    [InlineData("2016/2016-03-05-08-40-41.fit", "2016/2016-03-05-08-40-41.fit")]
    [InlineData(@"D:\BikeXperience\2026\2026-10-05-06-28-55.fit", "2026/2026-10-05-06-28-55.fit")]
    [InlineData(@"2016\a.fit", "2016/a.fit")]
    [InlineData("", "")]
    public void NormalisiereAnhangPfad_macht_Pfade_relativ(string pfad, string erwartet)
    {
        Assert.Equal(erwartet, NormalisiereAnhangPfad.Ausfuehren(pfad));
    }

    [Fact]
    public void ParseZahlenliste_liest_Zaehnezahlen()
    {
        Assert.Equal(new List<int> { 11, 12, 13 }, ParseZahlenliste.Ausfuehren("11,12, 13"));
        Assert.Empty(ParseZahlenliste.Ausfuehren(""));
    }
}
