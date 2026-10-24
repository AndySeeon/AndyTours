using AndyTours.Core.Modell;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest und schreibt die Tabelle <c>Aktivitaet</c>.
/// </summary>
public static class AktivitaetenTabelle
{
    /// <summary>
    /// Liest alle Aktivitäten, sortiert nach Nummer (entspricht der Reihenfolge im Original).
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Aktivitäten.</returns>
    public static List<Aktivitaet> Laden(SqliteConnection verbindung)
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

    /// <summary>
    /// Fügt alle übergebenen Aktivitäten mit ihrer vorhandenen Nummer ein.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="aktivitaeten">Die Aktivitäten.</param>
    public static void Speichern(SqliteConnection verbindung, SqliteTransaction transaktion, IEnumerable<Aktivitaet> aktivitaeten)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = """
            INSERT INTO Aktivitaet (Id, Name, IstHaupt, Geschwindigkeitseinheit, ZielKm, ZielHm, ZielAnzahl)
            VALUES ($id, $name, $istHaupt, $einheit, $zielKm, $zielHm, $zielAnzahl);
            """;

        foreach (Aktivitaet a in aktivitaeten)
        {
            befehl.Parameters.Clear();
            befehl.Parameters.AddWithValue("$id", a.Id);
            befehl.Parameters.AddWithValue("$name", a.Name);
            befehl.Parameters.AddWithValue("$istHaupt", a.IstHaupt);
            befehl.Parameters.AddWithValue("$einheit", a.Geschwindigkeitseinheit);
            befehl.Parameters.AddWithValue("$zielKm", a.ZielKm);
            befehl.Parameters.AddWithValue("$zielHm", a.ZielHm);
            befehl.Parameters.AddWithValue("$zielAnzahl", a.ZielAnzahl);
            befehl.ExecuteNonQuery();
        }
    }
}
