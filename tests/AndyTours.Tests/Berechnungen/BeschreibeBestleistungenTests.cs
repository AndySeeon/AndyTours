using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BeschreibeBestleistungen"/>.
/// </summary>
public class BeschreibeBestleistungenTests
{
    /// <summary>Heute: 24. Oktober 2026.</summary>
    private static readonly DateOnly Heute = new(2026, 10, 24);

    [Fact]
    public void Jahresrekorde_mit_Jahr_und_Hervorhebung_des_laufenden_Jahres()
    {
        Bestleistungen bestleistungen = new(
            [
                new Jahresrekord(Fortschrittswert.Kilometer, 2019, 8123.44),
                new Jahresrekord(Fortschrittswert.Anzahl, 2026, 152),
                new Jahresrekord(Fortschrittswert.Hoehenmeter, 2021, 98765),
                new Jahresrekord(Fortschrittswert.Fahrzeit, 2019, 345.5),
            ],
            []);

        (List<Bestleistungszeile> jahresrekorde, List<Bestleistungszeile> einzelrekorde) = BeschreibeBestleistungen.Ausfuehren(bestleistungen, Heute);

        Assert.Equal(
            new[]
            {
                new Bestleistungszeile("Kilometer", "8.123,4 km", "2019", false),
                new Bestleistungszeile("Anzahl Einheiten", "152 Einheiten", "2026", true),
                new Bestleistungszeile("Höhenmeter", "98.765 hm", "2021", false),
                new Bestleistungszeile("Fahrzeit", "345:30 h", "2019", false),
            },
            jahresrekorde);
        Assert.Empty(einzelrekorde);
    }

    [Fact]
    public void Einzelrekorde_mit_Datum_und_Name()
    {
        Trainingseinheit alpen = new() { Id = 7, Datum = new(2021, 6, 14), Name = "Alpenrunde", Fahrzeit = new TimeSpan(9, 5, 7) };
        Trainingseinheit heute = new() { Id = 9, Datum = new(2026, 5, 1), Name = "" };
        Bestleistungen bestleistungen = new(
            [],
            [
                new Einzelrekord(Einzelrekordart.Distanz, 212.35, alpen),
                new Einzelrekord(Einzelrekordart.Hoehenmeter, 4321, alpen),
                new Einzelrekord(Einzelrekordart.Fahrzeit, 9.085, alpen),
                new Einzelrekord(Einzelrekordart.Durchschnittsgeschwindigkeit, 31.26, heute),
                new Einzelrekord(Einzelrekordart.Hoechstgeschwindigkeit, 87.94, alpen),
                new Einzelrekord(Einzelrekordart.HoechsterPunkt, 2758, alpen),
            ]);

        (_, List<Bestleistungszeile> einzelrekorde) = BeschreibeBestleistungen.Ausfuehren(bestleistungen, Heute);

        Assert.Equal(
            new[]
            {
                new Bestleistungszeile("Längste Tour", "212,35 km", "14.06.2021 Alpenrunde", false, 7),
                new Bestleistungszeile("Meiste Höhenmeter", "4.321 hm", "14.06.2021 Alpenrunde", false, 7),
                new Bestleistungszeile("Längste Fahrzeit", "9:05:07 h", "14.06.2021 Alpenrunde", false, 7),
                new Bestleistungszeile("Höchste Ø-Geschwindigkeit", "31,3 km/h", "01.05.2026", true, 9),
                new Bestleistungszeile("Höchste Geschwindigkeit", "87,9 km/h", "14.06.2021 Alpenrunde", false, 7),
                new Bestleistungszeile("Höchster Punkt", "2.758 m", "14.06.2021 Alpenrunde", false, 7),
            },
            einzelrekorde);
    }
}
