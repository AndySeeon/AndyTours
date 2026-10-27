using Microsoft.Data.Sqlite;
using AndyTours.Daten;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Daten;

/// <summary>
/// Tests für <see cref="OberflaechengroesseEinstellung"/>.
/// </summary>
public class OberflaechengroesseTests
{
    [Fact]
    public void Stufen_sind_die_sechs_festen_Werte()
    {
        Assert.Equal([100, 110, 125, 150, 175, 200], OberflaechengroesseEinstellung.Stufen);
    }

    [Fact]
    public void Ohne_Eintrag_ist_die_Groesse_100_Prozent()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");

        Assert.Equal(100, OberflaechengroesseEinstellung.Laden(verbindung));
    }

    [Theory]
    [InlineData(100)]
    [InlineData(110)]
    [InlineData(125)]
    [InlineData(150)]
    [InlineData(175)]
    [InlineData(200)]
    public void Gespeicherte_Stufe_bleibt_nach_erneutem_Oeffnen_erhalten(int prozent)
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "andytours.db");
        using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(datei))
        {
            OberflaechengroesseEinstellung.Speichern(verbindung, prozent);
        }

        using SqliteConnection wieder = OeffneDatenbank.Ausfuehren(datei);
        Assert.Equal(prozent, OberflaechengroesseEinstellung.Laden(wieder));
    }

    [Theory]
    [InlineData("130")]
    [InlineData("0")]
    [InlineData("groß")]
    [InlineData("")]
    public void Ungueltiger_Wert_ergibt_100_Prozent(string wert)
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        using (SqliteCommand befehl = verbindung.CreateCommand())
        {
            befehl.CommandText = "INSERT INTO Einstellung (Schluessel, Wert) VALUES ('Darstellung.Groesse', $wert);";
            befehl.Parameters.AddWithValue("$wert", wert);
            befehl.ExecuteNonQuery();
        }

        Assert.Equal(100, OberflaechengroesseEinstellung.Laden(verbindung));
    }

    [Fact]
    public void Speichern_lehnt_Werte_ausserhalb_der_Stufen_ab()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");

        Assert.Throws<ArgumentOutOfRangeException>(() => OberflaechengroesseEinstellung.Speichern(verbindung, 130));
    }
}
