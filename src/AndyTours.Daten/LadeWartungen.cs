using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Lädt alle Wartungseinträge und Alarme aus der Tabelle <c>Wartung</c>.
/// </summary>
public static class LadeWartungen
{
    /// <summary>
    /// Liest alle Einträge, sortiert nach Datum.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Wartungseinträge.</returns>
    public static List<Wartung> Ausfuehren(SqliteConnection verbindung)
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
                Datum = TextAlsDatum.Ausfuehren(leser, 1)!.Value,
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
}
