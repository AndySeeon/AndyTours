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
    /// <summary>
    /// Fügt eine neue Wartung ein; die Nummer vergibt die Datenbank.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="wartung">Die neue Wartung (ihre Nummer wird nicht verwendet).</param>
    /// <returns>Die vergebene Nummer.</returns>
    public static long Einfuegen(SqliteConnection verbindung, Wartung wartung)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = """
            INSERT INTO Wartung (Datum, EquipmentId, Taetigkeit, Kommentar, Typ, AlarmKm, Aktiv)
            VALUES ($datum, $equipmentId, $taetigkeit, $kommentar, $typ, $alarmKm, $aktiv);
            SELECT last_insert_rowid();
            """;
        SetzeWerte(befehl, wartung);
        return (long)befehl.ExecuteScalar()!;
    }

    /// <summary>
    /// Speichert die geänderten Werte einer vorhandenen Wartung.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="wartung">Die Wartung mit ihrer Nummer.</param>
    /// <exception cref="InvalidOperationException">Die Wartung gibt es in der Datenbank nicht (mehr).</exception>
    public static void Aendern(SqliteConnection verbindung, Wartung wartung)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = """
            UPDATE Wartung SET
                Datum = $datum, EquipmentId = $equipmentId, Taetigkeit = $taetigkeit, Kommentar = $kommentar,
                Typ = $typ, AlarmKm = $alarmKm, Aktiv = $aktiv
            WHERE Id = $id;
            """;
        SetzeWerte(befehl, wartung);
        befehl.Parameters.AddWithValue("$id", wartung.Id);
        if (befehl.ExecuteNonQuery() != 1)
        {
            throw new InvalidOperationException($"Die Wartung Nr. {wartung.Id} ist nicht (mehr) vorhanden.");
        }
    }

    /// <summary>
    /// Löscht die Wartungen mit den angegebenen Nummern in einer Transaktion: entweder alle oder keine.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="ids">Die Nummern der zu löschenden Wartungen.</param>
    public static void Loeschen(SqliteConnection verbindung, IEnumerable<long> ids)
    {
        using SqliteTransaction transaktion = verbindung.BeginTransaction();
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = "DELETE FROM Wartung WHERE Id = $id;";
        SqliteParameter id = befehl.Parameters.Add("$id", SqliteType.Integer);
        foreach (long nummer in ids)
        {
            id.Value = nummer;
            befehl.ExecuteNonQuery();
        }

        transaktion.Commit();
    }

    /// <summary>
    /// Ersetzt eine Tätigkeit in allen Wartungen durch eine neue (Umbenennen in den Auswahllisten).
    /// </summary>
    /// <remarks>
    /// Verglichen wird ohne Leerzeichen am Rand und ohne Unterschied zwischen Groß- und Kleinschreibung, wie
    /// Einträge der Liste verglichen werden. Der Vergleich geschieht hier im Programm und nicht in SQL, weil
    /// SQLite Groß- und Kleinschreibung nur für A–Z gleichsetzt und Tätigkeiten Umlaute enthalten
    /// („Dämpfer eingestellt“). Läuft in der übergebenen Transaktion, damit Liste und Wartungen gemeinsam
    /// gespeichert werden.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="transaktion">Laufende Transaktion.</param>
    /// <param name="alt">Bisherige Tätigkeit.</param>
    /// <param name="neu">Neue Tätigkeit.</param>
    /// <returns>Die Anzahl der geänderten Wartungen.</returns>
    public static int ErsetzeTaetigkeit(SqliteConnection verbindung, SqliteTransaction transaktion, string alt, string neu)
    {
        string gesucht = alt.Trim();
        List<long> betroffen = [];
        using (SqliteCommand lesen = verbindung.CreateCommand())
        {
            lesen.Transaction = transaktion;
            lesen.CommandText = "SELECT Id, Taetigkeit FROM Wartung;";
            using SqliteDataReader leser = lesen.ExecuteReader();
            while (leser.Read())
            {
                if (string.Equals(leser.GetString(1).Trim(), gesucht, StringComparison.OrdinalIgnoreCase))
                {
                    betroffen.Add(leser.GetInt64(0));
                }
            }
        }

        using SqliteCommand aendern = verbindung.CreateCommand();
        aendern.Transaction = transaktion;
        aendern.CommandText = "UPDATE Wartung SET Taetigkeit = $neu WHERE Id = $id;";
        aendern.Parameters.AddWithValue("$neu", neu.Trim());
        SqliteParameter id = aendern.Parameters.Add("$id", SqliteType.Integer);
        foreach (long nummer in betroffen)
        {
            id.Value = nummer;
            aendern.ExecuteNonQuery();
        }

        return betroffen.Count;
    }

    /// <summary>Setzt die Werte einer Wartung (ohne Nummer) als Parameter.</summary>
    private static void SetzeWerte(SqliteCommand befehl, Wartung wartung)
    {
        befehl.Parameters.AddWithValue("$datum", DatumSpalte.AlsText(wartung.Datum));
        befehl.Parameters.AddWithValue("$equipmentId", (object?)wartung.EquipmentId ?? DBNull.Value);
        befehl.Parameters.AddWithValue("$taetigkeit", wartung.Taetigkeit);
        befehl.Parameters.AddWithValue("$kommentar", wartung.Kommentar);
        befehl.Parameters.AddWithValue("$typ", wartung.Typ.ToString());
        befehl.Parameters.AddWithValue("$alarmKm", wartung.AlarmKm);
        befehl.Parameters.AddWithValue("$aktiv", wartung.Aktiv);
    }
}

