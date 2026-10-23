using System.Globalization;

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
    /// Als Sicherung zählt nur eine Datei, deren Name genau dem Muster
    /// <c>andytours_JJJJ-MM-TT_hh-mm-ss.db</c> entspricht (siehe <see cref="Sicherungsdatei"/>).
    /// Alle anderen Dateien – auch ähnlich benannte wie <c>andytours_alt.db</c> oder eine
    /// übrig gebliebene <c>.tmp</c>-Datei – bleiben unberührt. Das Alter ergibt sich aus dem
    /// Zeitstempel im Namen, nicht aus dem Änderungsdatum der Datei, denn das kann sich beim
    /// Kopieren des Ordners ändern.
    /// </remarks>
    /// <param name="sicherungsordner">Der Sicherungsordner.</param>
    /// <param name="hoechstzahl">Höchstzahl der aufzubewahrenden Sicherungen (mindestens 1).</param>
    /// <returns>Die Anzahl der gelöschten Sicherungen.</returns>
    /// <exception cref="IOException">Eine Sicherung konnte nicht gelöscht werden.</exception>
    /// <exception cref="UnauthorizedAccessException">Keine Rechte zum Löschen.</exception>
    public static int Ausfuehren(string sicherungsordner, int hoechstzahl)
    {
        var sicherungen = new List<(string Datei, DateTime Zeitpunkt)>();
        foreach (string datei in Directory.EnumerateFiles(sicherungsordner, Sicherungsdatei.Praefix + "*" + Sicherungsdatei.Endung))
        {
            // Zeitstempel zwischen Präfix und Endung herauslösen und streng nach dem Format prüfen
            string name = Path.GetFileName(datei);
            if (!name.EndsWith(Sicherungsdatei.Endung, StringComparison.OrdinalIgnoreCase))
            {
                continue; // EnumerateFiles findet bei „*.db“ unter Windows z. B. auch „*.dbx“
            }

            string zeitstempel = name[Sicherungsdatei.Praefix.Length..^Sicherungsdatei.Endung.Length];
            if (DateTime.TryParseExact(zeitstempel, Sicherungsdatei.Zeitstempelformat, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime zeitpunkt))
            {
                sicherungen.Add((datei, zeitpunkt));
            }
        }

        // Neueste zuerst; alles nach den ersten „hoechstzahl“ Einträgen ist zu alt
        List<string> zuAlt = sicherungen
            .OrderByDescending(s => s.Zeitpunkt)
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
