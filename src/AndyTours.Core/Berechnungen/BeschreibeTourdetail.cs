using System.Globalization;
using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Erstellt die Kopfzeile des Fensters „Tourdetail“.
/// </summary>
public static class BeschreibeTourdetail
{
    /// <summary>Deutsche Kultur für Wochentag, Datum und Zahlen.</summary>
    private static readonly CultureInfo Deutsch = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>
    /// Liefert Datum, Name, Distanz, Höhenmeter und höchsten Punkt der Einheit in einer Zeile.
    /// </summary>
    /// <remarks>
    /// Beispiel: „Sa, 26.09.2026 · Alpenrunde · 90,83 km · 1.206 hm · höchster Punkt 1.379 m“. Ein leerer Name
    /// entfällt samt Trenner; ein höchster Punkt von 0 gilt als „nicht erfasst“ und entfällt ebenfalls.
    /// </remarks>
    /// <param name="einheit">Die Einheit.</param>
    /// <returns>Die Kopfzeile.</returns>
    public static string Ausfuehren(Trainingseinheit einheit)
    {
        List<string> teile = [einheit.Datum.ToString("ddd, dd.MM.yyyy", Deutsch)];
        if (!string.IsNullOrWhiteSpace(einheit.Name))
        {
            teile.Add(einheit.Name.Trim());
        }

        teile.Add(string.Format(Deutsch, "{0:N2} km", einheit.DistanzKm));
        teile.Add(string.Format(Deutsch, "{0:N0} hm", einheit.Hoehenmeter));
        if (einheit.MaxHoehe > 0)
        {
            teile.Add(string.Format(Deutsch, "höchster Punkt {0:N0} m", einheit.MaxHoehe));
        }

        return string.Join(" · ", teile);
    }
}
