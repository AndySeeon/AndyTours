using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Daten;

/// <summary>
/// Tests für <see cref="LadeAusgeblendeteSpalten"/> und <see cref="SpeichereAusgeblendeteSpalten"/>.
/// </summary>
public class AusgeblendeteSpaltenTests
{
    /// <summary>Schreibt einen beliebigen Text als gespeicherte Spaltenwahl.</summary>
    private static void SchreibeRoh(SqliteConnection verbindung, string wert)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "INSERT OR REPLACE INTO Einstellung (Schluessel, Wert) VALUES ($s, $w);";
        befehl.Parameters.AddWithValue("$s", Einstellungsschluessel.AusgeblendeteSpalten);
        befehl.Parameters.AddWithValue("$w", wert);
        befehl.ExecuteNonQuery();
    }

    [Fact]
    public void Neue_Datenbank_hat_alle_Spalten_sichtbar()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");

        Assert.Empty(LadeAusgeblendeteSpalten.Ausfuehren(verbindung));
    }

    [Fact]
    public void Gespeicherte_Wahl_bleibt_nach_erneutem_Oeffnen_erhalten()
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "andytours.db");
        using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(datei))
        {
            SpeichereAusgeblendeteSpalten.Ausfuehren(verbindung,
                new HashSet<Einheitenspalte> { Einheitenspalte.Trittfrequenz, Einheitenspalte.Temperatur });
        }

        using SqliteConnection wieder = OeffneDatenbank.Ausfuehren(datei);
        Assert.Equal(new HashSet<Einheitenspalte> { Einheitenspalte.Temperatur, Einheitenspalte.Trittfrequenz },
            LadeAusgeblendeteSpalten.Ausfuehren(wieder));
    }

    [Fact]
    public void Leere_Menge_loescht_den_Eintrag()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        SpeichereAusgeblendeteSpalten.Ausfuehren(verbindung, new HashSet<Einheitenspalte> { Einheitenspalte.Name });

        SpeichereAusgeblendeteSpalten.Ausfuehren(verbindung, new HashSet<Einheitenspalte>());

        Assert.Empty(LadeAusgeblendeteSpalten.Ausfuehren(verbindung));
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "SELECT COUNT(*) FROM Einstellung;";
        Assert.Equal(0L, befehl.ExecuteScalar());
    }

    [Fact]
    public void Unbekannte_Namen_werden_uebergangen()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        SchreibeRoh(verbindung, "Trittfrequenz, Leistung,3,,temperatur, MaxHoehe");

        Assert.Equal(new HashSet<Einheitenspalte> { Einheitenspalte.Trittfrequenz, Einheitenspalte.MaxHoehe },
            LadeAusgeblendeteSpalten.Ausfuehren(verbindung));
    }

    [Fact]
    public void Alle_Spalten_ausgeblendet_gilt_als_unbrauchbar()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        SchreibeRoh(verbindung, string.Join(",", Enum.GetNames<Einheitenspalte>()));

        Assert.Empty(LadeAusgeblendeteSpalten.Ausfuehren(verbindung));
    }

    [Fact]
    public void Sicherungseinstellungen_bleiben_unberuehrt()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        SpeichereSicherungseinstellungen.Ausfuehren(verbindung, new Sicherungseinstellungen { Intervall = 5 });

        SpeichereAusgeblendeteSpalten.Ausfuehren(verbindung, new HashSet<Einheitenspalte> { Einheitenspalte.Name });
        SpeichereAusgeblendeteSpalten.Ausfuehren(verbindung, new HashSet<Einheitenspalte>());

        Assert.Equal(5, LadeSicherungseinstellungen.Ausfuehren(verbindung).Intervall);
    }
}
