using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Fügt Wartungseinträge und Alarme in die Tabelle <c>Wartung</c> ein.
/// </summary>
public static class SpeichereWartungen
{
    /// <summary>
    /// Fügt alle übergebenen Einträge mit ihrer vorhandenen Nummer ein.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="wartungen">Die Wartungseinträge.</param>
    public static void Ausfuehren(SqliteConnection verbindung, SqliteTransaction transaktion, IEnumerable<Wartung> wartungen)
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
            befehl.Parameters.AddWithValue("$datum", DatumAlsText.Ausfuehren(w.Datum));
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
