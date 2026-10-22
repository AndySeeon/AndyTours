using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;

namespace AndyTours.Tests.Daten;

/// <summary>
/// Tests für Speichern und Laden über eine SQLite-Datenbank im Arbeitsspeicher.
/// </summary>
public class DatenbankTests
{
    /// <summary>Kleiner, aber vollständiger Datenbestand mit allen Tabellen.</summary>
    private static Datenbestand ErzeugeBeispielbestand()
    {
        return new Datenbestand
        {
            Fahrer = { new Fahrer { Id = 1, Vorname = "Andy", Geburtstag = new DateOnly(1963, 5, 28), HeimatBreitengrad = 47.97 } },
            Aktivitaeten = { new Aktivitaet { Id = 1, Name = "Bike", IstHaupt = true, ZielKm = 8000 } },
            Kategorien = { new Kategorie { Id = 1, Name = "Arbeit" }, new Kategorie { Id = 2, Name = "Urlaub", IstStandard = true } },
            Equipments = { new Equipment { Id = 1, Name = "Genius/Bike", AktivitaetId = 1, Kettenblaetter = { 24, 34 }, Ausgemustert = true } },
            Trainingseinheiten =
            {
                new Trainingseinheit
                {
                    Id = 1, FahrerId = 1, Datum = new DateOnly(2026, 8, 7), Startzeit = new TimeOnly(7, 39),
                    Name = "Bründlingalm", Fahrzeit = new TimeSpan(26, 5, 3), DistanzKm = 77.83, Hoehenmeter = 1200,
                    Temperatur = -3, AktivitaetId = 1, EquipmentId = 1, KategorieIds = { 1, 2 },
                    Anhaenge = { new Anhang { Id = 1, Typ = AnhangTyp.GpsTrack, RelativerPfad = "2026/a.fit" } },
                },
            },
            Wartungen = { new Wartung { Id = 1, Datum = new DateOnly(2026, 9, 6), EquipmentId = null, Taetigkeit = "Dämpfer", Typ = WartungsTyp.Alarm, AlarmKm = 1500 } },
            Schwellwerte = { new Schwellwert { Id = 1, Art = SchwellwertArt.Steigung, Name = "Hügel", Von = 3, Bis = 6, Farbe = "#CC6600" } },
        };
    }

    [Fact]
    public void Gespeicherter_Bestand_wird_unveraendert_geladen()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        SpeichereDatenbestand.Ausfuehren(verbindung, ErzeugeBeispielbestand());

        Datenbestand geladen = LadeDatenbestand.Ausfuehren(verbindung);

        // Einheit mit allen Besonderheiten: Umlaute, Fahrzeit > 24 h, negative Temperatur
        Trainingseinheit einheit = Assert.Single(geladen.Trainingseinheiten);
        Assert.Equal("Bründlingalm", einheit.Name);
        Assert.Equal(new TimeSpan(26, 5, 3), einheit.Fahrzeit);
        Assert.Equal(new TimeOnly(7, 39), einheit.Startzeit);
        Assert.Equal(-3, einheit.Temperatur);
        Assert.Equal(new List<long> { 1, 2 }, einheit.KategorieIds);
        Assert.Equal("2026/a.fit", Assert.Single(einheit.Anhaenge).RelativerPfad);

        Equipment equipment = Assert.Single(geladen.Equipments);
        Assert.Equal(new List<int> { 24, 34 }, equipment.Kettenblaetter);
        Assert.Empty(equipment.Ritzel);
        Assert.True(equipment.Ausgemustert);
        Assert.Null(equipment.Anschaffungsdatum);

        Wartung wartung = Assert.Single(geladen.Wartungen);
        Assert.Null(wartung.EquipmentId);
        Assert.Equal(WartungsTyp.Alarm, wartung.Typ);

        Assert.Equal(47.97, Assert.Single(geladen.Fahrer).HeimatBreitengrad);
        Assert.Equal("Hügel", Assert.Single(geladen.Schwellwerte).Name);
        Assert.True(geladen.Kategorien[1].IstStandard);
    }

    [Fact]
    public void Zweites_Speichern_wird_abgelehnt()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        SpeichereDatenbestand.Ausfuehren(verbindung, ErzeugeBeispielbestand());

        Assert.Throws<InvalidOperationException>(() => SpeichereDatenbestand.Ausfuehren(verbindung, ErzeugeBeispielbestand()));
    }

    [Fact]
    public void Unbekanntes_Equipment_verletzt_Fremdschluessel()
    {
        Datenbestand bestand = ErzeugeBeispielbestand();
        bestand.Trainingseinheiten[0].EquipmentId = 99;

        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");

        // Fremdschlüssel sind eingeschaltet → SQLite lehnt den Verweis auf Equipment 99 ab
        Assert.Throws<SqliteException>(() => SpeichereDatenbestand.Ausfuehren(verbindung, bestand));
        // … und dank Transaktion ist auch sonst nichts gespeichert worden
        Assert.Empty(LadeDatenbestand.Ausfuehren(verbindung).Fahrer);
    }
}
