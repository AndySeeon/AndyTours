using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Daten;

/// <summary>
/// Tests für <see cref="SpaltenanordnungEinstellung.Laden"/> und <see cref="SpaltenanordnungEinstellung.Speichern"/>.
/// </summary>
public class SpaltenanordnungTests
{
    [Fact]
    public void Neue_Datenbank_liefert_die_Standardanordnung()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");

        List<Spalteneinstellung> anordnung = SpaltenanordnungEinstellung.Laden(verbindung);

        Assert.Equal(Enum.GetValues<Einheitenspalte>(), anordnung.Select(a => a.Spalte));
        Assert.All(anordnung, a => Assert.Null(a.Breite));
    }

    [Fact]
    public void Reihenfolge_und_Breiten_bleiben_nach_erneutem_Oeffnen_erhalten()
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "andytours.db");
        List<Spalteneinstellung> anordnung = Enum.GetValues<Einheitenspalte>().Reverse()
            .Select((s, i) => new Spalteneinstellung(s, i % 2 == 0 ? 50.5 + i : null)).ToList();
        using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(datei))
        {
            SpaltenanordnungEinstellung.Speichern(verbindung, anordnung);
        }

        using SqliteConnection wieder = OeffneDatenbank.Ausfuehren(datei);
        Assert.Equal(anordnung, SpaltenanordnungEinstellung.Laden(wieder));
    }

    [Fact]
    public void Unbekannte_Spalten_und_unlesbare_Breiten_werden_uebergangen()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        using (SqliteCommand befehl = verbindung.CreateCommand())
        {
            befehl.CommandText = "INSERT INTO Einstellung VALUES ($s, 'Fahrzeit:80,Leistung:60,3:20,Name:breit,Datum:96.5');";
            befehl.Parameters.AddWithValue("$s", Einstellungsschluessel.Spaltenanordnung);
            befehl.ExecuteNonQuery();
        }

        List<Spalteneinstellung> anordnung = SpaltenanordnungEinstellung.Laden(verbindung);

        Assert.Equal(17, anordnung.Count);
        Assert.Equal(
            [new(Einheitenspalte.Fahrzeit, 80), new(Einheitenspalte.Name, null), new Spalteneinstellung(Einheitenspalte.Datum, 96.5)],
            anordnung.Take(3));
    }
}
