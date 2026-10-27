using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Daten;

/// <summary>
/// Tests für <see cref="TrainingseinheitenTabelle.Einfuegen"/>, <see cref="TrainingseinheitenTabelle.Aendern"/>,
/// <see cref="TrainingseinheitenTabelle.AendereMehrere"/>, <see cref="TrainingseinheitenTabelle.Loeschen"/> und <see cref="FahrerTabelle.Standardfahrer"/>.
/// </summary>
public class EinheitSchreibenTests
{
    /// <summary>Öffnet eine Arbeitsspeicher-Datenbank mit Fahrer, Aktivität, Equipment, Kategorien und zwei Einheiten.</summary>
    private static SqliteConnection DatenbankMitBestand()
    {
        SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        DatenbestandTabellen.Speichern(verbindung, new Datenbestand
        {
            Fahrer = { new Fahrer { Id = 3, Vorname = "Andreas" } },
            Aktivitaeten = { new Aktivitaet { Id = 1, Name = "Bike" } },
            Equipments = { new Equipment { Id = 7, Name = "Genius" } },
            Kategorien = { new Kategorie { Id = 1, Name = "Tour" }, new Kategorie { Id = 2, Name = "Alpen" } },
            Trainingseinheiten =
            {
                new Trainingseinheit
                {
                    Id = 10, FahrerId = 3, Datum = new DateOnly(2026, 8, 15), Name = "Alm", AktivitaetId = 1, EquipmentId = 7,
                    DistanzKm = 50, KategorieIds = [1, 2],
                    Anhaenge = [new Anhang { Id = 5, Typ = AnhangTyp.Hoehenprofil, RelativerPfad = "2026/alm.fit" }],
                },
                new Trainingseinheit { Id = 11, FahrerId = 3, Datum = new DateOnly(2026, 8, 16), Name = "See", AktivitaetId = 1 },
            },
        });
        return verbindung;
    }

    private static Trainingseinheit Lade(SqliteConnection verbindung, long id) =>
        TrainingseinheitenTabelle.Laden(verbindung).Single(e => e.Id == id);

    [Fact]
    public void Neue_Einheit_bekommt_eine_neue_Nummer_und_ihre_Kategorien()
    {
        using SqliteConnection verbindung = DatenbankMitBestand();
        var neu = new Trainingseinheit
        {
            FahrerId = 3, Datum = new DateOnly(2026, 10, 24), Name = "Neu", AktivitaetId = 1, KategorieIds = [2],
            DistanzKm = 12.5, Temperatur = -3,
        };

        (long id, long fahrerId) = TrainingseinheitenTabelle.Einfuegen(verbindung, neu);

        Assert.True(id > 11);
        Assert.Equal(3, fahrerId);
        Trainingseinheit geladen = Lade(verbindung, id);
        Assert.Equal("Neu", geladen.Name);
        Assert.Equal(12.5, geladen.DistanzKm);
        Assert.Equal(-3, geladen.Temperatur);
        Assert.Equal([2], geladen.KategorieIds);
        Assert.Empty(geladen.Anhaenge);
    }

    [Fact]
    public void Neue_Einheit_speichert_ihre_Anhaenge()
    {
        using SqliteConnection verbindung = DatenbankMitBestand();
        var neu = new Trainingseinheit
        {
            FahrerId = 3, Datum = new DateOnly(2026, 10, 24), Name = "Import", AktivitaetId = 1,
            Anhaenge =
            [
                new Anhang { Typ = AnhangTyp.Hoehenprofil, RelativerPfad = "2026/neu.fit" },
                new Anhang { Typ = AnhangTyp.GpsTrack, RelativerPfad = "2026/neu.fit" },
            ],
        };

        (long id, _) = TrainingseinheitenTabelle.Einfuegen(verbindung, neu);

        List<Anhang> geladen = Lade(verbindung, id).Anhaenge;
        Assert.Equal(2, geladen.Count);
        Assert.Contains(geladen, a => a.Typ == AnhangTyp.Hoehenprofil && a.RelativerPfad == "2026/neu.fit");
        Assert.Contains(geladen, a => a.Typ == AnhangTyp.GpsTrack && a.RelativerPfad == "2026/neu.fit");
        // die vergebenen Nummern stehen auch in den übergebenen Anhängen
        Assert.All(neu.Anhaenge, a => Assert.True(a.Id > 5));
    }

    [Fact]
    public void Ohne_Fahrer_wird_beim_Einfuegen_einer_angelegt()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        DatenbestandTabellen.Speichern(verbindung, new Datenbestand { Aktivitaeten = { new Aktivitaet { Id = 1, Name = "Bike" } } });

        (long id, long fahrerId) = TrainingseinheitenTabelle.Einfuegen(
            verbindung, new Trainingseinheit { Datum = new DateOnly(2026, 10, 24), AktivitaetId = 1 });

        Fahrer fahrer = Assert.Single(FahrerTabelle.Laden(verbindung));
        Assert.Equal(fahrer.Id, fahrerId);
        Assert.Equal(fahrerId, Lade(verbindung, id).FahrerId);
    }

    [Fact]
    public void Standardfahrer_ist_der_vorhandene()
    {
        using SqliteConnection verbindung = DatenbankMitBestand();
        using SqliteTransaction transaktion = verbindung.BeginTransaction();

        Assert.Equal(3, FahrerTabelle.Standardfahrer(verbindung, transaktion));
    }

    [Fact]
    public void Aendern_speichert_Werte_ersetzt_Kategorien_und_laesst_Anhaenge()
    {
        using SqliteConnection verbindung = DatenbankMitBestand();
        Trainingseinheit einheit = Lade(verbindung, 10);
        einheit.Name = "Bründlingalm";
        einheit.DistanzKm = 55.5;
        einheit.EquipmentId = null;
        einheit.KategorieIds = [1];
        einheit.Anhaenge.Clear(); // darf die gespeicherten Anhänge nicht berühren

        TrainingseinheitenTabelle.Aendern(verbindung, einheit);

        Trainingseinheit geladen = Lade(verbindung, 10);
        Assert.Equal("Bründlingalm", geladen.Name);
        Assert.Equal(55.5, geladen.DistanzKm);
        Assert.Null(geladen.EquipmentId);
        Assert.Equal([1], geladen.KategorieIds);
        Assert.Equal("2026/alm.fit", Assert.Single(geladen.Anhaenge).RelativerPfad);
        Assert.Equal("See", Lade(verbindung, 11).Name); // andere Einheit unverändert
    }

    [Fact]
    public void Aendern_einer_nicht_vorhandenen_Einheit_ist_ein_Fehler()
    {
        using SqliteConnection verbindung = DatenbankMitBestand();
        var fehlt = new Trainingseinheit { Id = 999, FahrerId = 3, Datum = new DateOnly(2026, 1, 1), AktivitaetId = 1 };

        Assert.Throws<InvalidOperationException>(() => TrainingseinheitenTabelle.Aendern(verbindung, fehlt));
    }

    [Fact]
    public void Mehrere_Einheiten_werden_zusammen_geaendert()
    {
        using SqliteConnection verbindung = DatenbankMitBestand();
        List<Trainingseinheit> einheiten = TrainingseinheitenTabelle.Laden(verbindung);
        foreach (Trainingseinheit einheit in einheiten)
        {
            einheit.KategorieIds = [2];
            einheit.EquipmentId = 7;
        }

        TrainingseinheitenTabelle.AendereMehrere(verbindung, einheiten);

        Assert.All(TrainingseinheitenTabelle.Laden(verbindung), e =>
        {
            Assert.Equal([2L], e.KategorieIds);
            Assert.Equal(7, e.EquipmentId);
        });
        Assert.Single(Lade(verbindung, 10).Anhaenge);
    }

    [Fact]
    public void Scheitert_eine_Einheit_bleiben_alle_unveraendert()
    {
        using SqliteConnection verbindung = DatenbankMitBestand();
        Trainingseinheit vorhanden = Lade(verbindung, 11);
        vorhanden.Name = "Geändert";
        vorhanden.KategorieIds = [1];
        var fehlt = new Trainingseinheit { Id = 999, FahrerId = 3, Datum = new DateOnly(2026, 1, 1), AktivitaetId = 1 };

        Assert.Throws<InvalidOperationException>(() => TrainingseinheitenTabelle.AendereMehrere(verbindung, [vorhanden, fehlt]));

        Trainingseinheit geladen = Lade(verbindung, 11);
        Assert.Equal("See", geladen.Name);
        Assert.Empty(geladen.KategorieIds);
    }

    [Fact]
    public void Loeschen_entfernt_Einheiten_samt_Zuordnungen_und_Anhang_Eintraegen()
    {
        using SqliteConnection verbindung = DatenbankMitBestand();

        TrainingseinheitenTabelle.Loeschen(verbindung, [10, 11]);

        Assert.Empty(TrainingseinheitenTabelle.Laden(verbindung));
        Assert.Equal(0L, Zaehle(verbindung, "EinheitKategorie"));
        Assert.Equal(0L, Zaehle(verbindung, "Anhang"));
        Assert.Equal(2, KategorienTabelle.Laden(verbindung).Count); // die Kategorien selbst bleiben
    }

    [Fact]
    public void Loeschen_laesst_die_Datei_des_Anhangs_liegen()
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "alm.fit");
        File.WriteAllText(datei, "Aufzeichnung");
        using SqliteConnection verbindung = DatenbankMitBestand();

        TrainingseinheitenTabelle.Loeschen(verbindung, [10]);

        Assert.True(File.Exists(datei));
        Assert.Equal([11L], TrainingseinheitenTabelle.Laden(verbindung).Select(e => e.Id));
    }

    /// <summary>Zählt die Zeilen einer Tabelle.</summary>
    private static long Zaehle(SqliteConnection verbindung, string tabelle)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = $"SELECT COUNT(*) FROM {tabelle};";
        return (long)befehl.ExecuteScalar()!;
    }
}
