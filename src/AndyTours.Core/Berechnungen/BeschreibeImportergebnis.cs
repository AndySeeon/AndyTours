namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Erzeugt die Texte, die nach einem Import erscheinen.
/// </summary>
public static class BeschreibeImportergebnis
{
    /// <summary>
    /// Liefert die Statusmeldung und gegebenenfalls die Meldung zu übersprungenen Aufzeichnungen.
    /// </summary>
    /// <param name="anzahl">Anzahl der importierten Touren.</param>
    /// <param name="uebersprungen">Nicht lesbare Aufzeichnungen, je Eintrag Dateiname und Grund.</param>
    /// <returns>
    /// Statustext, z. B. „3 Touren importiert.“, und die Meldung mit den übersprungenen Aufzeichnungen
    /// oder <c>null</c>, wenn alle gelesen werden konnten.
    /// </returns>
    public static (string Statustext, string? Meldung) Ausfuehren(int anzahl, IReadOnlyList<string> uebersprungen)
    {
        string statustext = anzahl == 1 ? "1 Tour importiert." : $"{anzahl} Touren importiert.";
        string? meldung = uebersprungen.Count == 0
            ? null
            : "Diese Aufzeichnungen konnten nicht gelesen werden und wurden übersprungen:\n\n" + string.Join("\n", uebersprungen);
        return (statustext, meldung);
    }
}
