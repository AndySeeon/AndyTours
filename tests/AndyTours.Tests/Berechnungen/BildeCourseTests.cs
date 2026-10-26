using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BildeCourse"/>.
/// </summary>
public class BildeCourseTests
{
    /// <summary>Etwa 1,1 km je 0,01 Grad Breite.</summary>
    private static Verlaufspunkt Punkt(double breite, double? hoehe = null, int sekunden = 0) =>
        new() { Position = new Geoposition(breite, 11.0), Hoehe = hoehe, Fahrzeit = TimeSpan.FromSeconds(sekunden) };

    private static readonly Geoposition[] Wegpunkte = [new(48.00, 11.0), new(48.01, 11.0), new(48.02, 11.0)];

    private static Tourenplanung Planung(params Teilstueck?[] teilstuecke) =>
        new(Fahrprofil.Trekking, Wegpunkte[..(teilstuecke.Length + 1)], teilstuecke);

    [Fact]
    public void Planung_reiht_die_Teilstuecke_ohne_doppelte_Punkte_aneinander()
    {
        var erstes = new Teilstueck([Punkt(48.00, 500), Punkt(48.005, 510), Punkt(48.01, 520)], [], false);
        var zweites = new Teilstueck([Punkt(48.01, 520), Punkt(48.02, 530)], [], false);

        Course course = BildeCourse.AusPlanung(Planung(erstes, zweites), "Runde");

        Assert.Equal("Runde", course.Name);
        Assert.Equal([48.00, 48.005, 48.01, 48.02], course.Punkte.Select(p => p.Position.Breite));
        Assert.Equal([500.0, 510.0, 520.0, 530.0], course.Punkte.Select(p => p.Hoehe));
        Assert.Equal(2.224, course.Punkte[^1].DistanzKm, 2);
    }

    [Fact]
    public void Planung_hat_Zeiten_bei_15_kmh()
    {
        var teilstueck = new Teilstueck([Punkt(48.00), Punkt(48.01)], [], false);

        Course course = BildeCourse.AusPlanung(Planung(teilstueck), "X");

        Assert.Equal(TimeSpan.Zero, course.Punkte[0].Zeit);
        Assert.Equal(course.Punkte[1].DistanzKm / 15 * 3600, course.Punkte[1].Zeit.TotalSeconds, 3);
    }

    [Fact]
    public void Abbiegepunkte_kommen_an_den_naechsten_Punkt_ihres_Teilstuecks()
    {
        var erstes = new Teilstueck([Punkt(48.00), Punkt(48.005), Punkt(48.01)],
            [new Abbiegepunkt(new Geoposition(48.0051, 11.0001), Abbiegerichtung.Links)], false);
        var zweites = new Teilstueck([Punkt(48.01), Punkt(48.015), Punkt(48.02)],
            [new Abbiegepunkt(new Geoposition(48.0149, 11.0), Abbiegerichtung.Kreisverkehr, 2)], false);

        Course course = BildeCourse.AusPlanung(Planung(erstes, zweites), "X");

        Assert.Equal([new Courseabbiegung(1, Abbiegerichtung.Links), new Courseabbiegung(3, Abbiegerichtung.Kreisverkehr, 2)], course.Abbiegungen);
    }

    [Fact]
    public void Nicht_ermitteltes_Teilstueck_zaehlt_als_gerade_Linie()
    {
        Course course = BildeCourse.AusPlanung(Planung((Teilstueck?)null), "X");

        Assert.Equal([48.00, 48.01], course.Punkte.Select(p => p.Position.Breite));
        Assert.All(course.Punkte, p => Assert.Null(p.Hoehe));
    }

    [Fact]
    public void Gefahrene_Strecke_behaelt_ihre_Linie_ohne_Punkte_ohne_Position()
    {
        List<Verlaufspunkt> verlauf = [Punkt(48.00, 500, 0), new Verlaufspunkt { Fahrzeit = TimeSpan.FromSeconds(30) }, Punkt(48.01, 510, 60), Punkt(48.02, 520, 120)];
        Courseabbiegung abbiegung = new(1, Abbiegerichtung.Rechts);

        Course course = BildeCourse.AusVerlauf(verlauf, "Tour", [abbiegung]);

        Assert.Equal([48.00, 48.01, 48.02], course.Punkte.Select(p => p.Position.Breite));
        Assert.Equal(BildeCourse.Spur(verlauf), course.Punkte.Select(p => p.Position));
        Assert.Equal([500.0, 510.0, 520.0], course.Punkte.Select(p => p.Hoehe));
        Assert.Equal([abbiegung], course.Abbiegungen);
    }

    [Fact]
    public void Gefahrene_Strecke_hat_ihre_eigenen_Fahrzeiten()
    {
        List<Verlaufspunkt> verlauf = [Punkt(48.00, sekunden: 100), Punkt(48.01, sekunden: 160), Punkt(48.02, sekunden: 400)];

        Course course = BildeCourse.AusVerlauf(verlauf, "Tour", []);

        Assert.Equal([0.0, 60.0, 300.0], course.Punkte.Select(p => p.Zeit.TotalSeconds));
    }

    [Fact]
    public void Ohne_ansteigende_Fahrzeiten_gelten_15_kmh()
    {
        // z. B. ein umgekehrter Track
        List<Verlaufspunkt> verlauf = [Punkt(48.00, sekunden: 400), Punkt(48.01, sekunden: 160), Punkt(48.02, sekunden: 100)];

        Course course = BildeCourse.AusVerlauf(verlauf, "Tour", []);

        Assert.Equal(course.Punkte[2].DistanzKm / 15 * 3600, course.Punkte[2].Zeit.TotalSeconds, 3);
    }

    [Fact]
    public void Zeit_geht_nie_zurueck()
    {
        List<Verlaufspunkt> verlauf = [Punkt(48.00, sekunden: 0), Punkt(48.01, sekunden: 100), Punkt(48.015, sekunden: 90), Punkt(48.02, sekunden: 200)];

        Course course = BildeCourse.AusVerlauf(verlauf, "Tour", []);

        Assert.Equal([0.0, 100.0, 100.0, 200.0], course.Punkte.Select(p => p.Zeit.TotalSeconds));
    }
}
