using AndyTours.Core.Modell;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest und schreibt die Tabelle <c>Wartung</c>.
/// </summary>
public static class WartungenTabelle
{
    /// <summary>
    /// Liest alle Einträge, sortiert nach Datum.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Wartungseinträge.</returns>
    public static List<Wartung> Laden(SqliteConnection verbindung)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = """
            SELECT Id, Datum, EquipmentId, Taetigkeit, Kommentar, Typ, AlarmKm, Aktiv
            FROM Wartung ORDER BY Datum, Id;
            """;

        var wartungen = new List<Wartung>();
        using SqliteDataReader leser = befehl.ExecuteReader();
        while (leser.Read())
        {
            wartungen.Add(new Wartung
            {
                Id = leser.GetInt64(0),
                Datum = DatumSpalte.Lesen(leser, 1)!.Value,
                EquipmentId = leser.IsDBNull(2) ? null : leser.GetInt64(2),
                Taetigkeit = leser.GetString(3),
                Kommentar = leser.GetString(4),
                Typ = Enum.Parse<WartungsTyp>(leser.GetString(5)),
                AlarmKm = leser.GetDouble(6),
                Aktiv = leser.GetBoolean(7),
            });
        }

        return wartungen;
    }

    /// <summary>
    /// Fügt alle übergebenen Einträge mit ihrer vorhandenen Nummer ein.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="wartungen">Die Wartungseinträge.</param>
    public static void Speichern(SqliteConnection verbindung, SqliteTransaction transaktion, IEnumerable<Wartung> wartungen)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = """
            INSERT INTO Wartung (Id, Datum, EquipmentId, Taetigkeit, Kommentar, Typ, AlarmKm, Aktiv)
            VALUES ($id, $datum, $equipmentId, $taetigkeit, $kommentar, $typ, $alarmKm, $aktiv);
            """;

        foreach (Wartung w in wartungen)
        {
            befehl.Parameters.Clear();
            befehl.Parameters.AddWithValue("$id", w.Id);
            befehl.Parameters.AddWithValue("$datum", DatumSpalte.AlsText(w.Datum));
            befehl.Parameters.AddWithValue("$equipmentId", (object?)w.EquipmentId ?? DBNull.Value);
            befehl.Parameters.AddWithValue("$taetigkeit", w.Taetigkeit);
            befehl.Parameters.AddWithValue("$kommentar", w.Kommentar);
            befehl.Parameters.AddWithValue("$typ", w.Typ.ToString());
            befehl.Parameters.AddWithValue("$alarmKm", w.AlarmKm);
            befehl.Parameters.AddWithValue("$aktiv", w.Aktiv);
            befehl.ExecuteNonQuery();
        }
    }
}
