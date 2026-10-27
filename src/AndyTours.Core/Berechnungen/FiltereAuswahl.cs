using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Wählt die Trainingseinheiten aus, die zur Jahres- und Aktivitätsauswahl im Hauptfenster passen.
/// </summary>
public static class FiltereAuswahl
{
    /// <summary>
    /// Filtert die Einheiten nach Jahr und Aktivität.
    /// </summary>
    /// <remarks>
    /// <c>null</c> bedeutet „keine Einschränkung“: <paramref name="jahr"/> = <c>null</c> entspricht
    /// dem Eintrag „alle Jahre“, <paramref name="aktivitaetId"/> = <c>null</c> dem Eintrag
    /// „alle Aktivitäten“.
    /// </remarks>
    /// <param name="einheiten">Alle Trainingseinheiten.</param>
    /// <param name="jahr">Gewähltes Kalenderjahr oder <c>null</c> für alle Jahre.</param>
    /// <param name="aktivitaetId">Nummer der gewählten Aktivität oder <c>null</c> für alle Aktivitäten.</param>
    /// <returns>Die passenden Einheiten in der ursprünglichen Reihenfolge.</returns>
    public static List<Trainingseinheit> Ausfuehren(IEnumerable<Trainingseinheit> einheiten, int? jahr, long? aktivitaetId)
    {
        return einheiten
            .Where(einheit => jahr is null || einheit.Datum.Year == jahr)
            .Where(einheit => aktivitaetId is null || einheit.AktivitaetId == aktivitaetId)
            .ToList();
    }
}
