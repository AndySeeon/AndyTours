using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Öffnet die SQLite-Datenbank von AndyTours.
/// </summary>
public static class OeffneDatenbank
{
    /// <summary>
    /// Öffnet (bzw. erzeugt) die Datenbankdatei und stellt sicher, dass das Schema vorhanden ist.
    /// </summary>
    /// <remarks>
    /// SQLite legt die Datei automatisch an, wenn sie noch nicht existiert.
    /// Fremdschlüssel-Prüfungen sind bei SQLite standardmäßig <b>aus</b> und müssen für jede
    /// Verbindung eingeschaltet werden – sonst ließen sich z. B. Einheiten mit einem nicht
    /// vorhandenen Equipment speichern.
    /// </remarks>
    /// <param name="dateipfad">Pfad zur Datenbankdatei, oder „:memory:" für eine reine Arbeitsspeicher-Datenbank (Tests).</param>
    /// <returns>Die geöffnete Verbindung. Der Aufrufer muss sie schließen (<c>using</c>).</returns>
    public static SqliteConnection Ausfuehren(string dateipfad)
    {
        var verbindungstext = new SqliteConnectionStringBuilder
        {
            DataSource = dateipfad,
            // Keine Verbindungs-Wiederverwendung: Sonst bleibt die Datei nach dem Schließen gesperrt
            // und könnte z. B. für eine erneute Migration nicht gelöscht werden.
            Pooling = false,
        }.ToString();

        var verbindung = new SqliteConnection(verbindungstext);
        verbindung.Open();

        using (SqliteCommand befehl = verbindung.CreateCommand())
        {
            befehl.CommandText = "PRAGMA foreign_keys = ON;";
            befehl.ExecuteNonQuery();
        }

        ErstelleSchema.Ausfuehren(verbindung);
        return verbindung;
    }
}
