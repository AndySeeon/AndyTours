using System.Globalization;
using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Erstellt die Kopfzeile des Tourdetails mehrerer Einheiten (Mehrtagestour).
/// </summary>
public static class BeschreibeMehrtagestour
{
    /// <summary>Deutsche Kultur für Wochentag, Datum und Zahlen.</summary>
    private static readonly CultureInfo Deutsch = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>
    /// Liefert Zeitraum, Anzahl der Etappen, Summen, Durchschnittsgeschwindigkeit und höchsten Punkt in einer Zeile.
    /// </summary>
    /// <remarks>
    /// Beispiel: „Sa, 12.07.2025 – Fr, 18.07.2025 · 7 Etappen · 612,40 km · 8.230 hm · 31:12:05 h · Ø 19,6 km/h ·
    /// höchster Punkt 2.504 m“. Die Werte stammen aus den Einheiten, auch aus denen ohne Aufzeichnung. Der
    /// Durchschnitt ist die gesamte Distanz durch die gesamte Fahrzeit; ohne Fahrzeit entfällt er. Liegen alle
    /// Etappen am selben Tag, steht nur ein Datum; ein höchster Punkt von 0 gilt als „nicht erfasst“ und entfällt.
    /// </remarks>
    /// <param name="einheiten">Die Einheiten der Mehrtagestour; mindestens eine.</param>
    /// <returns>Die Kopfzeile.</returns>
    public static string Ausfuehren(IReadOnlyList<Trainingseinheit> einheiten)
    {
        DateOnly erster = einheiten.Min(e => e.Datum);
        DateOnly letzter = einheiten.Max(e => e.Datum);
        string zeitraum = erster == letzter ? Datum(erster) : $"{Datum(erster)} – {Datum(letzter)}";

        double distanz = einheiten.Sum(e => e.DistanzKm);
        TimeSpan fahrzeit = einheiten.Aggregate(TimeSpan.Zero, (summe, e) => summe + e.Fahrzeit);
        int hoechsterPunkt = einheiten.Max(e => e.MaxHoehe);

        List<string> teile =
        [
            zeitraum,
            einheiten.Count == 1 ? "1 Etappe" : $"{einheiten.Count} Etappen",
            string.Format(Deutsch, "{0:N2} km", distanz),
            string.Format(Deutsch, "{0:N0} hm", einheiten.Sum(e => e.Hoehenmeter)),
            FormatiereDauer.Ausfuehren(fahrzeit) + " h",
        ];
        if (BerechneDurchschnittsgeschwindigkeit.Ausfuehren(distanz, fahrzeit) is double schnitt)
        {
            teile.Add(string.Format(Deutsch, "Ø {0:N1} km/h", schnitt));
        }

        if (hoechsterPunkt > 0)
        {
            teile.Add(string.Format(Deutsch, "höchster Punkt {0:N0} m", hoechsterPunkt));
        }

        return string.Join(" · ", teile);
    }

    /// <summary>Datum mit Wochentag, z. B. „Sa, 12.07.2025“.</summary>
    private static string Datum(DateOnly datum) => datum.ToString("ddd, dd.MM.yyyy", Deutsch);
}
