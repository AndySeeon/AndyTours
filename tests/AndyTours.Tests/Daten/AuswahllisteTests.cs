using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Daten;

/// <summary>
/// Tests für <see cref="AuswahllisteEinstellung"/> und <see cref="TrainingseinheitenTabelle.ErsetzeTrainingsbereich"/>.
/// </summary>
public class AuswahllisteTests
{
    /// <summary>Öffnet eine Arbeitsspeicher-Datenbank mit vier Einheiten (GA2 in zwei Schreibweisen, GA1, ohne Bereich).</summary>
    private static SqliteConnection DatenbankMitEinheiten()
    {
        SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        DatenbestandTabellen.Speichern(verbindung, new Datenbestand
        {
            Fahrer = { new Fahrer { Id = 1 } },
            Aktivitaeten = { new Aktivitaet { Id = 1, Name = "Bike" } },
            Trainingseinheiten =
            {
                new Trainingseinheit { Id = 1, FahrerId = 1, AktivitaetId = 1, Name = "A", Trainingsbereich = "GA2" },
                new Trainingseinheit { Id = 2, FahrerId = 1, AktivitaetId = 1, Name = "B", Trainingsbereich = "GA1" },
                new Trainingseinheit { Id = 3, FahrerId = 1, AktivitaetId = 1, Name = "C", Trainingsbereich = "ga2" },
                new Trainingseinheit { Id = 4, FahrerId = 1, AktivitaetId = 1, Name = "D", Trainingsbereich = "" },
            },
        });
        return verbindung;
    }

    /// <summary>Trainingsbereiche der Einheiten, nach Nummer geordnet.</summary>
    private static List<string> Bereiche(SqliteConnection verbindung) =>
        TrainingseinheitenTabelle.Laden(verbindung).OrderBy(e => e.Id).Select(e => e.Trainingsbereich).ToList();

    [Fact]
    public void Nie_angelegte_Liste_ist_null()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");

        Assert.Null(AuswahllisteEinstellung.Laden(verbindung, Auswahlliste.Trainingsbereich));
    }

    [Fact]
    public void Liste_bleibt_mit_Reihenfolge_nach_erneutem_Oeffnen_erhalten()
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "andytours.db");
        using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(datei))
        {
            AuswahllisteEinstellung.Speichern(verbindung, null, Auswahlliste.Trainingsbereich, ["K0", "GA1", "GA 2", "WSA"]);
        }

        using SqliteConnection wieder = OeffneDatenbank.Ausfuehren(datei);
        Assert.Equal(["K0", "GA1", "GA 2", "WSA"], AuswahllisteEinstellung.Laden(wieder, Auswahlliste.Trainingsbereich));
    }

    [Fact]
    public void Leere_Liste_ist_leer_und_nicht_null()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        AuswahllisteEinstellung.Speichern(verbindung, null, Auswahlliste.Trainingsbereich, []);

        Assert.Equal([], AuswahllisteEinstellung.Laden(verbindung, Auswahlliste.Trainingsbereich));
    }

    [Fact]
    public void Erneutes_Speichern_ersetzt_die_Liste()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        AuswahllisteEinstellung.Speichern(verbindung, null, Auswahlliste.Trainingsbereich, ["GA1", "GA2"]);
        AuswahllisteEinstellung.Speichern(verbindung, null, Auswahlliste.Trainingsbereich, ["GA2"]);

        Assert.Equal(["GA2"], AuswahllisteEinstellung.Laden(verbindung, Auswahlliste.Trainingsbereich));
    }

    [Fact]
    public void Ersetzen_aendert_alle_Schreibweisen_des_Bereichs()
    {
        using SqliteConnection verbindung = DatenbankMitEinheiten();
        using SqliteTransaction transaktion = verbindung.BeginTransaction();

        int anzahl = TrainingseinheitenTabelle.ErsetzeTrainingsbereich(verbindung, transaktion, "GA2", "GA 2");
        transaktion.Commit();

        Assert.Equal(2, anzahl);
        Assert.Equal(["GA 2", "GA1", "GA 2", ""], Bereiche(verbindung));
    }

    [Fact]
    public void Abgebrochene_Transaktion_aendert_weder_Liste_noch_Einheiten()
    {
        using SqliteConnection verbindung = DatenbankMitEinheiten();
        AuswahllisteEinstellung.Speichern(verbindung, null, Auswahlliste.Trainingsbereich, ["GA1", "GA2"]);

        using (SqliteTransaction transaktion = verbindung.BeginTransaction())
        {
            AuswahllisteEinstellung.Speichern(verbindung, transaktion, Auswahlliste.Trainingsbereich, ["GA1", "GA 2"]);
            TrainingseinheitenTabelle.ErsetzeTrainingsbereich(verbindung, transaktion, "GA2", "GA 2");
            // kein Commit: Dispose verwirft beide Änderungen
        }

        Assert.Equal(["GA1", "GA2"], AuswahllisteEinstellung.Laden(verbindung, Auswahlliste.Trainingsbereich));
        Assert.Equal(["GA2", "GA1", "ga2", ""], Bereiche(verbindung));
    }
}
