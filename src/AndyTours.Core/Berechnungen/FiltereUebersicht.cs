using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Wählt die Einheiten für die Karteikarte „Aktuell“ aus (Jahresfortschritt und Bestleistungen).
/// </summary>
/// <remarks>
/// Es wirken nur die Aktivität der Werkzeugleiste und die Kategorie der Filterleiste. Die Jahresauswahl und
/// die übrigen Filter bleiben unberücksichtigt, weil die Übersicht immer alle Jahre betrachtet. Diagramm und
/// Bestleistungen verwenden diese eine Funktion, damit sie sicher dieselben Einheiten auswerten.
/// </remarks>
public static class FiltereUebersicht
{
    /// <summary>
    /// Liefert die Einheiten der Übersicht.
    /// </summary>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="aktivitaetId">Gewählte Aktivität; <c>null</c> = alle Aktivitäten.</param>
    /// <param name="kategorie">Name der Kategorie aus der Filterleiste; <c>null</c> = alle.</param>
    /// <returns>Die passenden Einheiten aller Jahre.</returns>
    public static List<Trainingseinheit> Ausfuehren(Datenbestand bestand, long? aktivitaetId, string? kategorie)
    {
        List<Trainingseinheit> auswahl = FiltereAuswahl.Ausfuehren(bestand.Trainingseinheiten, null, aktivitaetId);
        return FiltereEinheiten.Ausfuehren(auswahl, Einheitenfilter.Leer with { Kategorie = kategorie }, bestand);
    }
}
