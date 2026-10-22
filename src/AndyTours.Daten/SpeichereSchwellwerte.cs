using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Fügt Schwellwerte in die Tabelle <c>Schwellwert</c> ein.
/// </summary>
public static class SpeichereSchwellwerte
{
    /// <summary>
    /// Fügt alle übergebenen Schwellwerte mit ihrer vorhandenen Nummer ein.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="schwellwerte">Die Schwellwerte.</param>
    public static void Ausfuehren(SqliteConnection verbindung, SqliteTransaction transaktion, IEnumerable<Schwellwert> schwellwerte)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = """
            INSERT INTO Schwellwert (Id, Art, Reihenfolge, Name, Von, Bis, Farbe)
            VALUES ($id, $art, $reihenfolge, $name, $von, $bis, $farbe);
            """;

        foreach (Schwellwert s in schwellwerte)
        {
            befehl.Parameters.Clear();
            befehl.Parameters.AddWithValue("$id", s.Id);
            befehl.Parameters.AddWithValue("$art", s.Art.ToString());
            befehl.Parameters.AddWithValue("$reihenfolge", s.Reihenfolge);
            befehl.Parameters.AddWithValue("$name", s.Name);
            befehl.Parameters.AddWithValue("$von", s.Von);
            befehl.Parameters.AddWithValue("$bis", s.Bis);
            befehl.Parameters.AddWithValue("$farbe", s.Farbe);
            befehl.ExecuteNonQuery();
        }
    }
}
