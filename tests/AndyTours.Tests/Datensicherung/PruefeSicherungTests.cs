using AndyTours.Daten;
using AndyTours.Daten.Datensicherung;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Datensicherung;

/// <summary>
/// Tests für <see cref="PruefeSicherung"/>.
/// </summary>
public class PruefeSicherungTests
{
    [Fact]
    public void Gueltige_Sicherung_ist_brauchbar()
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "s.db");
        TestSicherungen.Gueltig(datei, 3, "Bründlingalm");

        Assert.Equal(Sicherungspruefung.Brauchbar, PruefeSicherung.Ausfuehren(datei));
    }

    [Fact]
    public void Sicherung_einer_aelteren_Version_ist_brauchbar_und_bleibt_unveraendert()
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "s.db");
        TestSicherungen.Version1(datei, 3, "Alt");
        byte[] vorher = File.ReadAllBytes(datei);

        Assert.Equal(Sicherungspruefung.Brauchbar, PruefeSicherung.Ausfuehren(datei));
        Assert.Equal(vorher, File.ReadAllBytes(datei));
    }

    [Fact]
    public void Sicherung_einer_neueren_Version_wird_erkannt()
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "s.db");
        TestSicherungen.MitVersion(datei, ErstelleSchema.Schemaversion + 1);

        Assert.Equal(Sicherungspruefung.NeuereVersion, PruefeSicherung.Ausfuehren(datei));
    }

    [Fact]
    public void Textdatei_ist_unbrauchbar()
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "s.db");
        File.WriteAllText(datei, "Das ist keine Datenbank, sondern ein Einkaufszettel: Kettenöl, Schlauch, Riegel.");

        Assert.Equal(Sicherungspruefung.Unbrauchbar, PruefeSicherung.Ausfuehren(datei));
    }

    [Fact]
    public void Leere_Datei_ist_unbrauchbar()
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "s.db");
        File.WriteAllBytes(datei, []);

        Assert.Equal(Sicherungspruefung.Unbrauchbar, PruefeSicherung.Ausfuehren(datei));
    }

    [Fact]
    public void Abgeschnittene_Datei_ist_unbrauchbar()
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "s.db");
        TestSicherungen.Abgeschnitten(datei);

        Assert.Equal(Sicherungspruefung.Unbrauchbar, PruefeSicherung.Ausfuehren(datei));
    }

    [Fact]
    public void Fremde_SQLite_Datenbank_ist_unbrauchbar_und_bleibt_unveraendert()
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "s.db");
        TestSicherungen.Fremd(datei);
        byte[] vorher = File.ReadAllBytes(datei);

        Assert.Equal(Sicherungspruefung.Unbrauchbar, PruefeSicherung.Ausfuehren(datei));
        Assert.Equal(vorher, File.ReadAllBytes(datei));
    }

    [Fact]
    public void Fehlende_Kerntabelle_ist_unbrauchbar()
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "s.db");
        TestSicherungen.MitVersion(datei, 2);
        using (var verbindung = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={datei};Pooling=False"))
        {
            verbindung.Open();
            using var befehl = verbindung.CreateCommand();
            befehl.CommandText = "DROP TABLE Wartung;";
            befehl.ExecuteNonQuery();
        }

        Assert.Equal(Sicherungspruefung.Unbrauchbar, PruefeSicherung.Ausfuehren(datei));
    }
}
