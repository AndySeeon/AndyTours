using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleBestwerte"/>.
/// </summary>
public class ErmittleBestwerteTests
{
    /// <summary>Erzeugt eine Einheit mit den Bestwert-relevanten Werten.</summary>
    private static Trainingseinheit Einheit(double km, int hm, double stunden, double maxKmh) =>
        new() { DistanzKm = km, Hoehenmeter = hm, Fahrzeit = TimeSpan.FromHours(stunden), MaxGeschwindigkeit = maxKmh };

    [Fact]
    public void Je_Spalte_wird_der_hoechste_Wert_ermittelt()
    {
        Bestwerte bestwerte = ErmittleBestwerte.Ausfuehren(
        [
            Einheit(100, 500, 5, 60),   // Ø 20 km/h
            Einheit(60, 1800, 4, 72),   // Ø 15 km/h
            Einheit(50, 300, 2, 55),    // Ø 25 km/h
        ]);

        Assert.Equal(new Bestwerte(100, 1800, TimeSpan.FromHours(5), 25, 72), bestwerte);
    }

    [Fact]
    public void Fehlende_Werte_zaehlen_nicht()
    {
        // Die zweite Einheit hat keine Fahrzeit → keine Ø-Geschwindigkeit (statt „unendlich“)
        Bestwerte bestwerte = ErmittleBestwerte.Ausfuehren([Einheit(40, 200, 2, 0), Einheit(80, 0, 0, 0)]);

        Assert.Equal(20.0, bestwerte.DurchschnittKmh);
        Assert.Null(bestwerte.MaxKmh);
    }

    [Fact]
    public void Nur_fehlende_Werte_ergeben_keinen_Bestwert()
    {
        Bestwerte bestwerte = ErmittleBestwerte.Ausfuehren([Einheit(0, 0, 0, 0)]);

        Assert.Equal(new Bestwerte(null, null, null, null, null), bestwerte);
    }

    [Fact]
    public void Leere_Auswahl_ergibt_keine_Bestwerte()
    {
        Assert.Equal(new Bestwerte(null, null, null, null, null), ErmittleBestwerte.Ausfuehren([]));
    }
}
