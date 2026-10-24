using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;

namespace AndyTours.Tests.Daten;

/// <summary>
/// Tests für das Pflegen einzelner Equipments in der Datenbank: <see cref="EquipmentsTabelle.Einfuegen"/>,
/// <see cref="EquipmentsTabelle.Aendern"/> und <see cref="EquipmentsTabelle.Loeschen"/>.
/// </summary>
public class EquipmentsTests
{
    /// <summary>
    /// Arbeitsspeicher-Datenbank: Aktivitäten Bike (1), Laufen (2); Equipments Genius (1, Bike, Standard),
    /// FELT (2, Bike, ausgemustert), Schuhe (3, Laufen, unbenutzt), Rennrad (4, Bike, nur mit Wartung);
    /// Einheit 1 fährt Genius, Einheit 2 FELT; eine Wartung für Rennrad.
    /// </summary>
    private static SqliteConnection Datenbank()
    {
        SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        DatenbestandTabellen.Speichern(verbindung, new Datenbestand
        {
            Fahrer = { new Fahrer { Id = 1 } },
            Aktivitaeten = { new Aktivitaet { Id = 1, Name = "Bike", IstHaupt = true }, new Aktivitaet { Id = 2, Name = "Laufen" } },
            Equipments =
            {
                new Equipment { Id = 1, Name = "Genius", AktivitaetId = 1, IstStandard = true, GewichtKg = 14.5, Kettenblaetter = [32] },
                new Equipment { Id = 2, Name = "FELT", AktivitaetId = 1, Ausgemustert = true },
                new Equipment { Id = 3, Name = "Schuhe", AktivitaetId = 2 },
                new Equipment { Id = 4, Name = "Rennrad", AktivitaetId = 1 },
            },
            Trainingseinheiten =
            {
                new Trainingseinheit { Id = 1, FahrerId = 1, AktivitaetId = 1, Name = "A", EquipmentId = 1 },
                new Trainingseinheit { Id = 2, FahrerId = 1, AktivitaetId = 1, Name = "B", EquipmentId = 2 },
            },
            Wartungen = { new Wartung { Id = 1, EquipmentId = 4, Taetigkeit = "Kette" } },
        });
        return verbindung;
    }

    /// <summary>Führt eine Änderung in einer Transaktion aus und bestätigt sie.</summary>
    private static void InTransaktion(SqliteConnection verbindung, Action<SqliteTransaction> aenderung)
    {
        using SqliteTransaction transaktion = verbindung.BeginTransaction();
        aenderung(transaktion);
        transaktion.Commit();
    }

    /// <summary>Liest ein Equipment nach Nummer.</summary>
    private static Equipment Lesen(SqliteConnection verbindung, long id) => EquipmentsTabelle.Laden(verbindung).Single(e => e.Id == id);

    [Fact]
    public void Neues_Equipment_erhaelt_eine_eigene_Nummer_und_alle_Felder()
    {
        using SqliteConnection verbindung = Datenbank();
        long id = 0;
        InTransaktion(verbindung, t => id = EquipmentsTabelle.Einfuegen(verbindung, t, new Equipment
        {
            Name = "Gravel-Rad", AktivitaetId = 1, StandardKategorie = "Urlaub",
            Anschaffungsdatum = new DateOnly(2026, 10, 24), AnfangsdistanzKm = 12.5,
        }));

        Assert.Equal(5, id);
        Equipment gelesen = Lesen(verbindung, id);
        Assert.Equal("Gravel-Rad", gelesen.Name);
        Assert.Equal(1, gelesen.AktivitaetId);
        Assert.Equal("Urlaub", gelesen.StandardKategorie);
        Assert.Equal(new DateOnly(2026, 10, 24), gelesen.Anschaffungsdatum);
        Assert.Equal(12.5, gelesen.AnfangsdistanzKm);
    }

    [Fact]
    public void Aendern_schreibt_alle_Felder_und_laesst_die_Zuordnung_der_Einheiten()
    {
        using SqliteConnection verbindung = Datenbank();
        Equipment genius = Lesen(verbindung, 1);
        genius.Name = "Genius 29";
        genius.AktivitaetId = null;
        genius.IstStandard = false;
        genius.Ausgemustert = true;
        genius.AnfangsdistanzKm = 100;

        InTransaktion(verbindung, t => EquipmentsTabelle.Aendern(verbindung, t, genius));

        Equipment gelesen = Lesen(verbindung, 1);
        Assert.Equal("Genius 29", gelesen.Name);
        Assert.Null(gelesen.AktivitaetId);
        Assert.False(gelesen.IstStandard);
        Assert.True(gelesen.Ausgemustert);
        Assert.Equal(100, gelesen.AnfangsdistanzKm);
        Assert.Equal(14.5, gelesen.GewichtKg);
        Assert.Equal([32], gelesen.Kettenblaetter);
        Assert.Equal(1, TrainingseinheitenTabelle.Laden(verbindung).Single(e => e.Id == 1).EquipmentId);
    }

    [Fact]
    public void Vorhandener_Name_wird_von_der_Datenbank_abgewiesen()
    {
        using SqliteConnection verbindung = Datenbank();
        using SqliteTransaction transaktion = verbindung.BeginTransaction();

        Assert.Throws<SqliteException>(() => EquipmentsTabelle.Einfuegen(verbindung, transaktion, new Equipment { Name = "genius" }));
    }

    [Fact]
    public void Loeschen_entfernt_ein_unbenutztes_Equipment()
    {
        using SqliteConnection verbindung = Datenbank();
        InTransaktion(verbindung, t => EquipmentsTabelle.Loeschen(verbindung, t, 3));

        Assert.Equal(["Genius", "FELT", "Rennrad"], EquipmentsTabelle.Laden(verbindung).Select(e => e.Name));
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(4L)]
    public void Loeschen_mit_Einheiten_oder_Wartungen_scheitert_am_Fremdschluessel(long id)
    {
        using SqliteConnection verbindung = Datenbank();
        using SqliteTransaction transaktion = verbindung.BeginTransaction();

        Assert.Throws<SqliteException>(() => EquipmentsTabelle.Loeschen(verbindung, transaktion, id));
    }

    [Fact]
    public void Ohne_Bestaetigung_bleibt_alles_beim_alten_Stand()
    {
        using SqliteConnection verbindung = Datenbank();
        using (SqliteTransaction transaktion = verbindung.BeginTransaction())
        {
            EquipmentsTabelle.Einfuegen(verbindung, transaktion, new Equipment { Name = "Neu" });
            EquipmentsTabelle.Aendern(verbindung, transaktion, new Equipment { Id = 1, Name = "Umbenannt" });
            EquipmentsTabelle.Loeschen(verbindung, transaktion, 3);
            transaktion.Rollback();
        }

        Assert.Equal(["Genius", "FELT", "Schuhe", "Rennrad"], EquipmentsTabelle.Laden(verbindung).Select(e => e.Name));
    }
}
