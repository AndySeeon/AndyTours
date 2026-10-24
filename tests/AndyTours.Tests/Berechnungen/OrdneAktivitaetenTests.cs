using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="OrdneAktivitaeten"/>.
/// </summary>
public class OrdneAktivitaetenTests
{
    /// <summary>Drei Aktivitäten in Id-Reihenfolge.</summary>
    private static List<Aktivitaet> Aktivitaeten() =>
    [
        new() { Id = 1, Name = "Bike" },
        new() { Id = 2, Name = "Laufen" },
        new() { Id = 3, Name = "Gravel" },
    ];

    [Fact]
    public void Ohne_gespeicherte_Reihenfolge_gilt_die_Id_Reihenfolge()
    {
        Assert.Equal(["Bike", "Laufen", "Gravel"], OrdneAktivitaeten.Ausfuehren(Aktivitaeten(), []).Select(a => a.Name));
    }

    [Fact]
    public void Folgt_der_gespeicherten_Reihenfolge()
    {
        Assert.Equal(["Gravel", "Bike", "Laufen"], OrdneAktivitaeten.Ausfuehren(Aktivitaeten(), [3, 1, 2]).Select(a => a.Name));
    }

    [Fact]
    public void Fehlende_Ids_kommen_hinten_dazu_unbekannte_werden_uebergangen()
    {
        Assert.Equal(["Laufen", "Bike", "Gravel"], OrdneAktivitaeten.Ausfuehren(Aktivitaeten(), [99, 2]).Select(a => a.Name));
    }
}
