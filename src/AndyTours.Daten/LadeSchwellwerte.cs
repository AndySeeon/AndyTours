using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Lädt alle Schwellwerte aus der Tabelle <c>Schwellwert</c>.
/// </summary>
public static class LadeSchwellwerte
{
    /// <summary>
    /// Liest alle Schwellwerte, sortiert nach Art und Reihenfolge.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Schwellwerte.</returns>
    public static List<Schwellwert> Ausfuehren(SqliteConnection verbindung)
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
}
