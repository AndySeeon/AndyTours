namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Prüft einen neuen oder umbenannten Eintrag einer Auswahlliste.
/// </summary>
public static class PruefeAuswahleintrag
{
    /// <summary>
    /// Prüft, ob der Eintrag in die Liste aufgenommen werden darf.
    /// </summary>
    /// <remarks>
    /// Leerzeichen am Anfang und Ende zählen nicht. Abgewiesen wird ein leerer Eintrag und einer, den es
    /// in der Liste schon gibt – ohne Unterschied zwischen Groß- und Kleinschreibung. Beim Umbenennen
    /// zählt der umbenannte Eintrag selbst nicht mit, sodass sich z. B. „ga1“ in „GA1“ ändern lässt.
    /// </remarks>
    /// <param name="liste">Die bisherigen Einträge.</param>
    /// <param name="text">Der eingegebene Text.</param>
    /// <param name="umbenannterIndex">Beim Umbenennen die Stelle des umbenannten Eintrags, sonst <c>null</c>.</param>
    /// <returns>Der Hinweis mit dem Grund der Ablehnung oder <c>null</c>, wenn der Eintrag gültig ist.</returns>
    public static string? Ausfuehren(IReadOnlyList<string> liste, string? text, int? umbenannterIndex = null)
    {
        string eintrag = (text ?? string.Empty).Trim();
        if (eintrag.Length == 0)
        {
            return "Bitte einen Eintrag eingeben.";
        }

        for (int i = 0; i < liste.Count; i++)
        {
            if (i != umbenannterIndex && string.Equals(liste[i], eintrag, StringComparison.OrdinalIgnoreCase))
            {
                return $"‚{liste[i]}‘ steht bereits in der Liste.";
            }
        }

        return null;
    }
}
