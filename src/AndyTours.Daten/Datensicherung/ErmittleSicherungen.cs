using System.Globalization;

namespace AndyTours.Daten.Datensicherung;

/// <summary>
/// Ermittelt alle Sicherungen in einem Sicherungsordner.
/// </summary>
public static class ErmittleSicherungen
{
    /// <summary>
    /// Liefert alle Sicherungen des Ordners, die neueste zuerst.
    /// </summary>
    /// <remarks>
    /// Als Sicherung zählt nur eine Datei, deren Name genau dem Muster
    /// <c>andytours_JJJJ-MM-TT_hh-mm-ss.db</c> entspricht (siehe <see cref="Sicherungsdatei"/>).
    /// Alle anderen Dateien – auch ähnlich benannte wie <c>andytours_alt.db</c> oder eine
    /// übrig gebliebene <c>.tmp</c>-Datei – werden übergangen. Der Zeitpunkt ergibt sich aus dem
    /// Zeitstempel im Namen, nicht aus dem Änderungsdatum der Datei, denn das kann sich beim
    /// Kopieren des Ordners ändern.
    /// <para>
    /// Dieselbe Regel gilt für die Auswahlliste beim Wiederherstellen und für das Aufräumen
    /// (<see cref="EntferneAlteSicherungen"/>).
    /// </para>
    /// </remarks>
    /// <param name="sicherungsordner">Der Sicherungsordner.</param>
    /// <returns>Die Sicherungen, neueste zuerst; leer, wenn der Ordner nicht existiert.</returns>
    /// <exception cref="IOException">Der Ordner konnte nicht gelesen werden (z. B. Netzwerkfehler).</exception>
    /// <exception cref="UnauthorizedAccessException">Keine Leserechte für den Ordner.</exception>
    public static List<Sicherungseintrag> Ausfuehren(string sicherungsordner)
    {
        var sicherungen = new List<Sicherungseintrag>();
        if (!Directory.Exists(sicherungsordner))
        {
            return sicherungen; // noch nie gesichert oder Ordner nicht erreichbar
        }

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
                sicherungen.Add(new Sicherungseintrag(datei, zeitpunkt));
            }
        }

        return sicherungen.OrderByDescending(s => s.Zeitpunkt).ToList();
    }
}
