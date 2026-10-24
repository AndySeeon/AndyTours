using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleBestleistungen"/>.
/// </summary>
public class ErmittleBestleistungenTests
{
    /// <summary>
    /// Erzeugt den Testbestand.
    /// </summary>
    /// <remarks>
    /// Aktivitäten: Bike (1), Laufen (2). Kategorien: Training (10), Urlaub (11).
    /// <list type="bullet">
    /// <item>2023: Bike 100 km / 1.000 hm / 4 h, max 50 km/h, Höhe 800 m (Training)</item>
    /// <item>2024: Bike 60 km / 1.500 hm / 2 h, ohne Höchstgeschwindigkeit, Höhe 2.000 m (Urlaub)
    /// und Bike 40 km / 0 hm / 1 h, max 50 km/h (gleich wie 2023), Start 18:00 (Training)</item>
    /// <item>2023: Laufen 5 km / 0 hm / 0,5 h</item>
    /// <item>2025: Laufen 12 km / 100 hm / 1 h</item>
    /// </list>
    /// Jahressummen Bike: 2023 100 km, 1 Einheit, 1.000 hm, 4 h; 2024 100 km (Gleichstand), 2 Einheiten,
    /// 1.500 hm, 3 h.
    /// </remarks>
    private static Datenbestand Bestand() => new()
    {
        Aktivitaeten =
        [
            new() { Id = 1, Name = "Bike" },
            new() { Id = 2, Name = "Laufen" },
        ],
        Kategorien =
        [
            new() { Id = 10, Name = "Training" },
            new() { Id = 11, Name = "Urlaub" },
        ],
        Trainingseinheiten =
        [
            new() { Id = 1, AktivitaetId = 1, Datum = new(2023, 6, 1), Name = "Lang", DistanzKm = 100, Hoehenmeter = 1000, Fahrzeit = TimeSpan.FromHours(4), MaxGeschwindigkeit = 50, MaxHoehe = 800, KategorieIds = [10] },
            new() { Id = 3, AktivitaetId = 1, Datum = new(2024, 7, 1), Startzeit = new(18, 0), Name = "Abend", DistanzKm = 40, Fahrzeit = TimeSpan.FromHours(1), MaxGeschwindigkeit = 50, KategorieIds = [10] },
            new() { Id = 2, AktivitaetId = 1, Datum = new(2024, 7, 1), Startzeit = new(9, 0), Name = "Pass", DistanzKm = 60, Hoehenmeter = 1500, Fahrzeit = TimeSpan.FromHours(2), MaxHoehe = 2000, KategorieIds = [11] },
            new() { Id = 5, AktivitaetId = 2, Datum = new(2023, 8, 1), Name = "Kurz", DistanzKm = 5, Fahrzeit = TimeSpan.FromHours(0.5) },
            new() { Id = 4, AktivitaetId = 2, Datum = new(2025, 3, 1), Name = "Lauf", DistanzKm = 12, Hoehenmeter = 100, Fahrzeit = TimeSpan.FromHours(1) },
        ],
    };

    [Fact]
    public void Jahresrekorde_mit_Rekordjahr_und_frueherem_Jahr_bei_Gleichstand()
    {
        Bestleistungen ergebnis = ErmittleBestleistungen.Ausfuehren(Bestand(), 1, null);

        Assert.Equal(
            new[]
            {
                new Jahresrekord(Fortschrittswert.Kilometer, 2023, 100),
                new Jahresrekord(Fortschrittswert.Anzahl, 2024, 2),
                new Jahresrekord(Fortschrittswert.Hoehenmeter, 2024, 1500),
                new Jahresrekord(Fortschrittswert.Fahrzeit, 2023, 4),
            },
            ergebnis.Jahresrekorde);
    }

    [Fact]
    public void Einzelrekorde_mit_Einheit()
    {
        Bestleistungen ergebnis = ErmittleBestleistungen.Ausfuehren(Bestand(), 1, null);

        Assert.Equal(
            new[]
            {
                (Einzelrekordart.Distanz, 100.0, 1L),
                (Einzelrekordart.Hoehenmeter, 1500.0, 2L),
                (Einzelrekordart.Fahrzeit, 4.0, 1L),
                (Einzelrekordart.Durchschnittsgeschwindigkeit, 40.0, 3L),
                (Einzelrekordart.Hoechstgeschwindigkeit, 50.0, 1L),
                (Einzelrekordart.HoechsterPunkt, 2000.0, 2L),
            },
            ergebnis.Einzelrekorde.Select(r => (r.Art, r.Wert, r.Einheit.Id)));
    }

    [Fact]
    public void Bei_gleichem_Tag_gewinnt_die_fruehere_Startzeit()
    {
        // Pass (09:00) und Abend (18:00) am selben Tag mit gleicher Höchstgeschwindigkeit
        Datenbestand bestand = Bestand();
        bestand.Trainingseinheiten.RemoveAll(e => e.Id == 1);
        bestand.Trainingseinheiten.First(e => e.Id == 2).MaxGeschwindigkeit = 50;

        Bestleistungen ergebnis = ErmittleBestleistungen.Ausfuehren(bestand, 1, null);

        Assert.Equal(2, ergebnis.Einzelrekorde.Single(r => r.Art == Einzelrekordart.Hoechstgeschwindigkeit).Einheit.Id);
    }

    [Fact]
    public void Ohne_vorhandenen_Wert_fehlt_der_Rekord()
    {
        // Laufen hat weder Höchstgeschwindigkeit noch Höhe
        Bestleistungen ergebnis = ErmittleBestleistungen.Ausfuehren(Bestand(), 2, null);

        Assert.Equal(
            [Einzelrekordart.Distanz, Einzelrekordart.Hoehenmeter, Einzelrekordart.Fahrzeit, Einzelrekordart.Durchschnittsgeschwindigkeit],
            ergebnis.Einzelrekorde.Select(r => r.Art));
    }

    [Fact]
    public void Alle_Aktivitaeten_werden_gemeinsam_ausgewertet()
    {
        Bestleistungen ergebnis = ErmittleBestleistungen.Ausfuehren(Bestand(), null, null);

        // 2023: 100 km Bike + 5 km Laufen = 105 km, 2 Einheiten; 2024: 100 km, 2 Einheiten (Gleichstand → 2023)
        Assert.Equal(new Jahresrekord(Fortschrittswert.Kilometer, 2023, 105), ergebnis.Jahresrekorde[0]);
        Assert.Equal(new Jahresrekord(Fortschrittswert.Anzahl, 2023, 2), ergebnis.Jahresrekorde[1]);
    }

    [Fact]
    public void Kategorie_der_Filterleiste_schraenkt_ein()
    {
        Bestleistungen ergebnis = ErmittleBestleistungen.Ausfuehren(Bestand(), 1, "Urlaub");

        Assert.Equal(new Jahresrekord(Fortschrittswert.Kilometer, 2024, 60), ergebnis.Jahresrekorde[0]);
        Assert.Equal(2, ergebnis.Einzelrekorde.Single(r => r.Art == Einzelrekordart.Distanz).Einheit.Id);
    }

    [Fact]
    public void Ohne_Einheiten_gibt_es_keine_Bestleistungen()
    {
        Bestleistungen ergebnis = ErmittleBestleistungen.Ausfuehren(Bestand(), 2, "Urlaub");

        Assert.Empty(ergebnis.Jahresrekorde);
        Assert.Empty(ergebnis.Einzelrekorde);
    }
}
