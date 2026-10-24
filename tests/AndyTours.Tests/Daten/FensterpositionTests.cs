using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Daten;

/// <summary>
/// Tests für <see cref="LadeFensterposition"/> und <see cref="SpeichereFensterposition"/>.
/// </summary>
public class FensterpositionTests
{
    /// <summary>Schreibt einen beliebigen Text als gemerkte Fensterposition.</summary>
    private static void SchreibeRoh(SqliteConnection verbindung, string wert)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "INSERT OR REPLACE INTO Einstellung (Schluessel, Wert) VALUES ($s, $w);";
        befehl.Parameters.AddWithValue("$s", Einstellungsschluessel.Fensterposition);
        befehl.Parameters.AddWithValue("$w", wert);
        befehl.ExecuteNonQuery();
    }

    [Fact]
    public void Ohne_Eintrag_gibt_es_keine_gemerkte_Position()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");

        Assert.Null(LadeFensterposition.Ausfuehren(verbindung));
    }

    [Fact]
    public void Gemerkte_Position_bleibt_nach_erneutem_Oeffnen_erhalten()
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "andytours.db");
        var position = new Fensterposition(-1800.5, 120, 1280, 720.5, true);
        using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(datei))
        {
            SpeichereFensterposition.Ausfuehren(verbindung, position);
        }

        using SqliteConnection wieder = OeffneDatenbank.Ausfuehren(datei);
        Assert.Equal(position, LadeFensterposition.Ausfuehren(wieder));
    }

    [Theory]
    [InlineData("100;50;1200;800")]
    [InlineData("100;50;1200;800;ja")]
    [InlineData("100;50;0;800;0")]
    [InlineData("100;fünfzig;1200;800;0")]
    [InlineData("100;50;NaN;800;0")]
    public void Unlesbare_Position_wird_ignoriert(string wert)
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        SchreibeRoh(verbindung, wert);

        Assert.Null(LadeFensterposition.Ausfuehren(verbindung));
    }
}
