using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Prüft, ob eine Einheit zum Suchbegriff der Filterleiste passt.
/// </summary>
public static class PasstZumSuchbegriff
{
    /// <summary>
    /// Liefert <c>true</c>, wenn jedes Wort des Suchbegriffs im Namen oder im Kommentar der Einheit vorkommt.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Der Suchbegriff wird an Leerzeichen in Wörter zerlegt; die Reihenfolge der Wörter spielt keine
    /// Rolle. So findet „Sabine Alm“ eine Einheit „Bründlingalm“ mit dem Kommentar „mit Sabine“.</item>
    /// <item>Jedes Wort darf im Namen oder im Kommentar stehen, auch als Teil eines längeren Worts.</item>
    /// <item>Groß- und Kleinschreibung zählt nicht.</item>
    /// <item>Ein leerer Suchbegriff oder einer nur aus Leerzeichen hat keine Wörter; dann passt jede Einheit.</item>
    /// </list>
    /// </remarks>
    /// <param name="einheit">Die zu prüfende Einheit.</param>
    /// <param name="suchbegriff">Die Eingabe im Suchfeld.</param>
    /// <returns><c>true</c>, wenn die Einheit passt.</returns>
    public static bool Ausfuehren(Trainingseinheit einheit, string suchbegriff)
    {
        string[] woerter = suchbegriff.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Jedes Wort muss in Name oder Kommentar vorkommen
        return woerter.All(wort =>
            einheit.Name.Contains(wort, StringComparison.OrdinalIgnoreCase)
            || einheit.Kommentar.Contains(wort, StringComparison.OrdinalIgnoreCase));
    }
}
