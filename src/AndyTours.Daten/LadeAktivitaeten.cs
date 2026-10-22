using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Lädt alle Aktivitäten aus der Tabelle <c>Aktivitaet</c>.
/// </summary>
public static class LadeAktivitaeten
{
    /// <summary>
    /// Liest alle Aktivitäten, sortiert nach Nummer (entspricht der Reihenfolge im Original).
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Aktivitäten.</returns>
    public static List<Aktivitaet> Ausfuehren(SqliteConnection verbindung)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = """
            SELECT Id, Name, IstHaupt, Geschwindigkeitseinheit, ZielKm, ZielHm, ZielAnzahl
            FROM Aktivitaet ORDER BY Id;
            """;

        var aktivitaeten = new List<Aktivitaet>();
        using SqliteDataReader leser = befehl.ExecuteReader();
        while (leser.Read())
        {
            aktivitaeten.Add(new Aktivitaet
            {
                Id = leser.GetInt64(0),
                Name = leser.GetString(1),
                IstHaupt = leser.GetBoolean(2),
                Geschwindigkeitseinheit = leser.GetString(3),
                ZielKm = leser.GetDouble(4),
                ZielHm = leser.GetDouble(5),
                ZielAnzahl = leser.GetInt32(6),
            });
        }

        return aktivitaeten;
    }
}
