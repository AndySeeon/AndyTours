namespace AndyTours.Daten.Datenordner;

/// <summary>
/// Liest den zuletzt gewählten Datenordner aus der Merkdatei.
/// </summary>
public static class LeseGemerktenDatenordner
{
    /// <summary>
    /// Liefert den in der Merkdatei gespeicherten Ordnerpfad.
    /// </summary>
    /// <remarks>
    /// Ob der Ordner noch existiert, wird hier bewusst <b>nicht</b> geprüft: Ein gemerkter, aber
    /// gerade nicht erreichbarer Ordner (z. B. NAS ausgeschaltet) soll beim Start als solcher
    /// gemeldet werden, statt stillschweigend auf den Standardordner auszuweichen.
    /// </remarks>
    /// <param name="merkdatei">Vollständiger Pfad der Merkdatei.</param>
    /// <returns>Der gemerkte Pfad oder <c>null</c>, wenn noch keiner gemerkt wurde oder die Datei nicht lesbar ist.</returns>
    public static string? Ausfuehren(string merkdatei)
    {
        if (!File.Exists(merkdatei))
        {
            return null;
        }

        string ordner;
        try
        {
            ordner = File.ReadAllText(merkdatei).Trim();
        }
        catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException)
        {
            // Eine unlesbare Merkdatei behandeln wir wie eine fehlende: dann gilt der Standardordner.
            return null;
        }

        return ordner.Length > 0 ? ordner : null;
    }
}
