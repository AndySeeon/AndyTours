using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ermittelt die Daten der Karteikarte „Aktuell“: Jahressummen aller Jahre mit ihren Saisonzielen und die
/// Kennzahlen des laufenden Jahres.
/// </summary>
/// <remarks>
/// Entspricht der Übersicht „Aktuell“ in BikeXperience (<c>core.Overview</c>). Berücksichtigt werden nur
/// die Aktivität der Werkzeugleiste und die Kategorie der Filterleiste; die Jahresauswahl und die übrigen
/// Filter wirken hier nicht, weil das Diagramm immer alle Jahre zeigt.
/// </remarks>
public static class ErmittleJahresfortschritt
{
    /// <summary>
    /// Ermittelt den Jahresfortschritt für einen Wert.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><b>Jahre:</b> alle Jahre mit Einheiten in der Auswahl; das laufende Jahr ist immer dabei
    /// (auch mit Summe 0), sobald es überhaupt Einheiten gibt.</item>
    /// <item><b>Ziele:</b> je Jahr die Summe der wirksamen Saisonziele (<see cref="ErmittleSaisonziel"/>)
    /// der gewählten Aktivität bzw. aller Aktivitäten. Eine Summe von 0 heißt „kein Ziel“; für die Fahrzeit
    /// gibt es nie ein Ziel.</item>
    /// <item><b>Soll-Stand:</b> Ziel × Tag des Jahres ÷ Tage des Jahres (am 1. Januar also 1/365 des Ziels,
    /// am 31. Dezember das ganze Ziel).</item>
    /// <item><b>Wochenleistung:</b> was zum Ziel fehlt ÷ verbleibende Wochen; die verbleibenden Wochen sind
    /// die restlichen Tage einschließlich heute geteilt durch 7.</item>
    /// </list>
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="aktivitaetId">Gewählte Aktivität; <c>null</c> = alle Aktivitäten.</param>
    /// <param name="kategorie">Name der Kategorie aus der Filterleiste; <c>null</c> = alle.</param>
    /// <param name="wert">Der gezeigte Wert.</param>
    /// <param name="heute">Das heutige Datum (bestimmt das laufende Jahr und den Soll-Stand).</param>
    /// <returns>Jahrespunkte und Kennzahlen.</returns>
    public static Jahresfortschritt Ausfuehren(Datenbestand bestand, long? aktivitaetId, string? kategorie, Fortschrittswert wert, DateOnly heute)
    {
        // 1. Auswahl: Aktivität aus der Werkzeugleiste (alle Jahre), dann nur die Kategorie aus der Filterleiste
        List<Trainingseinheit> auswahl = FiltereAuswahl.Ausfuehren(bestand.Trainingseinheiten, null, aktivitaetId);
        auswahl = FiltereEinheiten.Ausfuehren(auswahl, Einheitenfilter.Leer with { Kategorie = kategorie }, bestand);

        // 2. Jahressummen; das laufende Jahr fehlt dort, solange es darin noch keine Einheit gibt
        Dictionary<int, double> summen = BerechneJahressummen.Ausfuehren(auswahl).ToDictionary(s => s.Jahr, s => Summe(s, wert));
        if (summen.Count > 0)
        {
            summen.TryAdd(heute.Year, 0);
        }

        // 3. Ziele der betroffenen Aktivitäten je Jahr
        List<Aktivitaet> aktivitaeten = bestand.Aktivitaeten.Where(a => aktivitaetId is null || a.Id == aktivitaetId).ToList();
        List<Jahrespunkt> punkte = summen
            .OrderBy(s => s.Key)
            .Select(s => new Jahrespunkt(s.Key, s.Value, Ziel(aktivitaeten, bestand.Saisonziele, s.Key, wert)))
            .ToList();

        return new Jahresfortschritt(wert, punkte, Kennzahlen(punkte, heute));
    }

    /// <summary>Die Jahressumme des gewählten Werts; die Fahrzeit in Stunden.</summary>
    private static double Summe(Jahressumme summe, Fortschrittswert wert) => wert switch
    {
        Fortschrittswert.Kilometer => summe.DistanzKm,
        Fortschrittswert.Anzahl => summe.Anzahl,
        Fortschrittswert.Hoehenmeter => summe.Hoehenmeter,
        _ => summe.Fahrzeit.TotalHours,
    };

    /// <summary>Summe der wirksamen Ziele der Aktivitäten in einem Jahr; <c>null</c> bei 0 und für die Fahrzeit.</summary>
    private static double? Ziel(List<Aktivitaet> aktivitaeten, List<Saisonziel> ziele, int jahr, Fortschrittswert wert)
    {
        if (wert == Fortschrittswert.Fahrzeit)
        {
            return null;
        }

        int summe = aktivitaeten.Sum(a =>
        {
            WirksamesSaisonziel ziel = ErmittleSaisonziel.Ausfuehren(a, ziele, jahr);
            return wert switch
            {
                Fortschrittswert.Kilometer => ziel.ZielKm,
                Fortschrittswert.Anzahl => ziel.ZielAnzahl,
                _ => ziel.ZielHm,
            };
        });
        return summe > 0 ? summe : null;
    }

    /// <summary>Berechnet die Kennzahlen des laufenden Jahres aus seinem Jahrespunkt.</summary>
    private static Jahreskennzahlen Kennzahlen(List<Jahrespunkt> punkte, DateOnly heute)
    {
        Jahrespunkt? punkt = punkte.FirstOrDefault(p => p.Jahr == heute.Year);
        double stand = punkt?.Summe ?? 0;
        if (punkt?.Ziel is not double ziel)
        {
            return new Jahreskennzahlen(heute.Year, stand, null, null, null, null, null, null);
        }

        // Soll-Stand: Anteil der verstrichenen Tage einschließlich heute
        int tageImJahr = DateTime.IsLeapYear(heute.Year) ? 366 : 365;
        double soll = ziel * heute.DayOfYear / tageImJahr;

        // Wochenleistung: restliche Tage einschließlich heute, in Wochen
        double rest = Math.Max(0, ziel - stand);
        double restWochen = (tageImJahr - heute.DayOfYear + 1) / 7.0;
        double? proWoche = rest > 0 ? rest / restWochen : null;

        return new Jahreskennzahlen(heute.Year, stand, ziel, stand / ziel, soll, stand - soll, rest, proWoche);
    }
}
