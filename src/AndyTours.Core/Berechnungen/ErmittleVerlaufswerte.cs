using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ermittelt, welche Werte der Verlauf einer Tour enthält.
/// </summary>
/// <remarks>
/// Danach richtet sich das Tourdetail: Ohne Höhendaten erscheint statt des Diagramms ein Hinweis, und nur die
/// vorhandenen Zusatzwerte lassen sich als Linie einblenden.
/// </remarks>
public static class ErmittleVerlaufswerte
{
    /// <summary>
    /// Prüft den Verlauf auf Höhendaten und Zusatzwerte.
    /// </summary>
    /// <remarks>
    /// Ein Wert gilt als vorhanden, wenn mindestens ein Messpunkt ihn enthält. Eine Höhe von genau 0 an allen
    /// Punkten zählt nicht als Höhendaten, weil manche Geräte dann nur einen Platzhalter schreiben.
    /// </remarks>
    /// <param name="verlauf">Die Messpunkte der Tour.</param>
    /// <returns>Ob Höhendaten vorhanden sind, und die vorhandenen Zusatzwerte in fester Reihenfolge.</returns>
    public static (bool HatHoehe, IReadOnlyList<Verlaufswert> Zusatzwerte) Ausfuehren(IReadOnlyList<Verlaufspunkt> verlauf)
    {
        bool hatHoehe = verlauf.Any(p => p.Hoehe is double hoehe && hoehe != 0);
        List<Verlaufswert> zusatzwerte = Enum.GetValues<Verlaufswert>()
            .Where(w => verlauf.Any(p => p.Wert(w) is not null))
            .ToList();
        return (hatHoehe, zusatzwerte);
    }
}
