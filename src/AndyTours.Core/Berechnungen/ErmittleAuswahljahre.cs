using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ermittelt die Jahre für die Jahresauswahl im Hauptfenster.
/// </summary>
public static class ErmittleAuswahljahre
{
    /// <summary>
    /// Liefert alle Jahre, in denen es Trainingseinheiten gibt, und zusätzlich immer das aktuelle Jahr.
    /// </summary>
    /// <remarks>
    /// Das aktuelle Jahr steht auch dann in der Liste, wenn es noch keine Einheiten hat (z. B. am
    /// 1. Januar oder bei einem leeren Datenordner), damit es beim Start vorausgewählt werden kann.
    /// Es wird als Parameter übergeben statt intern das Datum zu lesen – so sind die Tests
    /// unabhängig vom Tag, an dem sie laufen.
    /// </remarks>
    /// <param name="einheiten">Alle Trainingseinheiten des Datenbestands.</param>
    /// <param name="aktuellesJahr">Das aktuelle Kalenderjahr.</param>
    /// <returns>Die Jahre ohne Doppelte, das neueste zuerst.</returns>
    public static List<int> Ausfuehren(IEnumerable<Trainingseinheit> einheiten, int aktuellesJahr)
    {
        return einheiten
            .Select(einheit => einheit.Datum.Year)
            // Das aktuelle Jahr immer dazunehmen; Distinct entfernt es wieder, falls es schon vorkommt
            .Append(aktuellesJahr)
            .Distinct()
            .OrderDescending()
            .ToList();
    }
}
