using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BeschreibeAbbiegung"/>.
/// </summary>
public class BeschreibeAbbiegungTests
{
    private static readonly Geoposition Ort = new(48.0, 11.0);

    [Theory]
    [InlineData(Abbiegerichtung.Links, "Links")]
    [InlineData(Abbiegerichtung.Rechts, "Rechts")]
    [InlineData(Abbiegerichtung.LeichtLinks, "Leicht links")]
    [InlineData(Abbiegerichtung.LeichtRechts, "Leicht rechts")]
    [InlineData(Abbiegerichtung.ScharfLinks, "Scharf links")]
    [InlineData(Abbiegerichtung.ScharfRechts, "Scharf rechts")]
    [InlineData(Abbiegerichtung.Wenden, "Wenden")]
    public void Richtung_in_Worten(Abbiegerichtung richtung, string text)
    {
        Assert.Equal(text, BeschreibeAbbiegung.Ausfuehren(new Abbiegepunkt(Ort, richtung)));
    }

    [Fact]
    public void Kreisverkehr_mit_Ausfahrt()
    {
        Assert.Equal("Kreisverkehr 2. Ausfahrt", BeschreibeAbbiegung.Ausfuehren(new Abbiegepunkt(Ort, Abbiegerichtung.Kreisverkehr, 2)));
    }

    [Fact]
    public void Kreisverkehr_ohne_bekannte_Ausfahrt()
    {
        Assert.Equal("Kreisverkehr", BeschreibeAbbiegung.Ausfuehren(new Abbiegepunkt(Ort, Abbiegerichtung.Kreisverkehr)));
    }

    [Theory]
    [InlineData(Abbiegerichtung.Links, null, "Links")]
    [InlineData(Abbiegerichtung.ScharfRechts, null, "Scharf rechts")]
    [InlineData(Abbiegerichtung.Wenden, null, "Wenden")]
    [InlineData(Abbiegerichtung.Kreisverkehr, 2, "Kreisel 2. Ausf.")]
    [InlineData(Abbiegerichtung.Kreisverkehr, null, "Kreisel")]
    [InlineData(Abbiegerichtung.Kreisverkehr, 12, "Kreisel")]
    public void Kurze_Form_fuer_den_Edge(Abbiegerichtung richtung, int? ausfahrt, string text)
    {
        Assert.Equal(text, BeschreibeAbbiegung.Kurz(richtung, ausfahrt));
    }

    [Fact]
    public void Kurze_Formen_passen_auf_den_Edge()
    {
        foreach (Abbiegerichtung richtung in Enum.GetValues<Abbiegerichtung>())
        {
            Assert.InRange(BeschreibeAbbiegung.Kurz(richtung, 9).Length, 1, BeschreibeAbbiegung.KurzeLaenge);
        }
    }
}
