using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Benennt einen Trainingsbereich in den Einheiten im Speicher um.
/// </summary>
/// <remarks>
/// Gegenstück zum Ersetzen in der Datenbank: Es wird erst aufgerufen, nachdem die Datenbank erfolgreich
/// geändert wurde, damit Anzeige und Datenbank übereinstimmen.
/// </remarks>
public static class BenenneTrainingsbereichUm
{
    /// <summary>
    /// Ersetzt in allen Einheiten mit dem alten Bereich (ohne Unterschied zwischen Groß- und Kleinschreibung,
    /// ohne Leerzeichen am Rand) den Bereich durch den neuen.
    /// </summary>
    /// <param name="einheiten">Alle Trainingseinheiten; sie werden direkt geändert.</param>
    /// <param name="alt">Bisheriges Kürzel.</param>
    /// <param name="neu">Neues Kürzel.</param>
    /// <returns>Die Anzahl der geänderten Einheiten.</returns>
    public static int Ausfuehren(IEnumerable<Trainingseinheit> einheiten, string alt, string neu)
    {
        string gesucht = alt.Trim();
        int anzahl = 0;
        foreach (Trainingseinheit einheit in einheiten)
        {
            if (string.Equals(einheit.Trainingsbereich.Trim(), gesucht, StringComparison.OrdinalIgnoreCase))
            {
                einheit.Trainingsbereich = neu;
                anzahl++;
            }
        }

        return anzahl;
    }
}
