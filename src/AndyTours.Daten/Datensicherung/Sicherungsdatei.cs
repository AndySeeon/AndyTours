namespace AndyTours.Daten.Datensicherung;

/// <summary>
/// Feste Bestandteile der Dateinamen von Datensicherungen.
/// </summary>
/// <remarks>
/// Eine Sicherung heißt <c>andytours_JJJJ-MM-TT_hh-mm-ss.db</c>, z. B.
/// <c>andytours_2026-10-23_07-39-12.db</c>. Am Zeitstempel im Namen erkennt die App beim Aufräumen,
/// welche Dateien Sicherungen sind und welche die ältesten sind; alle anderen Dateien im
/// Sicherungsordner bleiben unberührt.
/// </remarks>
public static class Sicherungsdatei
{
    /// <summary>Anfang jedes Sicherungsnamens.</summary>
    public const string Praefix = "andytours_";

    /// <summary>Format des Zeitstempels im Namen (sortiert auch als Text richtig).</summary>
    public const string Zeitstempelformat = "yyyy-MM-dd_HH-mm-ss";

    /// <summary>Dateiendung einer Sicherung.</summary>
    public const string Endung = ".db";

    /// <summary>
    /// Zusätzliche Endung, unter der eine Sicherung geschrieben wird, solange sie noch nicht fertig ist.
    /// Eine abgebrochene Kopie trägt diese Endung und gilt deshalb nie als Sicherung.
    /// </summary>
    public const string Zwischenendung = ".tmp";
}
