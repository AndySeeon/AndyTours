namespace AndyTours.Import.Altdaten;

/// <summary>
/// Stellt fest, welche bisherigen Daten in einem Ordner liegen (Spezifikation 033).
/// </summary>
public static class ErkenneAltdaten
{
    /// <summary>Stammdaten-Datei von BikeXperience.</summary>
    public const string BikexperienceIni = "bikexperience.ini";

    /// <summary>Datei mit den Trainingseinheiten von BikeXperience.</summary>
    public const string WorkoutsCsv = "workouts.csv";

    /// <summary>Endung der Aufzeichnungen im FIT-Format.</summary>
    public const string FitEndung = ".fit";

    /// <summary>
    /// Prüft den Ordner und liefert die <see cref="Altdatenart"/> samt gefundener Aufzeichnungen.
    /// </summary>
    /// <remarks>
    /// Reihenfolge der Prüfung:
    /// <list type="number">
    /// <item>Liegen <c>bikexperience.ini</c> und <c>workouts.csv</c> direkt im Ordner, ist es ein BikeXperience-Ordner.
    /// Seine Aufzeichnungen übernimmt die Umwandlung selbst; sie werden hier nicht aufgelistet.</item>
    /// <item>Sonst werden alle Dateien mit der Endung <c>.fit</c> gesucht, auch in Unterordnern
    /// (z. B. Jahresordner oder der Ordner <c>Activity</c> eines Garmin-Geräts).</item>
    /// <item>Gibt es keine, liegt nichts Übernehmbares im Ordner.</item>
    /// </list>
    /// Der Ordner wird nur gelesen. Ein fehlender Ordner zählt wie ein leerer. Unterordner ohne Leserecht
    /// werden übersprungen, damit ein einzelner geschützter Ordner die Suche nicht abbricht.
    /// </remarks>
    /// <param name="ordner">Der vom Benutzer angegebene Ordner.</param>
    /// <returns>Art und Aufzeichnungen.</returns>
    public static Altdatenfund Ausfuehren(string ordner)
    {
        if (!Directory.Exists(ordner))
        {
            return new Altdatenfund(Altdatenart.Nichts, []);
        }

        if (File.Exists(Path.Combine(ordner, BikexperienceIni)) && File.Exists(Path.Combine(ordner, WorkoutsCsv)))
        {
            return new Altdatenfund(Altdatenart.BikeXperience, []);
        }

        var optionen = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            MatchCasing = MatchCasing.CaseInsensitive,
        };
        List<string> aufzeichnungen = Directory.EnumerateFiles(ordner, "*" + FitEndung, optionen)
            // „*.fit“ fände unter Windows auch Endungen wie „.fitx“ (kurze 8.3-Namen); deshalb genau prüfen
            .Where(d => string.Equals(Path.GetExtension(d), FitEndung, StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return aufzeichnungen.Count > 0
            ? new Altdatenfund(Altdatenart.NurAufzeichnungen, aufzeichnungen)
            : new Altdatenfund(Altdatenart.Nichts, []);
    }
}
