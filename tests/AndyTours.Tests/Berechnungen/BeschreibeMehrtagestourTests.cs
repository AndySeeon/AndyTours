using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BeschreibeMehrtagestour"/>.
/// </summary>
public class BeschreibeMehrtagestourTests
{
    [Fact]
    public void Zeitraum_Etappen_Summen_Durchschnitt_und_hoechster_Punkt()
    {
        Trainingseinheit[] einheiten =
        [
            new() { Datum = new DateOnly(2025, 7, 12), DistanzKm = 100.25, Hoehenmeter = 1500, Fahrzeit = new TimeSpan(5, 0, 0), MaxHoehe = 1800 },
            new() { Datum = new DateOnly(2025, 7, 13), DistanzKm = 80, Hoehenmeter = 2200, Fahrzeit = new TimeSpan(4, 30, 30), MaxHoehe = 2504 },
            new() { Datum = new DateOnly(2025, 7, 18), DistanzKm = 59.75, Hoehenmeter = 300, Fahrzeit = new TimeSpan(2, 29, 30), MaxHoehe = 900 },
        ];

        Assert.Equal(
            "Sa, 12.07.2025 – Fr, 18.07.2025 · 3 Etappen · 240,00 km · 4.000 hm · 12:00:00 h · Ø 20,0 km/h · höchster Punkt 2.504 m",
            BeschreibeMehrtagestour.Ausfuehren(einheiten));
    }

    [Fact]
    public void Einheiten_ohne_Aufzeichnung_zaehlen_mit()
    {
        // Die zweite Etappe hat keine Aufzeichnung (z. B. Radcomputer ausgefallen); ihre Werte wurden von Hand erfasst
        Trainingseinheit[] einheiten =
        [
            new() { Datum = new DateOnly(2025, 7, 12), DistanzKm = 50, Fahrzeit = new TimeSpan(2, 0, 0),
                    Anhaenge = [new Anhang { Typ = AnhangTyp.GpsTrack, RelativerPfad = "2025/a.fit" }] },
            new() { Datum = new DateOnly(2025, 7, 13), DistanzKm = 70, Fahrzeit = new TimeSpan(4, 0, 0) },
        ];

        Assert.Equal("Sa, 12.07.2025 – So, 13.07.2025 · 2 Etappen · 120,00 km · 0 hm · 6:00:00 h · Ø 20,0 km/h",
            BeschreibeMehrtagestour.Ausfuehren(einheiten));
    }

    [Fact]
    public void Am_selben_Tag_und_ohne_Fahrzeit()
    {
        Trainingseinheit[] einheiten =
        [
            new() { Datum = new DateOnly(2025, 7, 12), DistanzKm = 20 },
            new() { Datum = new DateOnly(2025, 7, 12), DistanzKm = 15 },
        ];

        Assert.Equal("Sa, 12.07.2025 · 2 Etappen · 35,00 km · 0 hm · 0:00:00 h", BeschreibeMehrtagestour.Ausfuehren(einheiten));
    }
}
