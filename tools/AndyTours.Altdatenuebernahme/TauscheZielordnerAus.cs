namespace AndyTours.Altdatenuebernahme;

/// <summary>
/// Ersetzt den Zielordner durch den fertigen Zwischenordner.
/// </summary>
public static class TauscheZielordnerAus
{
    /// <summary>
    /// Tauscht den Zielordner gegen den Zwischenordner aus – durch Umbenennen, nicht durch Kopieren.
    /// </summary>
    /// <remarks>
    /// Schritte, wenn der Zielordner existiert:
    /// <list type="number">
    /// <item>Zielordner in <c>.&lt;Name&gt;.alt-&lt;Kennung&gt;</c> umbenennen. Scheitert das (z. B. weil eine
    /// Datei darin geöffnet ist), bleibt alles wie es war.</item>
    /// <item>Zwischenordner in den Zielnamen umbenennen. Scheitert das, wird der alte Ordner
    /// zurückbenannt.</item>
    /// <item>Den alten Ordner löschen. Danach enthält der Zielordner nur noch die neuen Daten.</item>
    /// </list>
    /// Existiert der Zielordner nicht, genügt Schritt 2. In jedem Fehlerfall wird der Zwischenordner
    /// entfernt und der Fehler weitergegeben; der Zielordner ist dann unverändert.
    /// </remarks>
    /// <param name="zwischenordner">Der fertige Zwischenordner (siehe <see cref="ErstelleZwischenordner"/>).</param>
    /// <param name="zielordner">Der zu ersetzende bzw. neu anzulegende Zielordner.</param>
    /// <returns>Ein Hinweis, falls der alte Ordner nicht gelöscht werden konnte, sonst <c>null</c>.</returns>
    public static string? Ausfuehren(string zwischenordner, string zielordner)
    {
        string ziel = Path.TrimEndingDirectorySeparator(zielordner);

        if (!Directory.Exists(ziel))
        {
            Verschiebe(zwischenordner, ziel, zwischenordner);
            return null;
        }

        string alt = Path.Combine(Path.GetDirectoryName(ziel)!, $".{Path.GetFileName(ziel)}.alt-{Guid.NewGuid():N}");

        // 1. Alten Zielordner beiseitelegen
        Verschiebe(ziel, alt, zwischenordner);

        // 2. Neuen Ordner an seine Stelle setzen; bei Fehler den alten zurückholen
        try
        {
            Directory.Move(zwischenordner, ziel);
        }
        catch
        {
            Directory.Move(alt, ziel);
            LoescheStill(zwischenordner);
            throw;
        }

        // 3. Alten Ordner entfernen
        try
        {
            Directory.Delete(alt, recursive: true);
            return null;
        }
        catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException)
        {
            return $"Der alte Datenbestand liegt noch in {alt} und kann von Hand gelöscht werden ({fehler.Message}).";
        }
    }

    /// <summary>Benennt einen Ordner um; bei einem Fehler wird der Zwischenordner entfernt und der Fehler weitergegeben.</summary>
    private static void Verschiebe(string von, string nach, string zwischenordner)
    {
        try
        {
            Directory.Move(von, nach);
        }
        catch
        {
            LoescheStill(zwischenordner);
            throw;
        }
    }

    /// <summary>Löscht einen Ordner samt Inhalt und ignoriert dabei Fehler.</summary>
    private static void LoescheStill(string ordner)
    {
        try
        {
            if (Directory.Exists(ordner))
            {
                Directory.Delete(ordner, recursive: true);
            }
        }
        catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException)
        {
            // Rest wird beim nächsten Lauf von ErstelleZwischenordner entfernt
        }
    }
}
