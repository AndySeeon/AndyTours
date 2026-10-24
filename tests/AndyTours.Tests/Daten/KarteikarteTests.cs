using Microsoft.Data.Sqlite;
using AndyTours.Daten;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Daten;

/// <summary>
/// Tests für <see cref="LadeKarteikarte"/> und <see cref="SpeichereKarteikarte"/>.
/// </summary>
public class KarteikarteTests
{
    /// <summary>Schreibt einen beliebigen Text als gemerkte Karteikarte.</summary>
    private static void SchreibeRoh(SqliteConnection verbindung, string wert)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "INSERT OR REPLACE INTO Einstellung (Schluessel, Wert) VALUES ($s, $w);";
        befehl.Parameters.AddWithValue("$s", Einstellungsschluessel.Karteikarte);
        befehl.Parameters.AddWithValue("$w", wert);
        befehl.ExecuteNonQuery();
    }

    [Fact]
    public void Ohne_Eintrag_gibt_es_keine_gemerkte_Karteikarte()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");

        Assert.Null(LadeKarteikarte.Ausfuehren(verbindung));
    }

    [Fact]
    public void Gemerkte_Karteikarte_bleibt_nach_erneutem_Oeffnen_erhalten()
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "andytours.db");
        using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(datei))
        {
            SpeichereKarteikarte.Ausfuehren(verbindung, 1);
        }

        using SqliteConnection wieder = OeffneDatenbank.Ausfuehren(datei);
        Assert.Equal(1, LadeKarteikarte.Ausfuehren(wieder));
    }

    [Fact]
    public void Erneutes_Speichern_ueberschreibt_die_Karteikarte()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        SpeichereKarteikarte.Ausfuehren(verbindung, 1);
        SpeichereKarteikarte.Ausfuehren(verbindung, 0);

        Assert.Equal(0, LadeKarteikarte.Ausfuehren(verbindung));
    }

    [Theory]
    [InlineData("")]
    [InlineData("-1")]
    [InlineData("eins")]
    [InlineData("1.5")]
    public void Unlesbare_Karteikarte_wird_ignoriert(string wert)
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        SchreibeRoh(verbindung, wert);

        Assert.Null(LadeKarteikarte.Ausfuehren(verbindung));
    }

    [Fact]
    public void Negative_Nummer_wird_nicht_gespeichert()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");

        Assert.Throws<ArgumentOutOfRangeException>(() => SpeichereKarteikarte.Ausfuehren(verbindung, -1));
    }
}
