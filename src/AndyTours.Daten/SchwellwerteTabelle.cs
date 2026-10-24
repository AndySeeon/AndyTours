using AndyTours.Core.Modell;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest und schreibt die Tabelle <c>Schwellwert</c>.
/// </summary>
public static class SchwellwerteTabelle
{
    /// <summary>
    /// Liest alle Schwellwerte, sortiert nach Art und Reihenfolge.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Schwellwerte.</returns>
    public static List<Schwellwert> Laden(SqliteConnection verbindung)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "SELECT Id, Art, Reihenfolge, Name, Von, Bis, Farbe FROM Schwellwert ORDER BY Id;";

        var schwellwerte = new List<Schwellwert>();
        using SqliteDataReader leser = befehl.ExecuteReader();
        while (leser.Read())
        {
            schwellwerte.Add(new Schwellwert
            {
                Id = leser.GetInt64(0),
                Art = Enum.Parse<SchwellwertArt>(leser.GetString(1)),
                Reihenfolge = leser.GetInt32(2),
                Name = leser.GetString(3),
                Von = leser.GetDouble(4),
                Bis = leser.GetDouble(5),
                Farbe = leser.GetString(6),
            });
        }

        return schwellwerte;
    }

    /// <summary>
    /// Fügt alle übergebenen Schwellwerte mit ihrer vorhandenen Nummer ein.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="schwellwerte">Die Schwellwerte.</param>
    public static void Speichern(SqliteConnection verbindung, SqliteTransaction transaktion, IEnumerable<Schwellwert> schwellwerte)
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
