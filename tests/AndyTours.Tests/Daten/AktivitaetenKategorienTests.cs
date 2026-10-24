using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;

namespace AndyTours.Tests.Daten;

/// <summary>
/// Tests für das Pflegen einzelner Aktivitäten und Kategorien in der Datenbank:
/// <see cref="AktivitaetenTabelle"/>, <see cref="KategorienTabelle"/>,
/// <see cref="EquipmentsTabelle.ErsetzeStandardKategorie"/> und <see cref="AktivitaetenReihenfolgeEinstellung"/>.
/// </summary>
public class AktivitaetenKategorienTests
{
    /// <summary>
    /// Arbeitsspeicher-Datenbank: Aktivitäten Bike (1, Haupt), Laufen (2); Kategorien Urlaub (1),
    /// Trainieren (2, Standard), Training (3); Einheiten 1 (Bike, Trainieren), 2 (Bike, Trainieren +
    /// Training), 3 (Laufen, Training); Equipments Genius (Bike, Standardkategorie „trainieren“), Schuhe (Laufen).
    /// </summary>
    private static SqliteConnection Datenbank()
    {
        SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        DatenbestandTabellen.Speichern(verbindung, new Datenbestand
        {
            Fahrer = { new Fahrer { Id = 1 } },
            Aktivitaeten =
            {
                new Aktivitaet { Id = 1, Name = "Bike", IstHaupt = true },
                new Aktivitaet { Id = 2, Name = "Laufen", Geschwindigkeitseinheit = "min/km" },
            },
            Kategorien =
            {
                new Kategorie { Id = 1, Name = "Urlaub" },
                new Kategorie { Id = 2, Name = "Trainieren", IstStandard = true },
                new Kategorie { Id = 3, Name = "Training" },
            },
            Equipments =
            {
                new Equipment { Id = 1, Name = "Genius", AktivitaetId = 1, StandardKategorie = "trainieren" },
                new Equipment { Id = 2, Name = "Schuhe", AktivitaetId = 2, StandardKategorie = "" },
            },
            Trainingseinheiten =
            {
                new Trainingseinheit { Id = 1, FahrerId = 1, AktivitaetId = 1, Name = "A", KategorieIds = [2] },
                new Trainingseinheit { Id = 2, FahrerId = 1, AktivitaetId = 1, Name = "B", KategorieIds = [2, 3] },
                new Trainingseinheit { Id = 3, FahrerId = 1, AktivitaetId = 2, Name = "C", KategorieIds = [3] },
            },
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

    /// <summary>Einheiten nach Nummer geordnet.</summary>
    private static List<Trainingseinheit> Einheiten(SqliteConnection verbindung) =>
        TrainingseinheitenTabelle.Laden(verbindung).OrderBy(e => e.Id).ToList();

    [Fact]
    public void Neue_Aktivitaet_und_Kategorie_erhalten_eine_eigene_Nummer()
    {
        using SqliteConnection verbindung = Datenbank();
        long aktivitaetId = 0, kategorieId = 0;

        InTransaktion(verbindung, t =>
        {
            aktivitaetId = AktivitaetenTabelle.Einfuegen(verbindung, t, new Aktivitaet { Name = "Gravel" });
            kategorieId = KategorienTabelle.Einfuegen(verbindung, t, new Kategorie { Name = "Test" });
        });

        Assert.Equal(3, aktivitaetId);
        Assert.Equal(4, kategorieId);
        Assert.Equal("Gravel", AktivitaetenTabelle.Laden(verbindung).Single(a => a.Id == 3).Name);
        Assert.Equal("Test", KategorienTabelle.Laden(verbindung).Single(k => k.Id == 4).Name);
    }

    [Fact]
    public void Aendern_schreibt_Name_und_Kennzeichen_und_laesst_die_Zuordnung()
    {
        using SqliteConnection verbindung = Datenbank();

        InTransaktion(verbindung, t =>
        {
            AktivitaetenTabelle.Aendern(verbindung, t, new Aktivitaet { Id = 1, Name = "Rad", IstHaupt = false });
            AktivitaetenTabelle.Aendern(verbindung, t, new Aktivitaet { Id = 2, Name = "Lauf", IstHaupt = true });
            KategorienTabelle.Aendern(verbindung, t, new Kategorie { Id = 1, Name = "Ferien", IstStandard = true });
        });

        List<Aktivitaet> aktivitaeten = AktivitaetenTabelle.Laden(verbindung);
        Assert.Equal(["Rad", "Lauf"], aktivitaeten.Select(a => a.Name));
        Assert.Equal([false, true], aktivitaeten.Select(a => a.IstHaupt));
        Assert.Equal("min/km", aktivitaeten[1].Geschwindigkeitseinheit);
        Kategorie ferien = KategorienTabelle.Laden(verbindung)[0];
        Assert.Equal(("Ferien", true), (ferien.Name, ferien.IstStandard));
        Assert.Equal([1L, 1L, 2L], Einheiten(verbindung).Select(e => e.AktivitaetId));
    }

    [Fact]
    public void Standardkategorie_der_Equipments_wird_ohne_Unterschied_der_Schreibweise_ersetzt()
    {
        using SqliteConnection verbindung = Datenbank();
        int anzahl = 0;

        InTransaktion(verbindung, t => anzahl = EquipmentsTabelle.ErsetzeStandardKategorie(verbindung, t, "Trainieren ", "Intervalle"));

        Assert.Equal(1, anzahl);
        Assert.Equal(["Intervalle", ""], EquipmentsTabelle.Laden(verbindung).OrderBy(e => e.Id).Select(e => e.StandardKategorie));
    }

    [Fact]
    public void Loeschen_entfernt_unbenutzte_Eintraege()
    {
        using SqliteConnection verbindung = Datenbank();

        InTransaktion(verbindung, t =>
        {
            long id = AktivitaetenTabelle.Einfuegen(verbindung, t, new Aktivitaet { Name = "Gravel" });
            AktivitaetenTabelle.Loeschen(verbindung, t, id);
            KategorienTabelle.Loeschen(verbindung, t, 1);
        });

        Assert.Equal(["Bike", "Laufen"], AktivitaetenTabelle.Laden(verbindung).Select(a => a.Name));
        Assert.Equal(["Trainieren", "Training"], KategorienTabelle.Laden(verbindung).Select(k => k.Name));
    }

    [Fact]
    public void Kategorien_zusammenfuehren_haengt_Einheiten_ohne_Doppel_um_und_uebertraegt_Standard()
    {
        using SqliteConnection verbindung = Datenbank();

        InTransaktion(verbindung, t =>
        {
            KategorienTabelle.FuehreZusammen(verbindung, t, 2, 3);
            EquipmentsTabelle.ErsetzeStandardKategorie(verbindung, t, "Trainieren", "Training");
        });

        Assert.All(Einheiten(verbindung), e => Assert.Equal([3L], e.KategorieIds));
        List<Kategorie> kategorien = KategorienTabelle.Laden(verbindung);
        Assert.Equal(["Urlaub", "Training"], kategorien.Select(k => k.Name));
        Assert.True(kategorien[1].IstStandard);
        Assert.Equal("Training", EquipmentsTabelle.Laden(verbindung).Single(e => e.Id == 1).StandardKategorie);
    }

    [Fact]
    public void Aktivitaeten_zusammenfuehren_haengt_Einheiten_und_Equipments_um_und_uebertraegt_Hauptaktivitaet()
    {
        using SqliteConnection verbindung = Datenbank();

        InTransaktion(verbindung, t => AktivitaetenTabelle.FuehreZusammen(verbindung, t, 1, 2));

        Assert.All(Einheiten(verbindung), e => Assert.Equal(2, e.AktivitaetId));
        Assert.All(EquipmentsTabelle.Laden(verbindung), e => Assert.Equal(2, e.AktivitaetId));
        Aktivitaet laufen = Assert.Single(AktivitaetenTabelle.Laden(verbindung));
        Assert.True(laufen.IstHaupt);
    }

    [Fact]
    public void Ohne_Bestaetigung_bleibt_alles_beim_alten_Stand()
    {
        using SqliteConnection verbindung = Datenbank();

        using (SqliteTransaction transaktion = verbindung.BeginTransaction())
        {
            KategorienTabelle.FuehreZusammen(verbindung, transaktion, 2, 3);
            AktivitaetenTabelle.FuehreZusammen(verbindung, transaktion, 2, 1);
            transaktion.Rollback();
        }

        Assert.Equal(["Urlaub", "Trainieren", "Training"], KategorienTabelle.Laden(verbindung).Select(k => k.Name));
        Assert.Equal(["Bike", "Laufen"], AktivitaetenTabelle.Laden(verbindung).Select(a => a.Name));
        Assert.Equal([2L, 3L], Einheiten(verbindung)[1].KategorieIds);
    }

    [Fact]
    public void Loeschen_einer_verwendeten_Aktivitaet_scheitert_am_Fremdschluessel()
    {
        using SqliteConnection verbindung = Datenbank();
        using SqliteTransaction transaktion = verbindung.BeginTransaction();

        Assert.Throws<SqliteException>(() => AktivitaetenTabelle.Loeschen(verbindung, transaktion, 2));
    }

    [Fact]
    public void Reihenfolge_ist_ohne_Speichern_leer_und_bleibt_sonst_erhalten()
    {
        using SqliteConnection verbindung = Datenbank();
        Assert.Empty(AktivitaetenReihenfolgeEinstellung.Laden(verbindung));

        AktivitaetenReihenfolgeEinstellung.Speichern(verbindung, null, [2, 1]);

        Assert.Equal([2L, 1L], AktivitaetenReihenfolgeEinstellung.Laden(verbindung));
    }
}
