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

    /// <summary>
    /// Fügt eine neue Aktivität ein; die Datenbank vergibt die Nummer.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="aktivitaet">Die Aktivität (ihre <see cref="Aktivitaet.Id"/> wird nicht verwendet).</param>
    /// <returns>Die vergebene Nummer.</returns>
    public static long Einfuegen(SqliteConnection verbindung, SqliteTransaction transaktion, Aktivitaet aktivitaet)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = """
            INSERT INTO Aktivitaet (Name, IstHaupt, Geschwindigkeitseinheit, ZielKm, ZielHm, ZielAnzahl)
            VALUES ($name, $istHaupt, $einheit, $zielKm, $zielHm, $zielAnzahl)
            RETURNING Id;
            """;
        befehl.Parameters.AddWithValue("$name", aktivitaet.Name);
        befehl.Parameters.AddWithValue("$istHaupt", aktivitaet.IstHaupt);
        befehl.Parameters.AddWithValue("$einheit", aktivitaet.Geschwindigkeitseinheit);
        befehl.Parameters.AddWithValue("$zielKm", aktivitaet.ZielKm);
        befehl.Parameters.AddWithValue("$zielHm", aktivitaet.ZielHm);
        befehl.Parameters.AddWithValue("$zielAnzahl", aktivitaet.ZielAnzahl);
        return (long)befehl.ExecuteScalar()!;
    }

    /// <summary>
    /// Schreibt Name und Hauptaktivitäts-Kennzeichen einer vorhandenen Aktivität.
    /// </summary>
    /// <remarks>
    /// Geschwindigkeitseinheit und Saisonziele werden hier nicht gepflegt und bleiben unverändert.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="aktivitaet">Die Aktivität mit ihrer Nummer.</param>
    public static void Aendern(SqliteConnection verbindung, SqliteTransaction transaktion, Aktivitaet aktivitaet)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = "UPDATE Aktivitaet SET Name = $name, IstHaupt = $istHaupt WHERE Id = $id;";
        befehl.Parameters.AddWithValue("$id", aktivitaet.Id);
        befehl.Parameters.AddWithValue("$name", aktivitaet.Name);
        befehl.Parameters.AddWithValue("$istHaupt", aktivitaet.IstHaupt);
        befehl.ExecuteNonQuery();
    }

    /// <summary>
    /// Löscht eine Aktivität. Wird sie noch von Einheiten oder Equipments verwendet, scheitert das an den
    /// Fremdschlüsseln – die Oberfläche prüft das vorher mit <c>ErmittleLoeschsperre</c>.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="id">Nummer der Aktivität.</param>
    public static void Loeschen(SqliteConnection verbindung, SqliteTransaction transaktion, long id)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = "DELETE FROM Aktivitaet WHERE Id = $id;";
        befehl.Parameters.AddWithValue("$id", id);
        befehl.ExecuteNonQuery();
    }

    /// <summary>
    /// Führt zwei Aktivitäten zusammen: Einheiten und Equipments der Ursprungsaktivität gehören danach zur
    /// Zielaktivität, ein Hauptaktivitäts-Kennzeichen geht auf das Ziel über, die Ursprungsaktivität wird
    /// gelöscht.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion – alle Schritte gelten nur gemeinsam.</param>
    /// <param name="ursprungId">Nummer der Aktivität, die entfernt wird.</param>
    /// <param name="zielId">Nummer der Aktivität, die bleibt.</param>
    public static void FuehreZusammen(SqliteConnection verbindung, SqliteTransaction transaktion, long ursprungId, long zielId)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = """
            UPDATE Trainingseinheit SET AktivitaetId = $ziel WHERE AktivitaetId = $ursprung;
            UPDATE Equipment SET AktivitaetId = $ziel WHERE AktivitaetId = $ursprung;
            UPDATE Aktivitaet SET IstHaupt = 1
                WHERE Id = $ziel AND EXISTS (SELECT 1 FROM Aktivitaet WHERE Id = $ursprung AND IstHaupt <> 0);
            DELETE FROM Aktivitaet WHERE Id = $ursprung;
            """;
        befehl.Parameters.AddWithValue("$ursprung", ursprungId);
        befehl.Parameters.AddWithValue("$ziel", zielId);
        befehl.ExecuteNonQuery();
    }
}
