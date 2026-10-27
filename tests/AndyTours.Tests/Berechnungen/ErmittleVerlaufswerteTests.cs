using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleVerlaufswerte"/>.
/// </summary>
public class ErmittleVerlaufswerteTests
{
    /// <summary>Erwartet, wenn Puls und Temperatur vorhanden sind.</summary>
    private static readonly Verlaufswert[] PulsUndTemperatur = [Verlaufswert.Puls, Verlaufswert.Temperatur];

    [Fact]
    public void Vorhandene_Werte_genuegt_ein_Messpunkt()
    {
        Verlaufspunkt[] verlauf =
        [
            new() { Hoehe = 540, Temperatur = 14 },
            new() { Hoehe = 560, Puls = 120 },
        ];

        (bool hatHoehe, IReadOnlyList<Verlaufswert> zusatzwerte) = ErmittleVerlaufswerte.Ausfuehren(verlauf);

        Assert.True(hatHoehe);
        Assert.Equal(PulsUndTemperatur, zusatzwerte);
    }

    [Fact]
    public void Ohne_Hoehe_oder_nur_mit_Hoehe_0_keine_Hoehendaten()
    {
        Assert.False(ErmittleVerlaufswerte.Ausfuehren([new Verlaufspunkt { Puls = 100 }]).HatHoehe);
        Assert.False(ErmittleVerlaufswerte.Ausfuehren([new Verlaufspunkt { Hoehe = 0 }, new Verlaufspunkt { Hoehe = 0 }]).HatHoehe);
    }

    [Fact]
    public void Leerer_Verlauf_enthaelt_nichts()
    {
        (bool hatHoehe, IReadOnlyList<Verlaufswert> zusatzwerte) = ErmittleVerlaufswerte.Ausfuehren([]);

        Assert.False(hatHoehe);
        Assert.Empty(zusatzwerte);
    }
}
