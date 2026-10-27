using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BeschreibeJahresfortschritt"/>.
/// </summary>
public class BeschreibeJahresfortschrittTests
{
    /// <summary>Erzeugt einen Jahresfortschritt ohne Punkte mit den angegebenen Kennzahlen.</summary>
    private static Jahresfortschritt Fortschritt(Fortschrittswert wert, Jahreskennzahlen kennzahlen) => new(wert, [], kennzahlen);

    [Theory]
    [InlineData(Fortschrittswert.Kilometer, "Kilometer (Summe Entfernung)")]
    [InlineData(Fortschrittswert.Anzahl, "Anzahl Einheiten")]
    [InlineData(Fortschrittswert.Hoehenmeter, "Höhenmeter (Summe Höhenmeter)")]
    [InlineData(Fortschrittswert.Fahrzeit, "Fahrzeit")]
    public void Ueberschrift_nennt_den_Wert(Fortschrittswert wert, string erwartet)
    {
        Assert.Equal(erwartet, BeschreibeJahresfortschritt.Ueberschrift(wert));
    }

    [Fact]
    public void Rueckstand_mit_Wochenleistung()
    {
        Jahreskennzahlen k = new(2026, 4321.5, 6000, 0.72025, 4900.3, -578.8, 1678.5, 167.9);

        (string stand, string ausblick) = BeschreibeJahresfortschritt.Kennzahlen(Fortschritt(Fortschrittswert.Kilometer, k));

        Assert.Equal("2026: 4.321,5 km von 6.000 km (72 %) · Soll heute 4.900,3 km · 578,8 km Rückstand", stand);
        Assert.Equal("noch 1.678,5 km – 167,9 km pro Woche", ausblick);
    }

    [Fact]
    public void Vorsprung_wird_genannt()
    {
        Jahreskennzahlen k = new(2026, 60, 100, 0.6, 50, 10, 40, 1.5);

        (string stand, string ausblick) = BeschreibeJahresfortschritt.Kennzahlen(Fortschritt(Fortschrittswert.Anzahl, k));

        Assert.Equal("2026: 60 Einheiten von 100 Einheiten (60 %) · Soll heute 50 Einheiten · 10 Einheiten Vorsprung", stand);
        Assert.Equal("noch 40 Einheiten – 1,5 Einheiten pro Woche", ausblick);
    }

    [Fact]
    public void Erreichtes_Ziel_nennt_den_Ueberschuss()
    {
        Jahreskennzahlen k = new(2026, 85000, 80000, 1.0625, 40000, 45000, 0, null);

        (_, string ausblick) = BeschreibeJahresfortschritt.Kennzahlen(Fortschritt(Fortschrittswert.Hoehenmeter, k));

        Assert.Equal("Ziel erreicht – 5.000 hm darüber", ausblick);
    }

    [Fact]
    public void Ohne_Ziel_steht_der_Hinweis_auf_die_Saisonziele()
    {
        Jahreskennzahlen k = new(2026, 4321.5, null, null, null, null, null, null);

        (string stand, string ausblick) = BeschreibeJahresfortschritt.Kennzahlen(Fortschritt(Fortschrittswert.Kilometer, k));

        Assert.Equal("2026: 4.321,5 km", stand);
        Assert.Equal(BeschreibeJahresfortschritt.KeinZiel, ausblick);
    }

    [Fact]
    public void Fahrzeit_zeigt_nur_den_Stand()
    {
        Jahreskennzahlen k = new(2026, 123.75, null, null, null, null, null, null);

        (string stand, string ausblick) = BeschreibeJahresfortschritt.Kennzahlen(Fortschritt(Fortschrittswert.Fahrzeit, k));

        Assert.Equal("2026: 123:45 h", stand);
        Assert.Equal("", ausblick);
    }
}
