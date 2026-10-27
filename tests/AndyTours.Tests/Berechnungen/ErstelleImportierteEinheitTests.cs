using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErstelleImportierteEinheit"/>.
/// </summary>
public class ErstelleImportierteEinheitTests
{
    /// <summary>
    /// Fahrer 5. Aktivitäten: Bike (1, Hauptaktivität), Gravel (2). Kategorien: Training (10, Standard),
    /// Arbeit (11), Urlaub (12). Equipments: JAM2 (20, Gravel, Standardkategorie „arbeit “), Schuh (21, ohne Aktivität).
    /// </summary>
    private static Datenbestand Bestand() => new()
    {
        Fahrer = [new() { Id = 5 }],
        Aktivitaeten = [new() { Id = 1, Name = "Bike", IstHaupt = true }, new() { Id = 2, Name = "Gravel" }],
        Kategorien =
        [
            new() { Id = 10, Name = "Training", IstStandard = true },
            new() { Id = 11, Name = "Arbeit" },
            new() { Id = 12, Name = "Urlaub" },
        ],
        Equipments =
        [
            new() { Id = 20, Name = "JAM2", AktivitaetId = 2, StandardKategorie = "arbeit " },
            new() { Id = 21, Name = "Schuh" },
        ],
    };

    /// <summary>Eine Aufzeichnung mit allen Werten.</summary>
    private static readonly Aufzeichnung Aufzeichnung = new()
    {
        Dateiname = "2026-09-26-08-02-49.fit",
        Datum = new(2026, 9, 26),
        Startzeit = new(8, 2, 49),
        Fahrzeit = new TimeSpan(5, 40, 7),
        DistanzKm = 90.83,
        Hoehenmeter = 1206,
        MaxHoehe = 1379,
        StartHoehe = 540,
        MaxGeschwindigkeit = 52.23,
        HfDurchschnitt = 130,
        HfMax = 154,
        HfMin = 75,
        Trittfrequenz = 75,
        Temperatur = 18,
        LeistungWatt = 180,
        Profil = "MTB",
    };

    [Fact]
    public void Werte_kommen_aus_der_Aufzeichnung_und_Name_bleibt_leer()
    {
        Trainingseinheit einheit = ErstelleImportierteEinheit.Ausfuehren(Bestand(), Aufzeichnung, 20, "2026/2026-09-26-08-02-49.fit");

        Assert.Equal(0, einheit.Id);
        Assert.Equal(5, einheit.FahrerId);
        Assert.Equal("", einheit.Name);
        Assert.Equal(new DateOnly(2026, 9, 26), einheit.Datum);
        Assert.Equal(new TimeOnly(8, 2, 49), einheit.Startzeit);
        Assert.Equal(new TimeSpan(5, 40, 7), einheit.Fahrzeit);
        Assert.Equal(90.83, einheit.DistanzKm);
        Assert.Equal(1206, einheit.Hoehenmeter);
        Assert.Equal(1379, einheit.MaxHoehe);
        Assert.Equal(540, einheit.StartHoehe);
        Assert.Equal(52.23, einheit.MaxGeschwindigkeit);
        Assert.Equal((130, 154, 75), (einheit.HfDurchschnitt, einheit.HfMax, einheit.HfMin));
        Assert.Equal(75, einheit.Trittfrequenz);
        Assert.Equal(18, einheit.Temperatur);
        Assert.Equal(180, einheit.LeistungWatt);
    }

    [Fact]
    public void Equipment_mit_Aktivitaet_und_Standardkategorien()
    {
        Trainingseinheit einheit = ErstelleImportierteEinheit.Ausfuehren(Bestand(), Aufzeichnung, 20, "x.fit");

        Assert.Equal(20, einheit.EquipmentId);
        Assert.Equal(2, einheit.AktivitaetId);
        Assert.Equal(new long[] { 10, 11 }, einheit.KategorieIds);
    }

    [Fact]
    public void Equipment_ohne_Aktivitaet_erhaelt_die_Hauptaktivitaet()
    {
        Trainingseinheit einheit = ErstelleImportierteEinheit.Ausfuehren(Bestand(), Aufzeichnung, 21, "x.fit");

        Assert.Equal(21, einheit.EquipmentId);
        Assert.Equal(1, einheit.AktivitaetId);
        Assert.Equal(new long[] { 10 }, einheit.KategorieIds);
    }

    [Fact]
    public void Ohne_Equipment_Hauptaktivitaet_und_kein_Equipment()
    {
        Trainingseinheit einheit = ErstelleImportierteEinheit.Ausfuehren(Bestand(), Aufzeichnung, null, "x.fit");

        Assert.Null(einheit.EquipmentId);
        Assert.Equal(1, einheit.AktivitaetId);
    }

    [Fact]
    public void Aufzeichnung_als_Hoehenprofil_und_GPS_Track()
    {
        Trainingseinheit einheit = ErstelleImportierteEinheit.Ausfuehren(Bestand(), Aufzeichnung, 20, "2026/2026-09-26-08-02-49.fit");

        Assert.Equal(
            new[]
            {
                (AnhangTyp.Hoehenprofil, "2026/2026-09-26-08-02-49.fit"),
                (AnhangTyp.GpsTrack, "2026/2026-09-26-08-02-49.fit"),
            },
            einheit.Anhaenge.Select(a => (a.Typ, a.RelativerPfad)));
    }
}
