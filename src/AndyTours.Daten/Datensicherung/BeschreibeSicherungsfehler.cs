using System.Globalization;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten.Datensicherung;

/// <summary>
/// Übersetzt einen Fehler bei der Datensicherung in einen verständlichen deutschen Satz.
/// </summary>
public static class BeschreibeSicherungsfehler
{
    /// <summary>HRESULT von Windows für „Auf dem Datenträger ist nicht genug Speicherplatz“ (ERROR_DISK_FULL).</summary>
    internal const int DatentraegerVoll = unchecked((int)0x80070070);

    /// <summary>HRESULT von Windows für „Der Datenträger ist voll“ (ERROR_HANDLE_DISK_FULL).</summary>
    internal const int DatentraegerVollHandle = unchecked((int)0x80070027);

    /// <summary>SQLite-Fehlercode SQLITE_FULL: Datenbank oder Datenträger voll.</summary>
    internal const int SqliteVoll = 13;

    /// <summary>
    /// Liefert die Beschreibung für den Benutzer.
    /// </summary>
    /// <remarks>
    /// Die Meldungen der .NET-Ausnahmen sind je nach Windows-Installation englisch und technisch;
    /// für die häufigen Fälle (nicht erreichbar, keine Rechte, Datenträger voll, Zeitüberschreitung)
    /// gibt es deshalb eigene Sätze. Sonst wird die Originalmeldung angehängt.
    /// </remarks>
    /// <param name="fehler">Die aufgetretene Ausnahme.</param>
    /// <param name="zeitlimit">Das verwendete Zeitlimit (für die Meldung bei Zeitüberschreitung).</param>
    /// <returns>Ein Satz, der den Grund beschreibt.</returns>
    public static string Ausfuehren(Exception fehler, TimeSpan zeitlimit)
    {
        return fehler switch
        {
            TimeoutException =>
                "Der Sicherungsordner hat nicht innerhalb von "
                + ((int)zeitlimit.TotalSeconds).ToString(CultureInfo.InvariantCulture)
                + " Sekunden geantwortet (ist z. B. das NAS ausgeschaltet?).",
            DirectoryNotFoundException or DriveNotFoundException =>
                "Der Sicherungsordner ist nicht erreichbar (ist z. B. das NAS ausgeschaltet oder das Laufwerk getrennt?).",
            UnauthorizedAccessException =>
                "AndyTours darf im Sicherungsordner nicht schreiben oder löschen.",
            IOException io when io.HResult is DatentraegerVoll or DatentraegerVollHandle =>
                "Auf dem Datenträger des Sicherungsordners ist nicht genug Platz.",
            SqliteException { SqliteErrorCode: SqliteVoll } =>
                "Auf dem Datenträger des Sicherungsordners ist nicht genug Platz.",
            _ => $"Unerwarteter Fehler: {fehler.Message}",
        };
    }
}
