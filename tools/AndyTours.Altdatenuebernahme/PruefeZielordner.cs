using AndyTours.Daten.Datenordner;

namespace AndyTours.Altdatenuebernahme;

/// <summary>
/// Stellt fest, ob der Zielordner frei ist, schon AndyTours-Daten oder fremde Dateien enthält.
/// </summary>
public static class PruefeZielordner
{
    /// <summary>
    /// Ermittelt den <see cref="ZielordnerZustand"/>.
    /// </summary>
    /// <remarks>
    /// „AndyTours-Daten“ bedeutet wie in der App: Die Datenbankdatei liegt im Ordner.
    /// Fremde Dateien ohne Datenbank führen zum Abbruch, damit beim Ersetzen nichts gelöscht wird,
    /// was nicht zu AndyTours gehört (z. B. bei einem versehentlich angegebenen Ordner).
    /// </remarks>
    /// <param name="zielordner">Vollständiger Pfad des Zielordners.</param>
    /// <returns>Der Zustand des Ordners.</returns>
    public static ZielordnerZustand Ausfuehren(string zielordner)
    {
        if (!Directory.Exists(zielordner))
        {
            return ZielordnerZustand.FehltOderLeer;
        }

        if (File.Exists(Path.Combine(zielordner, DatenordnerInhalt.Datenbankdatei)))
        {
            return ZielordnerZustand.MitDaten;
        }

        return Directory.EnumerateFileSystemEntries(zielordner).Any()
            ? ZielordnerZustand.FremderInhalt
            : ZielordnerZustand.FehltOderLeer;
    }
}
