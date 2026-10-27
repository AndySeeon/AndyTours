using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="IstTourVorhanden"/>.
/// </summary>
public class IstTourVorhandenTests
{
    /// <summary>Vorhanden: 26.09.2026 um 08:02 (aus den Altdaten, ohne Sekunden) und eine Einheit ohne Startzeit.</summary>
    private static readonly List<Trainingseinheit> Einheiten =
    [
        new() { Id = 1, Datum = new(2026, 9, 26), Startzeit = new(8, 2) },
        new() { Id = 2, Datum = new(2026, 9, 28) },
    ];

    [Fact]
    public void Gleiches_Datum_und_gleiche_Minute_ist_vorhanden()
    {
        Assert.True(IstTourVorhanden.Ausfuehren(Einheiten, new(2026, 9, 26), new(8, 2, 49)));
    }

    [Fact]
    public void Andere_Minute_oder_anderes_Datum_ist_neu()
    {
        Assert.False(IstTourVorhanden.Ausfuehren(Einheiten, new(2026, 9, 26), new(8, 3, 0)));
        Assert.False(IstTourVorhanden.Ausfuehren(Einheiten, new(2026, 9, 27), new(8, 2, 49)));
    }

    [Fact]
    public void Einheit_ohne_Startzeit_zaehlt_nicht()
    {
        Assert.False(IstTourVorhanden.Ausfuehren(Einheiten, new(2026, 9, 28), new(0, 0)));
    }
}
