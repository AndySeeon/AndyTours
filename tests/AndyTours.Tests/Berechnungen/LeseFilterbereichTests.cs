using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="LeseFilterbereich"/>.
/// </summary>
public class LeseFilterbereichTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("  ", null)]
    [InlineData(null, null)]
    public void Leere_Felder_ergeben_einen_inaktiven_gueltigen_Bereich(string? von, string? bis)
    {
        Filterbereich bereich = LeseFilterbereich.Ausfuehren(von, bis);

        Assert.Null(bereich.Von);
        Assert.Null(bereich.Bis);
        Assert.True(bereich.IstGueltig);
        Assert.False(bereich.IstAktiv);
    }

    [Theory]
    [InlineData("12,5", 12.5)]
    [InlineData("12.5", 12.5)]
    [InlineData(" 100 ", 100)]
    [InlineData("0", 0)]
    public void Komma_und_Punkt_gelten_als_Dezimalzeichen(string text, double erwartet)
    {
        Filterbereich bereich = LeseFilterbereich.Ausfuehren(text, "");

        Assert.Equal(erwartet, bereich.Von);
        Assert.True(bereich.IstAktiv);
    }

    [Fact]
    public void Nur_bis_gesetzt()
    {
        Filterbereich bereich = LeseFilterbereich.Ausfuehren("", "50");

        Assert.Null(bereich.Von);
        Assert.Equal(50, bereich.Bis);
        Assert.True(bereich.IstAktiv);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.234,5")]
    [InlineData("12km")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    public void Ungueltiger_Text_markiert_nur_sein_Feld(string text)
    {
        Filterbereich bereich = LeseFilterbereich.Ausfuehren(text, "50");

        Assert.True(bereich.VonFehlerhaft);
        Assert.False(bereich.BisFehlerhaft);
        Assert.False(bereich.IstGueltig);
        Assert.False(bereich.IstAktiv);
    }

    [Fact]
    public void Ungueltiges_Bis_markiert_nur_bis()
    {
        Filterbereich bereich = LeseFilterbereich.Ausfuehren("10", "x");

        Assert.False(bereich.VonFehlerhaft);
        Assert.True(bereich.BisFehlerhaft);
    }

    [Fact]
    public void Von_groesser_als_bis_markiert_beide_Felder()
    {
        Filterbereich bereich = LeseFilterbereich.Ausfuehren("100", "50");

        Assert.True(bereich.VonFehlerhaft);
        Assert.True(bereich.BisFehlerhaft);
        Assert.False(bereich.IstAktiv);
    }

    [Fact]
    public void Von_gleich_bis_ist_gueltig()
    {
        Filterbereich bereich = LeseFilterbereich.Ausfuehren("50", "50,0");

        Assert.True(bereich.IstAktiv);
        Assert.True(bereich.Enthaelt(50));
        Assert.False(bereich.Enthaelt(50.01));
    }
}
