using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Fügt Aktivitäten in die Tabelle <c>Aktivitaet</c> ein.
/// </summary>
public static class SpeichereAktivitaeten
{
    /// <summary>
    /// Fügt alle übergebenen Aktivitäten mit ihrer vorhandenen Nummer ein.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="aktivitaeten">Die Aktivitäten.</param>
    public static void Ausfuehren(SqliteConnection verbindung, SqliteTransaction transaktion, IEnumerable<Aktivitaet> aktivitaeten)
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
