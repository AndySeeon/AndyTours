using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Benennt eine Tätigkeit in den Wartungen im Speicher um.
/// </summary>
/// <remarks>
/// Gegenstück zum Ersetzen in der Datenbank: Es wird erst aufgerufen, nachdem die Datenbank erfolgreich
/// geändert wurde, damit Anzeige und Datenbank übereinstimmen.
/// </remarks>
public static class BenenneTaetigkeitUm
{
    /// <summary>
    /// Ersetzt in allen Wartungen mit der alten Tätigkeit (ohne Unterschied zwischen Groß- und Kleinschreibung,
    /// ohne Leerzeichen am Rand) die Tätigkeit durch die neue.
    /// </summary>
    /// <param name="wartungen">Alle Wartungen; sie werden direkt geändert.</param>
    /// <param name="alt">Bisherige Tätigkeit.</param>
    /// <param name="neu">Neue Tätigkeit.</param>
    /// <returns>Die Anzahl der geänderten Wartungen.</returns>
    public static int Ausfuehren(IEnumerable<Wartung> wartungen, string alt, string neu)
    {
        string gesucht = alt.Trim();
        int anzahl = 0;
        foreach (Wartung wartung in wartungen)
        {
            if (string.Equals(wartung.Taetigkeit.Trim(), gesucht, StringComparison.OrdinalIgnoreCase))
            {
                wartung.Taetigkeit = neu.Trim();
                anzahl++;
            }
        }

        return anzahl;
    }
}
