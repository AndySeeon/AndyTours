namespace AndyTours.Daten.Datensicherung;

/// <summary>
/// Löscht die ältesten Sicherungen, wenn mehr als die eingestellte Höchstzahl vorhanden sind.
/// </summary>
public static class EntferneAlteSicherungen
{
    /// <summary>
    /// Behält die <paramref name="hoechstzahl"/> neuesten Sicherungen und löscht alle älteren.
    /// </summary>
    /// <remarks>
    /// Welche Dateien als Sicherung zählen und wie alt sie sind, bestimmt
    /// <see cref="ErmittleSicherungen"/> (genaues Namensmuster, Zeitstempel im Namen).
    /// Alle anderen Dateien im Sicherungsordner bleiben unberührt.
    /// </remarks>
    /// <param name="sicherungsordner">Der Sicherungsordner.</param>
    /// <param name="hoechstzahl">Höchstzahl der aufzubewahrenden Sicherungen (mindestens 1).</param>
    /// <returns>Die Anzahl der gelöschten Sicherungen.</returns>
    /// <exception cref="IOException">Eine Sicherung konnte nicht gelöscht werden.</exception>
    /// <exception cref="UnauthorizedAccessException">Keine Rechte zum Löschen.</exception>
    public static int Ausfuehren(string sicherungsordner, int hoechstzahl)
    {
        // Die Liste ist nach Zeitpunkt absteigend sortiert; alles nach den ersten „hoechstzahl“ Einträgen ist zu alt
        List<string> zuAlt = ErmittleSicherungen.Ausfuehren(sicherungsordner)
            .Skip(hoechstzahl)
            .Select(s => s.Datei)
            .ToList();

        foreach (string datei in zuAlt)
        {
            File.Delete(datei);
        }

        return zuAlt.Count;
    }
}
