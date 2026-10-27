using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Zählt die Einheiten, die einen Trainingsbereich verwenden.
/// </summary>
public static class ZaehleTrainingsbereich
{
    /// <summary>
    /// Liefert die Anzahl der Einheiten mit diesem Trainingsbereich – ohne Unterschied zwischen Groß- und
    /// Kleinschreibung und ohne Leerzeichen am Rand, so wie Einträge der Liste verglichen werden.
    /// </summary>
    /// <param name="einheiten">Alle Trainingseinheiten.</param>
    /// <param name="bereich">Das Kürzel, z. B. „GA2“.</param>
    /// <returns>Die Anzahl (0, wenn der Bereich nicht verwendet wird).</returns>
    public static int Ausfuehren(IEnumerable<Trainingseinheit> einheiten, string bereich)
    {
        string gesucht = bereich.Trim();
        return einheiten.Count(e => string.Equals(e.Trainingsbereich.Trim(), gesucht, StringComparison.OrdinalIgnoreCase));
    }
}
