using System.Globalization;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten.Datensicherung;

/// <summary>
/// Prüft, ob eine Datei als Sicherung zurückgespielt werden kann.
/// </summary>
public static class PruefeSicherung
{
    /// <summary>SQLite-Fehlercode SQLITE_CORRUPT: Die Datei ist beschädigt.</summary>
    private const int SqliteBeschaedigt = 11;

    /// <summary>SQLite-Fehlercode SQLITE_NOTADB: Die Datei ist keine SQLite-Datenbank.</summary>
    private const int SqliteKeineDatenbank = 26;

    /// <summary>
    /// Tabellen, die jede AndyTours-Datenbank seit Schemaversion 1 enthält. Fehlt eine davon,
    /// ist die Datei keine AndyTours-Datenbank. (Die Tabelle <c>Einstellung</c> gibt es erst seit
    /// Version 2 und fehlt deshalb hier – ältere Sicherungen sollen brauchbar bleiben.)
    /// </summary>
    private static readonly string[] Kerntabellen =
    [
        "Fahrer", "Aktivitaet", "Kategorie", "Equipment", "Trainingseinheit",
        "EinheitKategorie", "Anhang", "Wartung", "Schwellwert",
    ];

    /// <summary>
    /// Öffnet die Datei nur lesend und prüft sie.
    /// </summary>
    /// <remarks>
    /// Geprüft wird in dieser Reihenfolge:
    /// <list type="number">
    /// <item>Schemaversion (<c>PRAGMA user_version</c>): 0 heißt „keine AndyTours-Datenbank“, eine
    /// größere Zahl als <see cref="ErstelleSchema.Schemaversion"/> heißt „neuere Programmversion“.</item>
    /// <item>Alle <see cref="Kerntabellen"/> sind vorhanden.</item>
    /// <item><c>PRAGMA quick_check</c> meldet „ok“ – findet also keine beschädigten Seiten.</item>
    /// </list>
    /// Bewusst wird nicht <see cref="OeffneDatenbank"/> verwendet: Das würde das Schema anlegen und
    /// damit eine fremde oder ältere Datei verändern. Die Datei bleibt hier unverändert.
    /// </remarks>
    /// <param name="datei">Vollständiger Pfad der Sicherung.</param>
    /// <returns>Das Ergebnis der Prüfung.</returns>
    /// <exception cref="SqliteException">Die Datei konnte nicht geöffnet werden (z. B. nicht erreichbar).</exception>
    public static Sicherungspruefung Ausfuehren(string datei)
    {
        var verbindungstext = new SqliteConnectionStringBuilder
        {
            DataSource = datei,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false, // Datei nach dem Schließen sofort wieder frei
        }.ToString();

        try
        {
            using var verbindung = new SqliteConnection(verbindungstext);
            verbindung.Open();

            // 1. Schemaversion
            long version = Convert.ToInt64(Abfrage(verbindung, "PRAGMA user_version;"), CultureInfo.InvariantCulture);
            if (version <= 0)
            {
                return Sicherungspruefung.Unbrauchbar;
            }

            if (version > ErstelleSchema.Schemaversion)
            {
                return Sicherungspruefung.NeuereVersion;
            }

            // 2. Kerntabellen
            foreach (string tabelle in Kerntabellen)
            {
                using SqliteCommand befehl = verbindung.CreateCommand();
                befehl.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name;";
                befehl.Parameters.AddWithValue("$name", tabelle);
                if (Convert.ToInt64(befehl.ExecuteScalar(), CultureInfo.InvariantCulture) == 0)
                {
                    return Sicherungspruefung.Unbrauchbar;
                }
            }

            // 3. Unversehrtheit
            return Abfrage(verbindung, "PRAGMA quick_check;") as string == "ok"
                ? Sicherungspruefung.Brauchbar
                : Sicherungspruefung.Unbrauchbar;
        }
        catch (SqliteException fehler) when (fehler.SqliteErrorCode is SqliteBeschaedigt or SqliteKeineDatenbank)
        {
            // Textdatei, abgeschnittene oder anderweitig zerstörte Datei
            return Sicherungspruefung.Unbrauchbar;
        }
    }

    /// <summary>Führt eine Abfrage mit einem einzelnen Ergebniswert aus.</summary>
    private static object? Abfrage(SqliteConnection verbindung, string sql)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = sql;
        return befehl.ExecuteScalar();
    }
}
