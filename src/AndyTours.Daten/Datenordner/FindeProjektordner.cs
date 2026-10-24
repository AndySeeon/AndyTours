namespace AndyTours.Daten.Datenordner;

/// <summary>
/// Findet den Projektordner von AndyTours (den Ordner mit der Projektmappe).
/// </summary>
public static class FindeProjektordner
{
    /// <summary>
    /// Sucht ab dem Startordner aufwärts den ersten Ordner, der die Projektmappe enthält.
    /// </summary>
    /// <remarks>
    /// Während der Entwicklung liegt das Programm tief im Projekt, z. B. in
    /// <c>src\AndyTours.App\bin\Debug\net10.0-windows10.0.19041.0</c>. Von dort führt der Weg nach oben
    /// zuverlässig zum Projektordner – unabhängig davon, wo das Projekt auf dem Rechner liegt.
    /// </remarks>
    /// <param name="startordner">Ordner, in dem die Suche beginnt (in der App: der Programmordner).</param>
    /// <returns>Der Projektordner oder <c>null</c>, wenn bis zum Laufwerk keine Projektmappe gefunden wurde.</returns>
    public static string? Ausfuehren(string startordner)
    {
        DirectoryInfo? ordner = new DirectoryInfo(startordner);

        // Schritt für Schritt eine Ebene höher, bis die Projektmappe auftaucht oder das Laufwerk erreicht ist
        while (ordner is not null)
        {
            if (File.Exists(Path.Combine(ordner.FullName, DatenordnerInhalt.Projektmappe)))
            {
                return ordner.FullName;
            }

            ordner = ordner.Parent;
        }

        return null;
    }
}
