using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ermittelt die Bestleistungen der Karteikarte „Aktuell“: Jahresrekorde und Einzelrekorde.
/// </summary>
/// <remarks>
/// Entspricht den Bestleistungen der Übersicht „Aktuell“ in BikeXperience (<c>core.Overview</c>).
/// Ausgewertet werden dieselben Einheiten wie im Diagramm (<see cref="FiltereUebersicht"/>).
/// </remarks>
public static class ErmittleBestleistungen
{
    /// <summary>
    /// Ermittelt die Bestleistungen.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><b>Jahresrekorde:</b> je Wert (km, Anzahl, hm, Fahrzeit) das Jahr mit der höchsten Jahressumme.</item>
    /// <item><b>Einzelrekorde:</b> je <see cref="Einzelrekordart"/> die Einheit mit dem höchsten Wert;
    /// die Ø-Geschwindigkeit über <see cref="BerechneDurchschnittsgeschwindigkeit"/>.</item>
    /// <item><b>Fehlende Werte:</b> 0 gilt wie in der Tabelle als „nicht erfasst“ und zählt nicht. Hat kein Jahr
    /// bzw. keine Einheit einen Wert, fehlt der Rekord.</item>
    /// <item><b>Gleichstand:</b> Der frühere hält den Rekord – das frühere Jahr bzw. die frühere Einheit
    /// (nach Datum, Startzeit und Nummer).</item>
    /// </list>
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="aktivitaetId">Gewählte Aktivität; <c>null</c> = alle Aktivitäten.</param>
    /// <param name="kategorie">Name der Kategorie aus der Filterleiste; <c>null</c> = alle.</param>
    /// <returns>Die Bestleistungen; ohne Einheiten zwei leere Listen.</returns>
    public static Bestleistungen Ausfuehren(Datenbestand bestand, long? aktivitaetId, string? kategorie)
    {
        // Früheste Einheit zuerst, damit bei Gleichstand die frühere gewinnt
        List<Trainingseinheit> einheiten = FiltereUebersicht.Ausfuehren(bestand, aktivitaetId, kategorie)
            .OrderBy(e => e.Datum)
            .ThenBy(e => e.Startzeit)
            .ThenBy(e => e.Id)
            .ToList();

        // Jahressummen sind bereits aufsteigend nach Jahr sortiert
        List<Jahressumme> summen = BerechneJahressummen.Ausfuehren(einheiten);
        List<Jahresrekord> jahresrekorde = Enum.GetValues<Fortschrittswert>()
            .Select(wert => Hoechster(summen, s => Jahreswert(s, wert)) is (Jahressumme summe, double wertSumme)
                ? new Jahresrekord(wert, summe.Jahr, wertSumme)
                : null)
            .OfType<Jahresrekord>()
            .ToList();

        List<Einzelrekord> einzelrekorde = Enum.GetValues<Einzelrekordart>()
            .Select(art => Hoechster(einheiten, e => Einzelwert(e, art)) is (Trainingseinheit einheit, double wert)
                ? new Einzelrekord(art, wert, einheit)
                : null)
            .OfType<Einzelrekord>()
            .ToList();

        return new Bestleistungen(jahresrekorde, einzelrekorde);
    }

    /// <summary>Die Jahressumme eines Werts; die Fahrzeit in Stunden.</summary>
    private static double Jahreswert(Jahressumme summe, Fortschrittswert wert) => wert switch
    {
        Fortschrittswert.Kilometer => summe.DistanzKm,
        Fortschrittswert.Anzahl => summe.Anzahl,
        Fortschrittswert.Hoehenmeter => summe.Hoehenmeter,
        _ => summe.Fahrzeit.TotalHours,
    };

    /// <summary>Der Wert einer Einheit für eine Rekordart; <c>null</c>, wenn er fehlt.</summary>
    private static double? Einzelwert(Trainingseinheit einheit, Einzelrekordart art) => art switch
    {
        Einzelrekordart.Distanz => einheit.DistanzKm,
        Einzelrekordart.Hoehenmeter => einheit.Hoehenmeter,
        Einzelrekordart.Fahrzeit => einheit.Fahrzeit.TotalHours,
        Einzelrekordart.Durchschnittsgeschwindigkeit => BerechneDurchschnittsgeschwindigkeit.Ausfuehren(einheit.DistanzKm, einheit.Fahrzeit),
        Einzelrekordart.Hoechstgeschwindigkeit => einheit.MaxGeschwindigkeit,
        _ => einheit.MaxHoehe,
    };

    /// <summary>
    /// Liefert das erste Element mit dem höchsten Wert größer 0, oder <c>null</c>, wenn keines einen Wert hat.
    /// </summary>
    /// <remarks>Nur ein echt größerer Wert ersetzt den bisherigen; so gewinnt bei Gleichstand das frühere Element.</remarks>
    private static (T Element, double Wert)? Hoechster<T>(IEnumerable<T> elemente, Func<T, double?> wert)
    {
        (T Element, double Wert)? bester = null;
        foreach (T element in elemente)
        {
            if (wert(element) is double w && w > 0 && (bester is null || w > bester.Value.Wert))
            {
                bester = (element, w);
            }
        }

        return bester;
    }
}
